using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
namespace RetailOne.Utilidades
{
    /// <summary>
    /// Provee métodos de utilidad para el manejo de enumerados.
    /// </summary>
    public static class UtilidadesEnum
    {
        /// <summary>
        /// Obtiene la descripción textual de un valor enumerado, dadas las definiciones de atributos <see cref="System.ComponentModel.DescriptionAttribute"/> ó, en última instancia, usándose el nombre del valor en el enumerado.
        /// </summary>
        /// <param name="entrada">Value enumerado</param>
        /// <returns>Descripción textual extraída del atributo <see cref="DescriptionAttribute"/> o del nombre del valor</returns>
        public static string Descripcion(this Enum entrada)
        {
            var campo = entrada.GetType().GetField(entrada.ToString());
            object[] descripción = campo.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (descripción.Length > 0)
                return ((DescriptionAttribute)descripción[0]).Description;
            return campo.Name;
        }

        /// <summary>
        /// Representa como cadena de texto el valor numérico encapsulado por un valor enumerado.
        /// </summary>
        /// <param name="enumerado">Value enumerado</param>
        /// <returns>Cadena de texto con el valor numérico encapsulado por el valor del parámetro <paramref name="enumerado"/></returns>
        public static string ValorEnCadena(this Enum enumerado)
        {
            return enumerado.ToString("d");
        }

        /// <summary>
        /// Devuelve un diccionario que vincula los valores de un enumerado con sus descripciones textuales, filtrándolas por categorías (<seealso cref="CategoríaEnumAttribute"/>).
        /// </summary>
        /// <param name="tipoEnum">Objeto de tipo <see cref="System.Type"/> que refiera al tipo enumerado cuyas descripciones se extraerán</param>
        /// <param name="ordenarPorValor">Si se indica <c>true</c>, los elementos se listarán ordenados por el valor numérico encapsulado de los items; de lo contrario, se mostrarán en el orden que los liste el CLR.</param>
        /// <returns>Diccionario que lista los valores del enumerado
        /// Las claves del diccionario son los valores numéricos encapsulados, y los valores del diccionario son las descripciones textuales de los mismos</returns>
        public static Dictionary<object, string> ObtenerDescripciones(this Type tipoEnum, bool ordenarPorValor)
        {
            Dictionary<object, string> enums = new Dictionary<object, string>();

            IEnumerable<FieldInfo> datos = tipoEnum.GetFields().Where(x => x.IsStatic);
        
            if (ordenarPorValor)
                datos = datos.OrderBy(x => x.GetRawConstantValue());
        
            foreach (var campo in datos)
            {
                DescriptionAttribute descripcion = (DescriptionAttribute)campo.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
                object valor = campo.GetRawConstantValue();
                if (descripcion != null)
                    enums.Add(valor, descripcion.Description);
            }

            return enums;
        }

        /// <summary>
        /// <para>Devuelve un diccionario que vincula los valores de un enumerado con sus descripciones textuales.</para>
        /// <para>Es equivalente a llamar al método <see cref="Descripciones"/> y pasarle como parámetro el tipo del parámetro <paramref name="cualquierValorDelEnumerado"/>:</para>
        /// <code>
        /// Descripciones(cualquierValorDelEnumerado.GetType())
        /// </code>
        /// </summary>
        /// <param name="cualquierValorDelEnumerado">Cualquier valor del enumerado cuyas descripciones completas se quieren extraer</param>
        /// <returns>Diccionario que lista todos los valores del tipo enumerado.
        /// Las claves del diccionario son los valores numéricos encapsulados, y los valores del diccionario son las descripciones textuales de los mismos</returns>
        public static Dictionary<object, string> DescripcionesCompletas(this Enum cualquierValorDelEnumerado)
        {
            return Descripciones(cualquierValorDelEnumerado.GetType());
        }


        /// <summary>
        /// Devuelve un diccionario que vincula los valores de un tipo enumerado con sus descripciones textuales.
        /// </summary>
        /// <typeparam name="Type">Type enumerado cuyas descripciones y valores se desean extraer</typeparam>
        /// <returns>Diccionario que lista todos los valores del tipo enumerado.
        /// Las claves del diccionario son los valores numéricos encapsulados, y los valores del diccionario son las descripciones textuales de los mismos</returns>
        public static Dictionary<object, string> Descripciones<Type>()
            where Type : struct
        {
            return Descripciones(typeof(Type));
        }


        /// <summary>
        /// Devuelve un diccionario que vincula los valores de un tipo enumerado con sus descripciones textuales.
        /// </summary>
        /// <param name="tipoEnumerado">Objeto <see cref="System.Type"/> con el tipo enumerado cuyas descripciones y valores se desean extraer</param>
        /// <returns>Diccionario que lista todos los valores del tipo enumerado.
        /// Las claves del diccionario son los valores numéricos encapsulados, y los valores del diccionario son las descripciones textuales de los mismos</returns>
        public static Dictionary<object, string> Descripciones(Type tipoEnumerado)
        {
            return ObtenerDescripciones(tipoEnumerado, false);
        }

        private static Dictionary<Type, Dictionary<object, string>> cacheDescripcionesEnum = new Dictionary<Type, Dictionary<object, string>>();
        /// <summary>
        /// Mantiene almacenadas en tiempo de ejecución las descripciones extraídas de los tipos enumerados.
        /// Dado que normalmente esta información se extra vía reflexión (y es un proceso costoso en términos de recursos de ejecución),
        /// el uso de esta caché acelerará la adquisición posterior de las descripciones previamente solicitadas.
        /// </summary>
        public static Dictionary<Type, Dictionary<object, string>> CacheDescripcionesEnum
        {
            get { return cacheDescripcionesEnum; }
        }
    }
}
