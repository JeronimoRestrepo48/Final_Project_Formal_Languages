using CSharpSLRParser;

namespace CSharpSLRParser.Tests
{
    public class LRItemTests
    {
        [Fact]
        public void NextSymbol_ReturnsCorrectSymbol()
        {
            var item = new LRItem("S", new List<string> { "a", "B", "c" }, 1);
            Assert.Equal("B", item.NextSymbol());
        }

        [Fact]
        public void NextSymbol_ReturnsNullAtEnd()
        {
            var item = new LRItem("S", new List<string> { "a", "b" }, 2);
            Assert.Null(item.NextSymbol());
        }

        [Fact]
        public void NextSymbol_ReturnsNullForEmptyRHS()
        {
            var item = new LRItem("A", new List<string>(), 0); // Represents A -> e or A -> .
            Assert.Null(item.NextSymbol());
        }
        
        [Fact]
        public void NextSymbol_ReturnsFirstSymbolWhenDotAtStart()
        {
            var item = new LRItem("S", new List<string> { "a", "b" }, 0);
            Assert.Equal("a", item.NextSymbol());
        }

        [Fact]
        public void IsComplete_TrueWhenDotAtEnd()
        {
            var item = new LRItem("S", new List<string> { "a", "b" }, 2);
            Assert.True(item.IsComplete());
        }

        [Fact]
        public void IsComplete_FalseWhenDotNotAtEnd()
        {
            var item = new LRItem("S", new List<string> { "a", "b" }, 1);
            Assert.False(item.IsComplete());
        }

        [Fact]
        public void IsComplete_TrueForEmptyRHS()
        {
            // An item like A -> . (derived from A -> e)
            var item = new LRItem("A", new List<string>(), 0); 
            Assert.True(item.IsComplete());
        }
        
        [Fact]
        public void IsComplete_TrueForEpsilonProduction()
        {
            // An item like A -> e . (if 'e' is explicitly in RHS)
            var item = new LRItem("A", new List<string> { "e" }, 1);
            Assert.True(item.IsComplete());
        }
        
        [Fact]
        public void IsComplete_FalseForEpsilonProductionDotAtStart()
        {
            // An item like A -> . e
            var item = new LRItem("A", new List<string> { "e" }, 0);
            Assert.False(item.IsComplete());
        }


        [Fact]
        public void AdvanceDot_MovesDotCorrectly()
        {
            var item = new LRItem("S", new List<string> { "a", "b" }, 0);
            var advanced = item.AdvanceDot();
            Assert.Equal(1, advanced.DotPosition);
            Assert.Equal(item.LHS, advanced.LHS);
            Assert.Equal(item.RHS, advanced.RHS);
        }

        [Fact]
        public void AdvanceDot_AtEndReturnsSameOrNewInstanceWithSameState()
        {
            var item = new LRItem("S", new List<string> { "a", "b" }, 2);
            var advanced = item.AdvanceDot();
            Assert.Equal(2, advanced.DotPosition); 
            // Depending on implementation, it might return 'this' or a new identical item.
            // For immutability, new identical item is fine. Check core properties.
            Assert.Equal(item.LHS, advanced.LHS);
            Assert.Equal(item.RHS, advanced.RHS);
            Assert.Equal(item, advanced); // If it's truly immutable or returns self
        }
        
        [Fact]
        public void AdvanceDot_OnEmptyRHS()
        {
            var item = new LRItem("A", new List<string>(), 0);
            var advanced = item.AdvanceDot();
            Assert.Equal(0, advanced.DotPosition);
            Assert.Equal(item, advanced);
        }


        [Fact]
        public void Equals_TrueForIdenticalItems()
        {
            var item1 = new LRItem("S", new List<string> { "a", "b" }, 1);
            var item2 = new LRItem("S", new List<string> { "a", "b" }, 1);
            Assert.True(item1.Equals(item2));
            Assert.True(item2.Equals(item1));
            Assert.Equal(item1,item2);
        }

        [Fact]
        public void Equals_FalseForDifferentLHS()
        {
            var item1 = new LRItem("S", new List<string> { "a", "b" }, 1);
            var item2 = new LRItem("A", new List<string> { "a", "b" }, 1);
            Assert.False(item1.Equals(item2));
        }

