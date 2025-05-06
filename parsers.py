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
