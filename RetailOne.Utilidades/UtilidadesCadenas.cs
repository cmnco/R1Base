using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using RetailOne.Utilidades.Recursos;

namespace RetailOne.Utilidades
{
    /// <summary>
    /// Provee métodos de utilidad para el manejo de cadenas de texto.
    /// </summary>
    public static partial class UtilidadesCadenas
    {
        private static Regex regexNormalizarEspacios = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// Crea uniformidad en los espacios blancos de una cadena de texto, reemplazando todos los espacios (independientemente de su longitud)
        /// con espacios simples dada la logintud especificada por el parámetro <paramref name="cantidadEspaciosMaximoEntrePalabras"/>
        /// </summary>
        /// <param name="entrada">Texto de entrada</param>
        /// <param name="cantidadEspaciosMaximoEntrePalabras">Cantidad de caracteres en que se normalizarán los espacios (por lo general, 1)</param>
        /// <returns>Texto con los espacios normalizados</returns>
        public static string NormalizarEspacios(this string entrada, int cantidadEspaciosMaximoEntrePalabras = 1)
        {
            string reemplazo = new string(' ', cantidadEspaciosMaximoEntrePalabras);
            return regexNormalizarEspacios.Replace(entrada, reemplazo);
        }

        /// <summary>
        /// Devuelve una cadena de texto que contendrá sólo los números de la cadena de entrada. Todos los
        /// demás caracteres alfabéticos y especiales (como puntuaciones y espacios) son eliminados.
        /// </summary>
        /// <param name="entrada">Texto de entrada</param>
        /// <returns>Cadena que contiene únicamente los números en el texto de entrada</returns>
        public static string SoloNumeros(this string entrada)
        {
            string salida = "";
            byte b;
            foreach (char c in entrada)
            {
                if (byte.TryParse(c.ToString(), out b))
                {
                    salida += c;
                }
            }
            return salida;
        }

        private static Dictionary<Type, Type> cacheNulables = new Dictionary<Type, Type>();

        /// <summary>
        /// Realiza una conversión de tipos entre una cadena de texto y el tipo especificado en el parámetro <typeparam name="T"/>.
        /// Dependiendo del tipo al que se convertirá la cadena, así como del proveedor de formato, esta deberá cumplir con un formato específico.
        /// </summary>
        /// <typeparam name="T">Tipo de dato al que se convertirá la cadena</typeparam>
        /// <param name="entrada">Texto de entrada</param>
        /// <param name="proveedor">Proveedor de formato que brinda detalles de cómo se deben hacer las interpretaciones de los datos numéricos y de fecha.
        /// Si se intenta hacer la conversión de un texto proveniente de la interacción con el usuario en pantalla, será recomendable
        /// usar el proveedor de formato de la cultura especificada en la configuración de la aplicación.</param>
        /// <returns>Objeto convertido con el tipo de datos definido por el parámetro <typeparamref name="T"/></returns>
        public static T CambiarATipo<T>(this string entrada, IFormatProvider proveedor = null)
        {
            Type tipo = typeof(T);
            if (entrada.EsNuloOVacio())
            {
                return default(T);
            }
            if (tipo.EsTipoNulable())
            {
                if (!cacheNulables.ContainsKey(tipo))
                    cacheNulables[tipo] = new NullableConverter(tipo).UnderlyingType;

                tipo = cacheNulables[tipo];
            }
            if (tipo == typeof(string))
            {
                return (T)(object)entrada;
            }
            else if (tipo == typeof(DateTime))
            {
                return (T)(object)(DateTime.Parse(entrada, proveedor));
            }
            else if (tipo == typeof(bool))
            {
                if (entrada == "0") return (T)(object)false;
                if (entrada == "1") return (T)(object)true;
                return (T)(object)bool.Parse(entrada);
            }
            else if (tipo.IsEnum)
            {
                return (T)Enum.Parse(tipo, entrada);
            }
            else if (tipo.IsValueType)
            {
                return (T)(object)Convert.ChangeType(entrada, tipo, proveedor);
            }
            return default(T);
        }

        //public static T EsDelTipo<T>(this string entrada, Type tipo)
        //{

        //}


