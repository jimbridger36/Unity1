using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UITextHolderScript : MonoBehaviour
{
    // food
    public InputField foodFocalPointText, foodDelay, foodTimePeriod, foodNumber;
    public TextMeshProUGUI spawnerDisplay;
    public bool foodFocalPointToggle = true;
    public Settings settings;
    public Text validationTextBox;

    // genes
    public Slider Aggression, Size, ReproductiveUrge, Speed, Sight, Cooperation;
    public Slider AggressionSTD, SizeSTD, ReproductiveUrgeSTD, SpeedSTD, SightSTD, CooperationSTD;
    public Slider hunger1, hunger2, thirst1, thirst2;
    public float maxHunger, minHunger, maxThirst, minThirst;


    // water
    public InputField waterFocalPointText, waterNumber;
    public bool waterFocalPointToggle = true;
    public TextMeshProUGUI waterDisplay;


    public Canvas canvas;

    public InputField spawnInitially;


    public Text c1Action, c2Action;

    public Camera camera;


    public void Start()
    {
        //canvas.enabled = false;
    }

    public void setFoodFocalPointToggle(bool value)
    {
        foodFocalPointToggle = value;
    }

    public void setWaterFocalPointToggle(bool value)
    {
        waterFocalPointToggle = value;
    }

    public void displayFoodSpawners()
    {
        string message = "";
        int i = 0;
        while (i < settings.foodSpawnersCount && settings.foodSpawners[i] != null)
        {
            message += settings.foodSpawners[i].ToString() + " \n"; // \n is the newline character
            i++;
        }
        spawnerDisplay.text = message;
    }



    public void displayWaterSpawners()
    {

        string message = "";
        int i = 0;
        while(i < settings.waterSpawners.Length && settings.waterSpawners[i] != null)
        {
            message += settings.waterSpawners[i].ToString() + "\n";
            i++;
        }
        waterDisplay.text = message;

    }

    public void FollowCreature()
    {
        camera.transform.parent = canvas.transform;
    }


    public void addWaterSpawner()
    {
        settings.addWaterSpawner();
    }

    public void ContinueSettings() // called by the continue button
    {
        if (settings.setGenes())
        {
            return;
        }
        else
        {
            canvas.enabled = false;
            settings.emptyScript.SpawnInitially();
            c1 = GameObject.Find("c1").GetComponent<Creature>();
            c2 = GameObject.Find("c2").GetComponent<Creature>();
            // start simulation
        }
    }

    Creature c1, c2;
    public void DisplayInfo()
    {

    }





}
