using System;

namespace RetailOne.Ejecucion
{
    /// <summary>
    /// CssClass que provee la información de sesión a los métodos que capturan los eventos de sesión.
    /// </summary>
    [Serializable]
    public class DatosEventoSesion
    {
        #region Fields y propiedades

        /// <summary>
        /// Obtiene la instancia de la sesión en la que se capturó el evento.
        /// </summary>
        public SesionBase Sesion { get; private set; }

        #endregion

        #region Constructores

        /// <summary>
        /// Construye una clase de información de sesión.
        /// </summary>
        /// <param name="sesion"></param>
        public DatosEventoSesion(SesionBase sesion)
        {
            Sesion = sesion;
        }

        #endregion
    }
}