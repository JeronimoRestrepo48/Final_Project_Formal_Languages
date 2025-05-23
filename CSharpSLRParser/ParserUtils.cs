using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpSLRParser
{
    public static class ParserUtils
    {
        public static HashSet<LRItem> Closure(HashSet<LRItem> items, Grammar grammar)
        {
            var closureSet = new HashSet<LRItem>(items);
            var queue = new Queue<LRItem>(items);

            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                string nextSymbol = item.NextSymbol();

                if (nextSymbol != null && grammar.NonTerminals.Contains(nextSymbol))
                {
                    if (grammar.Productions.TryGetValue(nextSymbol, out var productionsForNextSymbol))
                    {
                        foreach (var rhs in productionsForNextSymbol)
                        {
                            var newItem = new LRItem(nextSymbol, rhs, 0);
                            if (closureSet.Add(newItem)) // Add returns true if item was added (not already present)
                            {
                                queue.Enqueue(newItem);
                            }
                        }
                    }
                }
            }
            return closureSet;
        }

        public static HashSet<LRItem> Goto(HashSet<LRItem> items, string symbolX, Grammar grammar)
        {
            var movedItems = new HashSet<LRItem>();
            foreach (var item in items)
            {
                if (item.NextSymbol() == symbolX)
                {
                    movedItems.Add(item.AdvanceDot());
                }
            }
            return Closure(movedItems, grammar);
        }

        public static Dictionary<string, HashSet<string>> ComputeFirst(Grammar grammar)
        {
            var firstSets = new Dictionary<string, HashSet<string>>();
            foreach (var nt in grammar.GetNonTerminals())
            {
                firstSets[nt] = new HashSet<string>();
            }

            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var productionEntry in grammar.Productions)
                {
                    string lhs = productionEntry.Key;
                    foreach (var rhs in productionEntry.Value)
                    {
                        bool allSymbolsCanProduceEpsilon = true;
                        foreach (var symbol in rhs)
                        {
                            if (symbol == "e")
                            {
                                if (firstSets[lhs].Add("e"))
                                {
                                    changed = true;
                                }
                                break; // Epsilon applies to this specific RHS, move to next RHS
                            }
                            else if (grammar.Terminals.Contains(symbol))
                            {
                                if (firstSets[lhs].Add(symbol))
                                {
                                    changed = true;
                                }
                                allSymbolsCanProduceEpsilon = false;
                                break; // Terminal found, stop processing this RHS
                            }
                            else if (grammar.NonTerminals.Contains(symbol))
                            {
                                // Add FIRST(symbol) - {e} to FIRST(lhs)
                                foreach (var term in firstSets[symbol])
                                {
                                    if (term != "e")
                                    {
                                        if (firstSets[lhs].Add(term))
                                        {
                                            changed = true;
                                        }
                                    }
                                }
                                if (!firstSets[symbol].Contains("e"))
                                {
                                    allSymbolsCanProduceEpsilon = false;
                                    break; // This non-terminal does not produce epsilon, stop
                                }
                                // If this symbol can produce epsilon, continue to the next symbol in RHS
                            }
                            else
                            {
                                // Should not happen if grammar is well-formed
                                // Or if symbol is not yet in NonTerminals (e.g. during initial runs)
                                if (!firstSets.ContainsKey(symbol)) firstSets[symbol] = new HashSet<string>();

                                if (!firstSets[symbol].Contains("e"))
                                {
                                     allSymbolsCanProduceEpsilon = false;
                                     break;
                                }
                            }
                        }

                        if (allSymbolsCanProduceEpsilon)
                        {
                            if (firstSets[lhs].Add("e"))
                            {
                                changed = true;
                            }
                        }
                    }
                }
            }
            return firstSets;
        }

        public static Dictionary<string, HashSet<string>> ComputeFollow(Grammar grammar, Dictionary<string, HashSet<string>> firstSets)
        {
            var followSets = new Dictionary<string, HashSet<string>>();
            foreach (var nt in grammar.GetNonTerminals())
            {
                followSets[nt] = new HashSet<string>();
            }

            if (!string.IsNullOrEmpty(grammar.StartSymbol) && followSets.ContainsKey(grammar.StartSymbol))
            {
                followSets[grammar.StartSymbol].Add("$");
            }


            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var productionEntry in grammar.Productions)
                {
                    string lhsA = productionEntry.Key;
                    foreach (var rhs in productionEntry.Value)
                    {
                        for (int i = 0; i < rhs.Count; i++)
                        {
                            string symbolB = rhs[i];
                            if (grammar.NonTerminals.Contains(symbolB))
                            {
                                bool hasEpsilonInRest = true;
                                // Process symbols in 'rest' (after B)
                                for (int j = i + 1; j < rhs.Count; j++)
                                {
                                    string symbolX = rhs[j];
                                    if (grammar.Terminals.Contains(symbolX))
                                    {
                                        if (followSets[symbolB].Add(symbolX))
                                        {
                                            changed = true;
                                        }
                                        hasEpsilonInRest = false;
                                        break; 
                                    }
                                    else if (grammar.NonTerminals.Contains(symbolX))
                                    {
                                        foreach (var term in firstSets[symbolX])
                                        {
                                            if (term != "e")
                                            {
                                                if (followSets[symbolB].Add(term))
                                                {
                                                    changed = true;
                                                }
                                            }
                                        }
                                        if (!firstSets[symbolX].Contains("e"))
                                        {
                                            hasEpsilonInRest = false;
                                            break;
                                        }
                                    } else { // Symbol X not in Terminals or NonTerminals (e.g. 'e' or invalid)
                                        hasEpsilonInRest = false; // Treat 'e' explicitly, or unknown as non-epsilon contributing
                                        break;
                                    }
                                }

                                // If all symbols in 'rest' can produce epsilon, or 'rest' is empty
                                if (hasEpsilonInRest)
                                {
                                    if (followSets.ContainsKey(lhsA)) // Ensure lhsA is a valid key
                                    {
                                        foreach (var term in followSets[lhsA])
                                        {
                                            if (followSets[symbolB].Add(term))
                                            {
                                                changed = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return followSets;
        }

        public static Tuple<Dictionary<Tuple<int, string>, Tuple<string, object>>, Dictionary<Tuple<int, string>, int>> BuildSlrTable(
            Grammar grammar, Dictionary<string, HashSet<string>> followSets)
        {
            var actionTable = new Dictionary<Tuple<int, string>, Tuple<string, object>>();
            var gotoTable = new Dictionary<Tuple<int, string>, int>();

            Grammar augmentedGrammar = grammar.GetAugmentedGrammar();
            
            // Ensure StartSymbol from augmentedGrammar is used for initial item creation.
            // Productions for the augmented start symbol should exist.
            if (!augmentedGrammar.Productions.TryGetValue(augmentedGrammar.StartSymbol, out var startProductions) || !startProductions.Any())
            {
                // This case should ideally not happen if GetAugmentedGrammar is correct.
                // Consider logging an error or throwing an exception.
                return Tuple.Create<Dictionary<Tuple<int, string>, Tuple<string, object>>, Dictionary<Tuple<int, string>, int>>(null, null);
            }
            List<string> startProductionRhs = startProductions.First(); 
            var startItem = new LRItem(augmentedGrammar.StartSymbol, startProductionRhs, 0);

            var initialItems = new HashSet<LRItem> { startItem };
            var initialClosure = Closure(initialItems, augmentedGrammar);

            var states = new List<HashSet<LRItem>> { initialClosure };
            var stateMap = new Dictionary<HashSet<LRItem>, int>(HashSet<LRItem>.CreateSetComparer())
            {
                { initialClosure, 0 }
            };

            var queue = new Queue<int>();
            queue.Enqueue(0);

            var allSymbols = grammar.GetTerminals().Concat(grammar.GetNonTerminals()).ToList();


            while (queue.Count > 0)
            {
                int stateIndex_i = queue.Dequeue();
                HashSet<LRItem> state_I = states[stateIndex_i];

                // Process items for ACTION table entries (shift, reduce, accept)
                foreach (var item in state_I)
                {
                    if (item.IsComplete())
                    {
                        if (item.LHS == augmentedGrammar.StartSymbol)
                        {
                            var actionKey = Tuple.Create(stateIndex_i, "$");
                            if (actionTable.ContainsKey(actionKey)) return Tuple.Create<Dictionary<Tuple<int, string>, Tuple<string, object>>, Dictionary<Tuple<int, string>, int>>(null, null); // Conflict
                            actionTable[actionKey] = Tuple.Create("acc", (object)null);
                        }
                        else
                        {
                            if (followSets.TryGetValue(item.LHS, out var follow_A))
                            {
                                foreach (var terminal_a in follow_A)
                                {
                                    var actionKey = Tuple.Create(stateIndex_i, terminal_a);
                                    var newAction = Tuple.Create("r", (object)Tuple.Create(item.LHS, item.RHS));
                                    if (actionTable.TryGetValue(actionKey, out var existingAction))
                                    {
                                        // Conflict detection: if existing action is different or is a reduce on a different rule.
                                        // Simple check: if key exists, it's a conflict for basic SLR.
                                        return Tuple.Create<Dictionary<Tuple<int, string>, Tuple<string, object>>, Dictionary<Tuple<int, string>, int>>(null, null); // Conflict
                                    }
                                    actionTable[actionKey] = newAction;
                                }
                            }
                        }
                    }
                    else // Item is not complete, A -> α.Xβ
                    {
                        string symbolX = item.NextSymbol();
                        if (grammar.Terminals.Contains(symbolX)) // X is a terminal
                        {
                            HashSet<LRItem> nextState_J_items = Goto(state_I, symbolX, augmentedGrammar);
                            if (nextState_J_items == null || !nextState_J_items.Any()) continue;

                            int stateIndex_j;
                            if (!stateMap.TryGetValue(nextState_J_items, out stateIndex_j))
                            {
                                stateIndex_j = states.Count;
                                states.Add(nextState_J_items);
                                stateMap[nextState_J_items] = stateIndex_j;
                                queue.Enqueue(stateIndex_j);
                            }

                            var actionKey = Tuple.Create(stateIndex_i, symbolX);
                            var newAction = Tuple.Create("s", (object)stateIndex_j);
                             if (actionTable.TryGetValue(actionKey, out var existingAction))
                            {
                                // Conflict detection
                                return Tuple.Create<Dictionary<Tuple<int, string>, Tuple<string, object>>, Dictionary<Tuple<int, string>, int>>(null, null); // Conflict
                            }
                            actionTable[actionKey] = newAction;
                        }
                    }
                }

                // Process items for GOTO table entries (for non-terminals)
                foreach (var nonTerminal_A in grammar.GetNonTerminals())
                {
                    HashSet<LRItem> nextState_J_items = Goto(state_I, nonTerminal_A, augmentedGrammar);
                    if (nextState_J_items == null || !nextState_J_items.Any()) continue;

                    int stateIndex_j;
                    if (!stateMap.TryGetValue(nextState_J_items, out stateIndex_j))
                    {
                        stateIndex_j = states.Count;
                        states.Add(nextState_J_items);
                        stateMap[nextState_J_items] = stateIndex_j;
                        queue.Enqueue(stateIndex_j);
                    }
                    gotoTable[Tuple.Create(stateIndex_i, nonTerminal_A)] = stateIndex_j;
                }
            }
            return Tuple.Create(actionTable, gotoTable);
        }

        public static bool ValidateStringSlr(string inputString, Grammar grammar,
            Dictionary<Tuple<int, string>, Tuple<string, object>> actionTable,
            Dictionary<Tuple<int, string>, int> gotoTable)
        {
            var stack = new Stack<int>();
            stack.Push(0); // Initial state

            var symbols = inputString.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!inputString.Any()) symbols = new List<string>(); // Handle empty string case explicitly if necessary
            symbols.Add("$"); // End of input marker

            int currentIndex = 0;

            while (true)
            {
                if (stack.Count == 0) return false; // Should not happen in a valid SLR parse
                int currentState = stack.Peek();
                string currentSymbol = symbols[currentIndex];

                var actionKey = Tuple.Create(currentState, currentSymbol);
                if (!actionTable.TryGetValue(actionKey, out var action))
                {
                    return false; // Error: No action defined
                }

                string actionType = action.Item1;
                object actionValue = action.Item2;

                if (actionType == "s") // Shift
                {
                    stack.Push((int)actionValue);
                    currentIndex++;
                }
                else if (actionType == "r") // Reduce
                {
                    var reductionRule = (Tuple<string, List<string>>)actionValue;
                    string lhs = reductionRule.Item1;
                    List<string> rhs = reductionRule.Item2;

                    // Handle epsilon production: if RHS is ["e"] or empty, pop 0 or 1 based on convention
                    // Python code: if β != 'e': for _ in β: stack.pop()
                    // Assuming RHS ["e"] means epsilon, or an empty list means epsilon.
                    // If RHS contains "e" as a symbol, it means the rule is X -> e.
                    // If Grammar class represents X -> epsilon as X -> [], then rhs.Count is 0.
                    // If Grammar class represents X -> epsilon as X -> ["e"], then rhs.Count is 1.
                    // Let's assume our Grammar class stores X -> e as RHS = ["e"].
                    // And an empty production A -> '' as RHS = []
                    if (!(rhs.Count == 1 && rhs[0] == "e") && rhs.Any()) // Not an epsilon production explicitly marked as "e" and not empty
                    {
                        for (int i = 0; i < rhs.Count; i++)
                        {
                            if (stack.Count == 0) return false; // Error: stack empty during pop
                            stack.Pop();
                        }
                    }
                    
                    if (stack.Count == 0) return false; // Error: stack empty before GOTO lookup
                    int peekState = stack.Peek();
                    var gotoKey = Tuple.Create(peekState, lhs);

                    if (!gotoTable.TryGetValue(gotoKey, out int nextStateForLhs))
                    {
                        return false; // Error: No GOTO defined
                    }
                    stack.Push(nextStateForLhs);
                    // Optional: Console.WriteLine($"Reduced by: {lhs} -> {string.Join(" ", rhs)}");
                }
                else if (actionType == "acc") // Accept
                {
                    return true;
                }
                else // Error
                {
                    return false;
                }
            }
        }
    }
}
