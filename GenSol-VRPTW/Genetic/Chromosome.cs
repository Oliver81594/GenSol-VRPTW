using System;
using System.Collections.Generic;
using System.Text;

namespace GenSol_VRPTW
{
    // Represents a single chromosome in the genetic algorithm, which is a sequence of customer visits
    // and its associated fitness score.
    internal class Chromosome
    {
        public int[] Sequence { get; }
        public double Fitness { get; set; }
        
        public Chromosome(int[] sequence)
        {
            Sequence = sequence;
            Fitness = double.MaxValue;
        }
    }
}
