using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace RetailOne.Utilidades
{
    /// <summary>
    /// Provee métodos de extensión para el manejo de tipos de datos.
    /// </summary>
    public static class UtilidadesTipos
    {
        /// <summary>
        /// Determina si un tipo es o "no nulable".
        /// </summary>
        /// <param name="tipo"><see cref="System.Type"/> con la definición del tipo a evaluar</param>
        /// <returns>Retorna <c>true</c> si el tipo es nulable (como int? ó Nullable&lt;bool&gt;); de lo contrario, retorna <c>false</c></returns>
        public static bool EsTipoNulable(this Type tipo)
        {
            return (tipo.IsGenericType && tipo.
              GetGenericTypeDefinition().Equals
              (typeof(Nullable<>)));
        }

        /// <summary>
        /// Devuelve un objeto <see cref="System.Reflection.FieldInfo"/> con la información de un campo de un tipo,
        /// independientemente de la clase de la jerarquía de herencia en la que se haya declarado dicho campo.
        /// </summary>
        /// <param name="tipo"><see cref="System.Type"/> del cual se desea extraer el campo</param>
        /// <param name="nombreCampo">Nombre del campo que se va a buscar</param>
        /// <returns>Objeto con la representación por defecto del tipo de datos</returns>
        public static FieldInfo ObtenerCampo(this Type tipo, string nombreCampo)
        {
            return ObtenerCampo(tipo, nombreCampo, null);
        }

        /// <summary>
        /// Devuelve un objeto <see cref="System.Reflection.FieldInfo"/> con la información de un campo de un tipo,
        /// independientemente de la clase de la jerarquía de herencia en la que se haya declarado dicho campo.
        /// </summary>
        /// <param name="tipo"><see cref="System.Type"/> del cual se desea extraer el campo</param>
        /// <param name="nombreCampo">Nombre del campo que se va a buscar</param>
        /// <param name="opciones">Opciones para la búsqueda</param>
        /// <returns>Objeto con la representación por defecto del tipo de datos</returns>
        public static FieldInfo ObtenerCampo(this Type tipo, string nombreCampo, BindingFlags? opciones)
        {
            if (tipo == null || nombreCampo.EsNuloOVacio()) return null;
            FieldInfo salida = null;
            while (tipo != null && tipo != typeof(Object))
            {
                salida = opciones.HasValue ? tipo.GetField(nombreCampo, opciones.Value) : tipo.GetField(nombreCampo);
                if (salida != null) return salida;
                tipo = tipo.BaseType;
            }
            return null;
        }

        /// <summary>
        /// Convertir un TimeSpan a valor numérico (Representacion utilizada por SAP para los campos hora en base de datos). 
        /// </summary>
        /// <param name="hora"></param>
        /// <returns></returns>
        public static short HoraANumero(TimeSpan? hora)
        {
            if (!hora.HasValue) return 0;

            string valor = hora.Value.Hours.ToString("00") + hora.Value.Minutes.ToString("00");
            
            return short.Parse(valor);
        }

        /// <summary>
        /// Convertir un TimeSpan a valor numérico (Representacion utilizada por SAP para los campos hora en base de datos). 
        /// </summary>
        /// <param name="hora"></param>
        /// <returns></returns>
        public static int HoraANumero(TimeSpan? hora, bool aplicanSegundos)
        {
            if (!hora.HasValue) return 0;

            string valor = hora.Value.Hours.ToString("00") + hora.Value.Minutes.ToString("00");
            if (aplicanSegundos)
                valor += hora.Value.Seconds.ToString("00");
            return int.Parse(valor);
        }

        /// <summary>
        /// Convertir un valor numérico a TimeSpan (Recuperar el objeto hora a partir del formato numérico utilizado por SAP en base de datos).
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        public static TimeSpan NumeroAHora(int? valor, bool aplicanSegundos = false)
        {
            if (!valor.HasValue)
                return new TimeSpan(0, 0, 0);

            if (aplicanSegundos)
            {
                int segundos = valor.Value % 100; valor = valor / 100;
                int minutos = valor.Value % 100; valor = valor / 100;
                int hora = valor.Value;
                return new TimeSpan(hora, minutos, segundos);
            }
            else
            {
                int minutos = valor.Value % 100;
                int hora = valor.Value / 100;
                return new TimeSpan(hora, minutos, 0);
            }
        }
    }
}