using System;
using System.Collections.Generic;
using System.Text;

namespace GenSol_VRPTW
{
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
