using CSharpSLRParser;

namespace CSharpSLRParser.Tests
{
    public class GrammarTests
    {
        [Fact]
        public void AddProduction_Simple()
        {
            var grammar = new Grammar();
            grammar.AddProduction("S -> a b c");
            Assert.True(grammar.Productions.ContainsKey("S"));
            Assert.Single(grammar.Productions["S"]);
            Assert.Equal(new List<string> { "a", "b", "c" }, grammar.Productions["S"][0]);
            Assert.Contains("S", grammar.NonTerminals);
            Assert.Contains("a", grammar.Terminals);
            Assert.Contains("b", grammar.Terminals);
            Assert.Contains("c", grammar.Terminals);
            Assert.False(grammar.Terminals.Contains("e"));
        }

        [Fact]
        public void AddProduction_WithEpsilon()
        {
            var grammar = new Grammar();
            grammar.AddProduction("A -> e");
            Assert.True(grammar.Productions.ContainsKey("A"));
            Assert.Single(grammar.Productions["A"]);
            Assert.Equal(new List<string> { "e" }, grammar.Productions["A"][0]);
            Assert.Contains("A", grammar.NonTerminals);
            Assert.DoesNotContain("e", grammar.Terminals); // Epsilon is not a terminal
        }

        [Fact]
        public void AddProduction_MultipleAlternatives()
        {
            var grammar = new Grammar();
            grammar.AddProduction("A -> x | y z");
            Assert.True(grammar.Productions.ContainsKey("A"));
            Assert.Equal(2, grammar.Productions["A"].Count);
            Assert.Equal(new List<string> { "x" }, grammar.Productions["A"][0]);
            Assert.Equal(new List<string> { "y", "z" }, grammar.Productions["A"][1]);
            Assert.Contains("A", grammar.NonTerminals);
            Assert.Contains("x", grammar.Terminals);
            Assert.Contains("y", grammar.Terminals);
            Assert.Contains("z", grammar.Terminals);
        }

        [Fact]
        public void AddProduction_ComplexAndTerminalsNonTerminalsPopulation()
        {
            var grammar = new Grammar();
            grammar.StartSymbol = "P";
            grammar.AddProduction("P -> S");
            grammar.AddProduction("S -> S + A | A");
            grammar.AddProduction("A -> A * B | B");
            grammar.AddProduction("B -> id | ( S )");

            Assert.Equal(4, grammar.Productions.Count);
            Assert.Contains("P", grammar.NonTerminals);
            Assert.Contains("S", grammar.NonTerminals);
            Assert.Contains("A", grammar.NonTerminals);
            Assert.Contains("B", grammar.NonTerminals);
            Assert.Equal(4, grammar.NonTerminals.Count);

            Assert.Contains("+", grammar.Terminals);
            Assert.Contains("*", grammar.Terminals);
            Assert.Contains("id", grammar.Terminals);
            Assert.Contains("(", grammar.Terminals);
            Assert.Contains(")", grammar.Terminals);
            Assert.Equal(5, grammar.Terminals.Count);

            Assert.False(grammar.Terminals.Contains("e"));
            Assert.False(grammar.Terminals.Contains("S")); // Ensure non-terminals are not in terminals
        }
        
        [Fact]
        public void AddProduction_EpsilonDoesNotBecomeTerminal()
        {
            var grammar = new Grammar();
            grammar.AddProduction("X -> Y e Z");
            grammar.AddProduction("Y -> y");
            grammar.AddProduction("Z -> z");

            Assert.Contains("X", grammar.NonTerminals);
            Assert.Contains("Y", grammar.NonTerminals);
            Assert.Contains("Z", grammar.NonTerminals);
            Assert.Contains("y", grammar.Terminals);
            Assert.Contains("z", grammar.Terminals);
            Assert.DoesNotContain("e", grammar.Terminals);
            Assert.Equal(2, grammar.Terminals.Count);
        }


        [Fact]
        public void GetAugmentedGrammar_Simple()
        {
            var grammar = new Grammar { StartSymbol = "S" };
            grammar.AddProduction("S -> a");
            var augmented = grammar.GetAugmentedGrammar();

            Assert.Equal("S'", augmented.StartSymbol);
            Assert.True(augmented.Productions.ContainsKey("S'"));
            Assert.Single(augmented.Productions["S'"]);
            Assert.Equal(new List<string> { "S" }, augmented.Productions["S'"][0]);

            Assert.True(augmented.Productions.ContainsKey("S"));
            Assert.Single(augmented.Productions["S"]);
            Assert.Equal(new List<string> { "a" }, augmented.Productions["S"][0]);
            
            Assert.Contains("S'", augmented.NonTerminals);
            Assert.Contains("S", augmented.NonTerminals);
            Assert.Contains("a", augmented.Terminals);
        }
        
        [Fact]
        public void GetAugmentedGrammar_NewStartSymbolClash()
        {
            var grammar = new Grammar { StartSymbol = "S" };
            grammar.AddProduction("S -> S'"); // Original grammar contains S'
            grammar.AddProduction("S' -> a");
            var augmented = grammar.GetAugmentedGrammar();

            Assert.Equal("S''", augmented.StartSymbol); // Should be S''
            Assert.True(augmented.Productions.ContainsKey("S''"));
            Assert.Single(augmented.Productions["S''"]);
            Assert.Equal(new List<string> { "S" }, augmented.Productions["S''"][0]);

            Assert.Contains("S", augmented.NonTerminals);
            Assert.Contains("S'", augmented.NonTerminals);
            Assert.Contains("S''", augmented.NonTerminals);
            Assert.Contains("a", augmented.Terminals);
        }

        [Fact]
        public void GetAugmentedGrammar_PopulatesTerminalsAndNonTerminalsCorrectly()
        {
            var grammar = new Grammar { StartSymbol = "E" };
            grammar.AddProduction("E -> T");
            grammar.AddProduction("T -> id");
            grammar.AddProduction("T -> ( E )");

            var augmented = grammar.GetAugmentedGrammar();

            Assert.Equal("E'", augmented.StartSymbol);
            Assert.Contains("E'", augmented.NonTerminals);
            Assert.Contains("E", augmented.NonTerminals);
            Assert.Contains("T", augmented.NonTerminals);
            Assert.Equal(3, augmented.NonTerminals.Count);

            Assert.Contains("id", augmented.Terminals);
            Assert.Contains("(", augmented.Terminals);
            Assert.Contains(")", augmented.Terminals);
            Assert.Equal(3, augmented.Terminals.Count);

            // Check augmented production
            Assert.True(augmented.Productions.ContainsKey("E'"));
            Assert.Equal(new List<string> { "E" }, augmented.Productions["E'"][0]);

            // Check original productions are copied
            Assert.True(augmented.Productions.ContainsKey("E"));
            Assert.Equal(new List<string> { "T" }, augmented.Productions["E"][0]);
            Assert.True(augmented.Productions.ContainsKey("T"));
            Assert.Equal(2, augmented.Productions["T"].Count); // T -> id | ( E )
        }
    }
}
