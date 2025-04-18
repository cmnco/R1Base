# Arquitectura de la Solución - R1Base

Este documento describe la estructura y la relación entre las bibliotecas que conforman la solución base de SoluOne, S.A. de C.V., así como el flujo típico de ejecución de un sistema que las utiliza.

---

## 🧱 Relación entre Proyectos

La solución está compuesta por tres bibliotecas tipo Class Library (librerías de clases) que siguen una relación jerárquica de dependencia:

```plaintext
RetailOne (depende de Datos y Utilidades)
│
├── RetailOne.Datos (depende de Utilidades)
│   └── RetailOne.Utilidades (independiente)
```
- RetailOne.Utilidades: Biblioteca independiente que contiene extensiones, validaciones, serialización, compresión, encriptación, y herramientas comunes.
- RetailOne.Datos: Biblioteca que implementa lógica para acceso a base de datos, definiciones de consultas, orígenes de datos, y conexión.
- RetailOne: Biblioteca principal que orquesta la inicialización del sistema, carga del archivo de configuración y otros parámetros globales. Depende de las dos anteriores.

## 📦 Uso por parte de Aplicaciones Cliente
Las aplicaciones cliente que deseen usar esta solución deben referenciar las tres bibliotecas:
- RetailOne.dll
- RetailOne.Datos.dll
- RetailOne.Utilidades.dll

Estas deben estar disponibles en la solución cliente, ya sea mediante referencia directa, publicación conjunta o como paquetes internos.

##⚙️ Flujo de Ejecución Típico
A continuación se muestra un flujo básico desde la inicialización hasta una consulta a la base de datos:

```csharp
public static void Main(string[] args) 
{
    // Inicialización del estándar, mediante el archivo RetailOne.config.
    if (!RetailOne.Ejecucion.Sistema.Inicializado)
    {
        RetailOne.Ejecucion.Sistema.Inicializacion("~/RetailOne.config");
    }

    using (Conexion conexion = new Conexion())
    {
        // Verificar si existe un folio
        conexion.CargarConsulta(ConsultasUtiles.ExisteFolio);
        conexion.AgregarParametro("@pFolio", "CH1001CA1234");
        bool existe = conexion.EjecutarEscalar<string>().Equals("Y");

        // Consultar encabezado de una venta
        conexion.CargarConsulta(ConsultasUtiles.ConsultarEncabezadoVenta);
        conexion.AgregarParametro("@pFolio", "CH1001CA1234");
        conexion.EjecutarLector();

        NotaCaja notaCaja = null;

        if (conexion.LeerFila())
        {
            notaCaja = new NotaCaja()
            {
                Folio = conexion.DatoLector<string>("Name"),
                Fecha = conexion.DatoLector<DateTime>("U_SO1_FECHA"),
                Total = conexion.DatoLector<decimal>("U_SO1_TOTALNETO")
            };
        }

        conexion.CerrarLector();
    }
}

internal class ConsultasUtiles
{
    internal static Consulta ExisteFolio = new Consulta(Proveedor.SQLServer,
        new ComandoConsulta(Proveedor.SQLServer, @"SELECT CASE WHEN EXISTS(SELECT [Code] FROM [@SO1_01VENTA] WHERE [Name] = @pFolio) THEN 'Y' ELSE 'N' END;"),
        new ComandoConsulta(Proveedor.Hana, @"SELECT CASE WHEN EXISTS(SELECT ""Code"" FROM ""@SO1_01VENTA"" WHERE ""Name"" = @pFolio) THEN 'Y' ELSE 'N' END FROM DUMMY;"));

    internal static Consulta ConsultarEncabezadoVenta = new Consulta(Proveedor.SQLServer,
        new ComandoConsulta(Proveedor.SQLServer, @"SELECT * FROM [@SO1_01VENTA] WHERE [Name] = @pFolio;"));
}

```
##🧾 Estructura del Archivo RetailOne.config
El archivo RetailOne.config debe encontrarse en el directorio raíz del proyecto o del entorno de publicación. Define la configuración inicial del sistema.

###📌 Ejemplo de estructura:
```XML
<?xml version="1.0" encoding="utf-8" ?>
<RetailOne xmlns="http://www.soluone.com/esquemas/retailone/config">
  <DatosImplementacion NombreSistema="RetailOne" Version="2.00" Abreviatura="R1" ModoDepuracion="true"/>
  <Globalizacion>
    <Cultura Codigo="es-MX" Nombre="Español" SeparadorMiles="," SeparadorDecimal="." CantidadDecimales="2" FormatoFechaCorta="dd/MM/yyyy" FormatoFechaLarga="dddd, d 'de' MMMM 'de' yyyy" FormatoHora="hh:mm tt" HusoHorario="-5"/>
    <Cultura Codigo="en-US" Nombre="Inglés" SeparadorMiles="," SeparadorDecimal="." CantidadDecimales="2" FormatoFechaCorta="MM.dd.yyyy" FormatoFechaLarga="dddd, MMMM d yyyy" FormatoHora="HH:mm" HusoHorario="-4.5"/>
  </Globalizacion>
  <Ambientes AmbientePredeterminado="PROD">
    <Ambiente ID="PROD" Nombre="RetailOne.PROD" IdiomaPredeterminado="es-MX">
      <OrigenDatos Nombre="Principal" CadenaConexion="Data Source=localhost;Initial Catalog=Retail One;User ID=sa;Password=1234;Persist Security Info=False;Integrated Security=false;" ProveedorDatos="SQLServer2014"/>
    </Ambiente>
  </Ambientes>
  <VariablesConfiguracion>
    <Variable ID="VersionSAP" Value="910000"/>
    <Variable ID="ActivaFiliales" Value="false"/>
    <Variable ID="AsignacionAutomaticaNumeroDeSerieExistente" Value="true"/>
    <Variable ID="AsignacionAutomaticaNumeroDeLoteExistente" Value="true"/>
    <Variable ID="RutaArchivosSalida" Value="~/Temporal"/>
    <Variable ID="ServicioWeb" Value="http://192.168.0.60:81/retailOneChile/SO1_SW_RetailOne.asmx"/>
  </VariablesConfiguracion>
</RetailOne>

```
## ✅ Recomendaciones
- Asegurar que las referencias entre librerías estén correctamente configuradas.
- Validar el archivo RetailOne.config antes de publicación.
- Evitar hardcodear cadenas de conexión en ambientes productivos. Se recomienda cifrado o configuración externa segura.
