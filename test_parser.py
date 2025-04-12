#!/usr/bin/env python3

from parser import Parser
import sys
from io import StringIO

def run_test(grammar, inputs, expected_outputs):
    """Run test with the given grammar and inputs."""
    # Save original stdin and stdout
    original_stdin = sys.stdin
    original_stdout = sys.stdout
    
    # Create string IO objects to simulate stdin and capture stdout
    fake_stdin = StringIO('\n'.join(grammar + inputs))
    fake_stdout = StringIO()
    
    sys.stdin = fake_stdin
    sys.stdout = fake_stdout
    
    try:
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
    
    finally:
        # Restore original stdin and stdout
        sys.stdin = original_stdin
        sys.stdout = original_stdout
    
    # Get output and check against expected
    output = fake_stdout.getvalue().strip().split('\n')
    
    if output == expected_outputs:
        print(f"Test passed!")
    else:
        print(f"Test failed:")
        print(f"Expected: {expected_outputs}")
        print(f"Got: {output}")

def test_example1():
    """Test Example 1 from the assignment."""
    grammar = [
        "3",
        "S -> S+T T",
        "T -> T*F F",
        "F -> (S) i"
    ]
    inputs = [
        "i+i",
        "(i)",
        "(i+i)*i)",
        ""
    ]
    expected_outputs = [
        "Grammar is SLR(1).",
        "yes",
        "yes",
        "no"
    ]
    
    print("Running Example 1 test...")
    run_test(grammar, inputs, expected_outputs)

def test_example2():
    """Test Example 2 from the assignment."""
    grammar = [
        "3",
        "S -> AB",
        "A -> aA d",
        "B -> bBc e"
    ]
    inputs = [
        "T",
        "d",
        "adbc",
        "a",
        "",
        "Q"
    ]
    expected_outputs = [
        "Select a parser (T: for LL(1), B: for SLR(1), Q: quit):",
        "yes",
        "yes",
        "no",
        "Select a parser (T: for LL(1), B: for SLR(1), Q: quit):"
    ]
    
    print("Running Example 2 test...")
    run_test(grammar, inputs, expected_outputs)

def test_example3():
    """Test Example 3 from the assignment."""
    grammar = [
        "2",
        "S -> A",
        "A -> A b"
    ]
    inputs = []
    expected_outputs = [
        "Grammar is neither LL(1) nor SLR(1)."
    ]
    
    print("Running Example 3 test...")
    run_test(grammar, inputs, expected_outputs)

if __name__ == "__main__":
    test_example1()
    test_example2()
    test_example3()