using System;
using System.Collections;

[Serializable]
public class Genome
{
    public BitArray Bits { get; set; }

    public double Fitness { get; set; }

    public Genome( int numBits )
    {
        Bits = new BitArray(numBits);
        for (int count = 0; count < numBits; ++count)
        {
            Bits.Set(count, UnityEngine.Random.value > 0.5f);
        }
        Fitness = 0.0;
    }
}
