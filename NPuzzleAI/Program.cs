using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPuzzleAI
{
    public enum Heuristic
    {
        Uniform,
        Misplaced_Tile,
        Manhattan
    }

    public static class Extensions
    {
        /// <summary>
        /// Finds index of target
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="target"></param>
        /// <returns>Array of the index</returns>
        public static int[] FindIndexOf(this int[,] arr, int target)
        {
            for (int i = 0; i < arr.GetLength(0); ++i)
            {
                for (int j = 0; j < arr.GetLength(1); ++j)
                {
                    if(arr[i, j] == target)
                    {
                        return new int[]{ i, j };
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

    public static class HeuristicFunctions
    {
        /// <summary>
        /// Returns 0 as Heuristic
        /// </summary>
        /// <param name="boardState"></param>
        /// <param name="goalState"></param>
        /// <returns></returns>
        public static int UniformCost(int[,] boardState, int[,] goalState)
        {
            return 0;
        }

        /// <summary>
        /// Iterates through the board and finds all non-matching goalstate positions
        /// </summary>
        /// <param name="boardState"></param>
        /// <param name="goalState"></param>
        /// <returns>Total Misplaced Tiles</returns>
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

        /// <summary>
        /// Calculates total minimum distance to achieve Goal State
        /// </summary>
        /// <param name="boardState"></param>
        /// <param name="goalState"></param>
        /// <returns>Total Distance to Goal State</returns>
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
        public int[,] boardState;
        private int[,] goalState;
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
            if(index[0] > 0)
            {
                int[,] newBoardState = boardState.Clone() as int[,];
                Extensions.Swap(ref newBoardState[index[0], index[1]], ref newBoardState[index[0] - 1, index[1]]);
                puzzles.Add(new Puzzle(newBoardState, heuristic));
            }
            if(index[0] < xLength - 1)
            {
                int[,] newBoardState = boardState.Clone() as int[,];
                Extensions.Swap(ref newBoardState[index[0], index[1]], ref newBoardState[index[0] + 1, index[1]]);
                puzzles.Add(new Puzzle(newBoardState, heuristic));
            }
            if(index[1] > 0)
            {
                int[,] newBoardState = boardState.Clone() as int[,];
                Extensions.Swap(ref newBoardState[index[0], index[1]], ref newBoardState[index[0], index[1] - 1]);
                puzzles.Add(new Puzzle(newBoardState, heuristic));
            }
            if(index[1] < yLength - 1)
            {
                int[,] newBoardState = boardState.Clone() as int[,];
                Extensions.Swap(ref newBoardState[index[0], index[1]], ref newBoardState[index[0], index[1] + 1]);
                puzzles.Add(new Puzzle(newBoardState, heuristic));
            }

            return puzzles.ToArray();
        }

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
        LinkedList<Node> nodes = new LinkedList<Node>();

        public bool Empty()
        {
            return nodes.Count == 0;
        }

        public void Enque(Node node)
        {

            for(LinkedListNode<Node> val = nodes.First; val != null; val = val.Next)
            {
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

            Prompt:
            Console.WriteLine("Welcome to Lorenzo Alamillo's 8-Puzzle Solver." +
                "Type \"1\" to use a default puzzle, or \"2\" to enter your own puzzle");
            ConsoleKeyInfo input = Console.ReadKey();

            if(input.KeyChar == '1')
            {
                puzzle = new int[3, 3] {
                    { 0, 1, 3},
                    { 4, 2, 5},
                    { 7, 8, 6} };
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
            PriorityQueue queue = new PriorityQueue();
            Puzzle initPuzzle = new Puzzle(puzzle, heuristic);

            Node firstNode = new Node(0, initPuzzle.GetHeuristicValue(), initPuzzle);
            queue.Enque(firstNode);

            while(true)
            {
                if(queue.Empty())
                {
                    return;
                }

                Node node = queue.Dequeue();
                Console.WriteLine($"Expanding Node with g(n) = {node.g} and h(n) = {node.h}");
                node.puzzle.Print();

                if (node.puzzle.GoalStateFound())
                {
                    Console.WriteLine("Goal State Found, Press any key to restart.");
                    Console.ReadKey(true);
                    Program.Main(new string[] { });
                }

                Puzzle[] children = node.puzzle.GetPossibleChildren();
                for(int i = 0; i < children.Length; ++i)
                {
                    queue.Enque(new Node(node.g + 1, children[i].GetHeuristicValue(), children[i]));
                }
            }
        }
    }
}
