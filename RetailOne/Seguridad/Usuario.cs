using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RetailOne.Utilidades;
using RetailOne.Configuracion;

namespace RetailOne.Seguridad
{
    [Serializable]
    public class Usuario
    {
        #region Fields y propiedades
        public string Login { get; set; }
        public string Contraseña { get; set; }
        public Estado EstadoUsuario { get; set; }
        #endregion

        #region Constructores
        public Usuario() { EstadoUsuario = Estado.Activo; }
        #endregion

        #region Métodos

        protected void Registrar()
        {
            
        }

        protected void Eliminar()
        {
            
        }

        protected void Modificar()
        {
           
        }

        public static Usuario Autenticar(Ambiente ambiente, string nombreUsuario, string contraseña)
        {
            using (RetailOne.Datos.ConexionDatos conn = new RetailOne.Datos.ConexionDatos(ambiente.Origen))
            {
                return new Usuario()
                {
                    Login = nombreUsuario,
                    Contraseña = RetailOne.Utilidades.Encriptacion.ConvertirABase64(contraseña)
                };
            }
        }
        #endregion
    }
}
