using System;
using System.Collections;

[Serializable]
public class DesertGenome
{
    public BitArray Bits { get; set; }

    public double Fitness { get; set; }

    public DesertGenome( int numBits )
    {
        Bits = new BitArray(numBits);
        for (int count = 0; count < numBits; ++count)
        {
            Bits.Set(count, UnityEngine.Random.value > 0.5f);
        }
        Fitness = 0.0;
    }
}
