using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Action
{
    public GameObject target;
    public string task;
    public float incentive;


    public Action(GameObject _target, string _task, float _incentive)
    {
        target = _target;
        task = _task;
        incentive = _incentive;
    }

    public Action(GameObject _target)
    {
        target = _target;
        task = "Searching";
        incentive = 0f;
    }


    public bool GreaterThan(Action action) // returns true if this > argument
    {
        return this.incentive > action.incentive;
    }

    public override string ToString()
    {
        string str = "";
        str += task + " " + target.name;
        str += " for incentive of " + incentive.ToString();
        return str;
    }
}


public class Creature : MonoBehaviour
{
    // This static linked list will hold all the creatures currently alive, every frame the list will be iterated through
    // and creatures near each other will be added to their nearby lists
    public static LinkedList<Creature> creaturesList = new LinkedList<Creature>();
    public static GameObject creaturePrefab;
    public static int BornCreatures;
    public Settings settings = EmptyScript.settings;
    public float sightRadius, sightSquared;
    public EmptyScript emptyScript;
    public static EmptyScript emptyScriptS;
    public Action currentAction;
    public float health = 1f;
    public float timeSinceStartedAction = 0;

    LinkedList<Action> potentialActions;


    // Debug
    public GameObject _target; public float incentive; public string task;



    //declaring attributes

    public Genome genome;
    Rigidbody rb;
    //the creature will move around randomly, to stop them completely changing direction they will have a direction,
    //which will be randomly altered every frame


    LinkedList<GameObject> nearbyFood, nearbyWater;
    LinkedList<Creature> nearbyCre;
    public float ageSeconds = 0f, hunger = 0.2f, thirst = 0.5f;

    Queue<float> wobblevals;
    float wobbleav = 0f;



    bool HandleOutsideMap()
    {
        // making sure it never goes off the map
        float x = transform.position.x;
        float y = transform.position.z;
        bool xLow = x < -settings.mapSizeX / 2, xHigh = x > settings.mapSizeX / 2;
        bool yLow = y < -settings.mapSizeY / 2, yHigh = y > settings.mapSizeY / 2;
        bool notInMap = xLow | xHigh | yLow | yHigh;

        if (notInMap)
        {
            rb.velocity = new Vector3();
            if (xLow)
            {
                rb.velocity += new Vector3(1, 0, 0);
            }
            else if (xHigh)
            {
                rb.velocity += new Vector3(-1, 0, 0);
            }
            if (yLow)
            {
                rb.velocity += new Vector3(0, 0, 1);
            }
            else if (yHigh)
            {
                rb.velocity += new Vector3(0, 0, -1);
            }
            rb.velocity.Normalize();
            rb.velocity *= genome.speed * settings.speedK;
            return true;
        }
        else
        {
            return false;
        }
    }




    void DoRandomMovement()
    {
        if (HandleOutsideMap())
        {
            // velocity has already been handled so any changes would overwrite it and be bad
            return;
        }

        if (rb.velocity.magnitude == 0)
        {
            rb.velocity = transform.rotation * Vector3.forward * genome.speed * settings.speedK;
            return;
        }

        float change = settings.directionWobble * (Random.value - 0.5f) / settings.WobbleRollingAverageValues;

        wobblevals.Enqueue(change);
        wobbleav -= wobblevals.Dequeue();
        wobbleav += change;

        gameObject.transform.Rotate(new Vector3(0f, wobbleav, 0f));  // rotating the creature
        rb.velocity = Quaternion.AngleAxis(wobbleav, Vector3.up) * rb.velocity; // rotates the velocity vector by direction wobble
    }



    void GoTo(Vector3 position)
    {
        gameObject.transform.rotation = Quaternion.LookRotation(position - gameObject.transform.position);
        rb.velocity = Quaternion.LookRotation(position - transform.position) * Vector3.forward * genome.speed * settings.speedK;
    }

