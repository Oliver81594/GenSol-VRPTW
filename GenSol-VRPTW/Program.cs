namespace GenSol_VRPTW
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Define paths and parameters
            String inputPath = "C:\\Users\\olima\\Downloads\\py-ga-VRPTW-master\\data\\text\\C201.txt";
            string outputCsvPath = @"C:\\Users\\olima\\Downloads\\Convergence_C101.csv";

            int populationSize = 500;
            int generationsCount = 1000;
            double mutationRate = 0.5;
            int elitismRate = 0;
            int perVehiclePenalty = 50;

            try
            {
                // Parse input
                Console.WriteLine("=== INITIALIZING INSTANCE ENGINE ===");
                SolomonParser parser = new SolomonParser(inputPath);
                ProblemInstance problem = parser.ParseFile();
                Console.WriteLine($"Loaded: {problem.InstanceName} | Customers: {problem.Customers.Count - 1}");

                // Initialize GA
                Console.WriteLine("\n=== RUNNING GENETIC ALGORITHM ===");
                GeneticEngine engine = new GeneticEngine(populationSize, problem);

                // Prepare output logging into CSV
                CSVLogger csvLogger = new CSVLogger(outputCsvPath);
                engine.OnGenerationCompleted += csvLogger.LogGeneration;

                engine.OnGenerationCompleted += (generation, fitness, vehicles, distance) =>
                {
                    if (generation == 1 || generation % 50 == 0)
                    {
                        Console.WriteLine($"Gen {generation,4} | Fit: {fitness,-8:F2} | Trucks: {vehicles,-2} | Dist: {distance:F2}");
                    }
                };

                // Run the Evolution
                Chromosome optimizedResult = engine.RunEvolution(generationsCount, mutationRate, elitismRate, perVehiclePenalty);

                // Print results
                Console.WriteLine("\n=== OPTIMIZATION RUN COMPLETE ===");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Final Score Achieved: {optimizedResult.Fitness:F2}");
                Console.ResetColor();

                // Decode the absolute best sequence one last time to inspect vehicle count
                EvaluationEngine decoder = new EvaluationEngine();
                var finalRoutes = decoder.DecodeChromosome(optimizedResult.Sequence, problem);
                Console.WriteLine($"Total Fleet Deployment: {finalRoutes.Count} trucks used.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[CRITICAL TERMINATION] {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
