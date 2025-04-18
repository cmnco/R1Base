using RetailOne.Utilidades.Recursos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailOne.Utilidades
{
    /// <summary>
    /// Provee métodos de utilidad para el manejo de números
    /// </summary>
    public static class UtilidadesNumeros
    {
        #region EsPositivo
        /// <summary>
        /// Método que valida que el valor sea igual o mayor a 0
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        public static bool EsPositivo(this int valor)
        {
            return (valor >= 0);
        }

        /// <summary>
        /// Método que valida que el valor sea igual o mayor a 0
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        public static bool EsPositivo(this decimal valor)
        {
            return (valor >= 0);
        }
        #endregion

        #region EsMayorQueCero
        /// <summary>
        /// Método que valida que el valor sea mayor a 0
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        public static bool EsMayorQueCero(this int valor)
        {
            return (valor > 0);
        }

        /// <summary>
        /// Método que valida que el valor sea mayor a 0
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        public static bool EsMayorQueCero(this decimal valor)
        {
            return (valor > 0);
        }
        #endregion

        #region ValorRedondeado        
        /// <summary>
        /// Método que regresa el valor redondeado a los decimales proporcionados
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="decimales"></param>
        /// <returns></returns>
        public static decimal Redondear(this decimal valor, int decimales)
        {
            return Math.Round(valor, decimales);
        }

        /// <summary>
        /// Método que regresa el valor redondeado a los decimales proporcionados
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="decimales"></param>
        /// <returns></returns>
        public static double Redondear(this double valor, int decimales)
        {
            return Math.Round(valor, decimales);
        }
        #endregion
    }
}
