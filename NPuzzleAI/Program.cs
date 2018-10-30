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
        private int[,] boardState;
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
                    goalState[i, j] = (i * xLength) + j;
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
                int[,] newBoardState = boardState;
                Extensions.Swap(ref newBoardState[index[0], index[1]], ref newBoardState[index[0] - 1, index[1]]);
                puzzles.Add(new Puzzle(newBoardState, heuristic));
            }
            if(index[0] < xLength - 1)
            {
                int[,] newBoardState = boardState;
                Extensions.Swap(ref newBoardState[index[0], index[1]], ref newBoardState[index[0] + 1, index[1]]);
                puzzles.Add(new Puzzle(newBoardState, heuristic));
            }
            if(index[1] > 0)
            {
                int[,] newBoardState = boardState;
                Extensions.Swap(ref newBoardState[index[0], index[1]], ref newBoardState[index[0], index[1] - 1]);
                puzzles.Add(new Puzzle(newBoardState, heuristic));
            }
            if(index[1] < yLength - 1)
            {
                int[,] newBoardState = boardState;
                Extensions.Swap(ref newBoardState[index[0], index[1]], ref newBoardState[index[0], index[1] + 1]);
                puzzles.Add(new Puzzle(newBoardState, heuristic));
            }

            return puzzles.ToArray();
        }

        public int GetHeuristicValue()
        {
            return heuristic.Invoke(boardState, goalState);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World");
            Console.ReadLine();
        }
    }
}
