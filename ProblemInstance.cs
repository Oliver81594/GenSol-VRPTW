using System;
using System.Collections.Generic;
using System.Text;

namespace GenSol_VRPTW
{
    internal class ProblemInstance
    {
        public String InstanceName { get; }
        public int VehicleNumber { get; }
        public int VehicleCapacity { get; }
        public List<Customer> Customers { get; }
        public Customer Depot {  get; }

        public ProblemInstance(String instanceName, int vehicleNumber, int vehicleCapacity, List<Customer> customers, Customer depot)
        {
            this.InstanceName = instanceName;
            this.VehicleNumber = vehicleNumber;
            this.VehicleCapacity = vehicleCapacity;
            this.Customers = customers;
            this.Depot = depot;
        }

        // Method for calculating euclidian distance between two customers/depot
        public double CalculateDistance(Customer a, Customer b)
        {
            double deltaX = a.X - b.X;
            double deltaY = a.Y - b.Y;

            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        public void PrintSummary()
        {
            Console.WriteLine("=================================================");
            Console.WriteLine($"INSTANCE NAME: {InstanceName}");
            Console.WriteLine($"Vehicle Capacity: {VehicleCapacity}");
            Console.WriteLine($"Total Customers Loaded: {Customers.Count}");
            Console.WriteLine("=================================================");
            Console.WriteLine("First 5 Customers:");

            for (int i = 0; i < Math.Min(5, Customers.Count); i++)
            {
                Console.WriteLine(Customers[i].ToString());
            }
            Console.WriteLine("=================================================");
        }
    }
}