    public static Creature reproduceandSpawn(Creature p1, Creature p2)
    {

        Genome genom = new Genome(p1, p2); //creating the new creatures genome


        Vector3 midPoint = (p1.transform.position + p2.transform.position)/2f; //gives the midpoint between creatures p1 and p2

        Vector3 direction = Vector3.Cross(p1.transform.position - p2.transform.position, Vector3.up).normalized;
        // give a direction vector that points away from both p1 and p2

        Vector3 spawnPosition = midPoint + direction * 4f;


        GameObject newcreatureobj = Instantiate(creaturePrefab, spawnPosition, Quaternion.identity); //instantiates the creature prefab


        Creature newcreature = newcreatureobj.GetComponent<Creature>(); //retrieves the creature object from the new creature GameObject

        Creature.creaturesList.AddLast(newcreature); //adds this creature to the list of current creatures

        // its genome is assigned here but the other attributes will stay (e.g. all creatures spawn with 0.5 food, 0.5 water etc)
        newcreature.genome = genom;


        Creature.BornCreatures++;
        newcreature.name = "c" + Creature.BornCreatures;
        newcreature.currentAction = new Action(p1.gameObject, "RunAway", 0f);
        EmptyScript.settings.log.logBirth();

        return newcreature;
    }

    public static Creature spawnFromPosition(Vector3 position)
    {
        //the same as above except reproduced from the initial parents and spawns at a specified position

        Genome genom = new Genome();
        GameObject newcreatureobj = Instantiate(creaturePrefab, position, Quaternion.identity);

        Creature newcreature = newcreatureobj.GetComponent<Creature>();
        Creature.creaturesList.AddLast(newcreature);
        newcreature.genome = genom;


        Creature.BornCreatures++;
        newcreature.name = "c" + Creature.BornCreatures;

        // Debug
        newcreature.genome.sight = 1000f;
        newcreature.sightRadius = 1000f;
        newcreature.sightSquared = 1000000f;


        


        return newcreature;
    }

    float distK = 5f;
    float hungerMultK = 10f, hungerAddK = 0.2f, hungerThreshold = 0.6f;
    float sizeK = 100f;

    float incentiveFoodChase(GameObject targetFood)
    {
        float incentive = 0f;
        incentive -= distK * (this.transform.position - targetFood.transform.position).magnitude * (genome.hungerUsage / genome.speed);  
        // ^ is to account for the hunger used on the journey

        if(hunger > 0.9f)
        {
            return -10000f;
        }

        if (hunger < hungerThreshold)
        {
            incentive += hungerMultK / (hungerAddK + hunger); // the hungrier the creature gets the more it want to eat (represented by this)
        }
        else // if hunger is greater than the hungerThreshold then there is little point in eating
        {
            incentive += hungerMultK * (1 - hunger);
        }

        float[] dangerPosed = new float[nearbyCre.Count];
        i = 0;

        foreach (Creature creat in nearbyCre) // evaluating the danger of the creatures near the food object
        {
            if (creat != this)
            {

                float dist = (creat.transform.position - targetFood.transform.position).magnitude;
                if (dist < 10f) // if the enemy is too far away to be a threat then it doesnt pose a danger, if it is close it does pose a danger
                {
                    dangerPosed[i] = sizeK * creat.genome.size / (-0.5f + dist) / (genome.size * (genome.size - creat.genome.size + 20f)) / genome.aggression;
                } // the more aggressive, the lower the dangerPosed, the bigger size: the lower the danger posed, the bigger the enemy creature: the bigger the dangerPosed
                  //   the closer the enemy to the food: the bigger the dangerPosed.
                else
                {
                    dangerPosed[i] = 0f;
                }
            }
        }

        foreach (float value in dangerPosed)
        {
            incentive -= value;
        }

        return incentive;
    }

    bool ConditionFight(GameObject enemyGO)
    {
        return Settings.distClose(transform.position, enemyGO.transform.position, 3f * 3f);
    }

    float incentiveFight(GameObject enemyGO)
    {
        Creature enemy = enemyGO.GetComponent<Creature>();
        
        float mismatch = this.DamageOutput() - enemy.DamageOutput();
        float ratioMismatch = this.DamageOutput() / enemy.DamageOutput();

        if (mismatch < -3f - genome.aggression) // if it is signicantly weaker than its enemy
        {
            return -100f; // dont fight
        }
        else if (mismatch > 12f - genome.aggression) // if it is significantly stronger than its enemy
        {
            return mismatch * (1 + health);
        }
        else // if it is inbetween then it is a bit more up in the air which task it does. 
        {
            return (5f + mismatch) * genome.aggression / 20f - 10f;
        }
    }

