using CSharpSLRParser;

namespace CSharpSLRParser.Tests
{
    public class ParserUtilsFirstFollowTests
    {
        // Helper to create a grammar from productions easily
        private Grammar CreateGrammar(string startSymbol, params string[] productions)
        {
            var grammar = new Grammar { StartSymbol = startSymbol };
            foreach (var p in productions)
            {
                grammar.AddProduction(p);
            }
            return grammar;
        }

        [Fact]
        public void ComputeFirst_SimpleTerminals()
        {
            var grammar = CreateGrammar("S", "S -> a b c");
            var firstSets = ParserUtils.ComputeFirst(grammar);
            Assert.Equal(new HashSet<string> { "a" }, firstSets["S"]);
        }

        [Fact]
        public void ComputeFirst_NonTerminalsToTerminals()
        {
            var grammar = CreateGrammar("S", "S -> A", "A -> x");
            var firstSets = ParserUtils.ComputeFirst(grammar);
            Assert.Equal(new HashSet<string> { "x" }, firstSets["S"]);
            Assert.Equal(new HashSet<string> { "x" }, firstSets["A"]);
        }

        [Fact]
        public void ComputeFirst_EpsilonProductionSimple()
        {
            var grammar = CreateGrammar("A", "A -> e | b");
            var firstSets = ParserUtils.ComputeFirst(grammar);
            Assert.Equal(new HashSet<string> { "e", "b" }, firstSets["A"]);
        }
        
        [Fact]
        public void ComputeFirst_EpsilonInSequence()
        {
            var grammar = CreateGrammar("S", "S -> A B", "A -> e", "B -> b");
            var firstSets = ParserUtils.ComputeFirst(grammar);
            Assert.Equal(new HashSet<string> { "e" }, firstSets["A"]); // FIRST(A) = {e}
            Assert.Equal(new HashSet<string> { "b" }, firstSets["B"]); // FIRST(B) = {b}
            Assert.Equal(new HashSet<string> { "b" }, firstSets["S"]); // FIRST(S) = FIRST(B) because A can be epsilon
        }


        [Fact]
        public void ComputeFirst_CascadingNonTerminals()
        {
            var grammar = CreateGrammar("S", "S -> A", "A -> B", "B -> C", "C -> c");
            var firstSets = ParserUtils.ComputeFirst(grammar);
            Assert.Equal(new HashSet<string> { "c" }, firstSets["S"]);
            Assert.Equal(new HashSet<string> { "c" }, firstSets["A"]);
            Assert.Equal(new HashSet<string> { "c" }, firstSets["B"]);
            Assert.Equal(new HashSet<string> { "c" }, firstSets["C"]);
        }
        
        [Fact]
        public void ComputeFirst_LeftRecursionShouldStillCompute()
        {
            // FIRST set calculation should handle left recursion without infinite looping
            // The issue with left-recursion is for the parser itself, not FIRST/FOLLOW typically.
            var grammar = CreateGrammar("S", "S -> S a | b");
            var firstSets = ParserUtils.ComputeFirst(grammar);
            Assert.Equal(new HashSet<string> { "b" }, firstSets["S"]);
        }


        [Fact]
        public void ComputeFirst_ExampleGrammar1()
        {
            var grammar = CreateGrammar("S", "S -> A B", "A -> a | e", "B -> b");
            var firstSets = ParserUtils.ComputeFirst(grammar);

            Assert.Equal(new HashSet<string> { "a", "e" }, firstSets["A"]);
            Assert.Equal(new HashSet<string> { "b" }, firstSets["B"]);
            // FIRST(S) = (FIRST(A) - {e}) U (if e in FIRST(A) then FIRST(B))
            // FIRST(S) = {a} U {b} = {a,b}
            // If A can be 'e', and B starts with 'b', then S can start with 'b'.
            // If A starts with 'a', then S can start with 'a'.
            // If A is 'e' AND B is 'e', then S can be 'e'. Here B cannot be 'e'.
            Assert.Equal(new HashSet<string> { "a", "b" }, firstSets["S"]);
        }

        [Fact]
        public void ComputeFirst_ComplexGrammarWithEpsilon()
        {
            var grammar = CreateGrammar("X", "X -> Y Z", "Y -> a | e", "Z -> b | e");
            var firstSets = ParserUtils.ComputeFirst(grammar);
            Assert.Equal(new HashSet<string> { "a", "e" }, firstSets["Y"]);
            Assert.Equal(new HashSet<string> { "b", "e" }, firstSets["Z"]);
            // FIRST(X): from Y (a), if Y is e, from Z (b), if Y and Z are e, then e
            Assert.Equal(new HashSet<string> { "a", "b", "e" }, firstSets["X"]);
        }
        
