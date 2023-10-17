using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Log
{

    public Settings settings = EmptyScript.settings;


    private int day;
    public int waterDrunk = 0;
    public int[] creatBornA, creatDiedA, deadToCreatA, deadToEnvA, populationA; // everything ending in A is an array

    public float[] sizeA, sightA, reproductiveurgeA, aggressionA, speedA, cooperationA, sizestdA, sightstdA, reproductiveurgestdA, aggressionstdA, speedstdA, cooperationstdA;
    public float[] ageA, ageAtDeathA;
    public int foodSpawned = 0;



    public Log()// constructor, just initialises the arrays to be empty nothing special
    {
        //initialising empty arrays
        creatBornA = new int[10];
        creatDiedA = new int[10];
        deadToCreatA = new int[10];
        deadToEnvA = new int[10];
        populationA = new int[10];

        sizeA = new float[10];
        sightA = new float[10];
        reproductiveurgeA = new float[10];
        aggressionA = new float[10];
        speedA = new float[10];
        cooperationA = new float[10];
        sizestdA = new float[10];
        sightstdA = new float[10];
        reproductiveurgestdA = new float[10];
        aggressionstdA = new float[10];
        speedstdA = new float[10];
        cooperationstdA = new float[10];

        ageA = new float[10];
        ageAtDeathA = new float[10];

    }

    public void logFoodSpawn() {
        foodSpawned += 1;
    }


    public static int[] rsint(int[] arr)
    {
        int[] tmp = new int[2 * arr.Length];
        arr.CopyTo(tmp,0);
        return tmp;
    }

    public static float[] rsflt(float[] arr) // a function to resize arrays
    {
        float[] tmp = new float[2 * arr.Length];
        arr.CopyTo(tmp, 0);
        return tmp;
    }

    public void resizeall() // just resizes all the arrays by a factor of 2
    {      
        sizeA = rsflt(sizeA); sizestdA = rsflt(sizestdA);
        sightA = rsflt(sightA); sightstdA = rsflt(sightstdA);
        reproductiveurgeA = rsflt(reproductiveurgeA); reproductiveurgestdA = rsflt(reproductiveurgestdA);
        aggressionA = rsflt(aggressionA); aggressionstdA = rsflt(aggressionstdA);
        speedA = rsflt(speedA); speedstdA = rsflt(speedstdA);
        cooperationA = rsflt(cooperationA); cooperationstdA = rsflt(cooperationstdA);

        creatBornA = rsint(creatBornA);creatDiedA  = rsint(creatDiedA);
        deadToCreatA = rsint(deadToCreatA); deadToEnvA = rsint(deadToEnvA);
        populationA = rsint(populationA);
    }

    public void logDeath(int age, string method) 
    {
        if(method == "creature"){
            deadToCreatA[day] += 1;
        }
        else{
            deadToEnvA[day] += 1;
        }

        creatDiedA[day] += 1;
        populationA[day] -= 1;
        ageAtDeathA[day] = (ageAtDeathA[day] * (creatDiedA[day] - 1) + age) / creatDiedA[day];
    }

    public void logBirth() {
        creatBornA[day] += 1;
        populationA[day] += 1;
    }

    public void logWaterDrunk()
    {
        waterDrunk += 1;
    }

    public void EvaluateGeneAverageSTD() // every day (to reduce expensive function calls) this procedure will
        //be called to evaluate the average and standard deviation of the genes and log it in the array.
        //if in the future this is not enough then it can quickly easily be added to the logDeath and logBirth procedures
    {
        float[] genesum = new float[6], genesumsqu = new float[6];

        foreach(Creature cre in Creature.creaturesList)
        {
            Genome gen = cre.genome;
            genesum[0] += gen.size; genesumsqu[0] += Mathf.Pow(gen.size,2);
            genesum[1] += gen.sight; genesumsqu[1] += Mathf.Pow(gen.sight, 2);
            genesum[2] += gen.reproductiveUrge; genesumsqu[2] += Mathf.Pow(gen.reproductiveUrge, 2);
            genesum[3] += gen.aggression; genesumsqu[3] += Mathf.Pow(gen.aggression, 2);
            genesum[4] += gen.speed; genesumsqu[4] += Mathf.Pow(gen.speed, 2);
            genesum[5] += gen.cooperation; genesumsqu[5] += Mathf.Pow(gen.cooperation, 2);
        }

        for(int i=0; i < 6; i++){
            genesum[i] /= Creature.creaturesList.Count; genesumsqu[i] /= Creature.creaturesList.Count;
            // getting the intermediate result needed for mean and std calculations
        }
        float std(int i) //gets the standard deviation of gene with index i
        {
            return genesumsqu[i] - genesum[i] * genesum[i]; // at the point this function is called the genesum and genesum squ are already averaged
        }
        sizeA[day] = genesum[0]; sizestdA[day] = std(0);
        sightA[day] = genesum[1]; sightstdA[day] = std(1);
        reproductiveurgeA[day] = genesum[2]; reproductiveurgestdA[day] = std(2);
        aggressionA[day] = genesum[3]; aggressionstdA[day] = std(3);
        speedA[day] = genesum[4]; speedstdA[day] = std(4);
        cooperationA[day] = genesum[5]; cooperationstdA[day] = std(5);
    }


    public void EvaluateAge() {
        float agesum = 0f;

        foreach(Creature cre in Creature.creaturesList)
        {
            agesum += cre.ageSeconds;
        }
        ageA[day] = agesum / Creature.creaturesList.Count;
    }


    public void endOfDayEvaluation()
    {
        EvaluateGeneAverageSTD();
        EvaluateAge();
    }









}
