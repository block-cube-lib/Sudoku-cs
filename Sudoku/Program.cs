using System;
using System.Linq;
using System.Collections.Generic;

namespace CSharpTest
{
    class Program
    {
        static Sudoku.Cell[,] CreateCellsFromArray(int[,] array)
        {
            if (array.Rank != 2)
            {
                return null;
            }
            var size = array.GetLength(0);

            // create cells
            var cells = new Sudoku.Cell[size, size];
            for (int y = 0; y < size; ++y)
            {
                for (int x = 0; x < size; ++x)
                {
                    var num = array[y, x];
                    var index = x + y * size;
                    var cell = num != 0 ? new Sudoku.Cell(index, num, size) : new Sudoku.Cell(index, size);
                    cells[y, x] = cell;
                }
            }

            return cells;
        }

        static List<Sudoku.Group> CreateGroupsFromCellArray(Sudoku.Cell[,] cells)
        {
            if (cells.Rank != 2)
            {
                return null;
            }
            var size = cells.GetLength(0);

            var groups = new List<Sudoku.Group>();

            // add group
            for (int i = 0; i < size; ++i)
            {
                var xGroup = new Sudoku.Group(); // vertical
                for (int j = 0; j < size; ++j)
                {
                    xGroup.AddCells(cells[i, j]);
                }
                groups.Add(xGroup);
            }
            for (int i = 0; i < size; ++i)
            {
                var yGroup = new Sudoku.Group(); // horizontal
                for (int j = 0; j < size; ++j)
                {
                    yGroup.AddCells(cells[j, i]);
                }
                groups.Add(yGroup);
            }

            var smallWidth = size == 9 ? 3 : 4;
            var smallHeight = size == 9 ? 3 : 4;
            for (int y = 0; y < smallHeight; ++y)
            {
                for (int x = 0; x < smallWidth; ++x)
                {
                    var group = new Sudoku.Group();
                    var startXIndex = x * smallWidth;
                    var startYIndex = y * smallHeight;
                    for (int v = 0; v < smallHeight; ++v)
                    {
                        for (int h = 0; h < smallWidth; ++h)
                        {
                            group.AddCells(cells[startYIndex + v, startXIndex + h]);
                        }
                        //group.AddCells(Enumerable.Range(0, smallWidth).Select(e => cells[startXIndex + e, startXIndex + v]).ToArray());
                    }
                    //group.AddCells(
                    //    cells[startXIndex + 0, startYIndex + 0],
                    //    cells[startXIndex + 1, startYIndex + 0],
                    //    cells[startXIndex + 2, startYIndex + 0],
                    //    cells[startXIndex + 0, startYIndex + 1],
                    //    cells[startXIndex + 1, startYIndex + 1],
                    //    cells[startXIndex + 2, startYIndex + 1],
                    //    cells[startXIndex + 0, startYIndex + 2],
                    //    cells[startXIndex + 1, startYIndex + 2],
                    //    cells[startXIndex + 2, startYIndex + 2]);
                    groups.Add(group);
                }
            }

            return groups;
        }

        static IEnumerable<T> ToEnumerable<T>(T[,] v)
        {
            for (int i = 0; i < v.GetLength(0); ++i)
            {
                for (int j = 0; j < v.GetLength(1); ++j)
                {
                    yield return v[i, j];
                }
            }
        }

        static int[,] ReadFromFile(string path)
        {
            var lines = System.IO.File.ReadAllLines(path);
            var size = int.Parse(lines.ElementAt(0).Split(",").First());
            int lineNum = 0;
            var cellNumbers = new int[size, size];
            foreach (var line in lines.Skip(1))
            {
                int x = 0;
                foreach (var s in line.Split(","))
                {
                    cellNumbers[lineNum, x] = s == string.Empty ? 0 : int.Parse(s);
                    x++;
                }
                lineNum++;
            }

            return cellNumbers;
        }

