#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
parser.py

Implementación de un analizador sintáctico para gramáticas libres de contexto.
Soporta:
 - Cálculo de FIRST y FOLLOW
 - Tabla LL(1)
 - Parser LL(1)
 - Tabla SLR(1)
 - Parser SLR(1)

Uso: python parser.py
"""

import sys
from collections import deque

# ====================== CLASE GRAMÁTICA ======================
class Grammar:
    """
    Representa una gramática libre de contexto.
    Almacena producciones, terminales, no terminales y símbolo inicial.
    """
    def __init__(self):
        # Diccionario: LHS -> lista de RHS (cada RHS es una cadena)
        self.productions = {}  
        self.terminals = set()       # Conjunto de símbolos terminales
        self.non_terminals = set()   # Conjunto de símbolos no terminales
        self.start_symbol = 'S'      # Símbolo inicial por defecto
    
    def add_production(self, line):
        """
        Parsea una línea "A -> α β ..." y la añade a self.productions.
        También actualiza terminales y no terminales.
        """
        lhs, rhs_str = line.split('->')
        lhs = lhs.strip()                # No terminal a la izquierda
        rhs_list = rhs_str.strip().split()  # Lista de alternativas

        # Inicializar lista de producciones para lhs si no existe
        if lhs not in self.productions:
            self.productions[lhs] = []
        # Añadir cada alternativa (como cadena) al diccionario
        self.productions[lhs].extend(rhs_list)

        # Registrar lhs como no terminal
        self.non_terminals.add(lhs)

        # Recorrer cada producción para clasificar símbolos
        for prod in rhs_list:
            for sym in prod:
                if sym == 'e':
                    # 'e' representa epsilon; no es terminal
                    continue
                elif sym.isupper():
                    # Letras mayúsculas son no terminales
                    self.non_terminals.add(sym)
                else:
                    # Resto de caracteres son terminales
                    self.terminals.add(sym)

        # Epsilon y marcador de fin '$' no son terminales
        self.terminals.discard('e')
        self.terminals.discard('$')
    def get_productions(self):
        """Devuelve el diccionario de producciones."""
        return self.productions

    def get_terminals(self):
        """Devuelve la lista de terminales."""
        return list(self.terminals)

    def get_non_terminals(self):
        """Devuelve la lista de no terminales."""
        return list(self.non_terminals)
     
    def get_augmented_grammar(self):
        """
        Construye y devuelve una gramática aumentada:
        - Nuevo símbolo inicial S' -> S
        - Copia de las producciones originales
        """
        # Generar un nuevo símbolo inicial único (S', S'', ...)
        new_start = self.start_symbol + "'"
        while new_start in self.non_terminals:
            new_start += "'"
        # Agrupar producciones: la nueva y luego las originales
        augmented = {
            new_start: [self.start_symbol],
            **self.productions.copy()
        }
        # Crear instancia de gramática aumentada
        g = Grammar()
        g.start_symbol = new_start
        g.productions = augmented
        # Rellenar terminales y no terminales en la nueva gramática
        for lhs, rhss in augmented.items():
            for rhs in rhss:
                for sym in rhs:
                    if sym.isupper():
                        g.non_terminals.add(sym)
                    elif sym != 'e':
                        g.terminals.add(sym)
        return g
     
# ====================== FIRST Y FOLLOW ======================
def compute_first(grammar):
    """
    Calcula el conjunto FIRST para cada no terminal de la gramática.
    Returns: dict nt -> set de terminales (y 'e' si puede producir epsilon)
    """
    # Inicializar FIRST(nt) = ∅ para cada no terminal
    first = {nt: set() for nt in grammar.get_non_terminals()}
    changed = True

    # Iterar hasta estabilizar los conjuntos
    while changed:
        changed = False
        # Para cada producción A -> α
        for A, rhss in grammar.productions.items():
            for rhs in rhss:
                # Recorrer símbolos en α de izquierda a derecha
                for i, sym in enumerate(rhs):
                    if sym == 'e':
                        # Epsilon en RHS => FIRST(A) incluye 'e'
                        if 'e' not in first[A]:
                            first[A].add('e')
                            changed = True
                        break
                    elif not sym.isupper():
                        # Terminal => FIRST(A) incluye sym
                        if sym not in first[A]:
                            first[A].add(sym)
                            changed = True
                        break
                    else:
                        # No terminal B => FIRST(A) incluye FIRST(B) \ {e}
                        for t in first[sym]:
                            if t != 'e' and t not in first[A]:
                                first[A].add(t)
                                changed = True
                        # Si FIRST(B) no contiene ε, detenerse
                        if 'e' not in first[sym]:
                            break
                        # Si llegamos al final y todos generan ε
                        if i == len(rhs) - 1:
                            if 'e' not in first[A]:
                                first[A].add('e')
                                changed = True
    return first

def compute_follow(grammar, first):
    """
    Calcula el conjunto FOLLOW para cada no terminal.
    Parámetros:
     - grammar: instancia Grammar
     - first: diccionario FIRST calculado previamente
    Returns: dict nt -> set de terminales (y '$' para el inicial)
    """
    # Inicializar FOLLOW(nt) = ∅ para cada no terminal
    follow = {nt: set() for nt in grammar.get_non_terminals()}
    # El símbolo inicial contiene '$' en su FOLLOW
    follow[grammar.start_symbol].add('$')
    changed = True

    # Iterar hasta estabilizar
    while changed:
        changed = False
        # Para cada producción A -> α
        for A, rhss in grammar.productions.items():
            for rhs in rhss:
                # Recorrer cada posición B en α
                for i, B in enumerate(rhs):
                    if B in grammar.non_terminals:
                        # Obtener la "restante" después de B
                        rest = rhs[i+1:]
                        has_eps = True
                        # Recorrer cada símbolo X tras B
                        for X in rest:
                            if X == 'e':
                                continue
                            if not X.isupper():
                                # X terminal => FOLLOW(B) incluye X
                                if X not in follow[B]:
                                    follow[B].add(X)
                                    changed = True
                                has_eps = False
                                break
                            else:
                                # X no terminal => FOLLOW(B) incluye FIRST(X)\{e}
                                for t in first[X]:
                                    if t != 'e' and t not in follow[B]:
                                        follow[B].add(t)
                                        changed = True
                                if 'e' not in first[X]:
                                    has_eps = False
                                    break
                        # Si todos los símbolos a la derecha pueden producir ε
                        if has_eps:
                            # FOLLOW(B) incluye FOLLOW(A)
                            for t in follow[A]:
                                if t not in follow[B]:
                                    follow[B].add(t)
                                    changed = True
    return follow

