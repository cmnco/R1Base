using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using RetailOne.Configuracion;
using RetailOne.Datos;

namespace RetailOne.Ejecucion
{
    public static partial class Sistema
    {
        public static void Agregar(Ambiente ambiente)
        {
            if (ambiente == null)
                throw new ArgumentNullException("La referencia del nuevo ambiente de datos no puede ser nula");

            if (string.IsNullOrEmpty(ambiente.Origen.CadenaConexion))
                throw new ArgumentNullException("No se han establecido los parámetros de conexión a la base de datos");

            if (Ambientes.Contains(ambiente.ID))
            {
                Ambientes.Remove(ambiente.ID);
                Ambientes.Add(ambiente);
            }
            else
            {
                Ambientes.Add(ambiente);
            }
        }

        public static void GuardarConfiguracion()
        {
            GuardarConfiguracion(null, null);
        }

        public static void GuardarConfiguracion(string rutaArchivoConfiguracion)
        {
            GuardarConfiguracion(null, rutaArchivoConfiguracion);
        }

        public static void GuardarConfiguracion(XElement xElement, string rutaArchivoConfiguracion)
        {
            if (string.IsNullOrEmpty(rutaArchivoConfiguracion))
            {
                if (string.IsNullOrEmpty(DireccionArchivoConfiguracion))
                {
                    InicializarDireccionesRaiz("~/RetailOne.config");
                }
            }
            else
            {
                InicializarDireccionesRaiz(rutaArchivoConfiguracion);
            }

            GuardarArchivoConfiguracion(xElement);
        }

        private static void GuardarArchivoConfiguracion(XElement xElement)
        {
            if(EspacioNombresArchivoConfiguracion == null)
                EspacioNombresArchivoConfiguracion = NsConfiguración;

            if (ContenidoArchivoConfiguracion == null)
                ContenidoArchivoConfiguracion = ObtenerXMLPorDefecto();

            XElement ambientes = ContenidoArchivoConfiguracion.Element(EspacioNombresArchivoConfiguracion + "Ambientes");

            ambientes.RemoveNodes();
            ambientes.Add(Environment.NewLine);

            foreach (Ambiente ambiente in Ambientes)
            {
                //XElement xambiente = new XElement(EspacioNombresArchivoConfiguracion + "Ambiente",
                //new XAttribute("ID", ambiente.ID),
                //new XAttribute("Nombre", ambiente.Nombre),
                //new XAttribute("IdiomaPredeterminado", ambiente.IdiomaPredeterminado.Código),
                //new XElement(EspacioNombresArchivoConfiguracion + "OrigenDatos",
                //    new XAttribute("Servidor", ambiente.Origen.ServidorBD),
                //    new XAttribute("NombreBD", ambiente.Origen.NombreBD),
                //    new XAttribute("Usuario", ambiente.Origen.UsuarioDB),
                //    new XAttribute("Contraseña", Utilidades.Encriptacion.ConvertirABase64(ambiente.Origen.ContraseñaDB)),
                //    new XAttribute("ProveedorDatos", ambiente.Origen.Proveedor)));

                //XElement xambiente = new XElement(EspacioNombresArchivoConfiguracion + "Ambiente",
                //new XAttribute("ID", ambiente.ID),
                //new XAttribute("Nombre", ambiente.Nombre),
                //new XAttribute("IdiomaPredeterminado", ambiente.IdiomaPredeterminado.Código),
                //new XElement(EspacioNombresArchivoConfiguracion + "OrigenDatos",
                //    new XAttribute("CadenaConexion", RetailOne.Utilidades.Encriptacion.ConvertirABase64(ambiente.Origen.CadenaConexion)),
                //    new XAttribute("ProveedorDatos", ambiente.Origen.Proveedor)));

                XElement xambiente = new XElement(EspacioNombresArchivoConfiguracion + "Ambiente",
                    new XAttribute("ID", ambiente.ID),
                    new XAttribute("Nombre", ambiente.Nombre),
                    new XAttribute("IdiomaPredeterminado", ambiente.IdiomaPredeterminado.Codigo));
                xambiente.Add(Environment.NewLine);

                xambiente.Add(new XElement(EspacioNombresArchivoConfiguracion + "OrigenDatos",
                    new XAttribute("CadenaConexion", RetailOne.Utilidades.Encriptacion.ConvertirABase64(ambiente.Origen.CadenaConexion)),
                    new XAttribute("ProveedorDatos", ambiente.Origen.Proveedor)));
                xambiente.Add(Environment.NewLine);

                ambientes.Add(xambiente);
                ambientes.Add(Environment.NewLine);
            }

            var variables = ContenidoArchivoConfiguracion.Element(EspacioNombresArchivoConfiguracion + "VariablesConfiguracion");
            
            if (variables != null)
            {
                variables.RemoveNodes();
                variables.Add(Environment.NewLine);

                foreach (var variable in VariablesConfiguracion)
                {
                    XElement xvariable = new XElement(EspacioNombresArchivoConfiguracion + "Variable",
                        new XAttribute("ID", variable.Key),
                        new XAttribute("Value", variable.Value));
                    variables.Add(xvariable);
                    variables.Add(Environment.NewLine);
                }
            }

            if(xElement != null)
            {
                string nombreNodo = xElement.Name.ToString();
                var nodo = ContenidoArchivoConfiguracion.Element(EspacioNombresArchivoConfiguracion + nombreNodo);
                if(nodo != null)
                {
                    nodo.Remove();
                }

                ContenidoArchivoConfiguracion.Add(xElement);
            }

            try
            {
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    Encoding = Encoding.UTF8,
                    Indent = true,
                    IndentChars = "\t",
                    NewLineChars = Environment.NewLine
                };
                
                using (XmlWriter writer = XmlTextWriter.Create(DireccionArchivoConfiguracion, settings))
                {
                    ContenidoArchivoConfiguracion.Save(writer);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron aplicar los cambios al archivo de configuración RetailOne.config", ex);
            }
            
        }

