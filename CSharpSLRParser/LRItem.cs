using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpSLRParser
{
    public class LRItem : IEquatable<LRItem>
    {
        public string LHS { get; }
        public List<string> RHS { get; }
        public int DotPosition { get; }

        public LRItem(string lhs, List<string> rhs, int dotPosition)
        {
            LHS = lhs;
            RHS = rhs ?? new List<string>(); // Ensure RHS is not null
            DotPosition = dotPosition;
        }

        public string NextSymbol()
        {
            if (DotPosition < RHS.Count)
            {
                return RHS[DotPosition];
            }
            return null;
        }

        public bool IsComplete()
        {
            return DotPosition == RHS.Count;
        }

        public LRItem AdvanceDot()
        {
            if (DotPosition < RHS.Count)
            {
                return new LRItem(LHS, new List<string>(RHS), DotPosition + 1);
            }
            return this; // Or throw an exception, depending on desired behavior
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LRItem);
        }

        public bool Equals(LRItem other)
        {
            if (other == null)
                return false;

            return LHS == other.LHS &&
                   RHS.SequenceEqual(other.RHS) &&
                   DotPosition == other.DotPosition;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + (LHS?.GetHashCode() ?? 0);
                if (RHS != null)
                {
                    foreach (var item in RHS)
                    {
                        hash = hash * 23 + (item?.GetHashCode() ?? 0);
                    }
                }
                hash = hash * 23 + DotPosition.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            var rhsBuilder = new StringBuilder();
            for (int i = 0; i < RHS.Count; i++)
            {
                if (i == DotPosition)
                {
                    rhsBuilder.Append(". ");
                }
                rhsBuilder.Append(RHS[i]);
                if (i < RHS.Count - 1)
                {
                    rhsBuilder.Append(" ");
                }
            }
            if (DotPosition == RHS.Count)
            {
                rhsBuilder.Append(" .");
            }
            return $"{LHS} -> {rhsBuilder.ToString().Trim()}";
        }
    }
}
