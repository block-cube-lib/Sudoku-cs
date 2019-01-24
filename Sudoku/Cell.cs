using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    public class Cell
    {
        public int Index { get; private set; }
        public int? Number { get; private set; } = null;
        public List<int> PossibilityList = null;
        public bool IsResolved => this.Number.HasValue;

        public Cell(int index, int maxPossibility)
        {
            this.Index = index;
            this.PossibilityList = Enumerable.Range(1, maxPossibility).ToList();
        }

        public Cell(int index, int number, int maxPossibility)
        {
            this.Index = index;
            this.PossibilityList = Enumerable.Range(1, maxPossibility).ToList();
            this.SetNumber(number);
        }

        public void SetNumber(int number)
        {
            this.Number = number;
            this.PossibilityList.Clear();
        }

        public void RemovePossibility(params int[] numbers)
        {
            foreach (var n in numbers)
            {
                if (PossibilityList.Any(e => e == n))
                {
                    PossibilityList.Remove(n);
                }
            }

            if (PossibilityList.Count == 1)
            {
                this.SetNumber(PossibilityList.First());
            }
        }
    }

}