        /// <summary>
        /// Realiza una conversión de tipos entre una cadena de texto y el tipo especificado en el parámetro <typeparam name="T"/>.
        /// Dependiendo del tipo al que se convertirá la cadena, así como del proveedor de formato, esta deberá cumplir con un formato específico.
        /// </summary>
        /// <typeparam name="T">Tipo de dato al que se convertirá la cadena</typeparam>
        /// <param name="entrada">Texto de entrada</param>
        /// <param name="proveedor">Proveedor de formato que brinda detalles de cómo se deben hacer las interpretaciones de los datos numéricos y de fecha.
        /// Si se intenta hacer la conversión de un texto proveniente de la interacción con el usuario en pantalla, será recomendable
        /// usar el proveedor de formato de la cultura especificada en la configuración de la aplicación.</param>
        /// <param name="valorARetornarSiFallaConversion">Valor que deberá retornar en caso de fallar la conversión de tipos</param>
        /// <returns>Objeto convertido con el tipo de datos definido por el parámetro <typeparamref name="T"/></returns>
        public static T CambiarATipo<T>(this string entrada, T valorARetornarSiFallaConversion, IFormatProvider proveedor = null)
        {
            try
            {
                return CambiarATipo<T>(entrada, proveedor);
            }
            catch (Exception ex)
            {
                return valorARetornarSiFallaConversion;
            }
        }

        /// <summary>
        /// Valida que el texto de entrada tenga un formato válido de e-mail
        /// </summary>
        /// <param name="email">Texto de entrada</param>
        /// <returns>Retorna <c>true</c> si el texto es una dirección de e-mail válida; de lo contrario, <c>false</c></returns>
        public static bool ValidarEmail(this string email)
        {
            if (email.EsNuloOVacio()) return false;
            string modelo = @"^([\w\d\-\.]+)@{1}(([\w\d\-]{1,67})| ([\w\d\-]+\.[\w\d\-]{1,67}))\.(([a-zA-Z\d]{2,4})(\.[a-zA-Z\d]{2})?)$";
            return ValidarExpresion(email, modelo);
        }

        /// <summary>
        /// Valida que el texto de entrada contenga caracteres alfanuméricos (letras, números, guiones bajos)
        /// </summary>
        /// <param name="cadena">Texto de entrada</param>
        /// <returns>Retorna <c>true</c> si el texto es vacío o contiene caracteres alfanuméricos; de lo contrario, <c>false</c></returns>
        public static bool ValidarCadenaAlfanumerica(this string cadena)
        {
            string modelo = @"^\w{0,}$";
            return ValidarExpresion(cadena, modelo);
        }

        /// <summary>
        /// Valida que el texto posea únicamente una cifra numérica entera (sin decimales ni otros caracteres, sólo números).
        /// </summary>
        /// <param name="cadena">Texto de entrada</param>
        /// <returns>Retorna <c>true</c> si el texto contiene únicamente dígitos numéricos; de lo contrario, <c>false</c></returns>
        public static bool ValidarEntero(this string cadena)
        {
            string modelo = @"^\d{1,}$";
            return ValidarExpresion(cadena, modelo);
        }

        /// <summary>
        /// Valida que el texto posea una cifra numérica entera o decimal.
        /// </summary>
        /// <param name="cadena">Texto de entrada</param>
        /// <returns>Retorna <c>true</c> si el texto contiene únicamente dígitos numéricos o separadores decimales (coma o punto); de lo contrario, <c>false</c></returns>
        public static bool ValidarDecimal(this string cadena)
        {
            string modelo = @"^\d{1,}((\.|,)\d{0,})?$";
            return ValidarExpresion(cadena, modelo);
        }

