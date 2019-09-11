using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GeneticAlgorithm : MonoBehaviour
{
    [SerializeField] private double CrossoverRate;
    [SerializeField] private double MutationRate;
    [SerializeField] private int PopulationSize;
    [SerializeField] private int ChromosomeLength;

    private List<Genome> mGenomes;
    private Map mMap;
    private double mBestFitness;
    private double mTotalFitness;
    private int mGeneration;
    private bool mBusy;

    private void Awake()
    {
        mGenomes = new List<Genome>();
        mMap = GetComponent<Map>();
        mBestFitness = 0.0f;
        mTotalFitness = 0.0f;
        mGeneration = 0;
        mBusy = false;

        CreateStartPopulation();
        StartCoroutine(Run());
    }

    private void CreateStartPopulation()
    {
        for (int count = 0; count < PopulationSize; ++count)
        {
            Genome genome = new Genome(ChromosomeLength);
            mGenomes.Add(genome);
        }
    }

    private void Crossover(Genome mum, Genome dad, Genome baby1, Genome baby2)
    {
        if (UnityEngine.Random.value > CrossoverRate || mum == dad)
        {
            baby1.Bits = mum.Bits;
            baby2.Bits = dad.Bits;
        }
        else
        {
            int point = UnityEngine.Random.Range(0, ChromosomeLength - 1);
            for (int count = 0; count < ChromosomeLength; ++count)
            {
                baby1.Bits.Set(count, count < point ? mum.Bits.Get(count) : dad.Bits.Get(count));
                baby2.Bits.Set(count, count < point ? dad.Bits.Get(count) : mum.Bits.Get(count));
            }
        }
    }

    private void Mutate(BitArray bits)
    {
        for(int count = 0; count < bits.Length; ++count)
        {
            if( UnityEngine.Random.value < MutationRate)
            {
                bits.Set(count, !bits.Get(count));
            }
        }
    }

    private Genome RouletteWheelSelection()
    {
        double slice = UnityEngine.Random.value * mTotalFitness;
        double total = 0.0;
        int selected = 0;

        for (int count = 0; count < PopulationSize; ++count)
        {
            total += mGenomes[count].Fitness;
            if( total > slice)
            {
                selected = count;
                break;
            }
        }

        return mGenomes[selected];
    }

    private List<Direction> Decode(BitArray bits)
    {
        List<Direction> result = new List<Direction>(); 
        for (int count = 0; count < bits.Length; count += 2)
        {
            bool one = bits.Get(count);
            bool two = bits.Get(count + 1);

            if( !one && !two )
            {
                result.Add(Direction.North);
            }
            else if( !one && two )
            {
                result.Add(Direction.South);
            }
            else if( one && !two )
            {
                result.Add(Direction.East);
            }
            else
            {
                result.Add(Direction.West);
            }
        }

        return result;
    }

    private IEnumerator UpdateFitnessScores()
    {
        mTotalFitness = 0.0f;
        mBestFitness = 0.0f;
        int bestIndex = -1;

        for ( int count = 0; count < mGenomes.Count; ++count)
        {
            mGenomes[count].Fitness = mMap.TestRoute(Decode(mGenomes[count].Bits));
            mTotalFitness += mGenomes[count].Fitness;

            if (mGenomes[count].Fitness > mBestFitness)
            {
                mBestFitness = mGenomes[count].Fitness;
                bestIndex = count;
            }
        }

        // Run the best solution again so that it is the one which is left showing when we stop
        mMap.TestRoute(Decode(mGenomes[bestIndex].Bits));

        yield return null;
    }

    private IEnumerator Epoch()
    {
        yield return UpdateFitnessScores();

        List<Genome> babyGenomes = new List<Genome>();

        while( babyGenomes.Count < PopulationSize )
        {
            Genome mum = RouletteWheelSelection();
            Genome dad = RouletteWheelSelection();
            Genome baby1 = new Genome(ChromosomeLength);
            Genome baby2 = new Genome(ChromosomeLength);

            Crossover(mum, dad, baby1, baby2);
            Mutate(baby1.Bits);
            Mutate(baby2.Bits);

            babyGenomes.Add(baby1);
            babyGenomes.Add(baby2);
        }

        mGenomes = babyGenomes;
        ++mGeneration;
    }

    private IEnumerator Run()
    {
        mBusy = true;
        while (enabled && mBestFitness < 1)
        {
            yield return Epoch();
        }
        mBusy = false;
    }
}
