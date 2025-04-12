#!/usr/bin/env python3

class Parser:
    def __init__(self):
        self.grammar = {}
        self.terminals = set()
        self.non_terminals = set()
        self.first_sets = {}
        self.follow_sets = {}
        self.parse_table_ll1 = {}
        self.action_table = {}
        self.goto_table = {}
        self.items = set()
        self.states = []
        self.is_ll1 = True
        self.is_slr1 = True
        self.start_symbol = 'S'
        self.augmented_grammar = {'S\'': ['S']}

    def read_grammar(self):
        """Read the grammar from standard input."""
        n = int(input())  # Number of non-terminals
        for _ in range(n):
            line = input().strip()
            parts = line.split(' -> ')
            non_terminal = parts[0]
            self.non_terminals.add(non_terminal)
            productions = parts[1].split(' ')
            if non_terminal not in self.grammar:
                self.grammar[non_terminal] = []
            self.grammar[non_terminal].extend(productions)
            
            # Identify terminals
            for prod in productions:
                for symbol in prod:
                    if symbol not in self.non_terminals and symbol != 'e':
                        self.terminals.add(symbol)
        
        # Add end marker as a terminal
        self.terminals.add('$')

    def calculate_first_sets(self):
        """Calculate the FIRST sets for all symbols in the grammar."""
        # Initialize FIRST sets
        for nt in self.non_terminals:
            self.first_sets[nt] = set()
        
        for t in self.terminals:
            self.first_sets[t] = {t}
        
        # Special case for epsilon
        self.first_sets['e'] = {'e'}
        
        # Repeat until no changes
        changed = True
        while changed:
            changed = False
            
            for nt, productions in self.grammar.items():
                for prod in productions:
                    # If production is epsilon, add epsilon to FIRST set
                    if prod == 'e':
                        if 'e' not in self.first_sets[nt]:
                            self.first_sets[nt].add('e')
                            changed = True
                        continue
                    
                    # Compute FIRST set for the production
                    all_derive_epsilon = True
                    for symbol in prod:
                        if symbol in self.terminals:
                            if symbol not in self.first_sets[nt]:
                                self.first_sets[nt].add(symbol)
                                changed = True
                            all_derive_epsilon = False
                            break
                        else:  # Non-terminal
                            # Add all non-epsilon symbols from FIRST(symbol) to FIRST(nt)
                            for s in self.first_sets.get(symbol, set()) - {'e'}:
                                if s not in self.first_sets[nt]:
                                    self.first_sets[nt].add(s)
                                    changed = True
                            
                            # If symbol doesn't derive epsilon, stop
                            if 'e' not in self.first_sets.get(symbol, set()):
                                all_derive_epsilon = False
                                break
                    
                    # If all symbols in the production can derive epsilon, add epsilon to FIRST set
                    if all_derive_epsilon and 'e' not in self.first_sets[nt]:
                        self.first_sets[nt].add('e')
                        changed = True

    def first_of_string(self, string):
        """Calculate the FIRST set of a string."""
        if not string or string == 'e':
            return {'e'}
        
        result = set()
        all_derive_epsilon = True
        
        for symbol in string:
            if symbol in self.terminals:
                result.add(symbol)
                all_derive_epsilon = False
                break
            else:  # Non-terminal
                # Add all non-epsilon symbols from FIRST(symbol) to result
                for s in self.first_sets.get(symbol, set()) - {'e'}:
                    result.add(s)
                
                # If symbol doesn't derive epsilon, stop
                if 'e' not in self.first_sets.get(symbol, set()):
                    all_derive_epsilon = False
                    break
        
        # If all symbols in the string can derive epsilon, add epsilon to result
        if all_derive_epsilon:
            result.add('e')
        
        return result

    def calculate_follow_sets(self):
        """Calculate the FOLLOW sets for all non-terminals in the grammar."""
        # Initialize FOLLOW sets
        for nt in self.non_terminals:
            self.follow_sets[nt] = set()
        
        # Add $ to FOLLOW(S)
        self.follow_sets[self.start_symbol].add('$')
        
        # Repeat until no changes
        changed = True
        while changed:
            changed = False
            
            for nt, productions in self.grammar.items():
                for prod in productions:
                    if prod == 'e':
                        continue
                    
                    for i, symbol in enumerate(prod):
                        if symbol in self.non_terminals:
                            # Case 2: A -> αBβ, add FIRST(β) - {ε} to FOLLOW(B)
                            if i < len(prod) - 1:
                                beta = prod[i+1:]
                                first_beta = self.first_of_string(beta)
                                
                                for s in first_beta - {'e'}:
                                    if s not in self.follow_sets[symbol]:
                                        self.follow_sets[symbol].add(s)
                                        changed = True
                                
                                # Case 3: A -> αBβ and ε ∈ FIRST(β), add FOLLOW(A) to FOLLOW(B)
                                if 'e' in first_beta:
                                    for s in self.follow_sets[nt]:
                                        if s not in self.follow_sets[symbol]:
                                            self.follow_sets[symbol].add(s)
                                            changed = True
                            
                            # Case 3: A -> αB, add FOLLOW(A) to FOLLOW(B)
                            elif i == len(prod) - 1:
                                for s in self.follow_sets[nt]:
                                    if s not in self.follow_sets[symbol]:
                                        self.follow_sets[symbol].add(s)
                                        changed = True

    def build_ll1_table(self):
        """Build the LL(1) parsing table."""
        self.parse_table_ll1 = {}
        
        # Initialize the table with error entries
        for nt in self.non_terminals:
            self.parse_table_ll1[nt] = {}
            for t in self.terminals:
                self.parse_table_ll1[nt][t] = []
        
        # Fill the table based on the grammar rules
        for nt, productions in self.grammar.items():
            for prod in productions:
                if prod == 'e':  # Production A -> ε
                    for t in self.follow_sets[nt]:
                        if self.parse_table_ll1[nt][t] and self.parse_table_ll1[nt][t] != ['e']:
                            self.is_ll1 = False
                        self.parse_table_ll1[nt][t].append('e')
                else:
                    first_of_prod = self.first_of_string(prod)
                    for t in first_of_prod - {'e'}:
                        if self.parse_table_ll1[nt][t] and self.parse_table_ll1[nt][t] != [prod]:
                            self.is_ll1 = False
                        self.parse_table_ll1[nt][t].append(prod)
                    
                    if 'e' in first_of_prod:
                        for t in self.follow_sets[nt]:
                            if self.parse_table_ll1[nt][t] and self.parse_table_ll1[nt][t] != [prod]:
                                self.is_ll1 = False
                            self.parse_table_ll1[nt][t].append(prod)

    def compute_closure(self, item_set):
        """Compute the closure of a set of LR(0) items."""
        closure = set(item_set)
        changed = True
        
        while changed:
            changed = False
            new_items = set()
            
            for item in closure:
                nt, prod, dot_pos = item
                if dot_pos < len(prod) and prod[dot_pos] in self.non_terminals:
                    next_symbol = prod[dot_pos]
                    for p in self.grammar.get(next_symbol, []):
                        new_item = (next_symbol, p, 0)
                        if new_item not in closure:
                            new_items.add(new_item)
                            changed = True
            
            closure.update(new_items)
        
        return closure

    def compute_goto(self, item_set, symbol):
        """Compute the GOTO function for a set of LR(0) items."""
        goto_set = set()
        
        for item in item_set:
            nt, prod, dot_pos = item
            if dot_pos < len(prod) and prod[dot_pos] == symbol:
                goto_set.add((nt, prod, dot_pos + 1))
        
        return self.compute_closure(goto_set)

    def build_slr1_tables(self):
        """Build the SLR(1) parsing tables (ACTION and GOTO)."""
        # Initialize with augmented grammar
        self.augmented_grammar = {'S\'': ['S']}
        self.augmented_grammar.update(self.grammar)
        
        # Compute canonical collection of LR(0) items
        initial_item = ('S\'', 'S', 0)
        initial_set = self.compute_closure({initial_item})
        self.states = [initial_set]
        
        # Build the states
        state_index = 0
        while state_index < len(self.states):
            state = self.states[state_index]
            
            for symbol in self.terminals.union(self.non_terminals):
                goto_set = self.compute_goto(state, symbol)
                
                if goto_set and goto_set not in self.states:
                    self.states.append(goto_set)
                
                if goto_set:
                    goto_index = self.states.index(goto_set)
                    
                    # Update ACTION and GOTO tables
                    if symbol in self.terminals:
                        if state_index not in self.action_table:
                            self.action_table[state_index] = {}
                        self.action_table[state_index][symbol] = ('shift', goto_index)
                    else:  # Non-terminal
                        if state_index not in self.goto_table:
                            self.goto_table[state_index] = {}
                        self.goto_table[state_index][symbol] = goto_index
            
            state_index += 1
        
        # Add reduce and accept actions
        for i, state in enumerate(self.states):
            for item in state:
                nt, prod, dot_pos = item
                if nt == 'S\'' and prod == 'S' and dot_pos == 1:
                    # Accept action
                    if i not in self.action_table:
                        self.action_table[i] = {}
                    self.action_table[i]['$'] = ('accept', None)
                elif dot_pos == len(prod):
                    # Reduce action
                    for t in self.follow_sets[nt]:
                        if i not in self.action_table:
                            self.action_table[i] = {}
                        
                        if t in self.action_table.get(i, {}):
                            # Conflict detected
                            self.is_slr1 = False
                        else:
                            if nt == 'S\'' and prod == 'S':
                                continue  # Skip reduction for augmented production
                            self.action_table[i][t] = ('reduce', (nt, prod))

    def parse_ll1(self, input_string):
        """Parse the input string using the LL(1) parser."""
        if not self.is_ll1:
            return False
        
        input_string = input_string + '$'
        stack = ['$', self.start_symbol]
        pointer = 0
        
        while stack:
            top = stack[-1]
            current = input_string[pointer]
            
            if top == current:
                stack.pop()
                pointer += 1
                if pointer >= len(input_string):
                    break
            elif top in self.terminals:
                return False  # Error: terminal on stack doesn't match input
            elif top in self.non_terminals:
                if current in self.parse_table_ll1.get(top, {}) and self.parse_table_ll1[top][current]:
                    production = self.parse_table_ll1[top][current][0]
                    stack.pop()
                    
                    if production != 'e':  # Only push if not epsilon
                        for symbol in reversed(production):
                            stack.append(symbol)
                else:
                    return False  # Error: no production rule
            else:
                return False  # Error: unexpected symbol on stack
        
        return pointer >= len(input_string)

    def parse_slr1(self, input_string):
        """Parse the input string using the SLR(1) parser."""
        if not self.is_slr1:
            return False
        
        input_string = input_string + '$'
        stack = [0]  # Start state
        pointer = 0
        
        while True:
            state = stack[-1]
            current = input_string[pointer]
            
            if state in self.action_table and current in self.action_table[state]:
                action, value = self.action_table[state][current]
                
                if action == 'shift':
                    stack.append(current)
                    stack.append(value)
                    pointer += 1
                elif action == 'reduce':
                    nt, prod = value
                    
                    # Pop 2 * |prod| elements (symbol and state for each symbol)
                    if prod != 'e':
                        pop_count = 2 * len(prod)
                        stack = stack[:-pop_count]
                    
                    # Push the non-terminal and its goto state
                    state = stack[-1]
                    if state in self.goto_table and nt in self.goto_table[state]:
                        goto_state = self.goto_table[state][nt]
                        stack.append(nt)
                        stack.append(goto_state)
                    else:
                        return False  # Error: no goto entry
                elif action == 'accept':
                    return True
            else:
                return False  # Error: no action entry
    
    def parse_string(self, input_string, parser_type):
        """Parse the input string using the specified parser."""
        if parser_type == 'T':  # LL(1)
            return self.parse_ll1(input_string)
        elif parser_type == 'B':  # SLR(1)
            return self.parse_slr1(input_string)
        else:
            return False

