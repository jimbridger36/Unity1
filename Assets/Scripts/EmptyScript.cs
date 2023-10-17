using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmptyScript : MonoBehaviour
{
    public static Settings settings = new Settings(); //  = new Settings()

    public GameObject prefabCreature;
    public GameObject foodPrefab;
    //public static Creature parent1, parent2;
    //public static settings settings;
    public static float time = 0;
    public LinkedList<GameObject> foodInGame = new LinkedList<GameObject>();

    public Camera camera;
    public Canvas settingsCanvas;
    public UITextHolderScript textHolder;
    public LinkedList<GameObject> waterInGame = new LinkedList<GameObject>();
    public GameObject waterTilePrefab;





    // Start is called before the first frame update
    void Start()
    {

        settings.emptyScript = this;
        settings.UIText = textHolder;
        settings.UIText.settings = settings;

        Creature.creaturePrefab = prefabCreature;
        Creature.creaturesList = new LinkedList<Creature>();
        Creature.BornCreatures = 0;
        Creature.emptyScriptS = this;




        settings.foodPrefab = foodPrefab;
        settings.waterTilePrefab = waterTilePrefab;



    }

    public void addFoodSpawner()
    {
        settings.addFoodSpawner();
    }




    private void Update()
    {
        time += Time.deltaTime;
        //Creature.DebugActions();
        if (Input.GetKeyDown("space"))
        {
            settings.foodSpawners[0].spawnWave();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Creature.reproduceandSpawn(Creature.creaturesList.First.Value, Creature.creaturesList.First.Next.Value);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            settingsCanvas.enabled = !settingsCanvas.isActiveAndEnabled;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            settings.UIText.FollowCreature();
        }


        foreach(FoodSpawner spawner in settings.foodSpawners)
        {
            if (spawner != null)
            {
                spawner.spawnWave();
            }
        }
    }


    public void SpawnInitially()
    {
        // I have made it into a function for ease of use
        Vector3 PickEvenPosition()
        {
            float x = -1000000f; // making sure its not in the range so that the value gets picked
            while (x < (-settings.mapSizeX / 2) | x > (settings.mapSizeX / 2)) // making sure that position is on the map 
            {                                                                   // as the range of the normal distribution is infinite
                x = (Random.value - 0.5f) * settings.mapSizeX;
            }
            float y = -1000000f; // same but for y
            while (y < (-settings.mapSizeY / 2) | y > (settings.mapSizeY / 2)) 
            {
                y = (Random.value - 0.5f) * settings.mapSizeY;
            }
            return new Vector3(x, 0, y);
        }

        for (int i = 0; i < settings.startingPop; i++)
        {
            Vector3 potentialPos = new Vector3();
            bool tooClose = true;
            while (tooClose)
            {
                potentialPos = PickEvenPosition();

                tooClose = false;
                foreach(Creature creat in Creature.creaturesList) 
                {
                    if ((potentialPos - creat.transform.position).magnitude < 1.5f) // checking if it is too close
                    {
                        tooClose = true; // if it is too close it will stop checking and restart
                        break;
                    }
                }
            }
            Creature creat1 =  Creature.spawnFromPosition(potentialPos);
            creat1.emptyScript = this;
        }






    }


    void UpdateStacks() {

        foreach (FoodSpawner foodSpawner in settings.foodSpawners)
        {
            foodSpawner.foodStack = new int[foodSpawner.foodStack.Length];
        }

        foreach (GameObject food in settings.emptyScript.foodInGame)
        {
            Vector3 pos = food.transform.position;
            pos.y = 0f;

            foreach(FoodSpawner foodSpawner in settings.foodSpawners)
            {
                if (foodSpawner.posToInd.ContainsKey(pos))
                {
                    foodSpawner.foodStack[foodSpawner.posToInd[pos]] += 1;
                }

            }
        }
    }
}
