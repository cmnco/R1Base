using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RetailOne.Datos
{
    public class UtilidadesConexion
    {
        public static Origen ObtenerDatosDeCadena(string cadenaConexion, Proveedor proveedor)
        {
            // Escoger el método de extracción según el tipo de proveedor
            if ((short)proveedor < 100)
            {
                return ExtraerDatosSQLServer(cadenaConexion, proveedor);
            }
            else if (proveedor == Proveedor.Hana)
            {
                return ExtraerDatosHana(cadenaConexion, proveedor);
            }
            return new Origen();
        }

        // Método de extracción específico para SQL Server
        private static Origen ExtraerDatosSQLServer(string cadenaConexion, Proveedor proveedor)
        {
            // Definir los patrones para extraer cada campo relevante
            var patrones = new Dictionary<string, string>
            {
                { "Data Source", @"(?i)Data\s*Source\s*=\s*([^;]+)" },
                { "User ID", @"(?i)User\s*ID\s*=\s*([^;]+)" },
                { "Password", @"(?i)Password\s*=\s*([^;]+)" },
                { "Initial Catalog", @"(?i)Initial\s*Catalog\s*=\s*([^;]+)" }
            };

            var valores = ExtraerValores(cadenaConexion, patrones);

            // Asignar valores extraídos al objeto orige
            Origen origen = new Origen(
                valores.ContainsKey("Data Source") ? valores["Data Source"] : "", //Servidor
                valores.ContainsKey("Initial Catalog") ? valores["Initial Catalog"] : "", //Base de datos
                valores.ContainsKey("User ID") ? valores["User ID"] : "sa", //Usuario
                valores.ContainsKey("Password") ? valores["Password"] : "", //Contraseña
                proveedor);

            return origen;
        }

        // Método de extracción específico para SAP HANA
        private static Origen ExtraerDatosHana(string cadenaConexion, Proveedor proveedor)
        {
            // Definir los patrones para extraer cada campo relevante
            var patrones = new Dictionary<string, string>
        {
            { "DRIVER", @"(?i)DRIVER\s*=\s*([^;]+)" },
            { "UID", @"(?i)UID\s*=\s*([^;]+)" },
            { "PWD", @"(?i)PWD\s*=\s*([^;]+)" },
            { "SERVERNODE", @"(?i)SERVERNODE\s*=\s*([^;]+)" },
            { "DATABASE", @"(?i)DATABASE\s*=\s*([^;]+)" },
            { "CS", @"(?i)CS\s*=\s*([^;]+)" },
            { "currentSchema", @"(?i)currentSchema\s*=\s*([^;]+)" },
            { "databaseName", @"(?i)databaseName\s*=\s*([^;]+)" }
        };

            var valores = ExtraerValores(cadenaConexion, patrones);

            // Asignar el nombre de la base de datos (priorizando "CS" o "currentSchema")
            string nombreBD = valores.ContainsKey("CS") && !string.IsNullOrEmpty(valores["CS"])
                ? valores["CS"].Replace("\"", "")
                : valores.ContainsKey("currentSchema")
                    ? valores["currentSchema"].Replace("\"", "")
                    : "";

            // Asignar el tenant
            string tenant = valores.ContainsKey("DATABASE") && !string.IsNullOrEmpty(valores["DATABASE"])
                ? valores["DATABASE"]
                : valores.ContainsKey("databaseName")
                    ? valores["databaseName"]
                    : "";

            // Asignar valores extraídos al objeto origen
            Origen origen = new Origen(
                valores.ContainsKey("SERVERNODE") ? valores["SERVERNODE"] : "", //Servidor
                nombreBD,
                valores.ContainsKey("UID") ? valores["UID"] : "", //Usuario
                valores.ContainsKey("PWD") ? valores["PWD"] : "", //Contraseña
                tenant,
                proveedor);

            return origen;
        }

        // Método auxiliar para extraer valores basados en patrones
        private static Dictionary<string, string> ExtraerValores(string cadenaConexion, Dictionary<string, string> patrones)
        {
            var valores = new Dictionary<string, string>();

            foreach (var campo in patrones)
            {
                // Usar expresión regular para buscar el valor de cada campo
                var match = Regex.Match(cadenaConexion, campo.Value);
                if (match.Success)
                {
                    // Almacenar el valor del campo encontrado, eliminando espacios en blanco
                    valores[campo.Key] = match.Groups[1].Value.Trim();
                }
            }

            return valores;
        }
    }
}
