using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings //This class does not inherit from monobehaviour. This way I can give it a
    //proper constructor and it doesnt need any of the monobehaviour behaviours anyway
{

    public  float DaysToRun = 50f;

    public EmptyScript emptyScript;
    public UITextHolderScript UIText;
    public Log log = new Log();

    public WaterSpawner[] waterSpawners = new WaterSpawner[10];



    //public Map map = Oasis (I havent implemented the map yet so this will remind me to change it later when the maps have been made)
    //public LinkedList<Foodlist> foodlistlist (same as above)

    //Gene spawning parameters
    public float aggressionMean = 10f, aggressionVariance = 10f;
    public float sizeMean = 10f, sizeVariance = 10f;
    public float reproductiveUrgeMean = 10f, reproductiveUrgeVariance = 4f;
    public float sightMean = 10f, sightVariance = 4f;
    public float cooperationMean = 15f, cooperationVariance = 4f;
    public float speedMean = 0, speedVariance = 0; // change
    public float hungerconstant = 0.0001f, waterconstant = 0.0001f;
    public float minGene = 0f, maxGene = 20f;
    public float secondsPerDay = 60f;

    public GameObject foodPrefab;
    public int maxFoodStack = 3;
    public int startingPop = 25;
    public float speedK = .1f;
    public float foodEatingTime = 1f, waterDrinkingTime = 1f;


    //General paramaters
    public float minHunger = 0.2f, maxHunger = 0.8f;
    public float minThirst = 0.2f, maxThirst = 0.8f;
    public float directionWobble = 30f;
    public int WobbleRollingAverageValues = 3;
    public float geneCloseness;
    public float foodFocalSTD = 10f;
    public float mapSizeX = 100f, mapSizeY = 100f;
    public float foodSideLength = 0.7f, foodDiagonalLength = 1.2f;

    public int totaltries = 0;
    public int framesPerDecision = 5;


    public int foodSpawnersCount = 0, MaxFoodSpawners = 7;
    public int waterSpawnersCount = 0, MaxWaterSpawners = 7;
    public GameObject waterTilePrefab;

    public FoodSpawner[] foodSpawners = new FoodSpawner[7];



    public Settings() { }


    public static bool distClose(Vector3 pos1, Vector3 pos2, float distSquared)
    {
        return (pos1 - pos2).sqrMagnitude < distSquared;
    }










    public bool setGenes()
    {
        // no validation required here, the sliders prevents dodgy inputs
        // settings main genes
        aggressionMean = UIText.Aggression.value * 20f; aggressionVariance = UIText.AggressionSTD.value * 20f;
        sizeMean = UIText.Size.value * 20f; sizeVariance = UIText.SizeSTD.value * 20f;
        reproductiveUrgeMean = UIText.ReproductiveUrge.value * 20f; reproductiveUrgeVariance = UIText.ReproductiveUrgeSTD.value * 20f;
        sightMean = UIText.Sight.value * 20f; sightVariance = UIText.SightSTD.value * 20f;
        cooperationMean = UIText.Cooperation.value * 20f; cooperationVariance = UIText.CooperationSTD.value * 20f;
        speedMean = UIText.Speed.value * 20f; speedVariance = UIText.SpeedSTD.value * 20f;

        // setting max/min hunger/thirst
        maxHunger = Mathf.Max(UIText.hunger1.value, UIText.hunger2.value);
        minHunger = Mathf.Min(UIText.hunger1.value, UIText.hunger2.value);
        maxThirst = Mathf.Max(UIText.thirst1.value, UIText.thirst2.value);
        minHunger = Mathf.Min(UIText.thirst1.value, UIText.thirst2.value);

        //validation required here
        try
        {
            startingPop = int.Parse(UIText.spawnInitially.text);
        }
        catch 
        {
            UIText.validationTextBox.text += "The starting population box must be an integer: " + UIText.spawnInitially.text + " is not a valid input. \n";

            return true; // if this returns true, the continue attempt will be failed and restarted. 
        }
        return false; // if it returns false, it will go through.
    }

    public void addFoodSpawner()
    {

        int Number = 0;
        float timePeriod = 0, delay = 0;
        string focalPointStr = "";
        bool focalPointOn = UIText.foodFocalPointToggle;
        Vector3 focalPos = new Vector3();


        UIText.validationTextBox.text = "";



        try
        {
            Number = int.Parse(UIText.foodNumber.text);
        }
        catch
        {
            UIText.validationTextBox.text += "The number must be an integer: " + UIText.foodNumber.text + " is not a valid input. \n";
        }

        try
        {
            timePeriod = float.Parse(UIText.foodTimePeriod.text) * this.secondsPerDay;
        }
        catch
        {
            UIText.validationTextBox.text += "The time period must be a number: " + UIText.foodTimePeriod.text + " is not a valid input. \n";
        }

        try
        {
            delay = float.Parse(UIText.foodDelay.text) * this.secondsPerDay;
        }
        catch
        {
            UIText.validationTextBox.text += "The delay must be a number: " + UIText.foodDelay.text + " is not a valid input. \n";
        }

        // the if statement ensures that if a focal point is not required, it doest matter what is in the focal point position box
        if (focalPointOn) { 
            try
            {
                focalPointStr = UIText.foodFocalPointText.text;

                string[] temp = focalPointStr.Split(',');
                float focalX = float.Parse(temp[0]), focalY = float.Parse(temp[1]);
                focalPos = new Vector3(focalX, 0f, focalY);
            }
            catch
            {
                UIText.validationTextBox.text += "The focal point must be a vector in the form '2.3, 4.5: " + UIText.foodFocalPointText.text + " is not a valid input. \n";
            }
        }
       


        if (UIText.validationTextBox.text == "")
        {
            FoodSpawner newFS = new FoodSpawner(timePeriod, Number, delay, focalPos, focalPointOn);
            foodSpawners[foodSpawnersCount] = newFS;
            foodSpawnersCount++;
        }

        UIText.displayFoodSpawners();

    }


    public void addWaterSpawner()
    {

        int Number = 0;
        string focalPointStr = "";
        bool focalPointOn = UIText.waterFocalPointToggle;
        Vector3 focalPos = new Vector3();


        UIText.validationTextBox.text = "";

        try
        {
            Number = int.Parse(UIText.waterNumber.text);
        }
        catch
        {
            UIText.validationTextBox.text += "The number must be an integer: " + UIText.waterNumber.text + " is not a valid input. \n";
        }

        // the if statement ensures that if a focal point is not required, it doest matter what is in the focal point position box
        if (focalPointOn)
        {
            try
            {
                focalPointStr = UIText.waterFocalPointText.text;

                string[] temp = focalPointStr.Split(',');
                float focalX = float.Parse(temp[0]), focalY = float.Parse(temp[1]);
                focalPos = new Vector3(focalX, 0f, focalY);
            }
            catch
            {
                UIText.validationTextBox.text += "The focal point must be a vector in the form '2.3, 4.5: " + UIText.waterFocalPointText.text + " is not a valid input. \n";
            }
        }



        if (UIText.validationTextBox.text == "")
        {
            WaterSpawner newWS = new WaterSpawner(Number, focalPos, focalPointOn);
            waterSpawners[waterSpawnersCount] = newWS;
            waterSpawnersCount++;
            newWS.Spawn();
        }

        UIText.displayWaterSpawners();







    }




}
