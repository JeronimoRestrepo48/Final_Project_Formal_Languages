using CSharpSLRParser;

namespace CSharpSLRParser.Tests
{
    public class ParserUtilsSlrTableValidationTests
    {
        private Grammar CreateDragonBookFig4_33Grammar()
        {
            // (1) E -> E + T
            // (2) E -> T
            // (3) T -> T * F
            // (4) T -> F
            // (5) F -> ( E )
            // (6) F -> id
            var grammar = new Grammar { StartSymbol = "E" };
            grammar.AddProduction("E -> E + T | T");
            grammar.AddProduction("T -> T * F | F");
            grammar.AddProduction("F -> ( E ) | id");
            return grammar;
        }

        // Helper to make action table keys more readable in tests
        private Tuple<int, string> Key(int state, string symbol) => Tuple.Create(state, symbol);
        // Helper for shift actions
        private Tuple<string, object> Shift(int state) => Tuple.Create("s", (object)state);
        // Helper for reduce actions (LHS and RHS strings)
        private Tuple<string, object> Reduce(string lhs, params string[] rhsSymbols) 
            => Tuple.Create("r", (object)Tuple.Create(lhs, new List<string>(rhsSymbols)));
        // Helper for accept action
        private Tuple<string, object> Accept() => Tuple.Create("acc", (object)null);