        [Fact]
        public void Equals_FalseForDifferentRHS()
        {
            var item1 = new LRItem("S", new List<string> { "a", "b" }, 1);
            var item2 = new LRItem("S", new List<string> { "a", "c" }, 1);
            Assert.False(item1.Equals(item2));
        }

        [Fact]
        public void Equals_FalseForDifferentDotPosition()
        {
            var item1 = new LRItem("S", new List<string> { "a", "b" }, 1);
            var item2 = new LRItem("S", new List<string> { "a", "b" }, 0);
            Assert.False(item1.Equals(item2));
        }
        
        [Fact]
        public void Equals_FalseForNull()
        {
            var item1 = new LRItem("S", new List<string> { "a", "b" }, 1);
            Assert.False(item1.Equals(null));
        }
        
        [Fact]
        public void Equals_HandlesEmptyRHS()
        {
            var item1 = new LRItem("A", new List<string>(), 0);
            var item2 = new LRItem("A", new List<string>(), 0);
            var item3 = new LRItem("B", new List<string>(), 0);
            Assert.True(item1.Equals(item2));
            Assert.False(item1.Equals(item3));
        }


        [Fact]
        public void GetHashCode_EqualForEqualObjects()
        {
            var item1 = new LRItem("S", new List<string> { "a", "b" }, 1);
            var item2 = new LRItem("S", new List<string> { "a", "b" }, 1);
            Assert.Equal(item1.GetHashCode(), item2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_DifferentForDifferentObjects()
        {
            var item1 = new LRItem("S", new List<string> { "a", "b" }, 1);
            var item2 = new LRItem("S", new List<string> { "a", "c" }, 1); // Diff RHS
            var item3 = new LRItem("X", new List<string> { "a", "b" }, 1); // Diff LHS
            var item4 = new LRItem("S", new List<string> { "a", "b" }, 0); // Diff DotPos
            
            // It's technically possible for different objects to have the same hash code (collision),
            // but for these simple changes, they should typically be different.
            Assert.NotEqual(item1.GetHashCode(), item2.GetHashCode());
            Assert.NotEqual(item1.GetHashCode(), item3.GetHashCode());
            Assert.NotEqual(item1.GetHashCode(), item4.GetHashCode());
        }
        
        [Fact]
        public void GetHashCode_HandlesEmptyRHS()
        {
            var item1 = new LRItem("A", new List<string>(), 0);
            var item2 = new LRItem("A", new List<string>(), 0);
            var item3 = new LRItem("B", new List<string>(), 0);
             Assert.Equal(item1.GetHashCode(), item2.GetHashCode());
             Assert.NotEqual(item1.GetHashCode(), item3.GetHashCode());
        }


        [Fact]
        public void ToString_FormatsCorrectly_DotAtStart()
        {
            var item = new LRItem("S", new List<string> { "a", "b" }, 0);
            Assert.Equal("S -> . a b", item.ToString());
        }

        [Fact]
        public void ToString_FormatsCorrectly_DotInMiddle()
        {
            var item = new LRItem("S", new List<string> { "a", "B", "c" }, 1);
            Assert.Equal("S -> a . B c", item.ToString());
        }

        [Fact]
        public void ToString_FormatsCorrectly_DotAtEnd()
        {
            var item = new LRItem("S", new List<string> { "a", "b" }, 2);
            Assert.Equal("S -> a b .", item.ToString());
        }
        
        [Fact]
        public void ToString_FormatsCorrectly_EmptyRHS()
        {
            // A -> e is typically represented as A -> . when the dot is at the start of 'e'
            // or A -> . if RHS is stored as an empty list for epsilon.
            // If Grammar stores A->e as RHS=["e"], then LRItem("A", ["e"], 0) is "A -> . e"
            // And LRItem("A", ["e"], 1) is "A -> e ."
            // If Grammar stores A->e as RHS=[], then LRItem("A", [], 0) is "A -> ."
            var item = new LRItem("A", new List<string>(), 0); 
            Assert.Equal("A -> .", item.ToString());
        }

        [Fact]
        public void ToString_FormatsCorrectly_EpsilonProductionExplicit()
        {
            var item = new LRItem("A", new List<string> { "e" }, 0);
            Assert.Equal("A -> . e", item.ToString());

            var item2 = new LRItem("A", new List<string> { "e" }, 1);
            Assert.Equal("A -> e .", item2.ToString());
        }
    }
}
