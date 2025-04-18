using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetailOne.Utilidades;

namespace RetailOne.Utilidades
{
    public static partial class UtilidadesCadenas
    {
        /// <summary>
        /// Determina si la cadena es "parseable" a "long" 
        /// </summary>
        /// <param name="entrada"></param>
        /// <returns></returns>
        public static bool EsNumeroLong(this string entrada)
        {
            return long.TryParse(entrada, out long dato);
        }

        /// <summary>
        /// Determina si la cadena es "parseable" a "int" 
        /// </summary>
        /// <param name="entrada"></param>
        /// <returns></returns>
        public static bool EsNumeroInt(this string entrada)
        {
            return int.TryParse(entrada, out int dato);
        }

        /// <summary>
        /// Determina si la cadena es "parseable" a "decimal" 
        /// </summary>
        /// <param name="entrada"></param>
        /// <returns></returns>
        public static bool EsNumeroDecimal(this string entrada)
        {
            return decimal.TryParse(entrada, out decimal dato);
        }

        /// <summary>
        /// Determina si la cadena es "parseable" a "short" 
        /// </summary>
        /// <param name="entrada"></param>
        /// <returns></returns>
        public static bool EsNumeroShort(this string entrada)
        {
            return short.TryParse(entrada, out short dato);
        }

        /// <summary>
        /// Determina si la cadena es "parseable" a "int" 
        /// </summary>
        /// <param name="entrada"></param>
        /// <returns></returns>
        public static bool EsNumeroDouble(this string entrada)
        {
            return double.TryParse(entrada, out double dato);
        }

        /// <summary>
        /// Método que regresa la cadena "parseada" a "long" 
        /// </summary>
        /// <param name="entrada"></param>
        /// <returns></returns>
        public static long ToLong(this string entrada)
        {
            if (long.TryParse(entrada, out long dato))
                return dato;
            return -1;
        }

        /// <summary>
        /// Método que regresa la cadena "parseada" a "int" 
        /// </summary>
        /// <param name="entrada"></param>
        /// <returns></returns>
        public static int ToInt(this string entrada)
        {
            if (int.TryParse(entrada, out int dato))
                return dato;
            return -1;
        }

        /// <summary>
        /// Método que regresa la cadena "parseada" a "Double" 
        /// </summary>
        /// <param name="entrada"></param>
        /// <returns></returns>
        public static double ToDouble(this string entrada)
        {
            if (double.TryParse(entrada, out double dato))
                return dato;
            return -1;
        }

        /// <summary>
        /// Método que regresa la cadena "parseada" a "Decimal" 
        /// </summary>
        /// <param name="entrada"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this string entrada)
        {
            if (decimal.TryParse(entrada, out decimal dato))
                return dato;
            return -1;
        }

        /// <summary>
        /// Método que regresa la cadena "parseada" a "Short" 
        /// </summary>
        /// <param name="entrada"></param>
        /// <returns></returns>
        public static short ToShort(this string entrada)
        {
            if (short.TryParse(entrada, out short dato))
                return dato;
            return -1;
        }

        /// <summary>
        /// Método que valida si el valor es null o vacio. De serlo se manda la Excepcion controlada 11156 con la referencia proporcionada
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="referencia"></param>
        /// <returns></returns>
        public static void NoDebeSerNullOVacio(this string valor, string referencia)
        {
            if (string.IsNullOrEmpty(valor))
                throw new ExcepcionControlada(11156, Excepciones.ValorNoValido[referencia], referencia);
        }