        [Fact]
        public void BuildSlrTable_DragonBookFig4_33_AndValidate()
        {
            var grammar = CreateDragonBookFig4_33Grammar();
            var firstSets = ParserUtils.ComputeFirst(grammar);
            var followSets = ParserUtils.ComputeFollow(grammar, firstSets);

            // Expected FOLLOW sets (for reference, these are used internally by BuildSlrTable)
            // FOLLOW(E) = {$, +, )}
            // FOLLOW(T) = {$, +, *, )}
            // FOLLOW(F) = {$, +, *, )}

            var (actionTable, gotoTable) = ParserUtils.BuildSlrTable(grammar, followSets);

            Assert.NotNull(actionTable);
            Assert.NotNull(gotoTable);

            // --- Expected ACTION Table (Dragon Book Figure 4.35 / Table 4.7) ---
            // State |  id   |   +   |   *   |   (   |   )   |   $   |
            // ------|-------|-------|-------|-------|-------|-------|
            //   0   |   s5  |       |       |   s4  |       |       |
            //   1   |       |   s6  |       |       |       |  acc  |
            //   2   |       |   r2  |   s7  |       |   r2  |   r2  | (E->T)
            //   3   |       |   r4  |   r4  |       |   r4  |   r4  | (T->F)
            //   4   |   s5  |       |       |   s4  |       |       |
            //   5   |       |   r6  |   r6  |       |   r6  |   r6  | (F->id)
            //   6   |   s5  |       |       |   s4  |       |       |
            //   7   |   s5  |       |       |   s4  |       |       |
            //   8   |       |   s6  |       |       |  s11  |       |
            //   9   |       |   r1  |   s7  |       |   r1  |   r1  | (E->E+T)
            //   10  |       |   r3  |   r3  |       |   r3  |   r3  | (T->T*F)
            //   11  |       |   r5  |   r5  |       |   r5  |   r5  | (F->(E))

            // Check some key action table entries
            // State 0
            Assert.Equal(Shift(5), actionTable[Key(0, "id")]);
            Assert.Equal(Shift(4), actionTable[Key(0, "(")]);

            // State 1
            Assert.Equal(Shift(6), actionTable[Key(1, "+")]);
            Assert.Equal(Accept(), actionTable[Key(1, "$")]);

            // State 2
            Assert.Equal(Reduce("E", "T"), actionTable[Key(2, "+")]); // E -> T
            Assert.Equal(Shift(7), actionTable[Key(2, "*")]);
            Assert.Equal(Reduce("E", "T"), actionTable[Key(2, ")")]);
            Assert.Equal(Reduce("E", "T"), actionTable[Key(2, "$")]);
            
            // State 3 (T -> F)
            Assert.Equal(Reduce("T", "F"), actionTable[Key(3, "+")]);
            Assert.Equal(Reduce("T", "F"), actionTable[Key(3, "*")]);
            Assert.Equal(Reduce("T", "F"), actionTable[Key(3, ")")]);
            Assert.Equal(Reduce("T", "F"), actionTable[Key(3, "$")]);

            // State 4
            Assert.Equal(Shift(5), actionTable[Key(4, "id")]);
            Assert.Equal(Shift(4), actionTable[Key(4, "(")]);
            
            // State 5 (F -> id)
            Assert.Equal(Reduce("F", "id"), actionTable[Key(5, "+")]);
            Assert.Equal(Reduce("F", "id"), actionTable[Key(5, "*")]);
            Assert.Equal(Reduce("F", "id"), actionTable[Key(5, ")")]);
            Assert.Equal(Reduce("F", "id"), actionTable[Key(5, "$")]);

            // State 6
            Assert.Equal(Shift(5), actionTable[Key(6, "id")]);
            Assert.Equal(Shift(4), actionTable[Key(6, "(")]);

            // State 7
            Assert.Equal(Shift(5), actionTable[Key(7, "id")]);
            Assert.Equal(Shift(4), actionTable[Key(7, "(")]);
            
            // State 8
            Assert.Equal(Shift(6), actionTable[Key(8, "+")]);
            Assert.Equal(Shift(11), actionTable[Key(8, ")")]);

            // State 9 (E -> E + T)
            Assert.Equal(Reduce("E", "E", "+", "T"), actionTable[Key(9, "+")]);
            Assert.Equal(Shift(7), actionTable[Key(9, "*")]);
            Assert.Equal(Reduce("E", "E", "+", "T"), actionTable[Key(9, ")")]);
            Assert.Equal(Reduce("E", "E", "+", "T"), actionTable[Key(9, "$")]);

            // State 10 (T -> T * F)
            Assert.Equal(Reduce("T", "T", "*", "F"), actionTable[Key(10, "+")]);
            Assert.Equal(Reduce("T", "T", "*", "F"), actionTable[Key(10, "*")]);
            Assert.Equal(Reduce("T", "T", "*", "F"), actionTable[Key(10, ")")]);
            Assert.Equal(Reduce("T", "T", "*", "F"), actionTable[Key(10, "$")]);

            // State 11 (F -> ( E ))
            Assert.Equal(Reduce("F", "(", "E", ")"), actionTable[Key(11, "+")]);
            Assert.Equal(Reduce("F", "(", "E", ")"), actionTable[Key(11, "*")]);
            Assert.Equal(Reduce("F", "(", "E", ")"), actionTable[Key(11, ")")]);
            Assert.Equal(Reduce("F", "(", "E", ")"), actionTable[Key(11, "$")]);


            // --- Expected GOTO Table (Dragon Book Figure 4.35 / Table 4.7) ---
            // State | E | T | F |
            // ------|---|---|---|
            //   0   | 1 | 2 | 3 |
            //   1   |   |   |   |
            //   2   |   |   |   |
            //   3   |   |   |   |
            //   4   | 8 | 2 | 3 |
            //   5   |   |   |   |
            //   6   |   | 9 | 3 |
            //   7   |   |   |10 |
            //   8   |   |   |   |
            //   9   |   |   |   |
            //   10  |   |   |   |
            //   11  |   |   |   |
            Assert.Equal(1, gotoTable[Key(0, "E")]);
            Assert.Equal(2, gotoTable[Key(0, "T")]);
            Assert.Equal(3, gotoTable[Key(0, "F")]);

            Assert.Equal(8, gotoTable[Key(4, "E")]);
            Assert.Equal(2, gotoTable[Key(4, "T")]);
            Assert.Equal(3, gotoTable[Key(4, "F")]);
            
            Assert.Equal(9, gotoTable[Key(6, "T")]); // After E+ (state 6), on T, go to 9
            Assert.Equal(3, gotoTable[Key(6, "F")]);

            Assert.Equal(10, gotoTable[Key(7, "F")]); // After T* (state 7), on F, go to 10

            // --- Validate Strings ---
            Assert.True(ParserUtils.ValidateStringSlr("id", grammar, actionTable, gotoTable));
            Assert.True(ParserUtils.ValidateStringSlr("id + id", grammar, actionTable, gotoTable));
            Assert.True(ParserUtils.ValidateStringSlr("id * id", grammar, actionTable, gotoTable));
            Assert.True(ParserUtils.ValidateStringSlr("id + id * id", grammar, actionTable, gotoTable));
            Assert.True(ParserUtils.ValidateStringSlr("( id )", grammar, actionTable, gotoTable));
            Assert.True(ParserUtils.ValidateStringSlr("( id + id )", grammar, actionTable, gotoTable));
            Assert.True(ParserUtils.ValidateStringSlr("( id + id ) * id", grammar, actionTable, gotoTable));

            Assert.False(ParserUtils.ValidateStringSlr("", grammar, actionTable, gotoTable)); // Empty string
            Assert.False(ParserUtils.ValidateStringSlr("id id", grammar, actionTable, gotoTable));
            Assert.False(ParserUtils.ValidateStringSlr("+ id", grammar, actionTable, gotoTable));
            Assert.False(ParserUtils.ValidateStringSlr("id +", grammar, actionTable, gotoTable));
            Assert.False(ParserUtils.ValidateStringSlr("( id", grammar, actionTable, gotoTable));
            Assert.False(ParserUtils.ValidateStringSlr("id )", grammar, actionTable, gotoTable));
            Assert.False(ParserUtils.ValidateStringSlr("( ( id )", grammar, actionTable, gotoTable));
            Assert.False(ParserUtils.ValidateStringSlr("id + * id", grammar, actionTable, gotoTable));
        }

