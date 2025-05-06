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
