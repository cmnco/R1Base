# R1Base - Librerías Base de SoluOne

Este repositorio contiene el conjunto de bibliotecas base que definen el estándar de desarrollo en SoluOne, S.A. de C.V. Estas bibliotecas proporcionan funcionalidades fundamentales para los sistemas desarrollados por la empresa, asegurando consistencia, reutilización de código y buenas prácticas.

## 📦 Contenido

El repositorio incluye los siguientes proyectos:

- **RetailOne**: Inicialización del sistema, carga de parámetros y métricas generales desde archivo de configuración (`RetailOne.config`) o de forma programática.
- **RetailOne.Datos**: Conexión y ejecución de comandos SQL, definición de orígenes de datos, consultas y acceso a bases de datos.
- **RetailOne.Utilidades**: Métodos utilitarios para validación, conversión, serialización, encriptación y otras operaciones comunes.

## 🚀 Inicialización del Estándar

### Con archivo `RetailOne.config`

```csharp
static void Main()
{
    if (!RetailOne.Ejecucion.Sistema.Inicializado)
    {
        RetailOne.Ejecucion.Sistema.Inicializacion("~/RetailOne.config");
    }
}
```

### Inicialización mínima sin archivo de configuración (Sin archivo `RetailOne.config`)

```csharp
static void Main()
{
    if (RetailOne.Ejecucion.Sistema.Ambientes.Count() <= 0)
    {
        Origen origen = new Origen("Data Source=localhost;Initial Catalog=Retail One;User ID=sa;Password=1234;", Proveedor.SQLServer);
        RetailOne.Ejecucion.Sistema.Ambientes.Add(new Configuracion.Ambiente("IdAmbiente", "Nombre del ambiente", "es-MX")
        {
            Origen = origen,
        });

        using (Conexion conn = new Conexion())
        {
            if (!conn.ProbarConexion())
                throw new Exception("No fue posible establecer conexión con la base de datos.");
        }
    }
}
```
## 👥 Dirigido a
Este repositorio está diseñado para el equipo de desarrollo de SoluOne, sirviendo como referencia y base técnica para los proyectos actuales y futuros.

## 🛠️ Compilación
Este proyecto se compila desde la solución RetailOneBase.sln, compatible con Visual Studio. Asegúrate de tener la versión adecuada del SDK .NET instalada:
Versión mínima requerida: .NET Framework 4.5.1. 
Versión recomendada: .NET Framework 4.8.1.

## 📐 Arquitectura
Este proyecto se compone de tres librerías con relaciones claras de dependencia.
Consulta el archivo [arquitectura.md](./docs/arquitectura.md) para ver:
- Relación entre las librerías.
- Flujo completo de ejecución desde la configuración hasta la consulta.
- Estructura del archivo RetailOne.config.

## 🧰 Estándar de Código
Se utiliza un estándar interno de nomenclatura y estructuras. Ver detalles completos en [estandar-codigo.md](./docs/estandar-codigo.md):
- Convención de nombres para clases, métodos y variables.
- Reglas de manejo de errores.
- Patrones típicos para acceso a datos.
- Uso de métodos de extensión y expresiones modernas.


## 📝 Licencia
Este repositorio es de uso interno exclusivo de SoluOne, S.A. de C.V. Todos los derechos reservados. Avisos de privacidad
---
