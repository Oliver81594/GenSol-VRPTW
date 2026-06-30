# GenSol-VRPTW

## Problem Introduction
This project is a C# solver for the Vehicle Routing Problem with Time Windows (VRPTW). The VRPTW is a combinatorial optimization problem where a fleet of vehicles with limited capacity must service a set of customers within strict time windows. The objective is to minimize the total distance traveled and the number of vehicles used.

The engine is built to solve and benchmark against the standard [Solomon VRPTW Benchmark instances](https://www.sintef.no/projectweb/top/vrptw/solomon-benchmark/) (e.g., C101, R101). Data is represented as `.txt` files are formatted acoording to [Documentation](https://www.sintef.no/projectweb/top/vrptw/documentation2/).

To solve this, I decided to implement a [Memetic Algorithm](https://en.wikipedia.org/wiki/Memetic_algorithm), an extension of a Evolutionary Algorithm with a local optima search, due to its modifiability and versatilitty - different hyperparameter settings and local search strategies allow for different convergence behaviours which are interesting to observe.

---

## Part 1: User Documentation

Because this is an academic optimization engine rather than a consumer application, the user and developer workflows heavily overlap. Interaction is handled via code configuration and console execution.

### Requirements & Execution
* **Prerequisites:** .NET 8.0 SDK and Visual Studio 2026.
* **Running the program:** 
   1. Clone the repository
   2. Open in Visual Studio
   3. Download the [Benchmark Dataset](https://www.sintef.no/globalassets/project/top/vrptw/solomon/solomon-100.zip) and specify its path in `Main` in `Program.cs` (more information below)
   4. Run the Program.cs
* **Input Data:** The solver parses [raw text files](https://www.sintef.no/globalassets/project/top/vrptw/solomon/solomon-100.zip) formatted according to the [Solomon benchmark standard](https://www.sintef.no/projectweb/top/vrptw/documentation2/). Datasets directory can be modified in `Main` in `Program.cs`.
   * Input data from the dataset can be divided into three groups based on their properties:
      * Clustered (CX0X - C101, C102, ...) - locations of customers are grouped into multiple neighbourhoods
      * Random (RX0X - R101, R102, ...) - locations of customers are random
      * Randomly Clustered (RCX0X - RC101, RC102) - locations of customers are randomly clustered
* **Configuration:** Evolutionary and Optimization Hyperparameters (Population Size, Mutation Rate, Generations...) and target file paths are configured directly in the `Main` method of `Program.cs`.
* **Outputs:** 
    * **Console:** Real-time generation progress and current best distance, vehicle number and fitness scores.
    * **CSV Export:** Generates a `Convergence.csv` file mapping generation numbers to fitness scores for analytical graphing.

---

## Part 2: Developer Documentation

The architecture is strictly decoupled to allow for rapid algorithmic experimentation and the swapping of heuristics without breaking the core evaluation logic.

### 1. Class Responsibilities
* **Models Layer (`Customer`, `ProblemInstance`, `Route`):** Core domain models. `Route` internally validates capacity and Time Window constraints during assignment.
* **Evaluation Layer (`EvaluationEngine`, `Chromosome`):** Decodes permutation sequences deterministically into physical vehicle routes.
* **Evolution Layer (`GeneticEngine`):** Loops the population lifecycles, Tournament Selection, Elitism, and genetic mutation.
* **Optimization Layer (`ILocalSearch`, `TwoOptOptimizer`):** Single routes optimization used to speed up the solution convergence.
* **Utilities Layer (`SolomonParser`, `CsvLogger`):** Handles file stream parsing and event-driven data exporting.

### 2. Algorithms & Mechanics
* **Permutation DNA & Order Crossover (OX1):** Standard binary crossover creates duplicate or missing nodes in VRPs. The engine utilizes OX1 to slice a sequence from Parent 1 and map the remaining genes strictly using the relative sequence of Parent 2.
* **Lamarckian Evolution:** The algorithm is a Memetic hybrid. When the `TwoOptOptimizer` untangles a route, the physical changes are reverse-encoded back into the child's `int[]` chromosome sequence before it enters the next generation. The DNA directly reflects acquired structural improvements.
* **Time-Window Aware 2-Opt:** Introduces a validation gate: every proposed edge-reversal is rebuilt into a temporary `Route` object to prove that arriving at subsequent nodes later will not violate any `DueDate` constraints.
* **Stochastic Inter-Route Relocation (Fast-Relocate):** To solve geographic misclustering and bypass evaluation bloat, this custom mutation extracts a customer and tests insertion across a small, randomized sample of alternative routes. It provides inter-route optimization without the heavy performance penalty of an exhaustive search.
* **Fisher-Yates Shuffle:** Used for initializing a random population of chromosomes efficiently and without bias.

### 3. Featured Software Engineering & OOP Concepts
* **Thread-Safe Parallel Execution:** The generation evaluation loop is multi-threaded using `Parallel.For` to utilize idle CPU cores. Thread safety is strictly enforced via locks on `System.Random` to prevent sequence corruption during concurrent genetic operations.
* **Event-Driven Output (Action Delegates):** The `GeneticEngine` has no dependency on IO streams. It utilizes an `Action<int, double, int, double>` event to broadcast generation completion, including actual vehicle counts and distances. The `CsvLogger` acts as a subscriber, strictly decoupling the logging layer.
* **Interface Abstraction:** The 2-Opt logic implements the `ILocalSearch` interface. The Genetic Engine accepts any local search strategy without requiring modifications to the main loop.
* **Defensive Immutability:** `ProblemInstance` uses constructor-only initialization. Data fields are read-only properties, making it mathematically impossible to mutate the dataset benchmark parameters during execution.
* **Targeted Exception Handling:** The Evaluation Engine guards against mathematically unsolvable configurations. If an isolated customer inherently violates global capacity or distance constraints, the engine throws an `InvalidOperationException` to halt execution, preventing the silent generation of invalid data.

### 4. Course of Work
* First step was parsing the dataset files (input part of *Utilization Layer*) and building suitabale classes to represent the problem (*Models Layer*) and solution instances - `Customer`, `ProblemInstance`, `Route`.
* Secondly, I build the Genetecic Algorithm - *Evolution Layer* and *Evaluation Layer* to have a minimal working prototype. It may be interesting to note, that checking the correctness of the solutions was already implemented in *Models Layer* which forbade any invalid solutions/routes from being created. Purely genetic approach has produced solutions about 20-40% worse from the best solutions presented in the [Solomon Benchmark Table](https://www.sintef.no/projectweb/top/vrptw/100-customers/).
* Afterwards I focused on local search to improve the solutions. I implemented [2-Opt](https://en.wikipedia.org/wiki/2-opt) search and random mutation in Genetic Engine. Again, it may be interestting to note that pure 2-Opt could not be used in isolation as the it may cause violetion of the customers' Time Windows. This was solved by checking rebuilding  the new results into a temporary `Route` object to prove that arriving at subsequent nodes later will not violate any `DueDate` constraints.
* Due to the heavier time complexity of 2-Opt which was used on every member of the population in each generation the time overhead increased drastically prompting me to implement Parallel Execution as described above.
* At this stage, the Memetic Approach produced quite good results for Random datasets - only about 2-10% more than the best solutions from the Benchmark but still produced mediocre results for Clustered dataset inputs. This motivated me to implement an Inter-Route optimization as well. To not introduce much time complexity I opted for a stochastic Inter-Route Relocation with the sample size being a new hyperparameter. This curbed the distance in Clustered dataset inputs to match the random ones.

### 5. Extensibility & Future Work
* **Adding Custom Heuristics:** New local search algorithms can be injected by creating a new class that implements `ILocalSearch` and passing it to the `GeneticEngine`, requiring zero changes to the core evolutionary loop.
* **Current Limitations & Scaling:** While the Memetic algorithm performs well on tightly clustered datasets, scaling to massive, highly scattered instances (e.g., 1000+ customers) would likely require abandoning flat-array representations in favor of Large Neighborhood Search (LNS) or Column Generation to destroy and rebuild larger segments of the fleet simultaneously.

### Conclusion
* This project proved to be much more interesting than I expected. Eyeballed hyperparameter setting produced much better results than I anticipated. The stochastic nature of the Memetic Approach was both entertaining to watch - you can get the best result at any moment - and discouraging - hyperparameter tunning becomes impossible without multiple measurments which in turn skyrocket the time overhead.