    bool ConditionRunAway(GameObject enemyGO)
    {
        return enemyGO.GetComponent<Creature>().currentAction.target == this.gameObject;
    }

    float incentiveRunAway(GameObject enemyGO)
    {
        Creature enemy = enemyGO.GetComponent<Creature>();

        if (enemy.currentAction.task != "Fight" | enemy.currentAction.target != this.gameObject)
        {
            return -1000000f; // if no-one is atticking it, there is no point in running away
        }

        float mismatch = this.DamageOutput() - enemy.DamageOutput();
        float ratioMismatch = this.DamageOutput() / enemy.DamageOutput();

        if (ratioMismatch > 10f)
        {
            return -10000f;
        }

        if (mismatch > 5f - genome.aggression)
        {
            return -10000f;
        }
        else if (mismatch < 5f - genome.aggression)
        {
            return -10000f;
        }
        else
        {
            return genome.aggression * (mismatch + genome.aggression) / 100f;
        }
    }

    float ThirstMultK = 10f, ThirstAddK = 0.2f, ThirstThreshold = 0.6f;
    //                    
    float incentiveWaterChase(GameObject targetWater)
    {
        float incentive = 0f;
        incentive -= distK * (gameObject.transform.position - targetWater.transform.position).magnitude * (genome.waterUsage / genome.speed);
        // ^ is to account for the water used on the journey

        if (thirst > 0.9f)
        {
            return -10000f;
        }
        if (thirst < ThirstThreshold)
        {
            incentive += ThirstMultK / (ThirstAddK + thirst); // the thirstier the creature gets the more it want to eat (represented by this)
        }
        else // if water is greater than the water Threshold then there is little point in drinking
        {
            incentive += ThirstMultK * (1 - thirst);
        }

        float[] dangerPosed = new float[nearbyCre.Count];
        i = 0;

        foreach (Creature creat in nearbyCre) // evaluating the danger of the creatures near the water
        {
            if (creat != this)
            {


                float dist = (creat.transform.position - targetWater.transform.position).magnitude;
                if (dist < 5f) // if the enemy is too far away to be a threat then it doesnt pose a danger, if it is close it does pose a danger
                {
                    dangerPosed[i] = 100f * sizeK * creat.genome.size / (-0.5f + dist) / (genome.size * (genome.size - creat.genome.size + 20f)) / genome.aggression;
                } // the more aggressive, the lower the dangerPosed, the bigger size: the lower the danger posed, the bigger the enemy creature: the bigger the dangerPosed
                  //   the closer the enemy to the water: the bigger the dangerPosed.
                else
                {
                    dangerPosed[i] = 0f;
                }
            }
        }

        foreach (float value in dangerPosed)
        {
            incentive -= value;
        }

        return incentive;
    }

    bool ConditionShare(GameObject partnerGO)
    {
        Creature partner = partnerGO.GetComponent<Creature>();
        bool shareFood = this.hunger > settings.maxHunger && partner.hunger < settings.minHunger;
        bool shareWater = this.thirst > settings.maxThirst && partner.thirst < settings.minThirst;

        return shareFood | shareWater;
    }

    bool ConditionRecieveShare(GameObject partnerGO)
    {
        Creature partner = partnerGO.GetComponent<Creature>();
        return partner.currentAction.task == "Share" && partner.currentAction.target == this.gameObject;
    }

    float incentiveShare(GameObject partnerGO)
    {
        Creature partner = partnerGO.GetComponent<Creature>();
        float incentive = 0f;

        bool shareFood = this.hunger > settings.maxHunger && partner.hunger < settings.minHunger;
        bool shareWater = this.thirst > settings.maxThirst && partner.thirst < settings.minThirst;

        if (ConditionRecieveShare(partnerGO))
        {
            // if partner is trying to share with it, it will go towards the partner so the partner doesnt have to chase it across the map
            return 100f;
        }

        incentive -= distK * (transform.position - partner.transform.position).magnitude * (genome.hungerUsage + genome.waterUsage) / genome.speed;
        
        if (shareFood)
        {
            incentive += 50f;
        }
        if (shareWater)
        {
            incentive += 50f;
        }
        return incentive;
    }

