using System;
using System.Collections.Generic;
using RetailOne.Configuracion;
using RetailOne.Seguridad;

namespace RetailOne.Ejecucion
{
    /// <summary>
    /// Define la base de las sesiones que inicien los usuarios para acceder al sistema.
    /// Esta clase es abstracta.
    /// </summary>
    [Serializable]
    public class SesionBase
    {
        #region Fields y propiedades

        /// <summary>
        /// Obtiene el ID de la sesión.
        /// </summary>
        public virtual string ID { get { return ""; } }

        /// <summary>
        /// Obtiene la información del usuario que inició la sesión.
        /// </summary>
        public IUsuario Usuario { get; private set; }

        /// <summary>
        /// Obtiene el ambiente de datos en que se inició la sesión.
        /// </summary>
        public Ambiente Ambiente { get; private set; }

        /// <summary>
        /// Obtiene la cultura aplicada a la sesión.
        /// </summary>
        public Cultura Cultura
        {
            get { return Ambiente.IdiomaPredeterminado; }
        }

        /// <summary>
        /// Obtiene un booleano que indica si la sesión está en proceso de ser cerrada.
        /// </summary>
        public bool CerrandoSesion { get; protected set; }

        private Dictionary<string, object> datosAdicionales = new Dictionary<string,object>();
        public Dictionary<string, object> DatosAdicionales { get { return datosAdicionales; } }

        public DateTime? FechaHoraUltimaInteraccion { get; set; }

        #endregion

        #region Constructores

        /// <summary>
        /// Construye una sesión estableciendo su ambiente de datos y el usuario ya autenticado
        /// </summary>
        /// <param name="ambiente">Ambiente de datos que se usará en la sesión</param>
        /// <param name="usuario">Usuario que inició la sesión</param>
        public SesionBase(Ambiente ambiente, IUsuario usuario)
        {
            Ambiente = ambiente;
            Usuario = usuario;
        }

        #endregion

        #region Eventos

        /// <summary>
        /// Evento que se ejecutará durante el inicio de cada sesión.
        /// </summary>
        public static event EventoSesion AlIniciar;

        /// <summary>
        /// Evento que se ejecutará durante el cierre de cada sesión.
        /// </summary>
        public static event EventoSesion AlCerrar;

        /// <summary>
        /// Ejecuta el evento <see cref="AlIniciar"/> si tiene métodos suscritos.
        /// </summary>
        protected virtual void EjecutarEventoIniciar()
        {
            if (AlIniciar != null)
            {
                AlIniciar(new DatosEventoSesion(this));
            }
        }

        /// <summary>
        /// Ejecuta el evento <see cref="AlCerrar"/> si tiene métodos suscritos.
        /// </summary>
        protected virtual void EjecutarEventoCerrar()
        {
            if (AlCerrar != null)
            {
                AlCerrar(new DatosEventoSesion(this));
            }
        }

        #endregion

        #region Métodos
        
        /// <summary>
        /// Cierra la sesión.
        /// </summary>
        public virtual void CerrarSesion()
        {
            EjecutarEventoCerrar();
        }

        /// <summary>
        /// Inicializa la sesión.
        /// </summary>
        internal virtual void Inicializar()
        {
            EjecutarEventoIniciar();

            //Registrar en eventos del sistema
        }
        #endregion
    }

    /// <summary>
    /// Define la firma de los eventos aplicables a las sesiones.
    /// </summary>
    /// <param name="datos">Objeto con los datos de la sesión</param>
    public delegate void EventoSesion(DatosEventoSesion datos);
}