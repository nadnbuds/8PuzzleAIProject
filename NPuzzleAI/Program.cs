using System;
using System.Collections.Generic;

namespace NPuzzleAI
{
    public enum Heuristic
    {
        Uniform,
        Misplaced_Tile,
        Manhattan
    }

    public static class HeuristicFunctions
    {
        public static int UniformCost(int[,] boardState, int[,] goalState)
        {
            return 0;
        }

        public static int MisplacedTileHeuristic(int[,] boardState, int[,] goalState)
        {
            int misplacedTiles = 0;

            for (int i = 0; i < boardState.GetLength(0); ++i)
            {
                for (int j = 0; j < boardState.GetLength(1); ++j)
                {
                    if ((boardState[i, j] != 0) && 
                        (boardState[i, j] != goalState[i, j]))
                    {
                        ++misplacedTiles;
                    }
                }
            }

            return misplacedTiles;

        }

        public static int ManhattanHeuristic(int[,] boardState, int[,] goalState)
        {
            int totalDistance = 0;

            for (int i = 0; i < boardState.GetLength(0); ++i)
            {
                for (int j = 0; j < boardState.GetLength(1); ++j)
                {
                    if(boardState[i, j] != 0)
                    {
                        int[] index = goalState.FindIndexOf(boardState[i, j]);
                        if(index[0] != -1)
                        {
                            totalDistance += Math.Abs(index[0] - i);
                        }
                        if(index[1] != -1)
                        {
                            totalDistance += Math.Abs(index[1] - j);
                        }
                    }
                }
            }

            return totalDistance;
        }
    }

    class Puzzle
    {
        private int xLength;
        private int yLength;
        //current board state
        public int[,] boardState;
        //goal state generated based on the dimensions
        private int[,] goalState;
        //reference to the heuristic function to be used
        private Func<int[,], int[,], int> heuristic;

        public Puzzle(int[,] boardState, Func<int[,], int[,], int> heuristic)
        {
            this.boardState = boardState;
            xLength = boardState.GetLength(0);
            yLength = boardState.GetLength(1);

            this.heuristic = heuristic;

            //Initialize Goal State
            goalState = new int[xLength, yLength];
            for(int i = 0; i < xLength; ++i)
            {
                for(int j = 0; j < yLength; ++j)
                {
                    goalState[i, j] = (i * xLength) + j + 1;
                }
            }
            goalState[xLength - 1, yLength - 1] = 0;
        }

        public Puzzle[] GetPossibleChildren()
        {
            List<Puzzle> puzzles = new List<Puzzle>();
            int[] index = boardState.FindIndexOf(0);
            //Gets child to the left if possible
            if(index[0] > 0)
            {
                int[,] newBoardState = boardState.Clone() as int[,];
                Extensions.Swap(ref newBoardState[index[0], index[1]], ref newBoardState[index[0] - 1, index[1]]);
                puzzles.Add(new Puzzle(newBoardState, heuristic));
            }
            //Gets child to the right if possible
            if(index[0] < xLength - 1)
            {
                int[,] newBoardState = boardState.Clone() as int[,];
                Extensions.Swap(ref newBoardState[index[0], index[1]], ref newBoardState[index[0] + 1, index[1]]);
                puzzles.Add(new Puzzle(newBoardState, heuristic));
            }
            //Gets child to the top if possible
            if(index[1] > 0)
            {
                int[,] newBoardState = boardState.Clone() as int[,];
                Extensions.Swap(ref newBoardState[index[0], index[1]], ref newBoardState[index[0], index[1] - 1]);
                puzzles.Add(new Puzzle(newBoardState, heuristic));
            }
            //Gets child to the bottom if possible
            if(index[1] < yLength - 1)
            {
                int[,] newBoardState = boardState.Clone() as int[,];
                Extensions.Swap(ref newBoardState[index[0], index[1]], ref newBoardState[index[0], index[1] + 1]);
                puzzles.Add(new Puzzle(newBoardState, heuristic));
            }

            return puzzles.ToArray();
        }

        //gets h(n)
        public int GetHeuristicValue()
        {
            return heuristic.Invoke(boardState, goalState);
        }

        public bool GoalStateFound()
        {
            return boardState.Compare(goalState);
        }

        public bool Equals(Puzzle puzzle)
        {
            return boardState.Compare(puzzle.boardState);
        }

        public void Print()
        {
            for(int i = 0; i < xLength; ++i)
            {
                for(int j = 0; j < yLength; ++j)
                {
                    Console.Write($"{boardState[i, j]} ");
                }

                Console.Write("\n");
            }
        }
    }

    public static class Extensions
    {
        public static int[] FindIndexOf(this int[,] arr, int target)
        {
            for (int i = 0; i < arr.GetLength(0); ++i)
            {
                for (int j = 0; j < arr.GetLength(1); ++j)
                {
                    if (arr[i, j] == target)
                    {
                        return new int[] { i, j };
                    }
                }
            }

            return new int[] { -1, -1 };
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        public static bool Compare(this int[,] arr, int[,] target)
        {
            bool equal = true;

            for (int i = 0; i < arr.GetLength(0); ++i)
            {
                for (int j = 0; j < arr.GetLength(1); ++j)
                {
                    equal &= (arr[i, j] == target[i, j]);
                }
            }

            return equal;
        }
    }

    class Node
    {
        public int g;
        public int h;
        public Puzzle puzzle;

        public Node(int g, int h, Puzzle puzzle)
        {
            this.g = g;
            this.h = h;
            this.puzzle = puzzle;
        }