    bool ConditionReproduce(GameObject partnerGO)
    {
        Creature partner = partnerGO.GetComponent<Creature>();
        return !(partner.hunger < 0.6f | partner.thirst < 0.6f | this.hunger < 0.6f | this.thirst < 0.6f); 
    }

    float incentiveReproduce(GameObject partnerGO)
    {
        Creature partner = partnerGO.GetComponent<Creature>();

        if (!ConditionReproduce(partnerGO))
        // ReproduceCondition just checks if both have the necessary food and water to reproduce 
        {
            return -10000f;
        }

        float dist = (transform.position - partner.transform.position).magnitude;
        float incentive = 0;
        incentive -= dist * genome.size * genome.size / genome.speed / 20;
        incentive += 5 * genome.reproductiveUrge;

        return incentive;
    }

    public float DamageOutput()
    {
        return genome.size * health;
    }

    public static void DebugActions()
    {
        foreach(Creature creature in Creature.creaturesList)
        {
            Debug.Log(creature.name + " : " + creature.currentAction.ToString());
        }
    }

    public void DecideAction()
    {
        nearbyCre = new LinkedList<Creature>();

        foreach(Creature creat in Creature.creaturesList)
        {
            if (Settings.distClose(transform.position, creat.transform.position, sightSquared)){
                nearbyCre.AddLast(creat);
            }
        }


        nearbyFood = new LinkedList<GameObject>();
        foreach(GameObject food in emptyScript.foodInGame)
        {
            if (Settings.distClose(food.transform.position, transform.position, sightSquared)){
                nearbyFood.AddLast(food);
            }
        }

        nearbyWater = new LinkedList<GameObject>();
        foreach(GameObject water in emptyScript.waterInGame)
        {
            if (Settings.distClose(water.transform.position, transform.position, sightSquared))
            {
                nearbyWater.AddLast(water);
            }
        }


        potentialActions = new LinkedList<Action>();
        potentialActions.AddLast(new Action(gameObject, "Search", - genome.size * genome.size / 20f)); 

        foreach(GameObject food in nearbyFood)
        {
            potentialActions.AddLast(new Action(food, "ChaseFood", incentiveFoodChase(food)));
        }

        foreach(GameObject water in nearbyWater)
        {
            potentialActions.AddLast(new Action(water, "ChaseWater", incentiveWaterChase(water)));
        }


        foreach(Creature creat in nearbyCre) // generating actions
        {
            if (creat != this)
            {
                GameObject creature = creat.gameObject;

                // fight
                if (ConditionFight(creature))
                {
                    potentialActions.AddLast(new Action(creat.gameObject, "Fight", incentiveFight(creat.gameObject)));
                }

                // run away
                if (ConditionRunAway(creature))
                {
                    potentialActions.AddLast(new Action(creat.gameObject, "RunAway", incentiveRunAway(creat.gameObject)));
                }

                // share
                if (ConditionShare(creature) | ConditionRecieveShare(creature))
                {
                    potentialActions.AddLast(new Action(creat.gameObject, "Share", incentiveShare(creat.gameObject)));
                }

                //reproduce
                if (ConditionReproduce(creature))
                {
                    potentialActions.AddLast(new Action(creat.gameObject, "Reproduce", incentiveReproduce(creat.gameObject)));
                }
            }
        }


        EvaluateActions();

    }

    public void EvaluateActions()
    {
        Action best = potentialActions.First.Value;

        foreach(Action action in potentialActions)
        {
            if (action.GreaterThan(best))
            {
                best = action;
            }
        }
        
        currentAction = best;

        _target = currentAction.target;
        incentive = currentAction.incentive;
        task = currentAction.task;
    }

    void PerformAction() {

        string task = currentAction.task;
        GameObject target = currentAction.target;
        if (task == "ChaseFood")
        {
            ChaseFood(target);
        }
        else if(task == "Search")
        {
            DoRandomMovement();
        }
        else if(task == null){
            Debug.Log(gameObject.name + " currentAction task is null");
        }
        else if(task == "Fight")
        {
            Fight(target);
        }
        else if (task == "RunAway")
        {
            RunAway(target);
        }
        else if (task == "Reproduce")
        {
            Reproduce(target);
        }
        else if (task == "GoTo")
        {
            GoTo(target.transform.position);
        }
        else if (task == "ChaseWater")
        {
            ChaseWater(target);
        }




    }


