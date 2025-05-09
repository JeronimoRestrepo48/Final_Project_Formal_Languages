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

# ====================== LL(1) PARSER ======================
def build_ll1_table(grammar, first, follow):
    """
    Construye la tabla LL(1) M[A, a] = producción.
    Devuelve None si detecta un conflicto.
    """
    table = {}
    for A, rhss in grammar.productions.items():
        for rhs in rhss:
            # Calcular FIRST(rhs)
            first_alpha = set()
            for sym in rhs:
                if sym == 'e':
                    first_alpha.add('e'); break
                if not sym.isupper():
                    first_alpha.add(sym); break
                # Si es no terminal, añadimos FIRST(sym)\{e}
                for t in first[sym]:
                    if t != 'e':
                        first_alpha.add(t)
                if 'e' not in first[sym]:
                    break
            # Para cada terminal t en FIRST(rhs)\{e}, M[A,t]=rhs
            for t in first_alpha - {'e'}:
                key = (A, t)
                if key in table:
                    return None
                table[key] = (A, rhs)
            # Si ε ∈ FIRST(rhs), para cada b ∈ FOLLOW(A), M[A,b]=A->ε
            if 'e' in first_alpha:
                for b in follow[A]:
                    key = (A, b)
                    if key in table:
                        return None
                    table[key] = (A, 'e')
    return table

def validate_string_ll1(s, grammar, table):
    """
    Valida si la cadena s pertenece al lenguaje usando LL(1).
    Returns True/False.
    """
    # Pila: [$, símbolo inicial]
    stack = ['$', grammar.start_symbol]
    # Entrada: lista de símbolos + ['$']
    input_syms = list(s) + ['$']
    idx = 0

    while stack:
        top = stack.pop()
        cur = input_syms[idx]
        # Si coincide, avanzar en entrada
        if top == cur:
            idx += 1
            if top == '$':
                return True
            continue
        # Si no coincide, buscar producción en la tabla
        if (top, cur) not in table:
            return False
        _, rhs = table[(top, cur)]
        # Si producción es ε, solo descartar
        if rhs == 'e':
            continue
        # Apilar RHS al revés
        for sym in reversed(rhs):
            stack.append(sym)
    return False

# ====================== SLR(1) PARSER ======================

class LRItem:
    """
    Representa un ítem LR(0): A -> α·β
    dot_pos indica la posición del punto.
    """
    def __init__(self, lhs, rhs, dot_pos):
        self.lhs = lhs            # No terminal A
        self.rhs = rhs            # Cadena β como RHS
        self.dot_pos = dot_pos    # Posición del punto

    def next_symbol(self):
        """Devuelve el símbolo a la derecha del punto, o None."""
        if self.dot_pos < len(self.rhs):
            return self.rhs[self.dot_pos]
        return None

    def is_complete(self):
        """True si el punto está al final (A -> α·)."""
        return self.dot_pos == len(self.rhs)

    def advance_dot(self):
        """Devuelve nuevo ítem con el punto avanzado en uno."""
        return LRItem(self.lhs, self.rhs, self.dot_pos + 1)

    def __eq__(self, other):
        return (self.lhs, self.rhs, self.dot_pos) == (other.lhs, other.rhs, other.dot_pos)

    def __hash__(self):
        return hash((self.lhs, tuple(self.rhs), self.dot_pos))

    def __repr__(self):
        # Formato: A → α·β
        before = ''.join(self.rhs[:self.dot_pos])
        after = ''.join(self.rhs[self.dot_pos:])
        return f"{self.lhs} → {before}·{after}"

def closure(items, grammar, first=None):
    """
    Cálculo de cierre LR(0) para un conjunto de ítems.
    items: lista de LRItem
    Devuelve: lista de ítems en el cierre.
    """
    C = set(items)
    queue = deque(items)
    while queue:
        item = queue.popleft()
        X = item.next_symbol()
        # Si X es no terminal y el ítem no está completo
        if X and X in grammar.non_terminals:
            for rhs in grammar.productions.get(X, []):
                nuevo = LRItem(X, rhs, 0)
                if nuevo not in C:
                    C.add(nuevo)
                    queue.append(nuevo)
    return list(C)

def goto(I, X, grammar):
    """
    Función GOTO: lleva el conjunto I al símbolo X.
    Devuelve el cierre de los ítems resultantes o [].
    """
    moved = []
    for item in I:
        if item.next_symbol() == X:
            moved.append(item.advance_dot())
    return closure(moved, grammar)

def build_slr_table(grammar, follow):
    """
    Construye las tablas ACTION y GOTO para SLR(1).
    Devuelve (action_table, goto_table), o (None,None) si hay conflicto.
    """
    # Gramática aumentada y estado inicial
    G_aug = grammar.get_augmented_grammar()
    start_rhs = G_aug.productions[G_aug.start_symbol][0]
    start_item = LRItem(G_aug.start_symbol, start_rhs, 0)

    # C = conjunto de estados (ítems) inicial
    C = [closure([start_item], G_aug)]
    state_map = {tuple(sorted(C[0], key=str)): 0}
    transitions = {}
    action, goto_t = {}, {}
    queue = deque([0])

    # Construir el autómata LR(0)
    while queue:
        i = queue.popleft()
        I = C[i]
        for X in list(grammar.non_terminals) + list(grammar.terminals):
            J = goto(I, X, G_aug)
            if not J: continue
            key = tuple(sorted(J, key=str))
            if key not in state_map:
                state_map[key] = len(C)
                C.append(J)
                queue.append(len(C)-1)
            j = state_map[key]
            transitions.setdefault(i, {})[X] = j

        # Rellenar ACTION y GOTO
        for item in I:
            if not item.is_complete():
                a = item.next_symbol()
                if a in grammar.terminals:
                    j = transitions[i][a]
                    if (i, a) in action: return None, None
                    action[(i,a)] = ('s', j)
            else:
                if item.lhs == G_aug.start_symbol:
                    action[(i,'$')] = ('acc', None)
                else:
                    for a in follow[item.lhs]:
                        if (i,a) in action: return None, None
                        action[(i,a)] = ('r', (item.lhs, item.rhs))
        # GOTO en no terminales
        for A in grammar.non_terminals:
            if (i, A) in transitions.get(i, {}):
                goto_t[(i, A)] = transitions[i][A]

    return action, goto_t

def validate_string_slr(s, grammar, action, goto_t):
    """
    Valida si la cadena s pertenece al lenguaje usando SLR(1).
    Returns True/False.
    """
    stack = [0]                # Pila de estados
    input_syms = list(s) + ['$']
    idx = 0

    while True:
        state = stack[-1]
        a = input_syms[idx]
        if (state, a) not in action:
            return False
        act, data = action[(state, a)]
        if act == 's':
            # shift: apilar estado y avanzar en la entrada
            stack.append(data)
            idx += 1
        elif act == 'r':
            # reduce: desapilar |β| estados y apilar GOTO
            A, β = data
            if β != 'e':
                for _ in β: stack.pop()
            state2 = stack[-1]
            stack.append(goto_t[(state2, A)])
        else:
            # accept
            return True