        /// <summary>
        /// Valida el cumplimiento de una expresión regular en un texto.
        /// </summary>
        /// <param name="entrada">Texto de entrada</param>
        /// <param name="expresionRegex">Expresión regular que se evaluará</param>
        /// <returns>Retorna <c>true</c> si el texto cumple aunque sea una vez con la expresión regular; de lo contrario, <c>false</c></returns>
        public static bool ValidarExpresion(string entrada, string expresionRegex)
        {
            if (string.IsNullOrEmpty(entrada))
                return true;
            Regex re = new Regex(expresionRegex);
            if (re.IsMatch(entrada))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Separa los items de una cadena que enumera, distinguidos por un separador, distintos valores, y los convierte a un nuevo tipo de datos.
        /// De acuerdo al parámetro de tipo <typeparamref name="Type"/>, los elementos enumerados en el texto deben cumplir con un formato determinado.
        /// </summary>
        /// <typeparam name="Type">Type de datos al que se convertirán los elementos separados</typeparam>
        /// <param name="cadena">Texto de entrada</param>
        /// <param name="separador">Texto o carácter que separa la representación de cada elemento en la cadena</param>
        /// <returns>Enumerable tipificado con los distintos elementos enumerados en la cadena de entrada</returns>
        public static IEnumerable<Type> SepararCadenaEnumerada<Type>(this string cadena, string separador)
        {
            return SepararCadenaEnumerada<Type>(cadena, separador, true, false);
        }


        /// <summary>
        /// Separa los items de una cadena que enumera, distinguidos por un separador, distintos valores, y los convierte a un nuevo tipo de datos.
        /// De acuerdo al parámetro de tipo <typeparamref name="Type"/>, los elementos enumerados en el texto deben cumplir con un formato determinado.
        /// </summary>
        /// <typeparam name="Type">Type de datos al que se convertirán los elementos separados</typeparam>
        /// <param name="cadena">Texto de entrada</param>
        /// <param name="separador">Texto o carácter que separa la representación de cada elemento en la cadena</param>
        /// <param name="trimEnCadaElemento">Si se indica <c>true</c>, se limpiarán los espacios en blanco que puedan tener los elementos luego de ser dividos por el separador; de lo contrario, se intentará hacer la conversión de tipo sin eliminar cualquier espacio en blanco</param>
        /// <param name="mantenerElementosNulos">Si se indica <c>true</c>, se incluirán en el enumerado los items que hayan estado vacíos; de lo contrario, se omitirán</param>
        /// <returns>Enumerable tipificado con los distintos elementos enumerados en la cadena de entrada</returns>
        public static IEnumerable<Type> SepararCadenaEnumerada<Type>(this string cadena, string separador, bool trimEnCadaElemento, bool mantenerElementosNulos)
        {
            var salida = new List<Type>();

            if (cadena.EsNuloOVacio()) return salida;

            var separados = cadena.Split(new string[] { separador }, mantenerElementosNulos ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries);
            if (trimEnCadaElemento)
            {
                foreach (var elemento in separados)
                {
                    salida.Add(elemento.Trim().CambiarATipo<Type>());
                }
            }
            else
            {
                foreach (var elemento in separados)
                {
                    salida.Add(elemento.CambiarATipo<Type>());
                }
            }

            return salida;
        }

        /// <summary>
        /// Genera una cadena que concatena, con el uso de un separador de texto, la evaluación del método <c>ToString()</c> de todos los elementos de un enumerable.
        /// </summary>
        /// <typeparam name="Type">Type de los elementos de entrada</typeparam>
        /// <param name="elementos">Enumerable con los elementos que se concatenarán en la lista textual</param>
        /// <param name="separador">Separador que se usará entre las expresiones de los elementos</param>
        /// <returns>Cadena de texto con todas las evaluaciones <c>ToString()</c> de la lista de entrada, separadas por el parámetro <paramref name="separador"/></returns>
        public static string EnumerarDadoMaximosCaracteres<Type>(this IEnumerable<Type> elementos, string separador)
        {
            return EnumerarDadoMaximosCaracteres(elementos, null, int.MaxValue, false, separador);
        }

        /// <summary>
        /// Genera una cadena que concatena, con el uso de un separador de texto, la evaluación del método <c>ToString()</c> de todos los elementos de un enumerable.
        /// </summary>
        /// <typeparam name="Type">Type de los elementos de entrada</typeparam>
        /// <param name="elementos">Enumerable con los elementos que se concatenarán en la lista textual</param>
        /// <param name="selectorTexto">Función que extraerá de cada objeto enumerado el dato a concatenar en la cadena de texto</param>
        /// <param name="separador">Separador que se usará entre las expresiones de los elementos</param>
        /// <returns>Cadena de texto con todas las evaluaciones <c>ToString()</c> de la lista de entrada, separadas por el parámetro <paramref name="separador"/></returns>
        public static string EnumerarDadoMaximosCaracteres<Type>(this IEnumerable<Type> elementos, Func<Type, string> selectorTexto, string separador)
        {
            return EnumerarDadoMaximosCaracteres(elementos, selectorTexto, int.MaxValue, false, separador);
        }

        /// <summary>
        /// Genera una cadena que concatena, con el uso de una coma y espacio como separador (", "), una expresión evaluada sobre todos los elementos de un enumerable.
        /// </summary>
        /// <typeparam name="Type">Type de los elementos de entrada</typeparam>
        /// <param name="elementos">Enumerable con los elementos que se concatenarán en la lista textual</param>
        /// <param name="selectorTexto">Función que extraerá de cada objeto enumerado el dato a concatenar en la cadena de texto</param>
        /// <param name="máximaCantidadCaracteres">Limita la cantidad de caracteres que podrá tomar la cadena definitiva; se concatenarán sólo los elementos completos que, incluyendo los separadores, no totalicen más de dicha cantidad de caracteres.</param>
        /// <param name="puntosSuspensivosAutomáticos">Si es <see langword="true"/>, agrega puntos suspensivos ('...', una cadena de tres caracteres de punto convencional) al final del texto recortado si este no pudo representarse por completo en el límite dado por <paramref name="máximaCantidadCaracteres"/></param>
        /// <returns>Cadena de texto con todas las evaluaciones <c>ToString()</c> de la lista de entrada, separadas por una coma seguida de un espacio (', ')</returns>
        public static string EnumerarDadoMaximosCaracteres<Type>(this IEnumerable<Type> elementos, Func<Type, string> selectorTexto, int máximaCantidadCaracteres, bool puntosSuspensivosAutomáticos)
        {
            return EnumerarDadoMaximosCaracteres<Type>(elementos, selectorTexto, máximaCantidadCaracteres, puntosSuspensivosAutomáticos, ", ");
        }

        /// <summary>
        /// Genera una cadena que concatena, con el uso de un separador de texto, una expresión evaluada sobre todos los elementos de un enumerable.
        /// </summary>
        /// <typeparam name="Type">Type de los elementos de entrada</typeparam>
        /// <param name="elementos">Enumerable con los elementos que se concatenarán en la lista textual</param>
        /// <param name="selectorTexto">Función que extraerá de cada objeto enumerado el dato a concatenar en la cadena de texto</param>
        /// <param name="maximaCantidadCaracteres">Limita la cantidad de caracteres que podrá tomar la cadena definitiva; se concatenarán sólo los elementos completos que, incluyendo los separadores, no totalicen más de dicha cantidad de caracteres.</param>
        /// <param name="puntosSuspensivosAutomaticos">Si es <see langword="true"/>, agrega puntos suspensivos ('...', una cadena de tres caracteres de punto convencional) al final del texto recortado si este no pudo representarse por completo en el límite dado por <paramref name="maximaCantidadCaracteres"/></param>
        /// <param name="separador">Separador que se usará entre las expresiones de los elementos</param>
        /// <returns>Cadena de texto con todas las evaluaciones <c>ToString()</c> de la lista de entrada, separadas por el parámetro <paramref name="separador"/></returns>
        public static string EnumerarDadoMaximosCaracteres<Type>(this IEnumerable<Type> elementos, Func<Type, string> selectorTexto, int maximaCantidadCaracteres, bool puntosSuspensivosAutomaticos, string separador)
        {
            if (elementos == null || elementos.Count() <= 0) return "";

            string salida = string.Empty;

            if(selectorTexto == null)
                salida = string.Join(separador, elementos);
            else
                salida = string.Join(separador, elementos.Select(selectorTexto));

            if (salida.Length > maximaCantidadCaracteres)
                salida = salida.Substring(0, maximaCantidadCaracteres);

            if (puntosSuspensivosAutomaticos)
                salida += "...";

            return salida;
        }

        /// <summary>
        /// Si la cadena es nula o vacía, la reemplaza por una nueva cadena.
        /// </summary>
        /// <param name="entrada">Texto de entrada</param>
        /// <param name="reemplazo">Cadena con que se reemplazará la de entrada si esta fuese nula o vacía</param>
        /// <returns>Si el texto de entrada es nulo o vacío, retornará la cadena del parámetro <paramref name="reemplazo"/>, de lo contrario,
        /// retornará la cadena original</returns>
        public static string SiEsNuloOVacio(this string entrada, string reemplazo)
        {
            if (entrada.EsNuloOVacio()) return reemplazo;
            return entrada;
        }

        /// <summary>
        /// Determina si una cadena es nula o vacía. Una cadena que sólo contenga espacios en blanco (tabulaciones,
        /// saltos de línea, espacios regulares, etc.) será considerada como vacía.
        /// </summary>
        /// <param name="entrada">Texto de entrada</param>
        /// <returns><c>True</c> si el texto es nulo, vacío o sólo contiene caracteres de espacio</returns>
        public static bool EsNuloOVacio(this string entrada)
        {
            if (entrada == null || entrada.Trim().Length == 0)
                return true;
            return false;
        }
    }
}
