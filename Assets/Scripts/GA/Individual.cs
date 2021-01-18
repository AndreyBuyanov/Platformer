using System;

namespace GA
{
    public class Individual: IComparable, ICloneable
    {
        private double[] _genes;

        public double Fitness
        {
            get;
            set;
        }

        public uint GenesCount
        {
            get
            {
                return (uint)_genes.Length;
            }
        }

        public Individual(uint genesCount)
        {
            _genes = new double[genesCount];
        }

        public double this[int i]
        {
            get
            {
                return _genes[i];
            }
            set
            {
                _genes[i] = value;
            }
        }

        // IComparable
        public int CompareTo(object other)
        {
            return (other as Individual).Fitness.CompareTo(-Fitness);
        }

        // ICloneable
        public object Clone()
        {
            Individual clone = new Individual(GenesCount);
            for (int i = 0; i < GenesCount; i++)
            {
                clone._genes[i] = _genes[i];
            }
            clone.Fitness = Fitness;
            return clone;
        }
    }

}
