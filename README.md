# Final_Project_Formal_Languages

## Parser CFG: Analizador de Gramáticas Libres de Contexto

Implementación avanzada de algoritmos para análisis sintáctico de gramáticas libres de contexto, incluyendo analizadores LL(1) y SLR(1).

## Autores

- Jerónimo Restrepo
- Juan Esteban Restrepo

## Descripción

Este proyecto implementa un conjunto completo de herramientas para el análisis sintáctico de gramáticas libres de contexto:

- **Análisis de gramáticas**: Procesamiento de gramáticas y clasificación automática como LL(1), SLR(1), ambas o ninguna
- **Algoritmos fundamentales**: Implementación de cálculo de conjuntos FIRST y FOLLOW
- **Parsers completos**: Analizadores sintácticos LL(1) y SLR(1) totalmente funcionales
- **Detección de conflictos**: Identificación de ambigüedades en tablas de análisis
- **Validación de cadenas**: Verificación de pertenencia de cadenas al lenguaje

El sistema determina automáticamente las características de la gramática proporcionada y aplica el analizador más adecuado.

## Características Técnicas

- **Modular**: Diseño orientado a objetos con separación clara de responsabilidades
- **Eficiente**: Implementación optimizada de algoritmos de análisis sintáctico
- **Robusto**: Manejo apropiado de casos especiales y detección de errores
- **Interactivo**: Interfaz de línea de comandos intuitiva para el usuario

## Requisitos

- **Python**: 3.7 o superior
- **Bibliotecas**: Solo bibliotecas estándar (`sys`, `collections.deque`)
- **Plataformas**: Compatible con Linux, macOS y Windows

## Estructura del Proyecto

```
parser-cfg/
├── src/
│   ├── parser.py          # Implementación principal
│   └── test_parser.py     # Suite de pruebas
├── examples/
│   ├── ll1_grammar.txt    # Ejemplos de gramáticas LL(1)
│   ├── slr1_grammar.txt   # Ejemplos de gramáticas SLR(1)
│   └── strings.txt        # Cadenas de prueba
├── docs/
│   ├── documentation.pdf  # Documentación técnica detallada
│   └── examples.md        # Ejemplos de uso documentados
└── README.md              # Este documento
```

## Instalación

No requiere instalación especial, simplemente clone el repositorio:

```bash
git clone https://github.com/usuario/parser-cfg.git
cd parser-cfg
```

## Uso

### Ejecución Principal

```bash
python src/parser.py
```

### Formato de Entrada

1. **Número de producciones**: La primera línea debe contener un entero `n`
2. **Producciones**: Las siguientes `n` líneas deben contener las producciones en formato:
   ```
   <no-terminal> -> <alternativa1> <alternativa2> ...
   ```

### Convenciones de Gramática

- **Símbolo inicial**: Siempre es 'S'
- **No terminales**: Letras mayúsculas (A-Z)
- **Terminales**: Cualquier otro carácter
- **Cadena vacía (ε)**: Representada como 'e'
- **Fin de cadena**: Todas las cadenas de entrada deben terminar con '$'

### Ejemplo de Interacción

```
Enter the number of productions:
3
Enter 3 productions:
S -> AB
A -> aA d
B -> bBc e

Grammar is LL(1) and SLR(1).
Select a parser (T=LL(1), B=SLR(1), Q=quit):
T
Enter strings to analyze (empty to go back):
adbce$
yes
aaaddbce$
yes
de$
yes

Select a parser (T=LL(1), B=SLR(1), Q=quit):
Q
```

## Ejemplos de Gramáticas

### 1. Gramática LL(1)

```
S -> AB
A -> aA d
B -> bBc e
```

### 2. Gramática SLR(1) (no LL(1))

```
S -> S+T T
T -> T*F F
F -> (S) i
```

### 3. Gramática ni LL(1) ni SLR(1)

```
S -> A
A -> A b
```

## Implementación Técnica

### Clase `Grammar`

