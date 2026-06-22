using System;
using System.Collections.Generic;
using System.Text;

namespace GenSol_VRPTW
{
    // Interface for local search optimization algorithms
    internal interface ILocalSearch
    {
        Route Optimize(Route originalRoute, ProblemInstance problem);
    }
}
