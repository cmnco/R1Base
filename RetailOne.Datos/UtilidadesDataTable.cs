using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace RetailOne.Datos
{
    public static class UtilidadesDataTable
    {
        /// <summary>
        /// Permite extraer de un DataTable un diccionario.
        /// </summary>
        /// <typeparam name="TKey">Tipo de la clave del diccionario.</typeparam>
        /// <typeparam name="TElement">Tipo correspondiente al valor del diccionario.</typeparam>
        /// <param name="source">Tabla origen</param>
        /// <param name="keySelector">Función selectora del campo que corresponderá con la clave del diccionario.</param>
        /// <param name="elementSelector">Función selectora del campo que corresponderá con el valor devuelto por el diccionario.</param>
        /// <returns></returns>
        public static Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(this DataTable source, Func<DataRow, TKey> keySelector, Func<DataRow, TElement> elementSelector)
        {
            Dictionary<TKey, TElement> d = new Dictionary<TKey, TElement>();
            foreach (DataRow element in source.Rows) d.Add(keySelector(element), elementSelector(element));
            return d;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fila"></param>
        /// <param name="nombreColumna"></param>
        /// <param name="omitirSiColumnaNoExiste"></param>
        /// <returns></returns>
        public static T Valor<T>(this DataRow fila, string nombreColumna, bool omitirSiColumnaNoExiste)
        {
            return fila.Valor(nombreColumna, omitirSiColumnaNoExiste, default(T));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fila"></param>
        /// <param name="nombreColumna"></param>
        /// <param name="omitirSiColumnaNoExiste"></param>
        /// <param name="valorPorDefecto"></param>
        /// <returns></returns>
        public static T Valor<T>(this DataRow fila, string nombreColumna, bool omitirSiColumnaNoExiste, T valorPorDefecto)
        {
            if (omitirSiColumnaNoExiste && !fila.Table.Columns.Contains(nombreColumna))
                return valorPorDefecto;

            try
            {
                return fila.Valor<T>(nombreColumna, valorPorDefecto);
            }
            catch
            {
                return valorPorDefecto;
            }
        }


        /// <summary>
        /// Retorna el valor del campo especificado.
        /// </summary>
        /// <typeparam name="T">Tipo</typeparam>
        /// <param name="fila"></param>
        /// <param name="nombreColumna">Nombre de la columna</param>
        /// <param name="valorPorDefecto">Valor a retornar si no existe la columna o su contenido es nulo</param>
        /// <returns></returns>
        public static T Valor<T>(this DataRow fila, string nombreColumna, T valorPorDefecto)
        {
            if (fila == null || !fila.Table.Columns.Contains(nombreColumna) || fila[nombreColumna] == null || fila[nombreColumna] is DBNull)
                return valorPorDefecto;

            Type tipo = typeof(T);
            if (fila[nombreColumna] is T || (tipo.IsEnum))
            {
                return (T)fila[nombreColumna];
            }
            else if (tipo == typeof(char) || tipo == typeof(char?))
            {
                string c = fila[nombreColumna] as string;
                if (c != null && c != "")
                    return (T)(object)c[0];
            }
            else if (tipo == typeof(bool))
            {
                int? i = fila[nombreColumna] as int?;
                if (i.HasValue && i == 1)
                    return (T)(object)true;
                else return (T)(object)false;
            }
            else if (tipo.Name.StartsWith("Nullable"))
            {
                var tipoNulable = System.Nullable.GetUnderlyingType(tipo);
                if (tipoNulable != null && tipoNulable.IsValueType && (tipoNulable.IsAssignableFrom(fila[nombreColumna].GetType()) || tipoNulable.IsEnum))
                {
                    return (T)Enum.ToObject(tipoNulable, fila[nombreColumna]);
                }
            }
            else
            {
                return (T)Convert.ChangeType(fila[nombreColumna], tipo);
            }
            return valorPorDefecto;
        }

        /// <summary>
        /// Retorna el valor del campo especificado.
        /// </summary>
        /// <typeparam name="T">Tipo</typeparam>
        /// <param name="fila"></param>
        /// <param name="nombreColumna">Nombre de la columna</param>
        /// <returns></returns>
        public static T Valor<T>(this DataRow fila, string nombreColumna)
        {
            return Valor<T>(fila, nombreColumna, default(T));
        }
    }
}
