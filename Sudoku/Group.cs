using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    public class Group
    {
        public bool IsResolved => Cells.All(e => e.Number.HasValue);

        public bool IsValidate =>
            Cells.Where(e => e.IsResolved).GroupBy(e => e.Number).All(e => e.Count() <= 1);

        public List<Cell> Cells { get; private set; } = new List<Cell>();

        public IEnumerable<Cell> ResolvedCells => Cells.Where(e => e.IsResolved);
        public IEnumerable<Cell> UnresolvedCells => Cells.Where(e => !e.IsResolved);

        public void AddCells(params Cell[] Cells)
        {
            this.Cells.AddRange(Cells);
        }

        public int Resolve()
        {
            var resolvedCellNumbers = this.ResolvedCells.Select(e => e.Number.Value);
            var beforeCount = resolvedCellNumbers.Count();

            foreach (var cell in this.Cells.Where(e => !e.Number.HasValue))
            {
                var resolvedCellsArray = resolvedCellNumbers.ToArray();
                cell.RemovePossibility(resolvedCellsArray);
            }

            //var unresolvedCells = this.Cells.Where(e => !e.Number.HasValue);
            //foreach (var unresolvedCell in unresolvedCells.ToArray())
            //{
            //    foreach (var possibilityNumber in unresolvedCell.PossibilityList.ToArray())
            //    {
            //        if (this.Cells.Where(cell => cell.PossibilityList.Any(n => n == possibilityNumber)).Count() == 1 &&
            //            resolvedCellNumbers.All(e => e != possibilityNumber))
            //        {
            //            var before = unresolvedCell.PossibilityList.ToList();
            //            unresolvedCell.SetNumber(possibilityNumber);
            //            if (!this.IsValidate)
            //            {
            //                Console.WriteLine($"Invalidate!!");
            //            }
            //        }
            //    }
            //}

            var afterCount = resolvedCellNumbers.Count();
            return afterCount - beforeCount;
        }
    }

}