        static void Main(string[] args)
        {
            Console.Write($"args = ");
            foreach (var arg in args)
            {
                Console.Write($"{arg} ");
            }
            Console.WriteLine($"");

            int[,] cellNumbers;
            if (args.Length >= 1)
            {
                cellNumbers = ReadFromFile(args[0]);
            }
            else
            {
                return;
            }

            var cells = CreateCellsFromArray(cellNumbers);
            var groups = CreateGroupsFromCellArray(cells);
            //CheckGroups(groups, cells);

            // disp
            DisplayCells(cells);
            if (groups.Any(e => !e.IsValidate))
            {
                Console.WriteLine("Error");
                return;
            }

            bool firstUnresolved = false;
            while (groups.All(e => e.IsValidate) && !groups.All(e => e.IsResolved))
            {
                var resolveCount = ResolveGroups(ToEnumerable(cells), groups);
                if (!firstUnresolved && groups.Any(e => !e.IsValidate))
                {
                    DisplayCells(cells);
                    Console.WriteLine("Error");
                    break;
                }
                else if (resolveCount == 0 && !firstUnresolved)
                {
                    for (int i = 0; i < cells.GetLength(0); ++i)
                    {
                        for (int j = 0; j < cells.GetLength(1); ++j)
                        {
                            cellNumbers[i, j] = cells[i, j].Number ?? 0;
                        }
                    }
                    firstUnresolved = true;
                    continue;
                }
                else if (groups.Any(e => !e.IsValidate))
                {
                    cells = CreateCellsFromArray(cellNumbers);
                    groups = CreateGroupsFromCellArray(cells);
                }
                else if (resolveCount == 0 && !groups.All(e => e.IsResolved))
                {
                    var unresolvedCells = ToEnumerable(cells)
                        .Where(e => !e.Number.HasValue)
                        .ToArray();
                    var minPossibilityCount = unresolvedCells.Select(e => e.PossibilityList.Count).OrderBy(e => e).First();
                    if (minPossibilityCount == 0)
                    {
                        cells = CreateCellsFromArray(cellNumbers);
                        groups = CreateGroupsFromCellArray(cells);
                        continue;
                    }
                    unresolvedCells = unresolvedCells.Where(e => e.PossibilityList.Count == minPossibilityCount).ToArray();
                    var random = new Random();
                    var cell = unresolvedCells.ElementAt(random.Next(0, unresolvedCells.Count()));
                    cell.SetNumber(cell.PossibilityList[random.Next(0, minPossibilityCount)]);
                }

                Console.WriteLine($"\ncount = {resolveCount}");
                //DisplayCells(cells);
                if (groups.Any(e => !e.IsValidate))
                {
                    Console.WriteLine($"Invalidate");
                }
            }

            //Console.WriteLine($"resolved = {group.IsResolved}");
            //group.Resolve();
            //Console.WriteLine($"resolved = {group.IsResolved}");

            if (groups.Any(e => !e.IsValidate))
            {
                var group = groups.First(e => !e.IsValidate);
                Console.WriteLine($"Invalidate\ncell numbers = [{string.Join(", ", group.Cells.OrderBy(e => e.Index).Select(e => e.Number?.ToString() ?? " "))}]");
            }
            else if (groups.Any(e => e.IsValidate) && groups.All(e => e.IsResolved))
            {
                Console.WriteLine("'\n\nResolved!!!!");
            }
            DisplayCells(cells);
            Console.WriteLine("Finish");
        }

        static int ResolveGroups(IEnumerable<Sudoku.Group> groups)
        {
            var resolveCount = 0;
            foreach (var group in groups)
            {
                resolveCount += group.Resolve();
            }
            return resolveCount;
        }

        static int ResolveGroups(IEnumerable<Sudoku.Cell> cells, IEnumerable<Sudoku.Group> groups)
        {
            var resolveCount = 0;
            foreach (var cell in cells)
            {
                foreach (var group in groups.Where(e => e.Cells.Any(c => c.Index == cell.Index)))
                {
                    resolveCount += group.Resolve();
                }
            }
            return resolveCount;
        }

        static void DisplayCells(Sudoku.Cell[,] cells)
        {
            var sparateCount = cells.GetLength(0) == 9 ? 3 : 4;
            var line = "".PadLeft(3 * sparateCount, '-');
            line = "|" + string.Join("+", Enumerable.Range(0, sparateCount).Select(_ => line)) + "|";
            for (int y = 0; y < cells.GetLength(0); ++y)
            {
                if (y % sparateCount == 0)
                {
                    Console.WriteLine(line);
                }
                for (int x = 0; x < cells.GetLength(1); ++x)
                {
                    if (x % sparateCount == 0)
                    {
                        Console.Write("|");
                    }
                    var str = $"{(cells[y, x].Number?.ToString() ?? " "),3}";
                    Console.Write(str);
                }
                Console.WriteLine("|");
            }
            Console.WriteLine(line);
        }

        static void CheckGroups(IEnumerable<Sudoku.Group> groups, Sudoku.Cell[,] cells)
        {
            foreach (var group in groups)
            {
                var sparateCount = cells.GetLength(0) == 9 ? 3 : 4;
                var line = "".PadLeft(3 * sparateCount, '-');
                line = "|" + string.Join("+", Enumerable.Range(0, sparateCount).Select(_ => line)) + "|";
                for (int y = 0; y < cells.GetLength(0); ++y)
                {
                    if (y % sparateCount == 0)
                    {
                        Console.WriteLine(line);
                    }
                    for (int x = 0; x < cells.GetLength(1); ++x)
                    {
                        if (x % sparateCount == 0)
                        {
                            Console.Write("|");
                        }
                        bool b = group.Cells.Any(e => ReferenceEquals(e, cells[y, x]));
                        var str = $"{(b ? "T" : " "),3}";
                        Console.Write(str);
                    }
                    Console.WriteLine("|");
                }
                Console.WriteLine(line);
                Console.WriteLine("");
            }
        }
    }
}
