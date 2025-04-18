# Estándar de Codificación - SoluOne

Este documento define las convenciones de codificación utilizadas por el equipo de desarrollo de SoluOne, S.A. de C.V. para los proyectos basados en la biblioteca R1Base.

---

## 🔠 Convenciones de Nombres

### 📌 Reglas generales

- Se utiliza **notación PascalCase** para nombres de:
  - Clases
  - Interfaces
  - Enumeraciones
  - Métodos
  - Propiedades

- Se utiliza **notación camelCase** para:
  - Variables
  - Atributos
  - Parámetros

### 📌 Estilo de nombres

- Idioma: **Español**
- Nombres legibles, gramaticalmente correctos y funcionales.
- El nombre debe reflejar **el propósito o comportamiento**, no el tipo.
- **Evitar abreviaturas** y el uso de números en los nombres.
- **No repetir el nombre de una entidad** (clase o enumeración) en sus miembros.
- Los métodos deben tener nombres descriptivos.

### 📌 Métodos de verificación

Se utilizan frases afirmativas, similares al estilo del .NET Framework:

- `EsNumeroEntero()`
- `EsAlmacenColectivo()`
- `EsUsuarioActivo()`
- `ContieneFolio(folio)`

---

## 🧱 Convenciones para Base de Datos

- Tablas de usuario: prefijo `@SO1_01`, `@SO1_02`, etc.
  - `@SO1` es el espacio de nombres de SoluOne.
  - `_01`, `_02` indica el número de proyecto.
- Campos de usuario: prefijo `U_SO1_`.
- Todos los nombres de tablas y campos deben estar en **MAYÚSCULAS**.
- La longitud de nombres de tablas y campos **no debe exceder los 19 caracteres**.

---

## ⚠️ Manejo de Errores y Excepciones

- Las excepciones se propagan hacia capas superiores con información relevante (parámetros de entrada, contexto).
- En la capa más externa se gestiona el error (registro en logs, notificación, etc.).
- Para errores de validación se lanza una `ExcepcionControlada` (de `RetailOne.Utilidades`) con mensajes orientados al usuario final.

---

## 🗃️ Acceso a Datos

### 📌 Convención de nombres para métodos de acceso

| Situación                         | Nombre de método                  |
|-----------------------------------|-----------------------------------|
| Un único identificador            | `Obtener()`                       |
| Múltiples criterios específicos   | `ObtenerPorId()`, `ObtenerPorFolio()` |
| Todos los elementos               | `Listar()`                        |
| Lista filtrada por criterios      | `Filtrar()`                       |

---

## 🧩 Métodos de Extensión

- Se usan donde mejoran la legibilidad y reutilización.
- Son comunes en vistas o validaciones internas de modelos.

### 📌 Ejemplos

```csharp
"1,250.3".CambiarATipo<decimal>(CultureInfo.InvariantCulture);
correo.ValidarEmail();
edad.ValidarEntero();
```
## ✅ Buenas Prácticas Permitidas
- Uso de expresiones lambda
- Uso de LINQ para manipulación de colecciones
- Soporte completo para async/await

## 📚 Organización del Código
- Separar lógicamente métodos y regiones dentro de las clases.
- Documentar con comentarios XML (///) cuando se trate de métodos públicos o de uso compartido.
- Mantener un solo propósito por clase y por método (principio de responsabilidad única).

## ✅ Consideraciones generales
- Documentación apropiada de los métodos y propiedades.
- Evitar la implementación de métodos demasiado extensos (de preferencia que el cuerpo del método se pueda visualizar por completo en pantalla).  
- Respetar márgenes y estructura jerárquica del código fuente.
- Evitar exceso de espacios en blanco o líneas vacías.
- Evitar exceso de código comentado o inutilizado.
- Evitar uso excesivo de parámetros en los métodos.
- Evitar el uso de parámetros “out” o “ref” para la modificación de variables dentro de métodos.
- Uso apropiado de tipos de datos (No todo puede ser object o string).
- Evitar declaración de variables dentro de ciclos.
- Evitar concatenación de cadenas. En su lugar, hacer uso de interpolación o StringBuilder para textos mas extensos.
---






