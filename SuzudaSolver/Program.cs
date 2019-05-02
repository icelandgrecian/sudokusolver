using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SuzudaSolver
{
    class Program
    {
        public static IList<CellModel> cells = new List<CellModel>();
        public static bool doingFindAnswers = false;
        public static bool doingXWing = false;

        static void Main(string[] args)
        {
            for (int row = 1; row <= 9; row++)
            {
                for (int col = 1; col <= 9; col++)
                {
                    CellModel cellModel = new CellModel(row, col, null);

                    cellModel.MatchedNumber += CellModel_MatchedNumber;
                    cellModel.PairOfNumbersLeft += CellModel_PairFound;

                    cells.Add(cellModel);
                }
            }

            //////SetValue(row, column = value
            //SetValue(3, 3, 8);
            //SetValue(2, 4, 3);
            //SetValue(1, 5, 2);
            //SetValue(1, 6, 8);
            //SetValue(3, 6, 1);
            //SetValue(1, 8, 7);
            //SetValue(2, 9, 8);
            //SetValue(3, 9, 4);

            //SetValue(4, 2, 4);
            //SetValue(5, 2, 8);
            //SetValue(6, 1, 5);
            //SetValue(6, 3, 7);
            //SetValue(5, 4, 7);
            //SetValue(5, 5, 5);
            //SetValue(5, 6, 6);
            //SetValue(4, 7, 7);
            //SetValue(4, 9, 6);
            //SetValue(5, 8, 4);
            //SetValue(6, 8, 1);

            //SetValue(7, 1, 9);
            //SetValue(8, 1, 8);
            //SetValue(9, 2, 2);
            //SetValue(7, 4, 8);
            //SetValue(9, 4, 5);
            //SetValue(9, 5, 4);
            //SetValue(8, 6, 9);
            //SetValue(7, 7, 6);

            SetValue(2, 1, 6);
            SetValue(2, 3, 7);
            SetValue(3, 3, 3);
            SetValue(1, 4, 9);
            SetValue(2, 4, 2);
            SetValue(1, 6, 6);
            SetValue(3, 6, 7);
            SetValue(1, 7, 1);
            SetValue(3, 7, 5);

            SetValue(4, 1, 5);
            SetValue(5, 2, 3);
            SetValue(6, 1, 7);
            SetValue(4, 5, 2);
            SetValue(6, 5, 4);
            SetValue(4, 9, 4);
            SetValue(5, 8, 5);
            SetValue(6, 9, 8);

            SetValue(7, 3, 9);
            SetValue(9, 3, 1);
            SetValue(7, 4, 5);
            SetValue(9, 4, 6);
            SetValue(8, 6, 2);
            SetValue(9, 6, 8);
            SetValue(7, 7, 8);
            SetValue(8, 7, 7);
            SetValue(8, 9, 5);


            foreach (CellModel cellModel in cells)
            {
                cellModel.Init();
            }

            FindSinglePossibleAnswers();

            while (cells.Any(f => !f.Number.HasValue))
            {
                string command = Console.ReadLine();

                switch (command)
                {
                    case "findanswers":
                        FindSinglePossibleAnswers();
                        break;
                    case "showwith2":
                        foreach (var t in cells.Where(f => !f.Number.HasValue && f.PossibleNumbers.Count == 2))
                        {
                            Console.WriteLine($"{t.Row},{t.Column}: {t.PossibleNumbers.First()} or {t.PossibleNumbers.Last()}");
                        }

                        break;

                    case "showwith3":
                        foreach (var t in cells.Where(f => !f.Number.HasValue && f.PossibleNumbers.Count == 3))
                        {
                            Console.WriteLine($"{t.Row},{t.Column}: {t.PossibleNumbers.First()} or {t.PossibleNumbers.Skip(1).First()} or {t.PossibleNumbers.Last()}");
                        }

                        break;
                    case "showwith4":
                        foreach (var t in cells.Where(f => !f.Number.HasValue && f.PossibleNumbers.Count == 4))
                        {
                            Console.WriteLine($"{t.Row},{t.Column}: {t.PossibleNumbers.First()} or {t.PossibleNumbers.Skip(1).First()} or {t.PossibleNumbers.Skip(2).First()} or {t.PossibleNumbers.Last()}");
                        }

                        break;
                    case "doxwing":
                        DoXWing(cells);
                        break;
                    case "dopairs":
                        DoPairs(); 
                        break;

                    case "set":
                        Console.WriteLine("Row?");
                        string row = Console.ReadLine();
                        Console.WriteLine("Column?");
                        string col = Console.ReadLine();
                        Console.WriteLine("Value?");
                        string value = Console.ReadLine();

                        CellModel cell = cells.Single(f => f.Row.ToString() == row && f.Column.ToString() == col);
                        cell.SetValue(int.Parse(value), "manual set");
                        
                        break;
                }
            }

            WriteGrid();
            
            Console.ReadLine();
        }

        public static void SetValue(int row, int column, int value)
        {
            cells.Single(f => f.Column == column && f.Row == row).SetValue(value, isInit: true, reason: "init");
        }

        public static int ProcessingCount = 0;

        private static void CellModel_PairFound(object sender, FoundMatchEventArgs e)
        {
            if (!doingXWing)
            {
                CellModel cellModel = (CellModel)sender;

                HandlePairs(cells.Where(f => f.Row == cellModel.Row));
                HandlePairs(cells.Where(f => f.Column == cellModel.Column));
                HandlePairs(cells.Where(f => f.Section == cellModel.Section));
            }
        }

        private static void CellModel_MatchedNumber(object sender, FoundMatchEventArgs e)
        {
            ProcessingCount += 1;

            Console.WriteLine(e.Reason);
            WriteGrid();

            CellModel cellModel = (CellModel)sender;

            int value = cellModel.Number.Value;

            ProcessSectionAfterRemoval(cells.Where(c => c.Section == cellModel.Section), cellModel);
            ProcessSectionAfterRemoval(cells.Where(c => c.Row == cellModel.Row), cellModel);
            ProcessSectionAfterRemoval(cells.Where(c => c.Column == cellModel.Column), cellModel);

            ProcessingCount -= 1;

            if (ProcessingCount == 0 && !e.IsInit)
            {
                FindSinglePossibleAnswers();
            }
        }

        public static void ProcessSectionAfterRemoval(IEnumerable<CellModel> items, CellModel cellModel)
        {
            int value = cellModel.Number.Value;

            IEnumerable<CellModel> emptyValues = items.Where(c => !c.Number.HasValue);

            foreach (CellModel cmSec in emptyValues.Where(c => c.PossibleNumbers.Contains(value)))
            {
                cmSec.RemoveNumber(cellModel.Number.Value, "Removal after match");
            }
        }

        public static void WriteGrid()
        {
            for (int row = 1; row <= 9; row++)
            {
                for (int col = 1; col <= 9; col++)
                {
                    CellModel cellModel = cells.Single(f => f.Row == row && f.Column == col);

                    Console.Write(cellModel.Number.HasValue ? cellModel.Number.Value.ToString() : "-");
                    Console.Write(" ");
                }
                Console.WriteLine();
            }


            Console.WriteLine("-----------------------");

            Console.WriteLine();
        }

        public static void FindSinglePossibleAnswers()
        {
            doingFindAnswers = true;

            bool gotMatch = false;

            for (int sec = 1; sec <= 9; sec++)
            {
                gotMatch = CheckThrough(cells.Where(h => h.Section == sec));

                if (gotMatch)
                {
                    break;
                }

                gotMatch = CheckThrough(cells.Where(h => h.Row == sec));

                if (gotMatch)
                {
                    break;
                }

                gotMatch = CheckThrough(cells.Where(h => h.Column == sec));

                if (gotMatch)
                {
                    break;
                }
             }

           
            if (!gotMatch)
            {
                //gotMatch = ProcessLargeSectionRows();
            }

            if (!gotMatch)
            {
                //gotMatch = ProcessLargeSectionColumns();
            }

            // check for x-wing

            DoXWing(cells);


            foreach (int section in cells.Where(f => !f.Number.HasValue).Select(f => f.Section).Distinct())
            {
                DoXWing(cells.Where(f => f.Section == section));
            }


            DoPairs();
        }

        private static bool ProcessLargeSectionColumns()
        {
            bool gotMatch = false;

            for (int largeCol = 1; largeCol <= 3; largeCol++)
            {
                IEnumerable<CellModel> allSellModels = cells.Where(f => f.LargeCol == largeCol);

                IEnumerable<CellModel> secPossibleNumbers = allSellModels.Where(f => !f.Number.HasValue);

                var numbersToGet = secPossibleNumbers.SelectMany(f => f.PossibleNumbers)
                    .Distinct().ToList();

                foreach (int number in numbersToGet)
                {
                    if (number == 6 && largeCol == 1)
                    {
                        var f = 6;
                    }

                    var sections = secPossibleNumbers
                        .Where(g => !g.Number.HasValue && g.PossibleNumbers.Contains(number))
                        .Select(f => f.Section)
                        .Distinct()
                        .ToDictionary(f => f, g => secPossibleNumbers.Where(f => f.Section == g && !f.Number.HasValue && f.PossibleNumbers.Contains(number)));

                    foreach (var section in sections)
                    {
                        if (section.Value.Select(f => f.Column).Distinct().Count() == 1)
                        {
                            int columnNumber = section.Value.Select(f => f.Column).Distinct().Single();

                            foreach (var otherSection in sections)
                            {
                                if (otherSection.Key != section.Key && !gotMatch)
                                {
                                    var cellModels = otherSection.Value.Where(c => c.Column == columnNumber
                                                && c.PossibleNumbers.Contains(number));

                                    foreach (var cell in cellModels)
                                    {
                                        cell.RemoveNumber(number, "Only 1 possible row in large section row");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            doingFindAnswers = false;

            return gotMatch;
        }

            private static bool ProcessLargeSectionRows()
        {
            bool gotMatch = false;

            for (int largeRow = 1; largeRow <= 3; largeRow++)
            {
                IEnumerable<CellModel> allSellModels = cells.Where(f => f.LargeRow == largeRow);

                IEnumerable<CellModel> secPossibleNumbers = allSellModels.Where(f => !f.Number.HasValue);

                var numbersToGet = secPossibleNumbers.SelectMany(f => f.PossibleNumbers)
                    .Distinct()
                    .ToList();

                foreach (int number in numbersToGet)
                {
                    var sections = secPossibleNumbers
                        .Where(g => !g.Number.HasValue && g.PossibleNumbers.Contains(number))
                        .Select(f => f.Section)
                        .Distinct()
                        .ToDictionary(f => f, g => secPossibleNumbers.Where(f => f.Section == g && !f.Number.HasValue && f.PossibleNumbers.Contains(number)));

                    foreach (var section in sections)
                    {
                        if (section.Value.Select(f => f.Row).Distinct().Count() == 1)
                        {
                            int rowNumber = section.Value.Select(f => f.Row).Distinct().Single();

                            foreach (var otherSection in sections)
                            {
                                if (otherSection.Key != section.Key && !gotMatch)
                                {
                                    var cellModels = otherSection.Value.Where(c => c.Row == rowNumber
                                                            && c.PossibleNumbers.Contains(number))
                                                            ;

                                    foreach (var cell in cellModels)
                                    {
                                        cell.RemoveNumber(number, "Only 1 possible row in large section row");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return gotMatch;
        }

        public static void DoPairs()
        {
            for (int sec = 1; sec <= 9; sec++)
            {
                HandlePairs(cells.Where(h => h.Row == sec));
                HandlePairs(cells.Where(h => h.Section == sec));
                HandlePairs(cells.Where(h => h.Column == sec));
            }
        }

        private static void DoXWing(IEnumerable<CellModel> cellsToHandle)
        {
           doingXWing = true;

            //The rule is
            //When there are only two possible cells for a value in each of two different rows,
            //    and these candidates lie also in the same columns,
            //    then all other candidates for this value in the columns can be eliminated.

            for (int number = 1; number <= 9; number++)
            {
                IEnumerable<CellModel> cellsWithNumber = cellsToHandle.Where(f => !f.Number.HasValue
                                && f.PossibleNumbers.Contains(number));

                IList<XModel> rows = new List<XModel>();

                foreach (int row in cellsWithNumber.Select(f => f.Row).Distinct())
                {
                    if (cellsWithNumber.Where(f => f.Row == row).Count() == 2)
                    {
                        rows.Add(new XModel
                        {
                            Row = row,
                            Col1 = cellsWithNumber.Where(f => f.Row == row).Select(f => f.Column).Min(),
                            Col2 = cellsWithNumber.Where(f => f.Row == row).Select(f => f.Column).Max()
                        }
                        );
                    }
                }

                if (rows.Count > 1)
                {
                    var sameRows = rows.Where(r1 =>
                                        rows.Any(r2 => r1.Row != r2.Row &&
                                                        r1.Col1 == r2.Col1 &&
                                                        r2.Col2 == r1.Col2));

                    if (!sameRows.Any())
                    {
                        //sameRows = rows.Where(r1 =>
                        //         rows.Any(r2 => r1.Row != r2.Row &&
                        //                      r1.Col1 + 2 == r2.Col1 && r2.Col1 % 2 == 0 &&
                        //                      r1.Col2 + 2 == r2.Col2 && r2.Col2 % 2 == 0)
                        //                      || rows.Any(r2 => r1.Row != r2.Row &&
                        //                      r1.Col1 - 2 == r2.Col1 && r2.Col1 % 2 == 0 &&
                        //                      r1.Col2 - 2 == r2.Col2 && r2.Col2 % 2 == 0));

                    }

                    if (sameRows.Count() == 2)
                    {
                        // we can then remove this number for any other cell

                        foreach (CellModel cell in
                            cellsWithNumber.Where(f => !sameRows.Select(g => g.Row).Contains(f.Row)
                                && (sameRows.Select(g => g.Col1).Contains(f.Column) ||
                                    sameRows.Select(g => g.Col2).Contains(f.Column))
                            ))
                        {
                            cell.RemoveNumber(number, "X wing = row");
                        }
                    }
                }
            }

            for (int number = 1; number <= 9; number++)
            {
                IEnumerable<CellModel> cellsWithNumber = cellsToHandle.Where(f => !f.Number.HasValue
                            && f.PossibleNumbers.Contains(number));

                IList<XModel> rows = new List<XModel>();

                foreach (int column in cellsWithNumber.Select(f => f.Column).Distinct())
                {
                    if (cellsWithNumber.Where(f => f.Column == column).Count() == 2)
                    {
                        rows.Add(new XModel
                        {
                            Row = column,
                            Col1 = cellsWithNumber.Where(f => f.Column == column).Select(f => f.Row).Min(),
                            Col2 = cellsWithNumber.Where(f => f.Column == column).Select(f => f.Row).Max()
                        }
                        );
                    }
                }

                if (rows.Count > 1)
                {
                    var sameColumns = rows.Where(r1 =>
                                        rows.Any(r2 => r1.Row != r2.Row &&
                                                        r1.Col1 == r2.Col1 &&
                                                        r2.Col2 == r1.Col2));
                    if (sameColumns.Count() == 2)
                    {
                        // we can then remove this number for any other cell

                        foreach (CellModel cell in cellsWithNumber.Where(f =>
                                    !sameColumns.Select(g => g.Row).Contains(f.Column)
                                    && (sameColumns.Select(g => g.Col1).Contains(f.Row) ||
                                        sameColumns.Select(g => g.Col2).Contains(f.Row))
                                    ))
                        {
                            cell.RemoveNumber(number, "X wing = col");
                        }
                    }
                }
            }

            doingXWing = false;
        }

        public static bool HandlePairs(IEnumerable<CellModel> allSellModels)
        {
            bool result = false;

            IEnumerable<CellModel> cellsWith2Possibilties = allSellModels.Where(f => !f.Number.HasValue && f.PossibleNumbers.Count == 2).ToList();

            if (cellsWith2Possibilties.Any())
            {
                foreach(CellModel cellWith2 in cellsWith2Possibilties)
                {
                    CellModel pair = cellsWith2Possibilties.FirstOrDefault(f => f != cellWith2 && !f.PossibleNumbers.Except(cellWith2.PossibleNumbers).Any());

                    if (pair != null )
                    {
                        foreach(CellModel cellModel in allSellModels.Where(f => !f.Number.HasValue && f.PossibleNumbers.Contains(pair.PossibleNumbers.First()) && f != cellWith2 && f != pair))
                        {
                            cellModel.RemoveNumber(pair.PossibleNumbers.First(), "Pair");
                            result = true;
                        }

                        foreach (CellModel cellModel in allSellModels.Where(f => !f.Number.HasValue && f.PossibleNumbers.Contains(pair.PossibleNumbers.Last()) && f != cellWith2 && f != pair))
                        {
                            cellModel.RemoveNumber(pair.PossibleNumbers.Last(), "Pair");
                        }
                    }
                }
            }

            return result;
        }
        
        public static bool CheckThrough(IEnumerable<CellModel> allSellModels)
        {
            bool gotMatch = false;

            IDictionary<CellModel, List<int>> secPossibleNumbers
                    = allSellModels.Where(f => !f.Number.HasValue).ToDictionary(f => f, g => new List<int>());

            var results = from p in allSellModels.Where(f => f.Number.HasValue)
                          group p by p.Number into g
                          select new
                          {
                              Number = g.Key,
                              /**/
                              Count = g.Count()/**/
                          };

            if (results.Any(f => f.Count > 1))
            {
                    throw new Exception("Duplicate value found");
            }

             gotMatch = false;

            var matchingCells = secPossibleNumbers.SelectMany(f => f.Key.PossibleNumbers)
              .Where(g => !allSellModels.Any(h => h.Number.HasValue && h.Number == g))
              .Distinct();

            foreach (int uniqueNum in matchingCells)
            {
                var possCells = secPossibleNumbers.Where(f => !f.Key.Number.HasValue 
                                            && f.Key.PossibleNumbers.Contains(uniqueNum));

                if (possCells.Count() == 1)
                {
                    CellModel cellModel = possCells.Single().Key;

                    cellModel.SetValue(uniqueNum, "Only one possible cells left");

                    gotMatch = true;

                    break;
                }
                else
                {
                    foreach (var possCell in possCells)
                    {
                        possCell.Value.Add(uniqueNum);
                    }
                }
            }


            if (!gotMatch)
            {

                foreach (var cellModel in secPossibleNumbers.Where(f => f.Value.Count == 1 && !f.Key.Number.HasValue))
                {
                    gotMatch = true;

                    cellModel.Key.SetValue(cellModel.Value.Single(), "Only one number left");

                    break;
                }
            }    

            return gotMatch;
        }
    }
}
