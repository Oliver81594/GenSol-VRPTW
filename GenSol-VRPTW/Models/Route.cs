using System;
using System.Collections.Generic;
using System.Text;

namespace GenSol_VRPTW
{
    // Represents a route taken by a vehicle in the VRPTW problem
    // includes the list of customers visited, total distance traveled, current load, and current time
    internal class Route
    {
        public List<Customer> Customers { get; } = new List<Customer>();
        public double TotalDistance { get; private set; } = 0;
        public double CurrentLoad { get; private set; } = 0;
        public double CurrentTime { get; private set; } = 0;
        private Customer _lastVisitedNode;

        public Route(Customer depot)
        {
            _lastVisitedNode = depot;
            CurrentTime = depot.ReadyTime;
        }

        // Method for checking if a customer can be added to the current truck without violating
        // time windows or capacity of the truck
        public bool TryAddCustomer(Customer nextCustomer, ProblemInstance problem)
        {
            // Check Capacity
            if(CurrentLoad + nextCustomer.Demand > problem.VehicleCapacity)
                return false;
            

            double travelTime = problem.CalculateDistance(_lastVisitedNode, nextCustomer);
            double arrivalTime = CurrentTime + travelTime;

            // Check time window (if we are late)
            if (arrivalTime > nextCustomer.DueDate)
                return false;

            // If we are early, we wait
            double startServiceTime = Math.Max(arrivalTime, nextCustomer.ReadyTime);

            Customers.Add(nextCustomer);
            CurrentLoad += nextCustomer.Demand;
            TotalDistance += travelTime;

            CurrentTime = startServiceTime + nextCustomer.ServiceTime;
            _lastVisitedNode = nextCustomer;

            return true;
        }
        
        // If the next customer cannot be served or it was the last one we end the route and go back to depot
        public void CloseRoute(ProblemInstance problem)
        {
            double travelHomeTime = problem.CalculateDistance(_lastVisitedNode, problem.Depot);
            TotalDistance += travelHomeTime;
            CurrentTime += travelHomeTime;
            _lastVisitedNode = problem.Depot;
        }
    }
}
