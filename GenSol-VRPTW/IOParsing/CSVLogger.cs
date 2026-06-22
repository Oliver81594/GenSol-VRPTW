using System;
using System.Collections.Generic;
using System.Text;

namespace GenSol_VRPTW
{
    // Handles logging of generation and best fitness data into a CSV file for convergence analysis.
    internal class CSVLogger
    {
        private readonly string _filePath;

        // Initializes a new instance of the CSVLogger class and prepares the CSV file for logging.
        public CSVLogger(string filePath)
        {
            _filePath = filePath;
            File.WriteAllText(_filePath, "Generation,BestFitness\n");
        }

        // Logs the generation number and best fitness score into the CSV file.
        public void LogGeneration(int generation, double bestFitness)
        {
            string csvLine = $"{generation},{bestFitness:F2}\n";
            File.AppendAllText(_filePath, csvLine);
        }
    }
}
