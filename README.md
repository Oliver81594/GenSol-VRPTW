# GenSol-VRPTW

1. Class Responsibilities
    - Models Layer (Customer, ProblemInstance, Route): Core domain models. Route internally validates capacity and Time Window constraints during assignment.
    - Evaluation Layer (EvaluationEngine, Chromosome): Decodes permutation sequences deterministically into physical vehicle routes.
    - Evolution Layer (GeneticEngine): Loops the population lifecycles, Tournament Selection, Elitism, and genetic mutation.
    - Optimization Layer (ILocalSearch, TwoOptOptimizer): Single routes optimizitation used to speed up the solution convergence.
    - Utilities Layer (SolomonParser, CsvLogger): Handles file stream parsing and event-driven data exporting.

2. Algorithms & Mechanics
    - Permutation DNA & Order Crossover (OX1): Standard binary crossover creates duplicate or missing nodes in VRPs. The engine utilizes OX1 to slice a sequence from Parent 1 and map the remaining genes strictly using the relative sequence of Parent 2.
    - Lamarckian Evolution: The algorithm is a Memetic hybrid. When the TwoOptOptimizer untangles a route, the physical changes are reverse-encoded back into the child's int[] chromosome sequence before it enters the next generation. The DNA directly reflects acquired structural improvements.
    - Time-Window Aware 2-Opt: Introduces a validation gate: every proposed edge-reversal is rebuilt into a temporary Route object to prove that arriving at subsequent nodes later will not violate any DueDate constraints.
    - Fisher-Yates Shuffe: Used for initializing a random population of chromosomes.

3. Featured Software Engineering & OOP Concepts
    - Event-Driven Output (Action Delegates): The GeneticEngine has no dependency on IO streams. It utilizes an Action<int, double> event to broadcast generation completion. The CsvLogger acts as a subscriber. Used for decoupling the the logging layer from the genetic engine.
    - Interface Abstraction: The 2-Opt logic implements the ILocalSearch interface. The Genetic Engine accepts any local search strategy without requiring modifications to the main loop.
    - Defensive Immutability: ProblemInstance uses constructor-only initialization. Data fields are read-only properties, making it mathematically impossible to mutate the dataset benchmark parameters during execution.
    - Targeted Exception Handling: The Evaluation Engine guards against mathematically unsolvable configurations. If an isolated customer inherently violates global capacity or distance constraints, the engine throws an InvalidOperationException to halt execution, preventing the silent generation of invalid data.