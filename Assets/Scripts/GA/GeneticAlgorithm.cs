using System;
using System.Linq;

namespace GA
{
    internal class TournamentSelection
    {
        private Random _random;
        private int _size;

        public TournamentSelection(int size, Random random)
        {
            _size = size;
            _random = random;
        }

        public Individual Select(Population population)
        {
            Individual[] selectedIndividuals = new Individual[_size];
            for (int i = 0; i < selectedIndividuals.Length; i++)
            {
                selectedIndividuals[i] = population[_random.Next(0, (int)population.Size - 1)];
            }
            Array.Sort(selectedIndividuals);
            return selectedIndividuals[selectedIndividuals.Length - 1];
        }
    }

    internal class SwapRecombination
    {
        private Random _random;
        private double _swapChance;

        public SwapRecombination(double swapChance, Random random)
        {
            _swapChance = swapChance;
            _random = random;
        }

        public (Individual, Individual) Recombine((Individual, Individual) parents)
        {
            if (parents.Item1.GenesCount != parents.Item2.GenesCount)
            {
                throw new Exception();
            }
            uint genesCount = parents.Item1.GenesCount;
            Individual child1 = new Individual(genesCount);
            Individual child2 = new Individual(genesCount);
            for (int i = 0; i < genesCount; i++)
            {
                if (_random.NextDouble() < _swapChance)
                {
                    child1[i] = parents.Item2[i];
                    child2[i] = parents.Item1[i];
                }
                else
                {
                    child1[i] = parents.Item1[i];
                    child2[i] = parents.Item2[i];
                }
            }
            return (child1, child2);
        }
    }

    internal class GaussianMutator
    {
        private Random _random;
        private double _mutationChance;
        private double _stddev;

        // https://gist.github.com/tansey/1444070
        public static double SampleGaussian(Random random, double mean, double stddev)
        {
            double x1 = 1 - random.NextDouble();
            double x2 = 1 - random.NextDouble();

            double y1 = System.Math.Sqrt(-2.0 * System.Math.Log(x1)) * System.Math.Cos(2.0 * System.Math.PI * x2);
            return y1 * stddev + mean;
        }

        public GaussianMutator(double mutationChance, double stddev, Random random)
        {
            _mutationChance = mutationChance;
            _stddev = stddev;
            _random = random;
        }

        public void Mutate(Individual individual)
        {
            for (int i = 0; i < individual.GenesCount; i++)
            {
                if (_random.NextDouble() > _mutationChance)
                {
                    individual[i] = SampleGaussian(_random, individual[i], _stddev);
                }
            }
        }
    }

    public class GeneticAlgorithm
    {
        private TournamentSelection _selector;
        private SwapRecombination _recombinator;
        private GaussianMutator _mutator;

        public GeneticAlgorithm(
            int tournamentSize,
            double swapChance,
            double mutationChance,
            double stddev,
            Random random)
        {
            _selector = new TournamentSelection(tournamentSize, random);
            _recombinator = new SwapRecombination(swapChance, random);
            _mutator = new GaussianMutator(mutationChance, stddev, random);
        }

        public void Run(Population population)
        {
            Population oldPopulation = (Population)population.Clone();
            Population parents = new Population(population.Size);

            for (int i = 0; i < parents.Size; i++)
            {
                parents[i] = _selector.Select(oldPopulation);
            }

            bool isEven = population.Size % 2 == 0;
            uint populationSize = isEven ? population.Size : population.Size - 1;
            for (int i = 0; i < populationSize; i += 2)
            {
                var childrens = _recombinator.Recombine((parents[i], parents[i + 1]));
                population[i] = childrens.Item1;
                population[i + 1] = childrens.Item2;
            }
            if (!isEven) {
                population[(int)population.Size - 1] = parents[(int)population.Size - 1];
            }

            for (int i = 0; i < population.Size; i++)
            {
                _mutator.Mutate(population[i]);
            }
        }
    }
}
