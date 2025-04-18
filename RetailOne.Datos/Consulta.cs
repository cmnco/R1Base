using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RetailOne.Utilidades;

namespace RetailOne.Datos
{
    /// <summary>
    /// Representa la agrupación de un conjunto de comandos de consulta que tienen un mismo propósito pero que están escritas
    /// para distintos proveedores de base de datos. De esta manera, se simplifica posteriormente el uso de múltiples
    /// orígenes de datos y plataformas con el mismo código de lógica.
    /// </summary>
    public partial class Consulta
    {
        #region Fields y propiedades

        /// <summary>
        /// Obtiene un diccionario con el texto de comando aplicable a cada <see cref="Proveedor"/>.
        /// La clave del diccionario es el proveedor.
        /// </summary>
        public Dictionary<Proveedor, string> TextosComando { get; private set; }

        /// <summary>
        /// Representa el proveedor base o predeterminado que se definió entre los comandos del objeto.
        /// </summary>
        public Proveedor Base { get; set; }

        #endregion

        #region Constructores

        /// <summary>
        /// Construye una consulta definiendo los textos de comando de para los distintos proveedores que se prevén usar.
        /// </summary>
        /// <param name="proveedorBase">Define el proveedor predeterminado de entre los comandos definidos en el parámetro 
        /// <paramref name="comandos"/>.
        /// Si una conexión intenta cargar un objeto <see cref="Consulta"/> y en dicha consulta no hay definido un comando
        /// para el proveedor que tiene la conexión, se considerará el comando de este proveedor predeterminado.</param>
        /// <param name="comandos">Define el conjunto de comandos que representarán esta consulta para los distintos
        /// proveedores de base de datos.</param>
        public Consulta(Proveedor proveedorBase, params ComandoConsulta[] comandos)
        {
            Base = proveedorBase;
            TextosComando = new Dictionary<Proveedor, string>();
            foreach (ComandoConsulta par in comandos)
            {
                TextosComando.Add(par.Proveedor, par.TextoComando);
            }
        }

        #endregion

        #region Métodos de instancia
        /// <summary>
        /// Retorna el texto del comando definido para el proveedor del parámetro <paramref name="proveedor"/>.
        /// Si no se encuentra un texto de comando para dicho proveedor, se retorna el comando del proveedor predeterminado
        /// (dada la propiedad <see cref="Base"/>).
        /// </summary>
        /// <param name="proveedor">Proveedor para el cual se buscará un comando definido.</param>
        /// <returns>Texto del comando definido para el valor del parámetro <paramref name="proveedor"/>.
        /// Si no se definió un comando para dicho proveedor, se retornará el texto del comando para el proveedor base.</returns>
        public string ObtenerTextoComando(Proveedor proveedor)
        {
            string comando = null;
            if (TextosComando.ContainsKey(proveedor))
            {
                comando = TextosComando[proveedor];
            }
            else if (TextosComando.ContainsKey(Base))
            {
                if(proveedor == Proveedor.Hana)
                {
                    TextosComando.Add(proveedor, ComandoConsulta.ValidarComandoHana(TextosComando[Base]));
                    comando = TextosComando[proveedor];
                }
                else
                {
                    comando = TextosComando[Base];
                }
            }
            return comando;
        }

        /// <summary>
        /// Reemplaza una cadena de texto incluida en los comandos de todos los proveedores con una nueva cadena de reemplazo.
        /// </summary>
        /// <param name="original">Cadena original que será reemplazada.</param>
        /// <param name="reemplazo">Cadena de reemplazo.</param>
        /// <returns>Retorna la referencia a la consulta actual (<c>this</c>), permitiendo el encadenamiento de llamados a función.</returns>
        public Consulta ReemplazarTexto(string original, string reemplazo)
        {
            foreach (var item in TextosComando.Keys.ToArray())
            {
                ReemplazarTexto(item, original, reemplazo);
            }
            return this;
        }

        /// <summary>
        /// Reemplaza una cadena de texto incluida en el comando de un proveedor de datos con una nueva cadena de reemplazo.
        /// </summary>
        /// <param name="original">Cadena original que será reemplazada.</param>
        /// <param name="reemplazo">Cadena de reemplazo.</param>
        /// <returns>Retorna la referencia a la consulta actual (<c>this</c>), permitiendo el encadenamiento de llamados a función.</returns>
        public Consulta ReemplazarTexto(Proveedor proveedor, string original, string reemplazo)
        {
            if (this.TextosComando.ContainsKey(proveedor))
            {
                TextosComando[proveedor] = TextosComando[proveedor].Replace(original, reemplazo);
            }
            return this;
        }
        #endregion
    }

    /// <summary>
    /// Representa el texto de comando definido para un proveedor de base de datos.
    /// </summary>
    public class ComandoConsulta
    {
        #region Fields y propiedades

        /// <summary>
        /// Obtiene el proveedor de base de datos para el cual se definió el comando.
        /// </summary>
        public Proveedor Proveedor { get; private set; }

        /// <summary>
        /// Obtiene el texto del comando.
        /// </summary>
        public string TextoComando { get; protected set; }

        #endregion

        #region Constructores

        /// <summary>
        /// Protegido. Construye un comando especificando únicamente el proveedor.
        /// </summary>
        /// <param name="proveedor">Proveedor de datos para el cual será definido el texto de comando</param>
        protected ComandoConsulta(Proveedor proveedor)
        {
            Proveedor = proveedor;
        }

        /// <summary>
        /// Construye un comando especificando el texto del comando y el proveedor para el cual fue escrito.
        /// </summary>
        /// <param name="proveedor">Proveedor de datos para el cual será definido el texto de comando</param>
        /// <param name="textoComando">Texto del comando</param>
        public ComandoConsulta(Proveedor proveedor, string textoComando)
        {
            Proveedor = proveedor;
            if (Proveedor == Proveedor.Hana)
                TextoComando = ValidarComandoHana(textoComando);
            else
                TextoComando = textoComando;
        }

        public static string ValidarComandoHana(string comando)
        {
            if (string.IsNullOrEmpty(comando)) return string.Empty;

            return comando.Replace("[", "\"")
                .Replace("]", "\"")
                .Replace("ISNULL", "IFNULL")
                .Replace("@p", ":p");
        }
        #endregion
    }
}