using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace RetailOne.Utilidades
{
    /// <summary>
    /// Identifica una cadena de texto con una cultura específica
    /// </summary>
    [Serializable]
    public struct Cadena
    {
        /// <summary>
        /// Crea un objeto de tipo <see cref="Cadena"/> relacionando un texto con la cultura/lenguaje en la que está expresado.
        /// </summary>
        /// <param name="idCultura">ID de la cultura según el estándar RFC 1766 (por ejemplo, '<c>en-US</c>' o '<c>es-MX</c>'</param>
        /// <param name="texto">Texto expresado en la cultura/lenguaje indicado por el parámetro <paramref name="idCultura"/></param>
        public Cadena(string idCultura, string texto)
            : this()
        {
            IDCultura = idCultura;
            Texto = texto;
        }

        /// <summary>
        /// Obtiene el ID de la cultura con que se inicializó la cadena.
        /// </summary>
        public string IDCultura { get; private set; }

        /// <summary>
        /// Obtiene el texto con que se inicializó el objeto.
        /// </summary>
        public string Texto { get; private set; }
    }

    /// <summary>
    /// Representa una colección de cadenas que son equivalentes en distintos lenguajes/culturas.
    /// </summary>
    [Serializable]
    public struct Cadenas
    {
        /// <summary>
        /// Representa el ID de la cultura usada en representaciones propias de código (no son usadas
        /// para representaciones en la interfaz de usuario y, por lo tanto, son transparentes al usuario).
        /// Es útil para unificar representaciones numéricas, de fecha y hora entre las capas de datos y modelo de negocio.
        /// </summary>
        public const string CulturaNeutra = "es-MX";

        /// <summary>
        /// ID de la cultura Español-Venezuela (es-MX), de uso frecuente
        /// </summary>
        public const string Español = "es-MX";
        /// <summary>
        /// ID de la cultura Inglés-Estados Unidos (en-US), de uso frecuente
        /// </summary>
        public const string English = "en-US";

        /// <summary>
        /// Obtiene el ID de cultura que se especificó como neutra al crear la colección de cadenas.
        /// </summary>
        public string IDCulturaNeutra { get; private set; }
        private Dictionary<string, string> cadenas;
        private string cadenaNeutra;

        private Type tipo;
        private string id;

        /// <summary>
        /// Crea una colección de cadenas relacionadas especificando una o más objetos de tipo <see cref="Cadena"/> (cada uno,
        /// representando un mismo texto o idea pero en distintas culturas). Se indica el ID de una cultura 'neutra' que definirá alguna de las 
        /// cadenas como predeterminada.
        /// </summary>
        /// <param name="idCulturaNeutra">ID de la cultura que, a efectos de esta colección, servirá como neutra o predeterminada.</param>
        /// <param name="cadenas">Conjunto de cadenas localizadas que conformarán la colección</param>
        public Cadenas(string idCulturaNeutra, params Cadena[] cadenas)
            : this()
        {
            tipo = null;
            id = null;
            this.cadenas = cadenas.ToDictionary(x => x.IDCultura, x => x.Texto);
            if (this.cadenas.ContainsKey(idCulturaNeutra))
                cadenaNeutra = this.cadenas[idCulturaNeutra];
            else
                cadenaNeutra = this.cadenas.Values.FirstOrDefault() ?? "";
            IDCulturaNeutra = idCulturaNeutra;
        }



        /// <summary>
        /// Crea una colección de cadenas relacionadas especificando una o más objetos de tipo <see cref="Cadena"/> (cada uno,
        /// representando un mismo texto o idea pero en distintas culturas). Se indica el ID de una cultura 'neutra' que definirá alguna de las 
        /// cadenas como predeterminada. Si el objeto será establecido como atributo estático de una clase, se identifica
        /// dicha clase con el parámetro <paramref name="tipo"/> y el nombre del atributo estático con el parámetro <paramref name="idCadena"/>.
        /// Esto habilita el objeto para poder identificarse con un ID único que permitirá referenciar nuevamente a la colección completa.
        /// </summary>
        /// <param name="tipo">Objeto de tipo <see cref="System.Type"/> que representa a la clase donde se definió como atributo estático la colección de cadenas.</param>
        /// <param name="idCadena">ID de la cadena, que deberá coincidir con el nombre del atributo estático donde ésta se almacenó.</param>
        /// <param name="idCulturaNeutra">ID de la cultura que, a efectos de esta colección, servirá como neutra o predeterminada.</param>
        /// <param name="cadenas">Conjunto de cadenas localizadas que conformarán la colección</param>
        //public Cadenas(Type tipo, string idCadena, string idCulturaNeutra, params Cadena[] cadenas)
        //    : this(idCulturaNeutra, cadenas)
        //{
        //    this.tipo = tipo;
        //    this.id = idCadena;
        //}

        /// <summary>
        /// Se genera la representación definitiva del texto encapsulado en la colección, con el uso de
        /// un ID de cultura específico y los argumentos de formato que los textos pueden exigir.
        /// </summary>
        /// <param name="idCultura">ID de cultura en que se quiere obtener el texto de la colección. Si este parámetro
        /// vale <see langword="null"/>, se intentará identificar la cultura actual con el uso del delegado <see cref="IdentificarIDCultura"/></param>
        /// <param name="argumentos">Argumentos que el texto puede requerir para formatearlo</param>
        /// <returns>Cadena localizada en la cultura especificada por el parámetro <paramref name="idCultura"/>, si dicha cultura
        /// estaba incluida en la colección. El texto reemplaza las marcas de argumentos (por ejemplo, '{0}') con los
        /// objetos indicados por el parámetro <paramref name="argumentos"/></returns>
        public string Leer(string idCultura, params object[] argumentos)
        {
            if (idCultura != null && cadenas.ContainsKey(idCultura))
                return String.Format(cadenas[idCultura], argumentos);
            idCultura = null;

            if (IdentificarIDCultura != null)
            {
                foreach (var item in IdentificarIDCultura.GetInvocationList())
                {
                    idCultura = (string)item.DynamicInvoke();
                    if (idCultura != null && cadenas.ContainsKey(idCultura))
                    {
                        break;
                    }
                }
            }
            if (idCultura != null && cadenas.ContainsKey(idCultura))
            {
                return String.Format(cadenas[idCultura], argumentos);
            }
            return String.Format(cadenaNeutra, argumentos);
        }

        /// <summary>
        /// Representa de forma implícita una colección de cadenas como una cadena de texto usando la cultura que se 
        /// haya especificado como neutra.
        /// </summary>
        /// <param name="cadenas">Colección de cadenas que se convertirá a texto.</param>
        /// <returns>Texto de la cultura neutra especificada en la colección del parámetro <paramref name="cadenas"/></returns>
        public static implicit operator string(Cadenas cadenas)
        {
            return cadenas.Leer(null);
        }

        /// <summary>
        /// Crea de forma implícita una colección de cadenas con una variable de tipo <see cref="System.String"/>.
        /// </summary>
        /// <param name="cadena">Único texto que conformará la colección de cadenas; este se inicializará usando como cultura neutra la constante <see cref="CulturaNeutra"/></param>
        /// <returns>Objeto de tipo <see cref="Cadenas"/> con el texto especificado por el parámetro <paramref name="cadena"/> como cultura neutra.</returns>
        public static implicit operator Cadenas(string cadena)
        {
            return new Cadenas(Cadenas.CulturaNeutra, new Cadena(Cadenas.CulturaNeutra, cadena));
        }

        /// <summary>
        /// Representa en la cultura neutra al texto con el uso de argumentos de formato.
        /// </summary>
        /// <param name="argumentos">Argumentos de formato exigidos por el texto</param>
        /// <returns>Texto representado en la cultura neutra</returns>
        public string this[params object[] argumentos]
        {
            get
            {
                return Leer(null, argumentos);
            }
        }

        /// <summary>
        /// Obtiene una cadena dado el tipo de la clase en la que se declaró el atributo estático <see cref="Cadenas"/>
        /// y el id de la cadena (nombre del atributo estático).
        /// </summary>
        /// <param name="tipoCadenas">CssClass en la que se buscará el atributo estático cuyo nombre sea igual al valor del parámetro <paramref name="id"/></param>
        /// <param name="id">Nombre del atributo estático que posee la cadena a obtener</param>
        /// <param name="argumentos">Argumentos opcionales que podría requerir la cadena para su formateo</param>
        /// <returns>Retorna la cadena identificada en la cultura actual o, en su defecto, en la cultura neutra</returns>
        public static string Leer(Type tipoCadenas, string id, params object[] argumentos)
        {
            if (tipoCadenas == null || id.EsNuloOVacio()) return id;
            var campo = tipoCadenas.ObtenerCampo(id);
            if (campo == null) return id;
            return ((Cadenas)campo.GetValue(null)).Leer(null, argumentos);
        }

        /// <summary>
        /// Obtiene o establece el delegado con que se podrá identificar, de acuerdo al contexto de ejecución, el ID de la cultura
        /// que se utilizará para representar las colecciones de cadenas.
        /// </summary>
        public static IdentificarCadenaPredeterminada IdentificarIDCultura { get; set; }
    }

    /// <summary>
    /// Delegado que determina las posibles funciones con que se identificará la cultura actual en un contexto de ejecución.
    /// </summary>
    /// <returns>ID de la cultura que corresponda al contexto de ejecución actual</returns>
    public delegate string IdentificarCadenaPredeterminada();

    public class Clase
    {
        public delegate string Prueba(string dato);

        public void Test2(Prueba test)
        {
            if (test != null)
                test("Prueba");
        }
    }
}