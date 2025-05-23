using CSharpSLRParser;

namespace CSharpSLRParser.Tests
{
    public class ParserUtilsClosureGotoTests
    {
        private Grammar CreateSimpleGrammar()
        {
            // S' -> S
            // S -> A A
            // A -> a A | b
            var grammar = new Grammar { StartSymbol = "S" }; // Real start symbol for the user grammar
            grammar.AddProduction("S -> A A");
            grammar.AddProduction("A -> a A | b");
            return grammar;
        }

        private Grammar CreateAugmentedGrammar()
        {
            // S' -> S
            // S -> A A
            // A -> a A | b
            var userGrammar = CreateSimpleGrammar();
            return userGrammar.GetAugmentedGrammar(); // This will be S' -> S, with S as its production.
        }


        [Fact]
        public void Closure_SingleItemDotAtEnd()
        {
            var grammar = CreateAugmentedGrammar(); // Use augmented for closure context
            var item = new LRItem("A", new List<string> { "b" }, 1); // A -> b .
            var initialSet = new HashSet<LRItem> { item };
            var closure = ParserUtils.Closure(initialSet, grammar);

            Assert.Single(closure);
            Assert.Contains(item, closure);
        }

        [Fact]
        public void Closure_SingleItemDotBeforeTerminal()
        {
            var grammar = CreateAugmentedGrammar();
            var item = new LRItem("A", new List<string> { "a", "A" }, 0); // A -> . a A
            var initialSet = new HashSet<LRItem> { item };
            var closure = ParserUtils.Closure(initialSet, grammar);

            Assert.Single(closure); // Dot before terminal 'a', no new items added from this rule itself
            Assert.Contains(item, closure);
        }

        [Fact]
        public void Closure_DotBeforeNonTerminal()
        {
            // S' -> . S, S -> . A A, A -> . a A, A -> . b
            var grammar = CreateAugmentedGrammar(); // S' -> S, S -> A A, A -> a A | b
            
            // Initial item: S' -> . S (dot at pos 0 of RHS ["S"])
            var initialItem = new LRItem(grammar.StartSymbol, grammar.Productions[grammar.StartSymbol][0], 0);
            var initialSet = new HashSet<LRItem> { initialItem };

            var closure = ParserUtils.Closure(initialSet, grammar);

            // Expected items in closure I0 for S' -> S, S -> AA, A -> aA | b
            // S' -> .S
            // S  -> .AA  (from S' -> .S)
            // A  -> .aA  (from S -> .AA, first A)
            // A  -> .b   (from S -> .AA, first A)
            
            var expectedSPrimeToDotS = initialItem; // S' -> . S
            var expectedSToDotAA = new LRItem("S", new List<string> { "A", "A" }, 0);
            var expectedAToDotaA = new LRItem("A", new List<string> { "a", "A" }, 0);
            var expectedAToDotb = new LRItem("A", new List<string> { "b" }, 0);

            Assert.Equal(4, closure.Count);
            Assert.Contains(expectedSPrimeToDotS, closure);
            Assert.Contains(expectedSToDotAA, closure);
            Assert.Contains(expectedAToDotaA, closure);
            Assert.Contains(expectedAToDotb, closure);
        }
        
        [Fact]
        public void Closure_ItemAlreadyInSet()
        {
            var grammar = CreateAugmentedGrammar();
            var item1 = new LRItem("S", new List<string> { "A", "A" }, 0); // S -> . A A
            var item2 = new LRItem("A", new List<string> { "a", "A" }, 0); // A -> . a A (will be added by S -> . A A)
            var initialSet = new HashSet<LRItem> { item1, item2 }; // Item2 is redundant if closure logic is correct
            
            var closure = ParserUtils.Closure(initialSet, grammar);
            
            // S -> . A A
            // A -> . a A (from S -> . A A, and also explicitly added)
            // A -> . b   (from S -> . A A)
            var expectedSToDotAA = item1;
            var expectedAToDotaA = item2;
            var expectedAToDotb = new LRItem("A", new List<string> { "b" }, 0);

            Assert.Equal(3, closure.Count);
            Assert.Contains(expectedSToDotAA, closure);
            Assert.Contains(expectedAToDotaA, closure);
            Assert.Contains(expectedAToDotb, closure);
        }


        // --- GOTO Tests ---

