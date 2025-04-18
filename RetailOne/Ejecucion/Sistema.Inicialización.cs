using RetailOne.Configuracion;
using RetailOne.Datos;
using RetailOne.Utilidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace RetailOne.Ejecucion
{
    public static partial class Sistema
    {
        public static void Inicializacion(string direcciónArchivoConfiguración)
        {
            if (Inicializado) return;

            InicializarDireccionesRaiz(direcciónArchivoConfiguración);
            CargarXmlConfiguracion();
            CargarDatosImplementacion();
            CargarGlobalizacion();
            CargarAmbientesDatos();
            CargarVariablesConfiguracion();
            CargarModoDepuracion();
            /// Culmina la inicialización del sistema.
            Inicializado = true;

            if(Ambientes != null && Ambientes.Count > 0)
            {
                Cadenas.IdentificarIDCultura = delegate () { return SesionActual != null ? SesionActual.Cultura.Codigo : Ambientes.Predeterminado.IdiomaPredeterminado.Codigo; };
            }
            else if(Globalizacion.Count > 0)
            {
                Cadenas.IdentificarIDCultura = delegate () { return SesionActual != null ? SesionActual.Cultura.Codigo : Globalizacion[0].Codigo; };
            }
            else
            {
                Cadenas.IdentificarIDCultura = delegate () { return SesionActual != null ? SesionActual.Cultura.Codigo : Cultura.CulturaNeutra.Codigo; };
            }
            EjecutarEventoSistemaInicializado();
        }

        //private static void InicializarDireccionesRaiz(string direcciónArchivoConfiguración)
        //{
        //    DireccionArchivoConfiguracion = direcciónArchivoConfiguración;

        //    if(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath != null || System.Web.HttpContext.Current != null)
        //    {
        //        string apPath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
        //        if (!string.IsNullOrEmpty(apPath))
        //            DirectorioRaíz = apPath;

        //        if(System.Web.HttpContext.Current != null)
        //        {
        //            UrlRaíz = System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + System.Web.HttpContext.Current.Request.ApplicationPath;
        //            if (!UrlRaíz.EndsWith("/")) UrlRaíz += "/";
        //        }
        //    }
        //    else
        //    {
        //        if (string.IsNullOrEmpty(DirectorioRaíz) || DirectorioRaíz.Equals("~"))
        //        {
        //            string fullPath = Assembly.GetExecutingAssembly().Location;
        //            DirectorioRaíz = fullPath.Replace(Path.GetFileName(fullPath), "");
        //        }
        //    }

        //    if (!DirectorioRaíz.EndsWith(Path.DirectorySeparatorChar.ToString())) DirectorioRaíz += Path.DirectorySeparatorChar;
        //    DireccionArchivoConfiguracion = DirectorioRaíz + Path.GetFileName(direcciónArchivoConfiguración);
        //}

        private static void InicializarDireccionesRaiz(string direccionArchivoConfiguracion)
        {
            // Si está en un entorno web, obtenemos el directorio físico y la URL base
            if (System.Web.Hosting.HostingEnvironment.IsHosted || System.Web.HttpContext.Current != null)
            {
                // Establecemos el directorio físico de la aplicación
                if (!string.IsNullOrEmpty(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath))
                {
                    DirectorioRaíz = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
                }

                // Establecemos la URL base
                if (System.Web.HttpContext.Current != null)
                {
                    UrlRaíz = System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority)
                              + System.Web.HttpContext.Current.Request.ApplicationPath;

                    // Aseguramos que la URL base termine con '/'
                    if (!UrlRaíz.EndsWith("/")) UrlRaíz += "/";
                }
            }
            else
            {
                // Si no es un entorno web, obtenemos la ruta del ensamblado ejecutándose
                if (string.IsNullOrEmpty(DirectorioRaíz) || DirectorioRaíz.Equals("~"))
                {
                    string fullPath = Assembly.GetExecutingAssembly().Location;
                    DirectorioRaíz = Path.GetDirectoryName(fullPath) ?? string.Empty;
                }
            }

            // Aseguramos que DirectorioRaíz termine con el separador de directorio
            if (!DirectorioRaíz.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                DirectorioRaíz += Path.DirectorySeparatorChar;
            }

            // Verificamos si la dirección del archivo de configuración es relativa o absoluta
            if (!Path.IsPathRooted(direccionArchivoConfiguracion))
            {
                // Si es relativa, la completamos con el DirectorioRaíz
                DireccionArchivoConfiguracion = Path.Combine(DirectorioRaíz, Path.GetFileName(direccionArchivoConfiguracion));
            }
            else
            {
                // Si es absoluta, la dejamos como está
                DireccionArchivoConfiguracion = direccionArchivoConfiguracion;
            }
        }


        private static void CargarXmlConfiguracion()
        {
            if (!File.Exists(DireccionArchivoConfiguracion))
            {
                GenerarArchivoConfiguracion(DireccionArchivoConfiguracion);
            }

            ContenidoArchivoConfiguracion = UtilidadesXml.ValidarXml(DireccionArchivoConfiguracion, NsConfiguración, null);
            EspacioNombresArchivoConfiguracion = NsConfiguración;
        }

        private static void CargarDatosImplementacion()
        {
            XElement datosImplementación = ContenidoArchivoConfiguracion.Element(EspacioNombresArchivoConfiguracion + "DatosImplementacion");
            Nombre = (string)datosImplementación.Attribute("NombreSistema");
            Version = (string)datosImplementación.Attribute("Version");
            Abreviatura = (string)datosImplementación.Attribute("Abreviatura");

            string modoDepuracion = (string)datosImplementación.Attribute("ModoDepuracion");
            if (!string.IsNullOrEmpty(modoDepuracion))
            {
                modoDepuracion = modoDepuracion.Trim().ToLower();
                ModoDepuracion = modoDepuracion.Equals("true") || modoDepuracion.Equals("y") || modoDepuracion.Equals("1");
            }
            else
            {
                modoDepuracion = (string)datosImplementación.Attribute("ModoDesarrollo");
                if (!string.IsNullOrEmpty(modoDepuracion))
                {
                    modoDepuracion = modoDepuracion.Trim().ToLower();
                    ModoDepuracion = modoDepuracion.Equals("true") || modoDepuracion.Equals("y") || modoDepuracion.Equals("1");
                }
            }
        }

        private static void CargarGlobalizacion()
        {
            IEnumerable<XElement> items = from Item in ContenidoArchivoConfiguracion.Element(EspacioNombresArchivoConfiguracion + "Globalizacion").Elements(EspacioNombresArchivoConfiguracion + "Cultura")
                                          select Item;
            Globalizacion.Clear();
            foreach (XElement itemCultura in items)
            {
                decimal huso = (decimal)itemCultura.Attribute("HusoHorario");
                TimeSpan? husoHorario = null;
                if (huso >= -12 && huso <= 12)
                {
                    husoHorario = new TimeSpan(Convert.ToInt32(huso), Convert.ToInt32(60 * (Math.Abs(Decimal.Truncate(huso)) - Math.Abs(huso))), 0);
                }

                string codigo = (string)itemCultura.Attribute("Codigo") ?? (string)itemCultura.Attribute("Código");

                Cultura cultura = new Cultura(codigo,
                    (string)itemCultura.Attribute("Nombre"),
                    (string)itemCultura.Attribute("FormatoFechaCorta"),
                    (string)itemCultura.Attribute("FormatoFechaLarga"),
                    (string)itemCultura.Attribute("FormatoHora"),
                    (string)itemCultura.Attribute("SeparadorDecimal"),
                    (string)itemCultura.Attribute("SeparadorMiles"),
                    (int)itemCultura.Attribute("CantidadDecimales"),
                    husoHorario);

                Globalizacion.Add(cultura);
            }
        }

        private static void CargarAmbientesDatos()
        {
            XElement ambientes = ContenidoArchivoConfiguracion.Element(EspacioNombresArchivoConfiguracion + "Ambientes");

            var items = from Item in ambientes.Elements(EspacioNombresArchivoConfiguracion + "Ambiente")
                        select Item;

            Ambientes.Clear();

            string cadenaConexion = null;

            foreach (XElement itemAmbiente in items)
            {
                Ambiente ambiente = new Ambiente(
                    (string)itemAmbiente.Attribute("ID"),
                    (string)itemAmbiente.Attribute("Nombre"),
                    (string)itemAmbiente.Attribute("IdiomaPredeterminado"));

                XElement itemOrigen = itemAmbiente.Element(EspacioNombresArchivoConfiguracion + "OrigenDatos");

                cadenaConexion = (string)itemOrigen.Attribute("CadenaConexion");

                if(RetailOne.Utilidades.UtilidadesCadenas.ValidarExpresion(cadenaConexion, "^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$"))
                {
                    try
                    {
                        cadenaConexion = RetailOne.Utilidades.Encriptacion.ObtenerDeBase64(cadenaConexion);
                    }
                    catch { }
                }

                Origen Origen = new Origen(cadenaConexion,
                        (Proveedor)Enum.Parse(typeof(Proveedor), (string)itemOrigen.Attribute("ProveedorDatos")));

                //Origen Origen = new Origen((string)itemOrigen.Attribute("Nombre"),
                //        (string)itemOrigen.Attribute("CadenaConexión"),
                //        (Proveedor)Enum.Parse(typeof(Proveedor), (string)itemOrigen.Attribute("ProveedorDatos")));


                //Origen Origen = new Origen((string)itemOrigen.Attribute("Servidor"),
                //        (string)itemOrigen.Attribute("NombreBD"),
                //        (string)itemOrigen.Attribute("Usuario"),
                //        Utilidades.Encriptacion.ObtenerDeBase64((string)itemOrigen.Attribute("Contraseña")),
                //        (Proveedor)Enum.Parse(typeof(Proveedor), (string)itemOrigen.Attribute("ProveedorDatos")));

                ambiente.Origen = Origen;
                Ambientes.Add(ambiente);
            }

            XAttribute ambientePredeterminado = ambientes.Attribute("AmbientePredeterminado");
            if(ambientePredeterminado != null && Ambientes.Contains((string)ambientePredeterminado))
            {
                Ambientes.Predeterminado = Ambientes[(string)ambientes.Attribute("AmbientePredeterminado")];
            }
        }

        private static void CargarVariablesConfiguracion()
        {
            VariablesConfiguracion.Clear();

            var variable = ContenidoArchivoConfiguracion.Element(EspacioNombresArchivoConfiguracion + "VariablesConfiguracion");
            if (variable != null)
            {
                foreach (XElement itemVariable in variable.Elements(EspacioNombresArchivoConfiguracion + "Variable"))
                {
                    VariablesConfiguracion[(string)itemVariable.Attribute("ID")]
                        = (string)itemVariable.Attribute("Value");
                }
            }
        }

        private static void CargarModoDepuracion()
        {
            if (ModoDepuracion)
            {
                RetailOne.Datos.Depuracion.Activa = true;
                RetailOne.Datos.Depuracion.AlNotificar += Debug_AlNotificar;
                RetailOne.Datos.Depuracion.AlEjecutar += Depuracion_AlEjecutar;
            }
        }

        private static void Depuracion_AlEjecutar(string comando)
        {
            try
            {
                RetailOne.Depuracion.Log.Registrar(Recursos.Mensajes.EjecucionDeConsultasABaseDeDatos,
                    new string[] { comando });
            }
            catch (Exception ex)
            {
                RetailOne.Depuracion.Log.Registrar(Recursos.Mensajes.EjecucionDeConsultasABaseDeDatos,
                    new string[] { ex.InnerException != null ? ex.InnerException.Message : ex.Message });
            }
        }

        private static void Debug_AlNotificar(string descripcion, Exception ex)
        {
            try
            {
                string mensaje = Recursos.Mensajes.ErrorEnConsultaABaseDeDatos;
                string json = "";

                if (ex != null)
                {
                    mensaje = $"ERROR: {ex.Message}" ;
                    json = RetailOne.Utilidades.Json.Serializar(ex);
                }
                
                RetailOne.Depuracion.Log.Registrar(mensaje,
                    new string[] { descripcion, json });
            }
            catch
            {
                RetailOne.Depuracion.Log.Registrar("ERROR", 
                    new string[]{ descripcion, ex != null ? ex.Detalles() : "" });
            }
        }
    }
}