        public static bool OrigenDeDatosInicializado
        {
            get
            {
                return Sistema.Ambientes != null &&
                Sistema.Ambientes.Predeterminado != null &&
                !string.IsNullOrEmpty(Sistema.Ambientes.Predeterminado.Origen.ServidorBD) &&
                !Sistema.Ambientes.Predeterminado.Origen.ServidorBD.Equals("${UNDEFINED}") &&
                !string.IsNullOrEmpty(Sistema.Ambientes.Predeterminado.Origen.NombreBD) &&
                !Sistema.Ambientes.Predeterminado.Origen.NombreBD.Equals("${UNDEFINED}");
            }
        }

        [Obsolete]
        private static void GenerarArchivoConfiguracion(string direccionArchivoConfiguracion)
        {
            XElement xml = ObtenerXMLPorDefecto();
            xml.Save(direccionArchivoConfiguracion);
        }

        private static XElement ObtenerXMLPorDefecto()
        {
            XNamespace @namespace = Sistema.NsConfiguración;

            XElement implementacion = new XElement(@namespace + "DatosImplementacion",
                    new XAttribute("NombreSistema", "RetailOne"),
                    new XAttribute("Version", "1.0"),
                    new XAttribute("Abreviatura", "R1"),
                    new XAttribute("ModoDepuracion", "false"));

            XElement globalizacion = new XElement(@namespace + "Globalizacion",
                new XElement(@namespace + "Cultura",
                    new XAttribute("Codigo", "es-MX"),
                    new XAttribute("Nombre", "Español"),
                    new XAttribute("SeparadorMiles", ","),
                    new XAttribute("SeparadorDecimal", "."),
                    new XAttribute("CantidadDecimales", "2"),
                    new XAttribute("FormatoFechaCorta", "dd/MM/yyyy"),
                    new XAttribute("FormatoFechaLarga", "dddd, d 'de' MMMM 'de' yyyy"),
                    new XAttribute("FormatoHora", "hh:mm tt"),
                    new XAttribute("HusoHorario", "-5")),
                new XElement(@namespace + "Cultura",
                    new XAttribute("Codigo", "en-US"),
                    new XAttribute("Nombre", "English"),
                    new XAttribute("SeparadorMiles", ","),
                    new XAttribute("SeparadorDecimal", "."),
                    new XAttribute("CantidadDecimales", "2"),
                    new XAttribute("FormatoFechaCorta", "MM.dd.yyyy"),
                    new XAttribute("FormatoFechaLarga", "dddd, MMMM yyyy"),
                    new XAttribute("FormatoHora", "HH:mm"),
                    new XAttribute("HusoHorario", "-4.5")));

            XElement ambientes = new XElement(@namespace + "Ambientes",
               new XAttribute("AmbientePredeterminado", "PROD"),
               new XElement(@namespace + "Ambiente",
                   new XAttribute("ID", "PROD"),
                   new XAttribute("Nombre", "RetailOne.PROD"),
                   new XAttribute("IdiomaPredeterminado", "es-MX"),
                   new XElement(@namespace + "OrigenDatos",
                        //new XAttribute("Servidor", "${UNDEFINED}"),
                        //new XAttribute("NombreBD", "${UNDEFINED}"),
                        //new XAttribute("Usuario", "sa"),
                        //new XAttribute("Contraseña", "${UNDEFINED}"),
                        new XAttribute("CadenaConexion", "Data Source=${UNDEFINED};Initial Catalog=${UNDEFINED};User ID=sa;Password=${UNDEFINED};Persist Security Info=False;Integrated Security=False;"),
                        new XAttribute("ProveedorDatos", "SQLServer"))));

            XElement variables = new XElement(@namespace + "VariablesConfiguracion",
                new XElement(@namespace + "Variable", 
                new XAttribute("ID", "VariablePrueba"),
                new XAttribute("Value", "1")));

            return new XElement(@namespace + "RetailOne",
                implementacion,
                globalizacion,
                ambientes,
                variables);
        }
    }
}
