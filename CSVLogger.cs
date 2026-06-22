using System;
using System.Collections.Generic;
using System.Text;

namespace GenSol_VRPTW
{
    internal class CSVLogger
    {
        private readonly string _filePath;

        public CSVLogger(string filePath)
        {
            _filePath = filePath;
            File.WriteAllText(_filePath, "Generation,BestFitness\n");
        }

        
        public void LogGeneration(int generation, double bestFitness)
        {
            string csvLine = $"{generation},{bestFitness:F2}\n";
            File.AppendAllText(_filePath, csvLine);
        }
    }
}