        [Fact]
        public void ComputeFirst_MutualRecursion()
        {
            var grammar = CreateGrammar("A", "A -> B x | a", "B -> A y | b");
            var firstSets = ParserUtils.ComputeFirst(grammar);
            // Iteration 1: FIRST(A)={a}, FIRST(B)={b}
            // Iteration 2: A -> Bx => FIRST(B) U {x} = {b,x} => FIRST(A)={a,b,x}
            //              B -> Ay => FIRST(A) U {y} = {a,y} => FIRST(B)={b,a,y}
            // Iteration 3: A -> Bx => FIRST(B) U {x} = {b,a,y,x} => FIRST(A)={a,b,x,y}
            //              B -> Ay => FIRST(A) U {y} = {a,b,x,y} => FIRST(B)={b,a,y,x}
            Assert.Equal(new HashSet<string> { "a", "b", "x", "y" }, firstSets["A"]);
            Assert.Equal(new HashSet<string> { "a", "b", "x", "y" }, firstSets["B"]);
        }


        // --- FOLLOW Set Tests ---

        [Fact]
        public void ComputeFollow_ExampleGrammar1()
        {
            var grammar = CreateGrammar("S", "S -> A B", "A -> a | e", "B -> b");
            var firstSets = ParserUtils.ComputeFirst(grammar);
            // Expected FIRST(S) = {a, b}, FIRST(A) = {a, e}, FIRST(B) = {b}
            var followSets = ParserUtils.ComputeFollow(grammar, firstSets);

            Assert.Equal(new HashSet<string> { "$" }, followSets["S"]);
            // FOLLOW(A): In S -> A B, FOLLOW(A) contains FIRST(B) - {e} = {b}
            // Since FIRST(B) does not contain 'e', we don't add FOLLOW(S) based on that.
            Assert.Equal(new HashSet<string> { "b" }, followSets["A"]);
            // FOLLOW(B): In S -> A B, FOLLOW(B) contains FOLLOW(S) = {$}
            Assert.Equal(new HashSet<string> { "$" }, followSets["B"]);
        }

        [Fact]
        public void ComputeFollow_BasicRules()
        {
            var grammar = CreateGrammar("S", "S -> A B C", "A -> a", "B -> b", "C -> c");
            var firstSets = ParserUtils.ComputeFirst(grammar);
            var followSets = ParserUtils.ComputeFollow(grammar, firstSets);

            // S -> A B C: FOLLOW(A) includes FIRST(B C) = FIRST(B) = {b}
            Assert.Contains("b", followSets["A"]);
            // S -> A B C: FOLLOW(B) includes FIRST(C) = {c}
            Assert.Contains("c", followSets["B"]);
            // S -> A B C: FOLLOW(C) includes FOLLOW(S) = {$}
            Assert.Contains("$", followSets["C"]);
        }

        [Fact]
        public void ComputeFollow_EndOfProductionRule()
        {
            var grammar = CreateGrammar("S", "S -> X A", "X -> x", "A -> a");
            var firstSets = ParserUtils.ComputeFirst(grammar);
            var followSets = ParserUtils.ComputeFollow(grammar, firstSets);

            // S -> X A: FOLLOW(A) includes FOLLOW(S) = {$}
            Assert.Equal(new HashSet<string> { "$" }, followSets["A"]);
            // S -> X A: FOLLOW(X) includes FIRST(A) = {a}
            Assert.Equal(new HashSet<string> { "a" }, followSets["X"]);
        }

        [Fact]
        public void ComputeFollow_EpsilonInFollowingProductionPart()
        {
            // S -> A B C, B -> e. FOLLOW(A) includes FIRST(C) because B can be epsilon.
            var grammar = CreateGrammar("S", "S -> A B C", "A -> a", "B -> e", "C -> c");
            var firstSets = ParserUtils.ComputeFirst(grammar);
            var followSets = ParserUtils.ComputeFollow(grammar, firstSets);

            // FIRST(B) = {e}
            // FOLLOW(A): from S -> A B C. FIRST(B C)
            //   FIRST(B) has 'e'. So FOLLOW(A) gets (FIRST(B)-{e}) U FIRST(C)
            //   FIRST(B)-{e} = {}
            //   FIRST(C) = {c}
            //   So {c} is in FOLLOW(A).
            //   Since B can be e, we also consider S -> A C effectively, so FIRST(C) added.
            //   AND, because B can be e, if C could also be e, then FOLLOW(S) would also be added.
            //   In S -> A B C:
            //   For A: look at B. FIRST(B) = {e}.
            //          So, add FIRST(C) to FOLLOW(A) = {c}.
            //          Since C cannot be e, stop.
            Assert.Equal(new HashSet<string> { "c" }, followSets["A"]);

            // FOLLOW(B): from S -> A B C. FOLLOW(B) includes FIRST(C) - {e} = {c}.
            // Since C cannot be e, FOLLOW(S) is not added for this rule based on C.
            Assert.Equal(new HashSet<string> { "c" }, followSets["B"]);
        }
        