    public void ChaseFood(GameObject food)
    {
        float dist = (transform.position - food.transform.position).magnitude;

        if (dist > 1.3f)
        {
            GoTo(food.transform.position);
            timeSinceStartedAction = 0f;
            return;
        }
        else 
        {
            rb.velocity = new Vector3();
            timeSinceStartedAction += Time.deltaTime;
            Debug.Log(timeSinceStartedAction);
            if (timeSinceStartedAction > settings.foodEatingTime)
            {
                hunger = 1f;
                timeSinceStartedAction = 0f;
                currentAction = new Action(this.gameObject);
                FoodSpawner.DeleteFood(food);
                Debug.Assert(false);

                Creature.ReDecideActions();

                // once its done eating it will start searching
            }
        }
    }

    public void ChaseWater(GameObject water)
    {
        float dist = (transform.position - water.transform.position).magnitude;

        if (dist > 1.3f)
        {
            GoTo(water.transform.position);
            timeSinceStartedAction = 0f;
            return;
        }
        else
        {
            rb.velocity = new Vector3();
            timeSinceStartedAction += Time.deltaTime;
            if (timeSinceStartedAction > settings.waterDrinkingTime)
            {
                thirst = 1f;
                timeSinceStartedAction = 0f;
                currentAction = new Action(this.gameObject);
                WaterSpawner.WaterDrunk(water);
                Debug.Log(this.name + " drank " + water.name);
                Creature.ReDecideActions();

                // once its done drinking it will start searching
            }
        }
    }

    static void ReDecideActions()
    {
        foreach(Creature creature in creaturesList)
        {
            creature.DecideAction();
        }

    }


    void Fight(GameObject enemyGO)
    {
        Creature enemy = enemyGO.GetComponent<Creature>();
        float damage = genome.size / 20f / 100f / 5f; // Debug the last value 
        damage *=  2 * Random.value;
        // so that there is a bit of variability in damage output

        // Debug
        float distance = (transform.position - enemy.transform.position).magnitude;

        

        if (distance > 1f) 
        {
            // chase
            GoTo(enemy.transform.position);
            return; // stop the subroutine
        }
        else if (distance < .90f)
        {
            // lock them in combat
            rb.velocity = new Vector3();
            enemy.rb.velocity = new Vector3();
            enemy.currentAction = new Action(this.gameObject, "Fight", 0f);
        }

        if (enemy.TakeDamage(damage, "Fighting")) // if enemy dies
        {
            DecideAction();
        }
        
    }

    public bool Die(string method)
    {
        Creature.creaturesList.Remove(this);
        foreach(Creature creature in Creature.creaturesList)
        {
            creature.DecideAction(); // so that no other creatures still target the dead creature
        }                                   // (below) to give age in days
        settings.log.logDeath(Mathf.FloorToInt(ageSeconds / settings.secondsPerDay), method);
        GameObject.Destroy(this.gameObject);
        return true;
    }

    public bool TakeDamage(float damage, string method) // returns true if dead, false if not. Helps the other creature to know if it has killed it. 
    {
        health -= damage;

        if (CheckDead())
        {
            return Die(method);
            // I was worried that the GameObject.Destroy would prevent this from returning, but I tested it and it didnt
        }
        else
        {
            return false;
        }
    }

    public bool CheckDead()
    {
        return health < 0;
    }

    public void RunAway(GameObject enemy)
    {
        if (HandleOutsideMap())
        {
            // if outside map, must get back on the map
            return;
        }

        Vector3 deltaPosition = (transform.position - enemy.transform.position).normalized;

        GoTo(transform.position + deltaPosition);
    }
    