        public Puzzle[] GetChildren()
        {
            return puzzle.GetPossibleChildren();
        }

        public int getWeight()
        {
            return g + h;
        }
    }

    class PriorityQueue
    {
        List<Puzzle> repeatedStates = new List<Puzzle>();
        LinkedList<Node> nodes = new LinkedList<Node>();

        public bool Empty()
        {
            return nodes.Count == 0;
        }

        public int Size()
        {
            return nodes.Count;
        }

        public void Enque(Node node)
        {
            //checks if is in repeated states
            for (int i = 0; i < repeatedStates.Count; ++i)
            {
                if (repeatedStates[i].Equals(node.puzzle))
                {
                    return;
                }
            }

            repeatedStates.Add(node.puzzle);

            for (LinkedListNode<Node> val = nodes.First; val != null; val = val.Next)
            {
                //Inserts the value in the priority que based on FIFO
                if(val.Value.getWeight() > node.getWeight())
                {
                    nodes.AddBefore(val, node);
                    return;
                }
            }

            nodes.AddLast(node);
        }

        public Node Dequeue()
        {
            Node node = nodes.First.Value;
            nodes.RemoveFirst();

            return node;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int[,] puzzle = new int[3, 3];

            //Start of program
            Prompt:
            Console.WriteLine("Welcome to Lorenzo Alamillo's 8-Puzzle Solver." +
                "Type \"1\" to use a default puzzle, or \"2\" to enter your own puzzle");
            ConsoleKeyInfo input = Console.ReadKey();

            if(input.KeyChar == '1')
            {
                puzzle = new int[3, 3] {
                    { 1, 2, 3},
                    { 4, 0, 6},
                    { 7, 5, 8} };
            }
            else if(input.KeyChar == '2')
            {
                Console.WriteLine("\nEnter your puzzle, use a zero to represent the blank");
                Console.WriteLine("Enter the first row, use space or tabs between numbers");
                string line1 = Console.ReadLine();
                string[] line = line1.Split(' ');
                for(int i = 0; i < line.Length; ++i)
                {
                    try
                    {
                        puzzle[0, i] = Convert.ToInt32(line[i]);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid Input");
                        goto Prompt;
                    }
                }

                Console.WriteLine("Enter the second row, use space or tabs between numbers");
                string line2 = Console.ReadLine();
                line = line2.Split(' ');
                for (int i = 0; i < line.Length; ++i)
                {
                    try
                    {
                        puzzle[1, i] = Convert.ToInt32(line[i]);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid Input");
                        goto Prompt;
                    }
                }

                Console.WriteLine("Enter the third row, use space or tabs between numbers");
                string line3 = Console.ReadLine();
                line = line3.Split(' ');
                for (int i = 0; i < line.Length; ++i)
                {
                    try
                    {
                        puzzle[2, i] = Convert.ToInt32(line[i]);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid Input");
                        goto Prompt;
                    }
                }
            }
            else
            {
                Console.WriteLine("Error, invalid input");
                goto Prompt;
            }

            Console.WriteLine("Enter your choice of algorithm \n" +
                "1: Uniform Cost Search \n" +
                "2: A* with Misplaced Tile Heuristic \n" +
                "3: A* with Manhattan distance Heuristic");
            Char key = Console.ReadKey().KeyChar;

            switch(key)
            {
                case '1':
                    GeneralSearch(puzzle, HeuristicFunctions.UniformCost);
                    break;
                case '2':
                    GeneralSearch(puzzle, HeuristicFunctions.MisplacedTileHeuristic);
                    break;
                case '3':
                    GeneralSearch(puzzle, HeuristicFunctions.ManhattanHeuristic);
                    break;
                default:
                    Console.WriteLine("Invalid input");
                    goto Prompt;
            }
        }

        public static void GeneralSearch(int[,] puzzle, Func<int[,], int[,], int> heuristic)
        {
            int MaxQueueSize = 0;
            int NodesExpanded = 0;
            PriorityQueue queue = new PriorityQueue();
            Puzzle initPuzzle = new Puzzle(puzzle, heuristic);

            Node firstNode = new Node(0, initPuzzle.GetHeuristicValue(), initPuzzle);
            queue.Enque(firstNode);

            while(true)
            {
                MaxQueueSize = queue.Size() > MaxQueueSize ? queue.Size() : MaxQueueSize;

                if(queue.Empty())
                {
                    Console.WriteLine("No solution found, Press any key to restart");
                    Console.WriteLine($"Max Queue Size: {MaxQueueSize}");
                    Console.WriteLine($"Nodes Expanded: {NodesExpanded}");
                    Console.ReadKey(true);
                    Program.Main(new string[] { });
                }

                Node node = queue.Dequeue();
                Console.WriteLine($"Expanding Node with g(n) = {node.g} and h(n) = {node.h}");
                node.puzzle.Print();

                if (node.puzzle.GoalStateFound())
                {
                    Console.WriteLine("Goal State Found, Press any key to restart.");
                    Console.WriteLine($"Max Queue Size: {MaxQueueSize}");
                    Console.WriteLine($"Nodes Expanded: {NodesExpanded}");
                    Console.ReadKey(true);
                    Program.Main(new string[] { });
                }

                Puzzle[] children = node.puzzle.GetPossibleChildren();
                NodesExpanded++;
                for(int i = 0; i < children.Length; ++i)
                {
                    queue.Enque(new Node(node.g + 1, children[i].GetHeuristicValue(), children[i]));
                }
            }
        }
    }
}
