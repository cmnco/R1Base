using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RetailOne.Utilidades
{
    public static class UtilidadesExceptiones
    {
        public static void Registrar(string descripcion, string[] detalles)
        {
            RetailOne.Seguridad.RegistroEvento.Registrar(RetailOne.Utilidades.Instancia.Predeterminada, 
                Seguridad.TipoEvento.Error, descripcion, detalles);
        }

        public static void Registrar(this Exception ex, string descripcion)
        {
            RetailOne.Seguridad.RegistroEvento.Registrar(RetailOne.Utilidades.Instancia.Predeterminada, Seguridad.TipoEvento.Error, descripcion, ex.Detalles());
        }

        public static string Detalles(this Exception ex)
        {
            if (ex == null)
                return null;

            StringBuilder sb = new StringBuilder();
            string stackTrace = ex.StackTrace;
            sb.AppendLine($"Exception: {ex.Message}");

            InnerException:
            ex = ex.InnerException;
            if (ex != null)
            {
                sb.AppendLine($"InnerException: {ex.Message}");
                goto InnerException;
            }

            sb.AppendLine("StackTrace:");
            sb.AppendLine(stackTrace);

            return sb.ToString();
        }
    }
}
