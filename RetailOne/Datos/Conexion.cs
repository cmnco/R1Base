using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RetailOne.Datos;
using RetailOne.Ejecucion;
using RetailOne.Utilidades;

namespace RetailOne.Datos
{
    [Serializable]
    public class Conexion : ConexionDatos
    {
        /// <summary>
        /// Crea un objeto <see cref="RetailOne.Datos.ConexionDatos"/> y establece el origen de datos a partir del ambiente de datos predeterminado.
        /// </summary>
        public Conexion()
            : base(Sistema.SesionActual != null ? Sistema.SesionActual.Ambiente.Origen : Sistema.Ambientes.Predeterminado.Origen)
        {

        }

        /// <summary>
        /// Crea un objeto <see cref="RetailOne.Datos.ConexionDatos"/> especificando el objeto de origen 
        /// de datos con que se establecerá la conexión.
        /// </summary>
        /// <param name="origen">Origen de datos con que se establecerá la conexión a la base de datos.</param>
        public Conexion(Origen origen) : base(origen)
        {

        }

        /// <summary>
        /// Crea un objeto <see cref="RetailOne.Datos.ConexionDatos"/> especificando un texto inicial de comando y el objeto de origen 
        /// de datos con que se establecerá la conexión.
        /// </summary>
        /// <param name="textoComando">Texto con que se inicializará el comando</param>
        /// <param name="origen">Origen de datos con que se establecerá la conexión a la base de datos.</param>
        public Conexion(string textoComando, Origen origen)
            : base(origen)
        {
            TextoComando = textoComando;
        }

        /// <summary>
        /// Crea un objeto <see cref="RetailOne.Datos.ConexionDatos"/> especificando un objeto de instancia. La conexión
        /// obtendrá el origen de datos de dicho objeto de instancia.
        /// </summary>
        /// <param name="instancia">Objeto de instancia con la información de origen de datos.</param>
        public Conexion(Instancia instancia) : base(instancia.OrigenDatos)
        {
            
        }
    }
}