        [Fact]
        public void ComputeFollow_EpsilonInMiddlePropagatesFollow()
        {
            // S -> X Y Z. Y -> e. FOLLOW(X) includes FIRST(Z).
            // If Z -> e also, then FOLLOW(X) includes FOLLOW(S).
            var grammar = CreateGrammar("S", "S -> X Y Z", "X -> x", "Y -> e", "Z -> e");
            var firstSets = ParserUtils.ComputeFirst(grammar);
            var followSets = ParserUtils.ComputeFollow(grammar, firstSets);

            // FOLLOW(X):
            // Rule S -> X Y Z. Look at Y. FIRST(Y)={e}.
            // So, look at Z. FIRST(Z)={e}. Add (FIRST(Z)-{e}) to FOLLOW(X) (nothing).
            // Since Z can also be e, add FOLLOW(S) to FOLLOW(X). So FOLLOW(X) gets {$}.
            Assert.Equal(new HashSet<string> { "$" }, followSets["X"]);

            // FOLLOW(Y):
            // Rule S -> X Y Z. Look at Z. FIRST(Z)={e}.
            // Add (FIRST(Z)-{e}) to FOLLOW(Y) (nothing).
            // Since Z can be e, add FOLLOW(S) to FOLLOW(Y). So FOLLOW(Y) gets {$}.
            Assert.Equal(new HashSet<string> { "$" }, followSets["Y"]);
            
            // FOLLOW(Z):
            // Rule S -> X Y Z. Z is at end. Add FOLLOW(S) to FOLLOW(Z). So FOLLOW(Z) gets {$}.
            Assert.Equal(new HashSet<string> { "$" }, followSets["Z"]);
        }


        [Fact]
        public void ComputeFollow_ExampleGrammar2_DragonBookFig3_19()
        {
            // E -> T E'
            // E' -> + T E' | e
            // T -> F T'
            // T' -> * F T' | e
            // F -> ( E ) | id
            var grammar = CreateGrammar("E",
                "E -> T E'",
                "E' -> + T E' | e",
                "T -> F T'",
                "T' -> * F T' | e",
                "F -> ( E ) | id"
            );
            var firstSets = ParserUtils.ComputeFirst(grammar);
            var followSets = ParserUtils.ComputeFollow(grammar, firstSets);

            // Expected FIRST sets (for reference, not primary assertion here)
            // FIRST(F) = {(, id}
            // FIRST(T') = {*, e}
            // FIRST(T) = {(, id} (since F is first in T's production and T' can be e)
            // FIRST(E') = {+, e}
            // FIRST(E) = {(, id} (since T is first in E's production and E' can be e)

            // Expected FOLLOW sets (from Dragon Book, Fig 3.19 or similar examples)
            // FOLLOW(E) = {$, )}  (E is start, and from F -> (E) )
            // FOLLOW(E') = FOLLOW(E) = {$, )} (Rule E -> T E', E' is at end)
            // FOLLOW(T) = FIRST(E') - {e} U FOLLOW(E) if e in FIRST(E')
            //           = {+} U FOLLOW(E)  (since e is in FIRST(E'))
            //           = {+, $, )}
            // FOLLOW(T') = FOLLOW(T) = {+, $, )} (Rule T -> F T', T' is at end)
            // FOLLOW(F) = FIRST(T') - {e} U FOLLOW(T) if e in FIRST(T')
            //           = {*} U FOLLOW(T) (since e is in FIRST(T'))
            //           = {*, +, $, )}

            Assert.Equal(new HashSet<string> { "$", ")" }, followSets["E"]);
            Assert.Equal(new HashSet<string> { "$", ")" }, followSets["E'"]);
            Assert.Equal(new HashSet<string> { "+", "$", ")" }, followSets["T"]);
            Assert.Equal(new HashSet<string> { "+", "$", ")" }, followSets["T'"]);
            Assert.Equal(new HashSet<string> { "*", "+", "$", ")" }, followSets["F"]);
        }
    }
}
