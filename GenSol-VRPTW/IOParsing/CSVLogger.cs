using System;
using System.Collections.Generic;
using System.Text;

namespace GenSol_VRPTW
{
    // Handles logging of generation and best fitness data into a CSV file for convergence analysis.
    internal class CSVLogger
    {
        private readonly string _filePath;

        public CSVLogger(string filePath)
        {
            _filePath = filePath;
            // Update the header to include the new columns
            File.WriteAllText(_filePath, "Generation,BestFitness,VehiclesUsed,TotalDistance\n");
        }

        // Match the new event signature
        public void LogGeneration(int generation, double bestFitness, int vehicles, double totalDistance)
        {
            string csvLine = $"{generation},{bestFitness:F2},{vehicles},{totalDistance:F2}\n";
            File.AppendAllText(_filePath, csvLine);
        }
    }
}