        [Fact]
        public void Goto_TransitionOnTerminal()
        {
            // I0 = { S' -> .S, S -> .AA, A -> .aA, A -> .b }
            // GOTO(I0, a) = Closure({ A -> a.A })
            // A -> a.A => adds A -> .aA, A -> .b
            var grammar = CreateAugmentedGrammar();
            var i0_item1 = new LRItem(grammar.StartSymbol, grammar.Productions[grammar.StartSymbol][0], 0); // S' -> .S
            var i0 = ParserUtils.Closure(new HashSet<LRItem> { i0_item1 }, grammar);

            var gotoResult = ParserUtils.Goto(i0, "a", grammar);

            // Expected:
            // A -> a . A  (from A -> .aA by advancing dot over 'a')
            // A -> . a A  (from A -> a . A, because dot is before non-terminal A)
            // A -> . b    (from A -> a . A, because dot is before non-terminal A)
            var expectedItem1 = new LRItem("A", new List<string> { "a", "A" }, 1); // A -> a . A
            var expectedItem2 = new LRItem("A", new List<string> { "a", "A" }, 0); // A -> . a A
            var expectedItem3 = new LRItem("A", new List<string> { "b" }, 0);    // A -> . b

            Assert.Equal(3, gotoResult.Count);
            Assert.Contains(expectedItem1, gotoResult);
            Assert.Contains(expectedItem2, gotoResult);
            Assert.Contains(expectedItem3, gotoResult);
        }

        [Fact]
        public void Goto_TransitionOnNonTerminal()
        {
            // I0 = { S' -> .S, S -> .AA, A -> .aA, A -> .b }
            // GOTO(I0, S) = Closure({ S' -> S. })
            var grammar = CreateAugmentedGrammar();
            var i0_item1 = new LRItem(grammar.StartSymbol, grammar.Productions[grammar.StartSymbol][0], 0); // S' -> .S
            var i0 = ParserUtils.Closure(new HashSet<LRItem> { i0_item1 }, grammar);

            var gotoResult = ParserUtils.Goto(i0, "S", grammar); // Transition on "S" (the original start symbol)

            // Expected: Closure({ S' -> S. }) which is just { S' -> S. }
            var expectedItem = new LRItem(grammar.StartSymbol, grammar.Productions[grammar.StartSymbol][0], 1); // S' -> S .

            Assert.Single(gotoResult);
            Assert.Contains(expectedItem, gotoResult);
        }
        
        [Fact]
        public void Goto_TransitionOnNonTerminal_LeadsToMoreItemsInClosure()
        {
            // I0 = { S' -> .S, S -> .AA, A -> .aA, A -> .b }
            // GOTO(I0, A) = Closure({ S -> A.A })
            // S -> A.A => adds A -> .aA, A -> .b
            var grammar = CreateAugmentedGrammar();
            var i0_item1 = new LRItem(grammar.StartSymbol, grammar.Productions[grammar.StartSymbol][0], 0); // S' -> .S
            var i0 = ParserUtils.Closure(new HashSet<LRItem> { i0_item1 }, grammar);

            var gotoResult = ParserUtils.Goto(i0, "A", grammar);

            // Expected:
            // S -> A . A  (from S -> .AA by advancing dot over 'A')
            // A -> . a A  (from S -> A . A, because dot is before non-terminal A)
            // A -> . b    (from S -> A . A, because dot is before non-terminal A)
            var expectedItem1 = new LRItem("S", new List<string> { "A", "A" }, 1); // S -> A . A
            var expectedItem2 = new LRItem("A", new List<string> { "a", "A" }, 0); // A -> . a A
            var expectedItem3 = new LRItem("A", new List<string> { "b" }, 0);    // A -> . b
            
            Assert.Equal(3, gotoResult.Count);
            Assert.Contains(expectedItem1, gotoResult);
            Assert.Contains(expectedItem2, gotoResult);
            Assert.Contains(expectedItem3, gotoResult);
        }


        [Fact]
        public void Goto_NoValidTransition()
        {
            // I0 = { S' -> .S, S -> .AA, A -> .aA, A -> .b }
            // GOTO(I0, x) where x is not 'a', 'b', 'A', or 'S'
            var grammar = CreateAugmentedGrammar();
             var i0_item1 = new LRItem(grammar.StartSymbol, grammar.Productions[grammar.StartSymbol][0], 0); // S' -> .S
            var i0 = ParserUtils.Closure(new HashSet<LRItem> { i0_item1 }, grammar);

            var gotoResult = ParserUtils.Goto(i0, "x", grammar); // 'x' is not a valid transition from I0

            Assert.Empty(gotoResult);
        }
        
        [Fact]
        public void Goto_FromStateWithCompletedItems()
        {
            // State I = { A -> b. }
            // GOTO(I, X) should be empty for any X
            var grammar = CreateAugmentedGrammar();
            var item = new LRItem("A", new List<string> { "b" }, 1); // A -> b .
            var stateI = new HashSet<LRItem> { item }; // This is already a closure

            var gotoResult = ParserUtils.Goto(stateI, "a", grammar);
            Assert.Empty(gotoResult);
            
            var gotoResult2 = ParserUtils.Goto(stateI, "A", grammar);
            Assert.Empty(gotoResult2);
        }
    }
}
