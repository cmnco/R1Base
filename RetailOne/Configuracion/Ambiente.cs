using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using RetailOne.Datos;
using RetailOne.Utilidades;
using RetailOne.Ejecucion;

namespace RetailOne.Configuracion
{
    /// <summary>
    /// CssClass que agrupa la cultura y orígenes de datos que se usarán en un ambiente de datos del sistema.
    /// <para>Los distintos ambientes de datos podrán apuntar a orígenes de datos y culturas distintas, permitiéndole al usuario
    /// elegir en cuál de dichos ambientes iniciar sesión y, de esa manera, interactuar a través del mismo ambiente de ejecución
    /// con distintos transfondos de datos (por ejemplo, distinguiendo un ambiente para datos de producción y otro para pruebas).</para>
    /// </summary>
    [Serializable]
    public class Ambiente
    {
        #region Fields y propiedades

        /// <summary>
        /// Obtiene el identificador del ambiente de datos.
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// Obtiene el nombre del ambiente de datos.
        /// </summary>
        public string Nombre { get; private set; }

        /// <summary>
        /// Obtiene la cultura predeterminada del ambiente de datos.
        /// </summary>
        public Cultura IdiomaPredeterminado { get; private set; }

        /// <summary>
        /// Obtiene o establece el origen de datos predeterminado del ambiente.
        /// </summary>
        public Origen Origen { get; set; }
        #endregion

        #region Constructores

        /// <summary>
        /// Construye un objeto de ambiente de datos dadas sus características básicas.
        /// </summary>
        /// <param name="id">Identificador del ambiente</param>
        /// <param name="nombre">Nombre que se dará al ambiente</param>
        /// <param name="idiomaPredeterminado">Identificador de la cultura predeterminada que tendrá el ambiente</param>
        /// <param name="datosInstanciaDirectorioUsuarios">Datos de la instancia (<c>IDOrigen.IDInstancia</c>) con que se manejará el directorio de usuarios</param>
        public Ambiente(string id, string nombre, string idiomaPredeterminado)
        {
            ID = id;
            Nombre = nombre;
            if (Sistema.Globalizacion.Contains(idiomaPredeterminado))
            {
                IdiomaPredeterminado = Sistema.Globalizacion[idiomaPredeterminado];
            }
            else
            {
                IdiomaPredeterminado = Cultura.CulturaNeutra;
            }
        }

        #endregion
    }

    /// <summary>
    /// Colección de ambientes de datos, identificadas por el valor de su propiedad <c>ID</c>.
    /// </summary>
    [Serializable]
    public class ColeccionAmbientes : Coleccion<string, Ambiente>
    {
        public ColeccionAmbientes() : base(x => x.ID) { }

        private Ambiente predeterminado;
        /// <summary>
        /// Obtiene el ambiente de datos marcado como predeterminado.
        /// </summary>
        public Ambiente Predeterminado {
            get
            {
                if (predeterminado == null)
                    predeterminado = this.FirstOrDefault();
                return predeterminado;
            }
            set
            {
                predeterminado = value;
            }
        }
    }
}