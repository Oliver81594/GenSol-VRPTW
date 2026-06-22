using System;
using System.Collections.Generic;
using System.Text;

namespace GenSol_VRPTW
{
    internal interface ILocalSearch
    {
        Route Optimize(Route originalRoute, ProblemInstance problem);
    }
}
