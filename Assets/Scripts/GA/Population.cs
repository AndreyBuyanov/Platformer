using System;
using System.Collections;
using System.Collections.Generic;

namespace GA
{

    internal class PopulationEnumerator: IEnumerator<Individual>
    {
        private Individual[] _individuals;
        private int _position = -1;
        public PopulationEnumerator(Individual[] individuals)
        {
            _individuals = individuals;
        }

        // IEnumerator<Individual>
        public Individual Current
        {
            get
            {
                if (_position == -1 || _position >= _individuals.Length) {
                    throw new InvalidOperationException();
                }
                return _individuals[_position];
            }
        }

        // IEnumerator
        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public bool MoveNext()
        {
            if (_position < _individuals.Length - 1) {
                _position++;
                return true;
            } else {
                return false;
            }
        }

        public void Reset()
        {
            _position = -1;
        }

        // IDisposable
        public void Dispose()
        {

        }
    }

    public class Population: ICloneable, IEnumerable<Individual>
    {
        private static Random _random = new Random(1);
        private Individual[] _individuals;

        public uint Size
        {
            get
            {
                return (uint)_individuals.Length;
            }
        }

        public uint GenesCount
        {
            get
            {
                return _individuals[0].GenesCount;
            }
        }

        public Individual this [int index]
        {
            get
            {
                return _individuals[index];
            }
            set
            {
                _individuals[index] = value;
            }
        }

        public Population(uint populationSize)
        {
            _individuals = new Individual[populationSize];
        }

        public Population(uint populationSize, uint genesCount)
        {
            _individuals = new Individual[populationSize];
            for (int i = 0; i < _individuals.Length; i++)
            {
                _individuals[i] = new Individual(genesCount);
            }
        }

        public void Init()
        {
            for (int i = 0; i < _individuals.Length; i++)
            {
                for (int j = 0; j < _individuals[i].GenesCount; j++)
                {
                    _individuals[i][j] = _random.NextDouble();
                }
            }
        }

        // ICloneable
        public object Clone()
        {
            Population clone = new Population(Size);
            for (int i = 0; i < Size; i++)
            {
                clone[i] = (Individual)this[i].Clone();
            }
            return clone;
        }

        // IEnumerable<Individual>
        public IEnumerator<Individual> GetEnumerator()
        {
            return new PopulationEnumerator(_individuals);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _individuals.GetEnumerator();
        }
    }
}