        /// <summary>
        /// Separa una lista de correos concatenada con cualquiera de los caracteres ";" (punto y coma), "," (coma) o  "|" (Barra vertical) y valida el formato de cada correo obtenido.
        /// </summary>
        /// <param name="listaConcatenada">Lista concatenada de correos</param>
        /// <returns>Retorna lista de correos obtenida de la cadena</returns>
        public static IEnumerable<string> ObtnerCorreosValidos(this string listaConcatenada)
        {
            if (string.IsNullOrEmpty(listaConcatenada))
                throw new ArgumentNullException(Excepciones.NoSeHaProporcionadoListaCorreos);

            string[] partes = listaConcatenada.Split(";,| ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            List<string> correos = new List<string>();

            foreach (string email in partes)
            {
                if (email.ValidarEmail())
                    correos.Add(email);
            }

            return correos;
        }

        /// <summary>
        /// Regresa una cadena con correos validos concatenado con cualquiera de los caracteres ";" (punto y coma), "," (coma) o  "|" (Barra vertical) y valida el formato de cada correo obtenido.
        /// </summary>
        /// <param name="listaConcatenada">Lista concatenada de correos</param>
        /// <returns>Retorna lista de correos obtenida de la cadena</returns>
        public static string ObtnerCadenaCorreosValidos(this string listaConcatenada)
        {
            string correos = string.Empty;

            if (string.IsNullOrEmpty(listaConcatenada))
                throw new ArgumentNullException(Excepciones.NoSeHaProporcionadoListaCorreos);

            string[] partes = listaConcatenada.Split(";,| ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (string email in partes)
            {
                if (email.ValidarEmail())
                    correos = email + ";";
            }

            return correos;
        }
    }

    public static class UtilidadesNumerosJhon
    {
        #region DebeSerPositivo
        /// <summary>
        /// Método que valida que el valor sea igual o mayor a 0. De lo contrario se manda la Excepcion controlada 11156 con la referencia proporcionada
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="referencia"></param>
        public static void DebeSerPositivo(this int valor, string referencia)
        {
            if (valor < 0)
                throw new ExcepcionControlada(11156, Excepciones.ValorNoValido[referencia], referencia);
        }

        /// <summary>
        /// Método que valida que el valor sea igual o mayor a 0. De lo contrario se manda la Excepcion controlada 11156 con la referencia proporcionada
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="referencia"></param>
        public static void DebeSerPositivo(this decimal valor, string referencia)
        {
            if (valor < 0)
                throw new ExcepcionControlada(11156, Excepciones.ValorNoValido[referencia], referencia);
        }
        #endregion

        #region DebeSerMayorQueCero
        /// <summary>
        /// Método que valida que el valor sea mayor a 0. De lo contrario se manda la Excepcion controlada 111049 con la referencia proporcionada
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="referencia"></param>
        /// <returns></returns>
        public static void DebeSerMayorQueCero(this decimal valor, string referencia)
        {
            if (valor <= 0)
                throw new ExcepcionControlada(111049, Excepciones.ValorDebeSerMayorACero[referencia], referencia);
        }

        /// <summary>
        /// Método que valida que el valor sea mayor a 0. De lo contrario se manda la Excepcion controlada 111049 con la referencia proporcionada
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="referencia"></param>
        /// <returns></returns>
        public static void DebeSerMayorQueCero(this int valor, string referencia)
        {
            if (valor <= 0)
                throw new ExcepcionControlada(111049, Excepciones.ValorDebeSerMayorACero[referencia], referencia);
        }
        #endregion
    }

    public static class Excepciones
    {
        /// <summary>
        /// El valor debe ser mayor a 0.
        /// </summary>
        public static Cadenas ValorDebeSerMayorACero = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "El valor debe ser mayor a 0. Referencia: '{0}."),
            new Cadena(Cadenas.English, "The value must be greater than 0. Reference: '{0}.'"));

        /// <summary>
        /// Valor no válido.
        /// </summary>
        public static Cadenas ValorNoValido = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "Valor no válido. Referencia: '{0}'."),
            new Cadena(Cadenas.English, "Invalid value. Reference: '{0}'"));

        /// <summary>
        /// No se ha proporcionado la lista de correos.
        /// </summary>
        public static Cadenas NoSeHaProporcionadoListaCorreos = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "No se ha proporcionado la lista de correos. Referencia: '{0}'."),
            new Cadena(Cadenas.English, "No mailing list provided. Reference: '{0}'."));
    }
}
