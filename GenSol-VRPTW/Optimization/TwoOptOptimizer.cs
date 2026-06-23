using System;
using System.Collections.Generic;
using System.Text;

namespace GenSol_VRPTW
{
    // Implements the 2-opt local search optimization algorithm for improving routes in the VRPTW problem
    internal class TwoOptOptimizer : ILocalSearch
    {
        public Route Optimize(Route originalRoute, ProblemInstance problem)
        {
            // There is nothing to optimize for 2 or less customers
            if (originalRoute.Customers.Count <= 2)
                return originalRoute;

            bool improvement = true;
            Route bestRoute = originalRoute;

            int[] bestSequence = new int[originalRoute.Customers.Count];
            for (int i = 0; i < originalRoute.Customers.Count; i++)
            {
                bestSequence[i] = originalRoute.Customers[i].Id;
            }

            while (improvement) 
            {
                improvement = false;
                
                for(int i=0; i<bestSequence.Length-1; i++)
                {
                    for(int j=i+1; j<bestSequence.Length; j++)
                    {
                        // Reverse the sub-segment
                        int[] testSequence = ReverseSegment(bestSequence, i, j);

                        // Check validity of time windows
                        Route testRoute = EvaluateSequence(testSequence, problem);

                        // Check validity and improvement in distance
                        if(testRoute != null && testRoute.TotalDistance < bestRoute.TotalDistance)
                        {
                            bestRoute = testRoute;
                            bestSequence = testSequence;
                            improvement = true;

                            // Restart Search
                            break;
                        }
                    }
                    if (improvement)
                        break;
                }
            }
            return bestRoute;
        }

        // Reverses a segment of the sequence from index 'start' to index 'end' (inclusive)
        private int[] ReverseSegment(int[] sequence, int start, int end)
        {
            int[] newSequence = (int[])sequence.Clone();
            while (start < end)
            {
                int temp = newSequence[start];
                newSequence[start] = newSequence[end];
                newSequence[end] = temp;
                start++;
                end--;
            }
            return newSequence;
        }

        // Returns null if sequence is invalid, otherwise returns a route instance
        private Route EvaluateSequence(int[] sequence, ProblemInstance problem)
        {
            Route testRoute = new Route(problem.Depot);

            foreach (int customerId in sequence)
            {
                Customer c = problem.Customers[customerId - 1];

                // If a time window is broken, TryAddCustomer returns false.
                if (!testRoute.TryAddCustomer(c, problem))
                {
                    return null;
                }
            }

            // The sequence is valid
            testRoute.CloseRoute(problem);
            return testRoute;
        }
    }
}
