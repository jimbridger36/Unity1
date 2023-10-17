using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSpawner
{
    //public LinkedList<GameObject> foodInGame = new LinkedList<GameObject>();
    public Vector3[] foodlistPos;


    public Vector3 focalPoint;
    public bool focus;
    public int number;

    public static Settings settings = EmptyScript.settings;
    public static EmptyScript emptyScript = EmptyScript.settings.emptyScript;




    public WaterSpawner(int _number, Vector3 _focalPoint, bool _focus)
    {
        number = _number;
        foodlistPos = new Vector3[number];


        focalPoint = _focalPoint;
        focus = _focus;




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
            return new Vector3(x, 0, y);
        }


        if (true)
        {

            float focusx = focalPoint.x;
            float focusy = focalPoint.y;

            //int actualNumber = number;
            for (int i = 0; i < number; i++)
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
                    foreach (WaterSpawner spawner in settings.waterSpawners)
                    {
                        if (spawner != null)
                        {
                            foreach (Vector3 foodPosT in spawner.foodlistPos)
                            {
                                if ((potentialPos - foodPosT).magnitude < 1.5f) // checking if it is too close
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

                        for (int j = 0; j < i - 1; j++) // check the j < i - 1
                        {
                            foodPos = foodlistPos[j];

                            if ((potentialPos - foodPos).magnitude < 1.5f) // checking if it is too close
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







    }






    public override string ToString()
    {

        string str = number.ToString() + " water. ";

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







    public void Spawn()
    {
        foreach(Vector3 pos in foodlistPos)
        {
            GameObject New = GameObject.Instantiate(settings.waterTilePrefab, pos + new Vector3(0, -.499f, 0), Quaternion.identity);
            settings.log.logWaterDrunk();
            New.name = "w" + settings.log.waterDrunk.ToString();
            emptyScript.waterInGame.AddLast(New);
        }
    }




    public static void WaterDrunk(GameObject water)
    {
        Vector3 posNew = water.transform.position;
        
        emptyScript.waterInGame.Remove(water);
        GameObject.Destroy(water);
        settings.log.logWaterDrunk();

        GameObject newWater = GameObject.Instantiate(emptyScript.waterTilePrefab, posNew, Quaternion.identity);

        newWater.name = "w" + settings.log.waterDrunk.ToString();
        emptyScript.waterInGame.AddLast(newWater);

    }
}