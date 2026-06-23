using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

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

        // Generate a set size population of random chromosomes == random orders of customers
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

        // Select the best chromosome from a random sample
        // This chromosome will be used as one of two parents
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

        // A chromosome mutation that will invert random segment of the sequence
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

        // Method for generating an offspring chromosome for the next generation
        // from two parent chromosomes by choosing a random segment from parent1
        // and filling the rest using parent2
        private int[] OrderCrossover(int[] parent1, int[] parent2)
        {
            int size = parent1.Length;
            int[] child = new int[size];

            Array.Fill(child, -1);

            // Choose random segment from parent1
            int point1 = _random.Next(size);
            int point2 = _random.Next(size);
            int start = Math.Min(point1, point2);
            int end = Math.Max(point1, point2);

            // Transfer it to the child
            for(int i=start; i <= end; i++)
            {
                child[i] = parent1[i];
            }

            int currentChildIndex = (end + 1) % size;
            int parent2Index = (end + 1) % size;
            int elementsCopied = (end - start) + 1;

            // Fill the rest of the child's sequence using parent2
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

        // Evolutionary loop for creating new generations using OrderCrossover and Mutations by inversing
        // Keeps track of the best individual chromosome found so far
        public Chromosome RunEvolution(int generations, double mutationRate, int elitisimRate, int perVehiclePenalty)
        {
            List<Chromosome> currentPopulation = InitializePopulation(perVehiclePenalty);

            Chromosome bestEverSolution = currentPopulation.OrderBy(c => c.Fitness).First();

            for(int gen=1; gen <= generations; gen++)
            {
                ConcurrentBag<Chromosome> nextPopulation = new ConcurrentBag<Chromosome>();

                var sortedPopulation = currentPopulation.OrderBy(c => c.Fitness).ToList();
                
                // Enforce Elitism - X shortest paths copy to the next generation
                // This way the solution never worsens
                int elitismCount = Math.Min(elitisimRate, sortedPopulation.Count);
                for(int i=0; i < elitismCount; i++)
                {
                    nextPopulation.Add(sortedPopulation[i]);
                }

                // Populate the new generation
                int childrenToGenerate = PopulationSize - elitisimRate;

                Parallel.For(0, childrenToGenerate, i =>
                {
                    Chromosome parent1, parent2;
                    int[] childSequence;

                    // We must lock the random number generator so threads don't crash it
                    lock (_random)
                    {
                        // Choose 2 parents and procure a child for next generation
                        parent1 = TournamentSelection(currentPopulation);
                        parent2 = TournamentSelection(currentPopulation);
                        childSequence = OrderCrossover(parent1.Sequence, parent2.Sequence);

                        // Mutate
                        if (_random.NextDouble() < mutationRate)
                        {
                            childSequence = InvertMutation(childSequence);
                        }
                    }

                    // Decode the child chromosome into a set of routes and optimize each route using 2-opt
                    List<Route> rawRoutes = _evaluator.DecodeChromosome(childSequence, _problem);

                    ILocalSearch optimizer = new TwoOptOptimizer();
                    List<Route> optimizedRoutes = new List<Route>();

                    foreach (Route truckRoute in rawRoutes)
                    {
                        optimizedRoutes.Add(optimizer.Optimize(truckRoute, _problem));
                    }

                    // Convert the optimized routes back into a chromosome sequence
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

                    // Create a new Chromosome object for the child and calculate its fitness
                    Chromosome child = new Chromosome(optimizedSequence)
                    {
                        Fitness = _evaluator.CalculateFitness(optimizedSequence, _problem, perVehiclePenalty)
                    };

                    // Add the child to the next generation
                    nextPopulation.Add(child);
                });

                currentPopulation = nextPopulation.ToList();

                // Update the best solution found so far if the best in the current generation is better
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
