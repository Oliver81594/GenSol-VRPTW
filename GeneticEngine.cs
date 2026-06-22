using System;
using System.Collections.Generic;
using System.Text;

namespace GenSol_VRPTW
{
    internal class GeneticEngine
    {
        private readonly ProblemInstance _problem;
        private readonly EvaluationEngine _evaluator;
        private readonly Random _random;

        public int PopulationSize { get; }

        // This is trigerred on completing a generation
        // Currently used for outputting into console and CSV convergence sheet
        public event Action<int, double> OnGenerationCompleted;

        public GeneticEngine(int populationSize, ProblemInstance problem) 
        {
            PopulationSize = populationSize;
            _evaluator = new EvaluationEngine();
            _problem = problem;
            _random = new Random();
        }

        public List<Chromosome> InitializePopulation(int perVehiclePenalty)
        {
            List<Chromosome> population = new List<Chromosome>();

            int numberOfCustomers = _problem.Customers.Count;

            for(int i = 0; i < PopulationSize; i++)
            {
                int[] sequence = new int[numberOfCustomers];

                // Generate a base sequence: 1, 2, 3, ...
                for(int j = 0; j < numberOfCustomers; j++)
                {
                    sequence[j] = j + 1;
                }

                // Shuffle using Fisher-Yates algorithm
                for(int j=sequence.Length - 1; j > 0; j--)
                {
                    int swapIndex = _random.Next(j + 1);
                    int temp = sequence[j];

                    sequence[j] = sequence[swapIndex];
                    sequence[swapIndex] = temp;
                }

                Chromosome child = new Chromosome(sequence);
                child.Fitness = _evaluator.CalculateFitness(sequence, _problem, perVehiclePenalty);

                population.Add(child);
            }

            return population;
        }

        private Chromosome TournamentSelection(List<Chromosome> population, int tournamenSize = 5)
        {
            Chromosome bestCandidate = null;

            for(int i=0; i < tournamenSize; i++)
            {
                int randomIndex = _random.Next(population.Count);
                Chromosome candidate = population[randomIndex];

                if (bestCandidate == null || candidate.Fitness < bestCandidate.Fitness)
                    bestCandidate = candidate;
            }

            return bestCandidate;
        }

        private int[] InvertMutation(int[] sequence)
        {
            int[] mutatedSequence = (int[])sequence.Clone();

            int index1 = _random.Next(mutatedSequence.Length);
            int index2 = _random.Next(mutatedSequence.Length);

            int start = Math.Min(index1, index2);
            int end = Math.Max(index1, index2);

            while (start < end)
            {
                int temp = sequence[end];
                sequence[end] = sequence[start];
                sequence[start] = temp;

                start++; end--;
            }

            return mutatedSequence;
        }

        private int[] OrderCrossover(int[] parent1, int[] parent2)
        {
            int size = parent1.Length;
            int[] child = new int[size];

            Array.Fill(child, -1);

            int point1 = _random.Next(size);
            int point2 = _random.Next(size);
            int start = Math.Min(point1, point2);
            int end = Math.Max(point1, point2);

            for(int i=start; i <= end; i++)
            {
                child[i] = parent1[i];
            }

            int currentChildIndex = (end + 1) % size;
            int parent2Index = (end + 1) % size;
            int elementsCopied = (end - start) + 1;

            while(elementsCopied < size)
            {
                int candidateGene = parent2[parent2Index];

                bool alreadyExists = false;
                for(int i=start; i <= end; i++)
                {
                    if (child[i] == candidateGene) { 
                        alreadyExists = true;
                        break;
                    }
                }

                if (!alreadyExists)
                {
                    child[currentChildIndex] = candidateGene;
                    currentChildIndex = (currentChildIndex + 1) % size;
                    elementsCopied++;
                }

                parent2Index = (parent2Index + 1) % size;
            }

            return child;
        }

        public Chromosome RunEvolution(int generations, double mutationRate, int elitisimRate, int perVehiclePenalty)
        {
            List<Chromosome> currentPopulation = InitializePopulation(perVehiclePenalty);

            Chromosome bestEverSolution = currentPopulation.OrderBy(c => c.Fitness).First();

            for(int gen=1; gen <= generations; gen++)
            {
                List<Chromosome> nextPopulation = new List<Chromosome>();

                var sortedPopulation = currentPopulation.OrderBy(c => c.Fitness).ToList();
                
                // Enforce Elitism - X shortest paths copy to the next generation
                // This way our solution never worsens
                int elitismCount = Math.Min(elitisimRate, sortedPopulation.Count);
                for(int i=0; i < elitismCount; i++)
                {
                    nextPopulation.Add(sortedPopulation[i]);
                }

                while ( nextPopulation.Count < PopulationSize )
                {
                    Chromosome parent1 = TournamentSelection(currentPopulation);
                    Chromosome parent2 = TournamentSelection(currentPopulation);

                    int[] childSequence = OrderCrossover(parent1.Sequence, parent2.Sequence);

                    if (_random.NextDouble() < mutationRate)
                    {
                        childSequence = InvertMutation(childSequence);
                    }

                    
                    List<Route> rawRoutes = _evaluator.DecodeChromosome(childSequence, _problem);

                    ILocalSearch optimizer = new TwoOptOptimizer();
                    List<Route> optimizedRoutes = new List<Route>();

                    foreach (Route truckRoute in rawRoutes)
                    {
                        optimizedRoutes.Add(optimizer.Optimize(truckRoute, _problem));
                    }

                    int[] optimizedSequence = new int[childSequence.Length];
                    int pointer = 0;
                    foreach (Route truckRoute in optimizedRoutes)
                    {
                        foreach (Customer c in truckRoute.Customers)
                        {
                            optimizedSequence[pointer] = c.Id;
                            pointer++;
                        }
                    }

                    Chromosome child = new Chromosome(optimizedSequence)
                    {
                        Fitness = _evaluator.CalculateFitness(optimizedSequence, _problem, perVehiclePenalty)
                    };

                    nextPopulation.Add(child);
                }

                currentPopulation = nextPopulation;

                Chromosome generationBest = currentPopulation.OrderBy(c => c.Fitness).First();
                if(generationBest.Fitness < bestEverSolution.Fitness)
                {
                    bestEverSolution = generationBest;
                }

                OnGenerationCompleted?.Invoke(gen, bestEverSolution.Fitness);
            }

            return bestEverSolution;
        }
    }
}
