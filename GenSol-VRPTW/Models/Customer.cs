using System;
using System.Collections.Generic;
using System.Text;

namespace GenSol_VRPTW
{
    // Represents a customer in the VRPTW problem, including its location, demand, and time window constraints
    internal class Customer
    {
        public int Id { get; }
        public double X { get; }
        public double Y { get; }
        public double Demand { get; }
        public double ReadyTime { get; }
        public double DueDate { get; }
        public double ServiceTime {  get; }

        // Initializes a new instance of the Customer class with the specified parameters
        public Customer(int id, double x, double y, double demand, double readyTime, double dueDate, double serviceTime)
        {
            this.Id = id;
            this.X = x;
            this.Y = y; 
            this.Demand = demand;
            this.ReadyTime = readyTime;
            this.DueDate = dueDate;
            this.ServiceTime = serviceTime;
        }

        // Returns a string representation of the Customer object, including its ID, coordinates, demand, and time window
        // for debugging and logging purposes
        public override string ToString()
        {
            return $"Customer {Id,3} -> X: {X,4}, Y: {Y,4} | Demand: {Demand,3} | Window: [{ReadyTime,4} - {DueDate,4}]";
        }
    }
}