        [Fact]
        public void BuildSlrTable_ShiftReduceConflict()
        {
            // Dangling else: S -> if E then S | if E then S else S | other
            // Simplified: S -> A | B, A -> x y, B -> x z (y is terminal, z is terminal)
            // If we have items like { X -> x . y, X -> x . z } this is not a conflict itself.
            // A shift-reduce conflict occurs if a state has:
            // Item1: A -> α . a β (shift on 'a')
            // Item2: B -> γ .     (reduce on 'a', if 'a' is in FOLLOW(B))
            var grammar = new Grammar { StartSymbol = "S" };
            grammar.AddProduction("S -> E + S | E");
            grammar.AddProduction("E -> n"); // 'n' for number/id

            // State I: { S -> E . + S, S -> E . }
            // If current input is '+':
            //  - From S -> E . + S: Shift to state J containing S -> E + . S
            //  - From S -> E .    : Reduce by S -> E (since FOLLOW(S) contains '$' and also '+' if S appears in RHS like X -> S + Y)
            // For this grammar: FOLLOW(S) = {$}. FOLLOW(E) = {+, $}
            // Let's trace:
            // I0 = Closure({S' -> .S}) = {S' -> .S, S -> .E+S, S -> .E, E -> .n}
            // GOTO(I0, n) = Closure({E -> n.}) = I_n = {E -> n.}
            // GOTO(I0, E) = Closure({S' -> S., S -> E.+S, S -> E.}) = I_E
            // In state I_E:
            //   Item S' -> S. (reduce by S'->S on $, i.e. accept)
            //   Item S -> E . + S (shift on +)
            //   Item S -> E . (reduce by S->E if current input is in FOLLOW(S) = {$})
            // This configuration IS a shift-reduce conflict if input is '$' for S->E. vs S'->S.
            // But the classic S/R conflict is on a terminal.
            // Consider: S -> E + S | E, E -> n
            // I0: {S'->.S, S->.E+S, S->.E, E->.n}
            // GOTO(I0, E) -> I1: {S'->S., S->E.+S, S->E.}
            // In I1, on input '+': Shift by S->E.+S
            // In I1, on input '$': Reduce by S->E. (because $ is in FOLLOW(S))
            // This is fine.

            // Let's use a more direct conflict grammar:
            // S -> E
            // E -> T + E | T
            // T -> id
            // This is not SLR(1) due to ambiguity that typically shows as S/R.
            // Actually, the above is SLR(1).
            // A common example for S/R conflict:
            // Stmt -> Expr
            // Stmt -> if Expr then Stmt
            // Expr -> id
            // This is not SLR(1).
            
            // Let's use a simpler, direct S/R conflict example:
            // S -> A a
            // S -> b
            // A -> b
            // FOLLOW(A) = {a}
            // I0 = { S'->.S, S->.Aa, S->.b, A->.b }
            // GOTO(I0, b) -> I1 = Closure({ S->b., A->b. })
            // In I1, on input 'a': (Conflict expected)
            //   From A->b. : Reduce by A->b (because 'a' is in FOLLOW(A))
            //   From S->b. : This rule cannot lead to shift.
            // This is a reduce/reduce conflict if 'a' is also in FOLLOW(S) for S->b.
            // Let's re-evaluate the conflict condition.
            // A state { A -> α . , B -> β . a γ } on 'a' is Shift/Reduce if 'a' in FOLLOW(A).

            // Standard S/R conflict:
            // E -> E + T | T
            // T -> id
            // This grammar is ambiguous but SLR table construction might not show it directly
            // as a conflict *in the table entries* but rather in how states are built or used.
            // The Dragon Book example E -> E+T | T ... F->id IS SLR(1).

            // Let's try a grammar known to have S/R:
            // S -> L = R | R
            // L -> * R | id
            // R -> L
            // This is from Wikipedia, known to be non-LR(1), likely non-SLR(1).
            grammar = new Grammar { StartSymbol = "S" };
            grammar.AddProduction("S -> L = R | R");
            grammar.AddProduction("L -> * R | id");
            grammar.AddProduction("R -> L");

            var firstSets_sr = ParserUtils.ComputeFirst(grammar);
            var followSets_sr = ParserUtils.ComputeFollow(grammar, firstSets_sr);
            var (actionTable_sr, gotoTable_sr) = ParserUtils.BuildSlrTable(grammar, followSets_sr);

            // We expect a conflict, so one or both tables should be null.
            // The conflict arises in a state like:
            // { L -> id ., R -> L . }
            // If next symbol is '=', FOLLOW(R) contains '=' (from S -> L = R).
            // So, reduce by R -> L.
            // If next symbol is '=', FOLLOW(L) contains '=' (from S -> L = R).
            // This is fine.
            //
            // Consider state I containing { S -> R . , L -> id . }
            // FOLLOW(S) = { $ }
            // FOLLOW(L) = { = } (from S -> L = R)
            // If current token is '$', reduce by S -> R.
            // If current token is '=', reduce by L -> id.
            // No conflict here.
            // This grammar *is* SLR(1). The example might be for LR(1) vs LALR(1).
            // Let's re-verify the example source or find a canonical non-SLR(1) grammar.

            // A canonical non-SLR grammar (often causing S/R):
            // S -> A a
            // S -> b A c
            // S -> d c
            // S -> b d a
            // A -> e
            // (This example is complex to trace manually here for a specific conflict point)

            // Simpler S/R:
            // S -> x A z
            // A -> y | e
            // FOLLOW(A) = {z}
            // Productions: S -> xAz, A -> y, A -> e
            // I0: S' -> .S, S -> .xAz, A -> .y, A -> .e
            // GOTO(I0, x) -> I1: { S -> x.Az, A -> .y, A -> .e }
            // In I1:
            //   If next is 'y': Shift (from A->.y)
            //   If next is 'z': Reduce by A->e (since z in FOLLOW(A) and A->.e means A->e.)
            //   This state (I1) has items S->x.Az, A->.y, A->.e
            //   If we see 'y', we shift to GOTO(I1,y) = Closure({A->y.})
            //   If we see 'z', what happens due to S->x.Az?
            //      If A can be epsilon: S -> x . z
            //      If A can be y: S -> x y . z
            //   The conflict arises when a state contains:
            //   1. A -> α . a β  (shift on 'a')
            //   2. B -> γ .      (reduce on 'a' if 'a' ∈ FOLLOW(B))
            grammar = new Grammar { StartSymbol = "S" };
            grammar.AddProduction("S -> A"); // S -> A
            grammar.AddProduction("A -> a b | a"); // A -> ab | a
            // This grammar is ambiguous: "a" can be parsed as A->a or A->ab (if b is optional/epsilon elsewhere)
            // But as is, it's not ambiguous. "ab" is A->ab. "a" is A->a.
            // Let's try: S -> x A, A -> y B, B -> z | e, S -> x y
            // S -> x A
            // A -> y B
            // B -> z | e
            // S -> x y  -- This rule creates the S/R conflict
            // FOLLOW(B) = { $ } (from A->yB, S->xA)
            // I0: {S'->.S, S->.xA, S->.xy, A->.yB, B->.z, B->.e}
            // GOTO(I0,x) -> Ix: { S->x.A, S->x.y, A->.yB, B->.z, B->.e }
            // In Ix, on 'y':
            //    From S->x.y: Shift to GOTO(Ix,y) = Closure({S->xy.})
            //    From A->.yB: Shift to GOTO(Ix,y) = Closure({A->y.B, B->.z, B->.e})
            //    If these GOTO results are different states, that's fine. If same, also fine.
            //    The issue is if S->x.y means "shift and complete S->xy" AND A->.yB means "shift for A->yB"
            //    This is not a conflict yet.
            //
            // Let's use a known non-SLR grammar that causes S/R:
            // S -> E
            // E -> E * E | E + E | id
            // This grammar is ambiguous and will have S/R conflicts.
            grammar = new Grammar { StartSymbol = "S" };
            grammar.AddProduction("S -> E");
            grammar.AddProduction("E -> E * E | E + E | id");
            firstSets_sr = ParserUtils.ComputeFirst(grammar);
            followSets_sr = ParserUtils.ComputeFollow(grammar, firstSets_sr);
            (actionTable_sr, gotoTable_sr) = ParserUtils.BuildSlrTable(grammar, followSets_sr);
            Assert.Null(actionTable_sr); // Expecting conflict
            Assert.Null(gotoTable_sr);   // Expecting conflict
        }