    public void Share(GameObject partnerGO)
    {
        Creature partner = partnerGO.GetComponent<Creature>();

        float dist = (transform.position - partner.transform.position).magnitude;

        if (dist < 1f) // if too far away
        {
            GoTo(partner.transform.position);
            return;
        }
        else
        {
            bool isRecipientHunger = hunger < settings.minHunger;
            bool isRecipientWater = thirst < settings.minThirst;

            if (isRecipientHunger)
            {
                hunger += (partner.hunger - this.hunger) / 3f;
                partner.hunger -= (partner.hunger - this.hunger) / 3f;
            }
            if (isRecipientWater)
            {
                thirst += (partner.thirst - this.thirst) / 3f;
                partner.thirst -= (partner.thirst - this.thirst) / 3f;
            }
            // no need to worry about if partner is recipient as partner will call this procedure too and if partner
            // is recipient then partner will take food/water

            if (!(isRecipientHunger | isRecipientWater | partner.hunger < settings.minHunger | partner.thirst < settings.minThirst))
            // if neither this nor partner is recipient then they are done sharing and can go on about their lives
            {
                currentAction = new Action(partner.gameObject, "RunAway", 0f);
                partner.currentAction = new Action(this.gameObject, "RunAway", 0f);
                // they will run away so that they dont risk fighting each other
            }
        }


    }

    public void Reproduce(GameObject partnerGO)
    {
        Creature partner = partnerGO.GetComponent<Creature>();

        float dist = (this.transform.position - partner.transform.position).magnitude;

        if (dist > 1.2f)
        {
            GoTo(partner.transform.position);
            return;
        }
        else if (ConditionReproduce(partnerGO))
        {
            this.hunger -= 0.5f; partner.hunger -= 0.5f;
            this.thirst -= 0.5f; partner.hunger -= 0.5f;

            Creature child = reproduceandSpawn(this, partner);
            // so they dont immediately fight each other
            child.currentAction = new Action(this.gameObject, "RunAway", 0f);
            partner.currentAction = new Action(this.gameObject, "RunAway", 0f);
            this.currentAction = new Action(partner.gameObject, "RunAway", 0f);
        }
    }

    void Start()
    {

        // assigning the various objects
        rb = gameObject.GetComponent<Rigidbody>();
        emptyScript = settings.emptyScript;


        // initialising the wobblevals all to 0
        float[] temp0 = new float[settings.WobbleRollingAverageValues];
        wobblevals = new Queue<float>(temp0);

        sightRadius = 100f; // genome.sight;
        sightSquared = 10000f; //genome.sight * genome.sight;

        DecideAction();


    }

    
    public GameObject cube;
    int i;
    GameObject cub;

    void placecubes(int howoften)
    {
        i += 1;
        if (i % howoften == 0)
        {
            cub = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cub.transform.localScale = new Vector3(1f, 1f, 1f) * 0.2f;
            cub.transform.position = gameObject.transform.position + new Vector3(0, 2f, 0);
            cub.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        }
    }



    
    public static void CreatureInteracter()
    {
        Creature c1, c2;
        LinkedListNode<Creature> n1, n2;
        n1 = Creature.creaturesList.First; c1 = n1.Value;
        n2 = n1.Next; c2 = n2.Value;
        int count = creaturesList.Count;



        for (int i = 1; i < count; i++)
        {

            for (int j = i+1; j < count + 1; j++)
            {

                //Interactions


                if (j != count) { n2 = n2.Next; c2 = n2.Value; } // to stop it from trying to find the next
            }
            if (i != count - 1){
                n1 = n1.Next; c1 = n1.Value;
                n2 = n1.Next; c2 = n2.Value;
            }
        }



    }


    int frames = 0;


    //if (j != count) { n2 = n2.Next; c2 = n2.Value; } // to stop it from trying to find the next
    void FixedUpdate()
    {
        //DoRandomMovement();


        // lock rotation
        Vector3 New;
        New = transform.rotation.eulerAngles;
        New.x = 0;
        New.z = 0;
        transform.rotation = Quaternion.Euler(New);

        // lock vertical position
        New = transform.position; New.y = 0f;
        transform.position = New;

        // lock vertical velocity
        New = rb.velocity; New.y = 0f;
        rb.velocity = New;


        frames++; // in order to reduce the number of expensive calls
        if (frames > settings.framesPerDecision)
        { // currently at 5^
            frames -= settings.framesPerDecision;
            DecideAction();
        }
        PerformAction();






        
    }
}
