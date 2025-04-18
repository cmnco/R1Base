using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using RetailOne.Utilidades;

namespace RetailOne.Depuracion
{
    public class Log
    {
        #region Propiedades
        private static object _padlock = new object();
        private static bool? _permisosEscritura = null;
        public static bool PermisosEscritura
        {
            get
            {
                if (!_permisosEscritura.HasValue)
                {
                    _permisosEscritura = ValidarPermisosEscritura();
                }
                return _permisosEscritura.Value;
            }
        }
        private static string _ruta = null;
        public static string Ruta
        {
            get
            {
                if (_ruta == null)
                {
                    string directorio = Ejecucion.Sistema.VariablesConfiguracion["RutaArchivosSalida"] ?? "~/Log";
                    _ruta = directorio.Replace("~/", Ejecucion.Sistema.DirectorioRaíz);
                }
                return _ruta;
            }
            set
            {
                _ruta = null;
                _permisosEscritura = false;
            }
        }
        #endregion

        #region Métodos
        /// <summary>
        /// Valida si se cuentan con los permisos de escritura
        /// </summary>
        /// <returns></returns>
        public static bool ValidarPermisosEscritura()
        {
            ValidarDirectorio();

            string rutaArchivo = "";

            lock (_padlock)
            {
                try
                {
                    rutaArchivo = Path.Combine(Ruta, "testLogExtendido.txt");
                    File.AppendAllText(rutaArchivo, "test");
                    if (File.Exists(rutaArchivo))
                        File.Delete(rutaArchivo);

                    return true;
                }
                catch (IOException ex)
                {
                    return false;
                }
                catch (UnauthorizedAccessException ex)
                {
                    return false;
                }
                catch (SecurityException ex)
                {
                    return false;
                }
                catch (Exception ex)
                {
                    throw new Exception(Recursos.Excepciones.NoDisponeDePermisosParaLecturaEscritura[rutaArchivo], ex);
                }
            }
        }

        /// <summary>
        /// Valida si existe el directorio para el registro de log.En caso de no existir intenta crearlo.
        /// </summary>
        public static void ValidarDirectorio()
        {
            try
            {
                lock (_padlock)
                {
                    if (!Directory.Exists(Ruta))
                    {
                        Directory.CreateDirectory(Ruta);
                    }
                }
            }
            catch (Exception ex)
            {
                _permisosEscritura = false;
                throw new Exception(Recursos.Excepciones.NoSePudoCrearElDirectorio[Ruta], ex);
            }
        }

        public static void Registrar(string descripcion)
        {
            if (!RetailOne.Ejecucion.Sistema.ModoDepuracion)
                return;

            Registrar(descripcion, false, null);
        }

        public static void Registrar(string descripcion, Exception ex)
        {
            Registrar(descripcion, true, ex.SiNoEsNulo(x=>x.Detalles()));
        }

        public static void Registrar(string descripcion, params string[] detalles)
        {
            Registrar(descripcion, false, detalles);
        }

        private static void Registrar(string descripcion, bool forzarEscritura = false, params string[] detalles)
        {
            if (!RetailOne.Ejecucion.Sistema.ModoDepuracion && !forzarEscritura)
                return;

            if (!PermisosEscritura)
                return;

            ValidarDirectorio();

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"[{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:ffff")}]");
                sb.AppendLine($"Descripción: {descripcion ?? "Sin descripción"}");
                if (detalles != null && detalles.Count() > 0)
                {
                    sb.AppendLine("Detalles:");
                    foreach (var detalle in detalles)
                    {
                        sb.AppendLine(detalle);
                    }
                }
                sb.AppendLine("---------------------------------------------------------------------------");
                sb.AppendLine("");
                lock (_padlock)
                {
                    
                    string rutaArchivo = Path.Combine(Ruta, $"Log_{DateTime.Now.ToString("yyyyMMdd")}.txt");
                    File.AppendAllText(rutaArchivo, sb.ToString());
                }
            }
            catch (Exception ex) {  }
        }
        #endregion
    }
}
