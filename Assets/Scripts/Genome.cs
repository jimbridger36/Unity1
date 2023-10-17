using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Genome
{
    //declaring the attributes
    public float aggression = 10f;
    public float size = 10f;
    public float reproductiveUrge = 10f; //just hardcoded temporarily
    public float speed = 10f;
    public float sight = 10f;
    public float cooperation = 10f;
    public float hungerUsage = 2f;
    public float waterUsage = 2f;

    /*
    public static float aggressionD = 10f;
    public static float sizeD = 10f;
    public static float reproductiveUrgeD = 10f; //just hardcoded temporarily
    public static float speedD = 10f;
    public static float sightD = 10f;
    public static float cooperationD = 10f;
    public static float hungerUsageD = 2f;
    public static float waterUsageD = 2f;
    */

    public Settings settings = EmptyScript.settings;



    //the constructor takes the parents as arguments to create the new genome
    public Genome(Creature p1, Creature p2)
    {
        Genome g1 = p1.genome, g2 = p2.genome;
        float mean(float a, float b){return (a + b) / 2;}

        Debug.Log("a baby is born");




        // the normal distribution models genetic mutations quite well
        this.aggression = range(RandomFromDistribution.RandomNormalDistribution(mean(g1.aggression,g2.aggression),settings.aggressionVariance)); 
        this.size = range(RandomFromDistribution.RandomNormalDistribution(mean(g1.size, g2.size), settings.sizeVariance));
        this.reproductiveUrge = range(RandomFromDistribution.RandomNormalDistribution(mean(g1.reproductiveUrge, g2.reproductiveUrge), settings.reproductiveUrgeVariance));
        this.speed = range(RandomFromDistribution.RandomNormalDistribution(mean(g1.speed, g2.speed), settings.speedVariance));
        this.sight = range(RandomFromDistribution.RandomNormalDistribution(mean(g1.sight, g2.sight), settings.sightVariance));
        this.cooperation = range(RandomFromDistribution.RandomNormalDistribution(mean(g1.cooperation, g2.cooperation), settings.cooperationVariance));
        this.hungerUsage = Mathf.Pow(this.size, 3f) * Mathf.Pow(this.speed, 2f) * settings.hungerconstant;
        this.waterUsage = Mathf.Pow(this.size, 3f) * Mathf.Pow(this.speed, 2f) * settings.waterconstant;

    } 
     
    float range(float a)
    {
        if (a < settings.minGene)
        {
            return 0.001f;
        }
        else if (a > settings.maxGene)
        {
            return settings.maxGene;
        }
        else
        {
            return a;
        }
    }//the normal distribution can theoretically be any number, including negative ones and ones out of our gene range (although very unlikely).
     //to get around this every gene will be constrained into the gene range (by default 0-20)
    public Genome()
    { 
        this.aggression = range(RandomFromDistribution.RandomNormalDistribution(settings.aggressionMean, settings.aggressionVariance));
        this.size = range(RandomFromDistribution.RandomNormalDistribution(settings.sizeMean, settings.sizeVariance));
        this.reproductiveUrge = range(RandomFromDistribution.RandomNormalDistribution(settings.sizeMean, settings.reproductiveUrgeVariance));
        this.speed = range(RandomFromDistribution.RandomNormalDistribution(settings.speedMean, settings.speedVariance));
        this.sight = range(RandomFromDistribution.RandomNormalDistribution(settings.sightMean, settings.sightVariance));
        this.cooperation = range(RandomFromDistribution.RandomNormalDistribution(settings.cooperationMean, settings.cooperationVariance));
        this.hungerUsage = range(Mathf.Pow(this.size, 3f) * Mathf.Pow(this.speed, 2f) * settings.hungerconstant);
        this.waterUsage = range(Mathf.Pow(this.size, 3f) * Mathf.Pow(this.speed, 2f) * settings.waterconstant);
    }


    public bool sameSpecies(Creature c1, Creature c2)
    {
        bool close(float a, float b) {
            return Mathf.Abs(a - b) < settings.geneCloseness;
        } // will return true if the genes are close enough to be considered similar
        
        Genome g1 = c1.genome, g2 = c2.genome;

        //all genes must be similar in order to be considered the same species
        return close(g1.aggression, g2.aggression) & close(g1.size, g2.size) & close(g1.aggression, g2.aggression) & close(g1.reproductiveUrge, g2.reproductiveUrge) & close(g1.speed, g2.speed) & close(g1.sight, g2.sight) & close(g1.cooperation, g2.cooperation);
    }


}