def main():
    parser = Parser()
    parser.read_grammar()
    parser.calculate_first_sets()
    parser.calculate_follow_sets()
    parser.build_ll1_table()
    parser.build_slr1_tables()
    
    if parser.is_ll1 and parser.is_slr1:
        print("Select a parser (T: for LL(1), B: for SLR(1), Q: quit):")
        
        while True:
            choice = input().strip()
            
            if choice == 'Q':
                break
            elif choice in ['T', 'B']:
                while True:
                    input_string = input().strip()
                    if not input_string:
                        break
                    
                    result = parser.parse_string(input_string, choice)
                    print("yes" if result else "no")
                
                print("Select a parser (T: for LL(1), B: for SLR(1), Q: quit):")
            else:
                print("Invalid choice.")
    
    elif parser.is_ll1:
        print("Grammar is LL(1).")
        
        while True:
            input_string = input().strip()
            if not input_string:
                break
            
            result = parser.parse_ll1(input_string)
            print("yes" if result else "no")
    
    elif parser.is_slr1:
        print("Grammar is SLR(1).")
        
        while True:
            input_string = input().strip()
            if not input_string:
                break
            
            result = parser.parse_slr1(input_string)
            print("yes" if result else "no")
    
    else:
        print("Grammar is neither LL(1) nor SLR(1).")

if __name__ == "__main__":
    main()