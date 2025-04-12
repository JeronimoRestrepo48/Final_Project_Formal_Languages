# Proyecto de Lenguajes Formales y Compiladores

Implementación de algoritmos para computar los conjuntos FIRST y FOLLOW, así como analizadores sintácticos LL(1) y SLR(1).

## Integrantes del Grupo

- [Nombre del Estudiante 1]
- [Nombre del Estudiante 2]
- [Nombre del Estudiante 3] (opcional)

## Requisitos del Sistema

- **Sistema operativo**: Linux, Windows o macOS
- **Versión de Python**: Python 3.7 o superior
- **Bibliotecas requeridas**: No se requieren bibliotecas externas

## Instrucciones de Uso

### Compilación y Ejecución

Este proyecto está desarrollado en Python y no requiere compilación. Para ejecutarlo, siga los siguientes pasos:

1. Asegúrese de tener Python 3.7 o una versión superior instalada.
2. Descargue o clone el repositorio.
3. Abra una terminal en el directorio del proyecto.
4. Ejecute el analizador con el comando:

```bash
python parser.py
```

### Formato de Entrada

El programa espera un formato de entrada específico:

1. La primera línea contiene un número entero `n` que representa la cantidad de no terminales en la gramática.
2. Las siguientes `n` líneas contienen las producciones de la gramática en el formato:
   ```
   <no terminal> -> <alternativas de derivación separadas por espacios>
   ```

### Formato de Salida

El programa determina si la gramática es LL(1), SLR(1), ambas o ninguna, y procesa las cadenas de entrada según corresponda:

- Si la gramática es LL(1) y SLR(1), el programa permite elegir el tipo de parser.
- Si la gramática es solo LL(1) o solo SLR(1), el programa utiliza el parser disponible.
- Si la gramática no es LL(1) ni SLR(1), el programa informa esta situación.

Para cada cadena de entrada, el programa responde con:
- `yes` si la cadena pertenece al lenguaje generado por la gramática.
- `no` si la cadena no pertenece al lenguaje generado por la gramática.

### Ejemplos de Uso

#### Ejemplo 1 (Solo SLR(1))
```
3
S -> S+T T
T -> T*F F
F -> (S) i
```

#### Ejemplo 2 (LL(1) y SLR(1))
```
3
S -> AB
A -> aA d
B -> bBc e
```

#### Ejemplo 3 (Ni LL(1) ni SLR(1))
```
2
S -> A
A -> A b
```

## Pruebas

El proyecto incluye un script de pruebas que verifica el correcto funcionamiento del analizador con los ejemplos proporcionados en la especificación. Para ejecutar las pruebas:

```bash
python test_parser.py
```

## Estructura del Proyecto

- `parser.py`: Implementación principal del analizador sintáctico.
- `test_parser.py`: Script de pruebas automatizadas.
- `documentation.pdf`: Documentación detallada del proyecto.
- `README.md`: Este archivo con instrucciones de uso.

## Detalles de Implementación

### Algoritmos Implementados

1. **Cálculo de conjuntos FIRST**:
   - Implementa el algoritmo para calcular los conjuntos FIRST para todos los símbolos de la gramática.

2. **Cálculo de conjuntos FOLLOW**:
   - Implementa el algoritmo para calcular los conjuntos FOLLOW para todos los no terminales de la gramática.

3. **Construcción de tabla LL(1)**:
   - Implementa el algoritmo para construir la tabla de análisis sintáctico LL(1).
   - Detecta conflictos que indicarían que la gramática no es LL(1).

4. **Construcción de tabla SLR(1)**:
   - Implementa el algoritmo para construir las tablas ACTION y GOTO para el análisis SLR(1).
   - Detecta conflictos que indicarían que la gramática no es SLR(1).

5. **Análisis sintáctico**:
   - Implementa los algoritmos de análisis sintáctico LL(1) y SLR(1) para determinar si una cadena pertenece al lenguaje.

## Notas Adicionales

- El símbolo inicial de la gramática siempre es 'S'.
- Los no terminales son letras mayúsculas.
- Los terminales no son letras mayúsculas.
- La cadena vacía (ε) se representa con la letra 'e'.
- Todas las cadenas de entrada terminan con el símbolo '$'.
- Todos los no terminales producen alguna cadena.