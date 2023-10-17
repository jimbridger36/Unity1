using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner
{
    //public LinkedList<GameObject> foodInGame = new LinkedList<GameObject>();
    public Vector3[] foodlistPos;


    public int[] foodStack;
    protected float TimePeriod { get; set; } // in days
    public int spawnsDone = -1;
    public float TimeDelay;
    public Vector3 focalPoint;
    public bool focus;
    public int number;

    public static Settings settings = EmptyScript.settings;
    public static EmptyScript emptyScript = EmptyScript.settings.emptyScript;



    public Dictionary<Vector3, int> posToInd; // used to assign food to the foodlist

    public FoodSpawner(float timeP, int _number, float delay, Vector3 _focalPoint, bool _focus) 
    {
        TimePeriod = timeP;
        TimeDelay = delay;
        number = _number;
        foodStack = new int[number];
        foodlistPos = new Vector3[number];


        focalPoint = _focalPoint;
        focus = _focus;
        int previousTotalTries = settings.totaltries;




        Vector3 PickEvenPosition()
        {
            float x = -1000000f; // making sure its not in the range so that the value gets picked
            while (x < (-settings.mapSizeX / 2) | x > (settings.mapSizeX / 2)) // making sure that the food spawns on the map 
            {                                                                   // as the range of the normal distribution is infinite
                x = (Random.value - 0.5f) * settings.mapSizeX;
            }
            float y = -1000000f; // making sure its not in the correct range
            while (y < (-settings.mapSizeY / 2) | y > (settings.mapSizeY / 2)) // making sure that the position is on the map
            {
                y = (Random.value - 0.5f) * settings.mapSizeY;
            }
                return new Vector3(x,0,y);
        }


        if (true){

            float focusx = focalPoint.x;
            float focusy = focalPoint.y;

            //int actualNumber = number;
            for (int i = 0; i <number; i++)
            {
                bool tooClose = true;
                Vector3 potentialPos = new Vector3();
                while (tooClose) // && tries < 15
                {

                    if (!focus)
                    {
                        potentialPos = PickEvenPosition();
                    }
                    else
                    {
                        float x = -1000000f; // making sure its not in the range so that the value gets picked
                        while (x < (-settings.mapSizeX / 2) | x > (settings.mapSizeX / 2)) // making sure that the food spawns on the map 
                        {   
                            x = RandomFromDistribution.RandomNormalDistribution(focusx, settings.foodFocalSTD);
                        }
                        float y = -1000000f; // making sure its not in the correct range
                        while (y < (-settings.mapSizeY / 2) | y > (settings.mapSizeY / 2)) // making sure that the food spawns on the map (while (not in the map bounds))
                        {
                            y = RandomFromDistribution.RandomNormalDistribution(focusy, settings.foodFocalSTD);
                        }
                        potentialPos = new Vector3(x, 0f, y);
                    }



                    // check if it is on a water tile




                    tooClose = false;
                    foreach (FoodSpawner spawner in settings.foodSpawners)
                    {
                        if (spawner != null)
                        {
                            foreach (Vector3 foodPosT in spawner.foodlistPos)
                            {
                                if ((potentialPos - foodPosT).magnitude < settings.foodDiagonalLength) // checking if it is too close
                                {
                                    tooClose = true; // if it is too close it will stop checking and restart
                                    break;
                                }

                            }
                        }
                        if (tooClose)
                        {
                            break;
                        }
                    }
                    

                    if (!tooClose) { 
                    Vector3 foodPos = new Vector3();

                    // checking if it is too close to the other locations in the foodlist itself

                    for (int j = 0; j < i - 1; j++) // check the j < i - 1
                    {
                        foodPos = foodlistPos[j];

                        if ((potentialPos - foodPos).magnitude < settings.foodDiagonalLength) // checking if it is too close
                        {
                            tooClose = true; // if it is too close it will stop checking and restart
                            settings.totaltries++;
                            break;
                        }
                    }
                }
                }
                foodlistPos[i] = potentialPos;
            }
        }
        


            




        posToInd = new Dictionary<Vector3, int>();

        for(int i = 0; i < foodlistPos.Length; i++)
        {
            posToInd.Add(foodlistPos[i], i);
        }
    }



    



    public void spawnWave() //this function will be called every frame
    {
        if (EmptyScript.time > TimePeriod * (spawnsDone + 1) + TimeDelay) // checks if its time to spawn another wave
        {
            for (int i = 0; i < foodlistPos.Length; i++)
            {
                spawnIndex(i);
            }
            spawnsDone += 1;
        }
        else
        {
            return;
        }
    }

    public override string ToString() 
    {

        string str = "";

        str += number.ToString() + " food every " + (TimePeriod / settings.secondsPerDay).ToString() + " days, for " + (Mathf.Round(10f * number * settings.secondsPerDay / TimePeriod) / 10f).ToString();
        str += " food per day. ";
        if (focus)
        {
            str += "Distributed by the focal point: (" + focalPoint.x.ToString() + ", " + focalPoint.z.ToString() + ").";
        }
        else
        {
            str += "Distributed evenly.";
        }

        return str;

    }




    public bool checkPosition(Vector3 pos)
    {

        bool tooClose = false;
        foreach (FoodSpawner spawner in settings.foodSpawners)
        {
            if (spawner != null)
            {
                foreach (Vector3 foodPosT in spawner.foodlistPos)
                {
                    if ((pos - foodPosT).magnitude < settings.foodDiagonalLength) // checking if it is too close
                    {
                        tooClose = true; // if it is too close it will stop checking and restart
                        break;
                    }

                }
            }
            if (tooClose)
            {
                break;
            }
        }



        if (!tooClose)
        {
            Vector3 foodPos = new Vector3();

            // checking if it is too close to the other locations in the foodlist itself

            for (int j = 0; j < number & foodlistPos[j] != null; j++)
            {
                foodPos = foodlistPos[j];

                if ((pos - foodPos).magnitude < settings.foodDiagonalLength) // checking if it is too close
                {
                    tooClose = true; // if it is too close it will stop checking and restart
                    settings.totaltries++;
                    break;
                }
            }



        }
        return !tooClose;
    }



    void spawnIndex(int i) // handles the spawning of food location with index i in the foodList
    {
        if(foodStack[i] >= settings.maxFoodStack)
        {
            return;
        }
        else
        {
            Vector3 position = foodlistPos[i] + Vector3.up * (-0.15f + foodStack[i] * 0.7f);
            foodStack[i] += 1;


            settings.emptyScript.foodInGame.AddLast(GameObject.Instantiate(settings.foodPrefab, position, Quaternion.identity));//spawns the food and adds it to the foodInGame LinkedList
            settings.emptyScript.foodInGame.Last.Value.name = "food" + settings.log.foodSpawned.ToString();
            settings.log.logFoodSpawn();
            ReOrderHeight(foodlistPos[i]);
        }
    }





    public static void DeleteFood(GameObject food)
    {
        Vector3 pos = food.transform.position;
        pos.y = 0f;
        bool debug = false;

        foreach (FoodSpawner foodSpawner in FoodSpawner.settings.foodSpawners) // checks if the food is in the foodlist
        {
            if(foodSpawner == null)
            {
                // do nothing
            }
            else if (foodSpawner.posToInd.ContainsKey(pos))
            {
                foodSpawner.foodStack[foodSpawner.posToInd[pos]] -= 1;
                debug = true;
                break;
            }
        }

        Debug.Assert(debug);

        emptyScript.foodInGame.Remove(food);
        GameObject.Destroy(food);

        ReOrderHeight(pos);
    }


    public static void ReOrderHeight(Vector3 pos)
    {
        Debug.Log(pos);
        List<GameObject> DropDownList = new List<GameObject>();
        Vector3 position = new Vector3();
        foreach (GameObject fod in emptyScript.foodInGame)
        {
            position = fod.transform.position;
            position.y = 0;
            if (position == pos)
            {
                DropDownList.Add(fod);
            }
        }

        foreach(GameObject item1 in DropDownList)
        {
            Debug.Log(item1.transform.position);
        }


        float previousHeight = -0.15f - 0.7f;
        Vector3 position1 = pos;
        position1.y = previousHeight;

        foreach (GameObject item in DropDownList)
        {
            position1.y = previousHeight + 0.7f;
            previousHeight += 0.7f;
            item.transform.position = position1;
        }
    }

}


