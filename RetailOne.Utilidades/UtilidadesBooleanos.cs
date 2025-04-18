using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailOne.Utilidades
{
    /// <summary>
    /// Provee métodos de utilidad para el manejo de booleanos
    /// </summary>
    public static class UtilidadesBooleanos
    {
        /// <summary>
        /// Método que regresa 'Y' si el valor es True y 'N' si el valor es False
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        public static char ValorChar(this bool valor)
        {
            return (valor ? 'Y' : 'N');
        }
    }
}
