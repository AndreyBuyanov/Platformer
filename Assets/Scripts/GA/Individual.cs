using System;

namespace GA
{
    public class Individual: IComparable<Individual>, ICloneable
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

        public Individual(double[] genes)
        {
            _genes = genes;
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

        // IComparable<Individual>
        public int CompareTo(Individual other)
        {
            return other.Fitness.CompareTo(-Fitness);
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