Encapsula toda la información y operaciones relacionadas con una gramática:
- Producciones (`dict`): Mapeo de no terminales a sus derivaciones
- Terminales (`set`): Conjunto de símbolos terminales
- No terminales (`set`): Conjunto de símbolos no terminales
- Símbolo inicial (`str`): Primer símbolo de la gramática

### Algoritmos Clave

#### Cálculo de FIRST

```python
def compute_first(grammar):
    """
    Calcula el conjunto FIRST para cada no terminal.
    Implementa el algoritmo iterativo de punto fijo para determinar
    qué terminales pueden aparecer al inicio de las derivaciones.
    """
    # Implementación detallada en parser.py
```

#### Cálculo de FOLLOW

```python
def compute_follow(grammar, first):
    """
    Calcula el conjunto FOLLOW para cada no terminal.
    Determina qué terminales pueden aparecer inmediatamente
    después de un no terminal en las derivaciones válidas.
    """
    # Implementación detallada en parser.py
```

#### Construcción de Tabla LL(1)

```python
def build_ll1_table(grammar, first, follow):
    """
    Construye la tabla de análisis sintáctico LL(1).
    Retorna None si hay conflictos (gramática no es LL(1)).
    """
    # Implementación detallada en parser.py
```

#### Construcción de Tablas SLR(1)

```python
def build_slr_table(grammar, follow):
    """
    Construye las tablas ACTION y GOTO para análisis SLR(1).
    Utiliza los conjuntos de ítems LR(0) y FOLLOW para determinar
    acciones en cada estado.
    """
    # Implementación detallada en parser.py
```

### Análisis Sintáctico

Los métodos `validate_string_ll1` y `validate_string_slr` implementan los algoritmos de análisis:

- **LL(1)**: Análisis descendente basado en pila, dirigido por tabla
- **SLR(1)**: Análisis ascendente shift-reduce basado en autómata LR(0)

## Casos de Uso

- **Educativo**: Aprendizaje de conceptos de compiladores y lenguajes formales
- **Investigación**: Base para experimentos en análisis sintáctico
- **Desarrollo**: Núcleo para implementaciones de compiladores simples
- **Validación**: Comprobación de gramáticas y cadenas de lenguajes formales

## Contribuciones

Las contribuciones son bienvenidas. Para contribuir:

1. Fork del repositorio
2. Crear una rama (`git checkout -b feature/nueva-caracteristica`)
3. Commit de cambios (`git commit -am 'Añadir nueva característica'`)
4. Push a la rama (`git push origin feature/nueva-caracteristica`)
5. Crear Pull Request

## Licencia

Este proyecto está licenciado bajo la Licencia MIT - vea el archivo LICENSE para más detalles.

## Referencias

- Aho, A. V., Lam, M. S., Sethi, R., & Ullman, J. D. (2006). *Compilers: Principles, Techniques, and Tools* (2nd ed.). Pearson Education.
- Cooper, K. D., & Torczon, L. (2011). *Engineering a Compiler* (2nd ed.). Morgan Kaufmann.
- Grune, D., & Jacobs, C. J. (2008). *Parsing Techniques: A Practical Guide* (2nd ed.). Springer Science & Business Media.

---

## Notas de Implementación

### Clase LRItem

La clase `LRItem` representa un ítem LR(0) utilizado en el análisis SLR:

```python
class LRItem:
    """
    Representa un ítem LR(0): A -> α·β
    """
    def __init__(self, lhs, rhs, dot_pos):
        self.lhs = lhs            # No terminal izquierdo
        self.rhs = rhs            # Lado derecho de la producción
        self.dot_pos = dot_pos    # Posición del punto
```

### Funciones de Autómata LR

Las funciones `closure` y `goto` implementan operaciones fundamentales para construir el autómata LR(0):

```python
def closure(items, grammar):
    """Calcula el cierre de un conjunto de ítems LR(0)"""
    # Implementación detallada en parser.py

def goto(I, X, grammar):
    """Calcula el conjunto de ítems alcanzables desde I
    al procesar el símbolo X"""
    # Implementación detallada en parser.py
```


