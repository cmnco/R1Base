using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetailOne.Configuracion;

namespace RetailOne.Seguridad
{
    public interface IUsuario
    {
        string Email { get; set; }
        string Contraseña { get; set; }
        string IDRol { get; set; }

        bool Autenticar(Ambiente ambiente);
        IUsuario Autenticar(Ambiente ambiente, string nombreUsuario, string contraseña);
    }
}
