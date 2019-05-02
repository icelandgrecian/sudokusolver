using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuzudaSolver
{
    public enum Action
    {
        Removal,
        FindMatch
    }

    public class FoundMatchEventArgs
    {
        public bool IsInit { get; set; }

        public string Reason { get; set; }
    }

    public class CellModel : ICloneable
    {
        public event EventHandler<FoundMatchEventArgs> MatchedNumber;

        public event EventHandler<FoundMatchEventArgs> PairOfNumbersLeft;

        public string Id { get { return Row.ToString() + "=" + Column.ToString(); } }

        public CellModel(int row, int column, int? number)
        {
            this.Row = row;
            this.Column = column;

            switch (row)
            {
                case 1:
                case 2:
                case 3:
                    this.LargeRow = 1;

                    switch (column)
                    {
                        case 1:
                        case 2:
                        case 3:
                            this.LargeCol = 1;
                            this.Section = 1;
                            break;
                        case 4:
                        case 5:
                        case 6:
                            this.LargeCol = 2;
                            this.Section = 2;
                            break;
                        case 7:
                        case 8:
                        case 9:
                            this.LargeCol = 3;
                            this.Section = 3;
                            break;
                    }
                    break;
                case 4:
                case 5:
                case 6:
                    this.LargeRow = 2;

                    switch (column)
                    {
                        case 1:
                        case 2:
                        case 3:
                            this.LargeCol = 1;
                            this.Section = 4;
                            break;
                        case 4:
                        case 5:
                        case 6:
                            this.LargeCol = 2;
                            this.Section = 5;
                            break;
                        case 7:
                        case 8:
                        case 9:
                            this.LargeCol = 3;
                            this.Section = 6;
                            break;
                    }
                    break;
                case 7:
                case 8:
                case 9:
                    this.LargeRow = 3;

                    switch (column)
                    {
                        case 1:
                        case 2:
                        case 3:
                            this.LargeCol = 1;
                            this.Section = 7;
                            break;
                        case 4:
                        case 5:
                        case 6:
                            this.LargeCol = 2;
                            this.Section = 8;
                            break;
                        case 7:
                        case 8:
                        case 9:
                            this.LargeCol = 3;
                            this.Section = 9;
                            break;
                    }
                    break;
            }

            this.Number = number;

            PossibleNumbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        }

        public int Row { get; private set; }

        public int Column { get; private set; }

        public int Section { get; private set; }

        public int LargeRow { get; private set; }

        public int LargeCol { get; private set; }

        public IList<int> PossibleNumbers { get; }

        public int? Number { get; private set; }

        public void SetValue (int value, string reason, bool isInit = false)
        {
            if (PossibleNumbers.Contains(value))
            {
                Number = value;

                PossibleNumbers.Clear();
                PossibleNumbers.Add(value);

                if (!isInit)
                {
                    RaiseFindMatch(false, reason);
                }
            }
            else
            {
                throw new Exception("Invalid Value");
            }
        }

        public void RemoveNumber(int number, string reason)
        {

            if (!Number.HasValue && PossibleNumbers.Any(f => f == number))
            {
                PossibleNumbers.Remove(number);

                if (PossibleNumbers.Count == 1)
                {
                    SetValue(PossibleNumbers.Single(), "Only other Number left after remmove: " + reason);
                }
                else if (PossibleNumbers.Count == 2)
                {
                    RaisePairLeft(false, reason);
                }
                
            }
        }

        public void Init()
        {
            if (this.Number.HasValue)
            {
                RaiseFindMatch(true, "init");
            }
        }

        private void RaiseFindMatch(bool isInit, string reason)
        {
            EventHandler<FoundMatchEventArgs> handler = MatchedNumber;
            if (handler != null)
            {
                FoundMatchEventArgs foundMatchEventArgs = new FoundMatchEventArgs
                {
                    IsInit = isInit,
                    Reason = reason
                };

                handler(this, foundMatchEventArgs);
            }
        }

        private void RaisePairLeft(bool isInit, string reason)
        {
            EventHandler<FoundMatchEventArgs> handler = PairOfNumbersLeft;
            if (handler != null)
            {
                FoundMatchEventArgs foundMatchEventArgs = new FoundMatchEventArgs
                {
                    IsInit = isInit,
                    Reason = reason
                };

                handler(this, foundMatchEventArgs);
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
