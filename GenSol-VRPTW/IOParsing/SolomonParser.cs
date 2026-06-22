using System;
using System.Collections.Generic;
using System.Text;

// Class for reading and parsing the Solomon Benchmark files
// Parsing is based of Documentation available at: https://www.sintef.no/projectweb/top/vrptw/documentation2/
// Data source: https://github.com/iRB-Lab/py-ga-VRPTW/tree/master/data/text

namespace GenSol_VRPTW
{
    internal class SolomonParser
    {
        String file_path;
        const int instanceNameLineNumber = 0;
        const int vehicleNumberAndCapacityLineNumber = 4;
        const int depotLineNumber = 9;
        const int customerListStartLineNumber = 10;

        public SolomonParser(string file_path)
        {
            this.file_path = file_path;
        }

        public ProblemInstance ParseFile()
        {
            // Read file
            String problemText = File.ReadAllText(file_path);
            String[] lines = problemText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Parse main variables
            String instanceName = lines[instanceNameLineNumber].Trim();
            String[] vehicleNumberAndCapacity = lines[vehicleNumberAndCapacityLineNumber].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int vehicleNumber = Convert.ToInt32(vehicleNumberAndCapacity[0]);
            int vehicleCapacity = Convert.ToInt32(vehicleNumberAndCapacity[1]);

            // Prepare for parsing each customer
            int numberOfCustomers = lines.Length - customerListStartLineNumber;
            List<Customer> customers = new List<Customer>(numberOfCustomers);
            Customer depot = ParseCustomer(lines[depotLineNumber]);

            // Parse each customer
            for(int i=0; i<numberOfCustomers; i++)
            {
                // Skip empty lines
                if (string.IsNullOrWhiteSpace(lines[i + customerListStartLineNumber]))
                    continue;

                customers.Add(ParseCustomer(lines[i + customerListStartLineNumber]));
            }

            ProblemInstance instance = new ProblemInstance(instanceName, vehicleNumber, vehicleCapacity, customers, depot);
            return instance;
        }

        // Parses a single line of customer data into a Customer object
        Customer ParseCustomer(String line)
        {
            String[] splitLine = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int id = Convert.ToInt32(splitLine[0]);
            double x = Double.Parse(splitLine[1]);
            double y = Double.Parse(splitLine[2]);
            double demand = Double.Parse(splitLine[3]);
            double readyTime = Double.Parse(splitLine[4]);
            double dueDate = Double.Parse(splitLine[5]);
            double serviceTime = Double.Parse(splitLine[6]);
            
            Customer customer = new Customer(id, x, y, demand, readyTime, dueDate, serviceTime);
            return customer;
        }
    }
}