        [Fact]
        public void BuildSlrTable_ReduceReduceConflict()
        {
            // R/R conflict: A state contains items A -> α . and B -> β .
            // And for some terminal 'a', 'a' is in FOLLOW(A) and 'a' is in FOLLOW(B).
            // S -> A a | B b
            // A -> x
            // B -> x
            // FOLLOW(A) = {a}, FOLLOW(B) = {b} -- No conflict here.
            //
            // Try: S -> X | Y, X -> z, Y -> z
            // FOLLOW(X) = {$} (if S is start and S->X, then $ in FOLLOW(X))
            // FOLLOW(Y) = {$} (if S is start and S->Y, then $ in FOLLOW(Y))
            // I0: {S'->.S, S->.X, S->.Y, X->.z, Y->.z}
            // GOTO(I0, z) -> Iz: Closure({X->z., Y->z.}) = {X->z., Y->z.}
            // In Iz, on input '$':
            //   Reduce by X->z (since $ in FOLLOW(X))
            //   Reduce by Y->z (since $ in FOLLOW(Y))
            // This is a R/R conflict.
            var grammar = new Grammar { StartSymbol = "S" };
            grammar.AddProduction("S -> X | Y");
            grammar.AddProduction("X -> z");
            grammar.AddProduction("Y -> z");

            var firstSets_rr = ParserUtils.ComputeFirst(grammar);
            var followSets_rr = ParserUtils.ComputeFollow(grammar, firstSets_rr);
            // FOLLOW(S) = {$}
            // FOLLOW(X) = {$} (from S->X, S'->S)
            // FOLLOW(Y) = {$} (from S->Y, S'->S)
            var (actionTable_rr, gotoTable_rr) = ParserUtils.BuildSlrTable(grammar, followSets_rr);

            Assert.Null(actionTable_rr); // Expecting R/R conflict
            Assert.Null(gotoTable_rr);
        }
    }
}
