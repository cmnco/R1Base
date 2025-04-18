using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RetailOne.Utilidades
{
    /// <summary>
    /// Clase para registrar en archivo información relacionada con la depuración del código
    /// </summary>
    public class Depuracion
    {
        private const string _LINEA = "----------------------------------------------------";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="detalles"></param>
        public static void Registrar(params string[] detalles)
        {
            Registrar(null, detalles);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="detalles"></param>
        public static void Registrar(Exception ex, params string[] detalles)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(_LINEA);
            stringBuilder.AppendLine(string.Format("Fecha/Hora: {0}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
            stringBuilder.AppendLine("");

            if (ex != null)
            {
                stringBuilder.AppendLine("Excepción:");
                string stackTrace = ex.StackTrace;

                do
                {
                    stringBuilder.AppendLine(ex.Message);
                    ex = ex.InnerException;
                } while (ex != null);

                stringBuilder.AppendLine("Pila:");
                stringBuilder.AppendLine(stackTrace);
                stringBuilder.AppendLine("");
            }
            
            if(detalles != null && detalles.Count() > 0)
            {
                foreach(var detalle in detalles)
                {
                    stringBuilder.AppendLine(detalle);
                    stringBuilder.AppendLine("");
                }
            }
            
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(RetailOne.Utilidades.Depuracion)).Location);
            if (!path.EndsWith("\\"))
            {
                path = path + "\\";
            }

            try
            {
                File.AppendAllText(string.Format("{0}Debug_{1}.txt", path, DateTime.Now.ToString("yyMMddHHmm")), stringBuilder.ToString());
            }
            catch(Exception err)
            {
                
            }
            finally
            {

            }
        }
    }
}
