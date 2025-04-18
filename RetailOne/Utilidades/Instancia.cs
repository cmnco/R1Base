using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RetailOne.Configuracion;
using RetailOne.Datos;
using RetailOne.Ejecucion;

namespace RetailOne.Utilidades
{
    /// <summary>
    /// Define la información de origen de datos que permitirá a las clases de distintas capas distinguir en qué base de datos
    /// se gestionarán los datos.
    /// <para>Básicamente, está conformado por:</para>
    /// <para>1. El ambiente de datos (de la sesión o contexto) en que está siendo gestionada una entidad con el uso de la instancia. Esta propiedad es opcional.</para>
    /// <para>2. El origen de datos, que representa una cadena de conexión a una base de datos específica. Esta propiedad es requerida.</para>
    /// <para>3. El identificador de la instancia ('<c>IDInstancia</c>'). Esta propiedad es opcional.</para>
    /// </summary>
    [Serializable]
    public class Instancia
    {
        #region Fields y propiedades

        /// <summary>
        /// Obtiene el identificador de la instancia, que generalmente se incorpora a las tablas de datos como una
        /// columna de la clave primaria, particionando las filas de la tabla de acuerdo a sus valores.
        /// </summary>
        public string IDInstancia { get; private set; }

        public string Nombre { get { return AmbienteDatos.SiNoEsNulo(x => x.Nombre); } }

        /// <summary>
        /// Obtiene el objeto de tipo <see cref="RetailOne.Datos.Origen"/> con la información de conexión a la base de datos a la que
        /// apunta la instancia.
        /// </summary>
        public Origen OrigenDatos {
            get { return this.AmbienteDatos.SiNoEsNulo(x => x.Origen); }
            set { this.AmbienteDatos.Origen = value; }
        }

        /// <summary>
        /// Obtiene el ambiente de datos desde el que se creó la instancia.
        /// </summary>
        public Ambiente AmbienteDatos { get; protected set; }

        /// <summary>
        /// Obtiene un booleano que indica si fue inicializada la instancia (si fue establecido correctamente un origen de datos
        /// y un identificador de instancia.
        /// </summary>
        public bool Inicializada
        {
            get { return !OrigenDatos.CadenaConexion.EsNuloOVacio() && !IDInstancia.EsNuloOVacio(); }
        }

        private static Instancia principal;
        /// <summary>
        /// Obtiene una definición de instancia llamada 'principal' o 'genérica', que apunta al origen de datos 'Principal' del ambiente de datos predeterminado
        /// con el identificador de instancia 'Principal' (su <c>DatosResumidos</c> sería 'Principal.Principal').
        /// </summary>
        public static Instancia Predeterminada
        {
            get

            {
                if (principal == null || !principal.Inicializada)
                {
                    Ambiente ambientePredeterminado = Sistema.Ambientes.SiNoEsNulo(x=>x.Predeterminado);
                    if(ambientePredeterminado != null)
                        principal = new Instancia(ambientePredeterminado.ID, ambientePredeterminado);
                }
                return principal;
            } 
        }

        #endregion

        #region Constructores

        /// <summary>
        /// Construye una instancia a partir un ambiente de datos.
        /// </summary>
        /// <param name="idInstancia">id de la instancia</param>
        /// <param name="ambiente">Ambiente de datos al que se asociará la instancia</param>
        public Instancia(string idInstancia, Ambiente ambiente)
        {
            if (ambiente == null || string.IsNullOrEmpty(ambiente.Origen.CadenaConexion))
            {
                throw new ArgumentNullException("Falta ambiente");
            }
            IDInstancia = string.IsNullOrEmpty(idInstancia) ? ambiente.ID : idInstancia;
            AmbienteDatos = ambiente;
        }

        /// <summary>
        /// Construye una instancia a partir de su origen de datos y el identificador de la instancia. En este caso se omite el
        /// ambiente de datos, por lo que dicha propiedad siempre valdrá <see cref="null"/>.
        /// </summary>
        /// <param name="idInstancia">Identificador de la instancia</param>
        /// <param name="origenDatos">Origen de datos que apunta a la base de datos que se usará en la instancia</param>
        public Instancia(string idInstancia, Origen origenDatos)
        {
            IDInstancia = string.IsNullOrEmpty(idInstancia) ? "Principal" : idInstancia;
            this.AmbienteDatos = new Ambiente(IDInstancia, origenDatos.NombreBD, Cultura.CulturaNeutra.Codigo);
            this.AmbienteDatos.Origen = origenDatos;
        }

        /// <summary>
        /// Construye una instancia a partir de su origen de datos.
        /// </summary>
        /// <param name="origenDatos">Origen de datos que apunta a la base de datos que se usará en la instancia</param>
        public Instancia(Origen origenDatos) 
            : this(null, origenDatos)
        {

        }

        /// <summary>
        /// Construye una instancia a partir de los identificadores de instancia y ambiente.
        /// </summary>
        /// <param name="idInstancia">Identificador de la instancia</param>
        /// <param name="idAmbiente">Identificador del ambiente de datos. Dado este id se buscará el objeto de ambiente de datos en la colección <see cref="Sistema.Ambientes"/></param>
        public Instancia(string idInstancia, string idAmbiente)
        {
            if(string.IsNullOrEmpty(idAmbiente) || !Sistema.Ambientes.Contains(idAmbiente))
            {
                throw new ArgumentNullException("Falta ambiente");
            }

            AmbienteDatos = Sistema.Ambientes[idAmbiente];
            IDInstancia = string.IsNullOrEmpty(idInstancia) ? AmbienteDatos.ID : idInstancia;
        }

        /// <summary>
        /// Construye una instancia a partir de un ambiente.
        /// </summary>
        /// <param name="ambiente"></param>
        public Instancia(Ambiente ambiente)
            :this(null, ambiente)
        {

        }

        public override string ToString()
        {
            return IDInstancia;
        }

        #endregion
    }
}
