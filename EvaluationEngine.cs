using System;
using System.Collections.Generic;
using System.Text;

namespace GenSol_VRPTW
{
    internal class EvaluationEngine
    {
        // Method for splitting the raw list of customers into individual routes
        // so that no time windows and capacity limits are violated and the order of customers is preserved
        public List<Route> DecodeChromosome(int[] chromosome, ProblemInstance problem)
        {
            // Final list of individual routes each complying with time windows and capacity
            List<Route> fleet = new List<Route>();

            // We will keep track of the current truck and use it until a time window or capacity is broken
            Route currentTruck = new Route(problem.Depot);

            // Order of the customers (the chromosome sequence) must remain same for the Genetic Algorithm to work
            foreach (int customerId in chromosome)
            {
                Customer customer = problem.Customers[customerId - 1];

                // Check if we can add another stop to our current truck (time window and capacity are okey)
                bool success = currentTruck.TryAddCustomer(customer, problem);

                if (!success)
                {
                    // If we cannot we end the current route
                    currentTruck.CloseRoute(problem);
                    fleet.Add(currentTruck);

                    // and dispatch a new truck from the depot
                    currentTruck = new Route(problem.Depot);

                    bool retrySuccess = currentTruck.TryAddCustomer(customer, problem);

                    // If there is a customer that cannot be visited straight from depot there is no viable solution
                    if (!retrySuccess)
                    {
                        throw new InvalidOperationException(
                            $"CRITICAL ERROR: Customer {customer.Id} is fundamentally impossible to serve. " +
                            $"Check if their demand ({customer.Demand}) exceeds vehicle capacity ({problem.VehicleCapacity}) " +
                            $"or if they are too far from the depot to meet their DueDate ({customer.DueDate}).");
                    }
                }
            }

            // Send the last truck back to depot
            currentTruck.CloseRoute(problem);
            fleet.Add(currentTruck);

            return fleet;
        }

        // Method for calculating a fitness score of each chromosome
        // Fitness = distance travelled + number of vehicles used * penalty for vehicle
        public double CalculateFitness(int[] chromosome, ProblemInstance problem, int perVehiclePenalty)
        {
            List<Route> decodedRoutes = DecodeChromosome(chromosome, problem);

            double totalDistance = 0;
            foreach (Route route in decodedRoutes)
            {
                totalDistance += route.TotalDistance;
            }

            // Secondary goal is minimising number of used vehicles
            double vehiclePenalty = decodedRoutes.Count * perVehiclePenalty;

            return totalDistance + vehiclePenalty;
        }
    }
}
