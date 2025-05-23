using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpSLRParser
{
    public class Grammar
    {
        public Dictionary<string, List<List<string>>> Productions { get; private set; }
        public HashSet<string> Terminals { get; private set; }
        public HashSet<string> NonTerminals { get; private set; }
        public string StartSymbol { get; set; }

        public Grammar()
        {
            Productions = new Dictionary<string, List<List<string>>>();
            Terminals = new HashSet<string>();
            NonTerminals = new HashSet<string>();
            StartSymbol = "S"; // Default start symbol
        }

        public void AddProduction(string line)
        {
            var parts = line.Split(new[] { "->" }, StringSplitOptions.None);
            string lhs = parts[0].Trim();
            string[] rhsAlternatives = parts[1].Trim().Split('|');

            if (!Productions.ContainsKey(lhs))
            {
                Productions[lhs] = new List<List<string>>();
            }
            NonTerminals.Add(lhs);

            foreach (var alt in rhsAlternatives)
            {
                var symbols = alt.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                Productions[lhs].Add(symbols);

                foreach (var symbol in symbols)
                {
                    if (symbol == "e") // Epsilon
                    {
                        continue;
                    }
                    else if (char.IsUpper(symbol[0]))
                    {
                        NonTerminals.Add(symbol);
                    }
                    else
                    {
                        Terminals.Add(symbol);
                    }
                }
            }
            Terminals.Remove("e"); // 'e' is not a terminal
        }

        public Dictionary<string, List<List<string>>> GetProductions()
        {
            return Productions;
        }

        public List<string> GetTerminals()
        {
            return Terminals.ToList();
        }

        public List<string> GetNonTerminals()
        {
            return NonTerminals.ToList();
        }

        public Grammar GetAugmentedGrammar()
        {
            Grammar augmentedGrammar = new Grammar();
            string newStartSymbol = StartSymbol + "'";
            while (NonTerminals.Contains(newStartSymbol) || Terminals.Contains(newStartSymbol)) // Ensure uniqueness
            {
                newStartSymbol += "'";
            }

            augmentedGrammar.StartSymbol = newStartSymbol;
            augmentedGrammar.NonTerminals.Add(newStartSymbol);

            // Add S' -> S production
            augmentedGrammar.Productions[newStartSymbol] = new List<List<string>>
            {
                new List<string> { StartSymbol }
            };

            // Copy original productions
            foreach (var prod in Productions)
            {
                if (!augmentedGrammar.Productions.ContainsKey(prod.Key))
                {
                    augmentedGrammar.Productions[prod.Key] = new List<List<string>>();
                }
                foreach (var rhs in prod.Value)
                {
                    augmentedGrammar.Productions[prod.Key].Add(new List<string>(rhs));
                }
            }

            // Copy terminals and non-terminals
            foreach (var t in Terminals)
            {
                augmentedGrammar.Terminals.Add(t);
            }
            foreach (var nt in NonTerminals)
            {
                augmentedGrammar.NonTerminals.Add(nt);
            }
            
            // Ensure all symbols in productions are classified
            foreach (var prodList in augmentedGrammar.Productions.Values)
            {
                foreach (var rhs in prodList)
                {
                    foreach (var symbol in rhs)
                    {
                        if (symbol == "e") continue;
                        if (char.IsUpper(symbol[0]) && !augmentedGrammar.NonTerminals.Contains(symbol))
                        {
                            augmentedGrammar.NonTerminals.Add(symbol);
                        }
                        else if (!char.IsUpper(symbol[0]) && !augmentedGrammar.Terminals.Contains(symbol))
                        {
                             augmentedGrammar.Terminals.Add(symbol);
                        }
                    }
                }
            }
            augmentedGrammar.Terminals.Remove("e");


            return augmentedGrammar;
        }
    }
}
