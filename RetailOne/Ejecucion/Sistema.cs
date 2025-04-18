using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RetailOne.Utilidades;
using RetailOne.Configuracion;
using System.Xml.Linq;
using RetailOne.Seguridad;

namespace RetailOne.Ejecucion
{
    public static partial class Sistema
    {
        #region Constantes
        /// <summary>
        /// Espacio de nombres que identifica el esquema del XML de configuración para el sistema RetailOne (generalmente llamado <c>RetailOne.config</c> en la raíz de la aplicación).
        /// </summary>
        public const string NsConfiguración = "http://www.soluone.com/esquemas/retailone/config";
        #endregion

        #region Fields y propiedades

        /// <summary>
        /// Obtiene el nombre del sistema especificado en el archivo de configuración. Generalmente, identifica al sistema en los títulos de sus ventanas.
        /// </summary>
        public static string Nombre { get; private set; }

        /// <summary>
        /// Obtiene la versión del sistema especificada en el archivo de configuración. En lugar de ser un dato numérico, es de tipo <see langword="string"/>
        /// porque es puramente informativo y permite especificar datos no numéricos asociados a la versión (como indicaciones 'beta' o similares).
        /// </summary>
        public static string Version { get; private set; }

        /// <summary>
        /// Obtiene la abreviatura del sistema especificada en el archivo de configuración. Esta abreviatura es útil a la hora de identificar cookies y
        /// otras variables de aplicación.
        /// </summary>
        public static string Abreviatura { get; private set; }

        /// <summary>
        /// Indica si se encuentra activo el modo depuración. Tener en consideración para el uso de log extendido o registro en bitácora de acciones puntuales en código,
        /// que permitan dar seguimiento de la ejecución del mismo e identificar posibles bugs.
        /// Esta propiedad queda descontinuada para futuras versiones, se reemplaza por la propiedad: ModoDeputacion.
        /// </summary>
        [Obsolete]
        public static bool ModoDesarrollo { get { return ModoDepuracion; } }

        /// <summary>
        /// Indica si se encuentra activo el modo depuración. Tener en consideración para el uso de log extendido o registro en bitácora de acciones puntuales en código,
        /// que permitan dar seguimiento de la ejecución del mismo e identificar posibles bugs.
        /// Esta propiedad queda descontinuada para futuras versiones, se reemplaza por la propiedad: ModoDepuracion.
        /// </summary>
        public static bool ModoDepuracion { get; private set; }

        /// <summary>
        /// Obtiene el estado actual del sistema. Si el valor de la propiedad es <see langword="true"/>, el sistema ya fue correctamente inicializado.
        /// De lo contrario, retorna <see langword="false"/>.
        /// </summary>
        public static bool Inicializado { get; private set; }

        /// <summary>
        /// Obtiene el directorio raíz de aplicación del sistema. Este se deduce a partir del directorio del archivo <c>RetailOne.config</c>,
        /// que es facilitado en el método de inicialización.
        /// </summary>
        public static string DirectorioRaíz { get; private set; }

        /// <summary>
        /// Obtiene la url raíz del sitio.
        /// </summary>
        public static string UrlRaíz { get; private set; }

        /// <summary>
        /// Obtiene la cultura del contexto de ejecución actual. Si se consulta en el contexto de ejecución de un usuario y la 
        /// propiedad <see cref="SesionActual"/> retorna un objeto, se devolverá la cultura de dicha sesión actual.
        /// En caso contrario, retornará la cultura neutra definida por defecto ex-MX.
        /// </summary>
        public static Cultura CulturaActual
        {
            get { return (Sistema.SesionActual.SiNoEsNulo(x => x.Cultura) ?? Cultura.CulturaNeutra); }
        }

        private static ColeccionAmbientes ambientes = new ColeccionAmbientes();
        /// <summary>
        /// Colección con los ambientes de datos establecidos en el archivo de configuración (<c>RetailOne.config</c>).
        /// </summary>
        public static ColeccionAmbientes Ambientes
        {
            get { return ambientes; }
        }

        private static ColeccionCulturas globalización = new ColeccionCulturas();
        /// <summary>
        /// Colección de configuraciones de cultura establecidas en el archivo de configuración (<c>RetailOne.config</c>),
        /// para la globalización o internacionalización de las aplicaciones del sistema.
        /// </summary>
        public static ColeccionCulturas Globalizacion
        {
            get { return globalización; }
        }

        private static SesionBase sesionActual;
        public static SesionBase SesionActual
        {
            get { return sesionActual; }
        }

        private static DiccionarioCadenas variablesConfiguración = new DiccionarioCadenas();
        /// <summary>
        /// Diccionario de cadenas con el conjunto de variables generales (ID/valor) definidos en el 
        /// archivo de configuración (<c>RetailOne.config</c>).
        /// </summary>
        public static DiccionarioCadenas VariablesConfiguracion
        {
            get { return variablesConfiguración; }
        }

        public static string DireccionArchivoConfiguracion { get; private set; }

        /// <summary>
        /// Obtiene el contenido del archivo de configuración del sistema (<c>RetailOne.config</c>) luego de haber sido parseado y validado.
        /// </summary>
        public static XElement ContenidoArchivoConfiguracion { get; private set; }

        /// <summary>
        /// Obtiene el espacio de nombres de tipo <see cref="System.Xml.Linq.XNamespace"/> correspondiente al archivo de configuración.
        /// </summary>
        public static XNamespace EspacioNombresArchivoConfiguracion { get; private set; }
        #endregion

        #region Métodos
        /// <summary>
        /// Autentica las credenciales del usuario en el ambiente de datos especificado y, en caso de que sean válidas,
        /// inicializa la sesión.
        /// </summary>
        /// <param name="ambiente">Ambiente de datos en el que se iniciará sesión.</param>
        /// <param name="nombreUsuario">Nombre del usuario que se autenticará</param>
        /// <param name="contraseña">Contraseña del usuario que se autenticará</param>
        /// <returns>Dependiendo del ambiente de ejecución del sistema (web u otro), se devolverá una derivación
        /// distinta de la clase <see cref="SesionBase"/>.</returns>
        public static SesionBase IniciarSesion(Ambiente ambiente, IUsuario usuario)
        {
            if (usuario != null && usuario.Autenticar(ambiente))
            {
                sesionActual = new SesionBase(ambiente, usuario);
                sesionActual.Inicializar();
                return sesionActual;
            }
            else
            {
                return null;
            }
            //IUsuario usuario = Seguridad.Usuario.Autenticar(ambiente, nombreUsuario, contraseña);
            //sesiónActual = new SesionBase(ambiente, usuario);
            //sesiónActual.Inicializar();
        }

        /// <summary>
        /// Permite habilitar o deshabilitar el modo depuración.
        /// </summary>
        /// <param name="habilitado"></param>
        public static void HabilitarModoDepuracion(bool habilitado)
        {
            ModoDepuracion = habilitado;
            RetailOne.Datos.Depuracion.Activa = habilitado;
        }

        #region Registro de eventos en log del sistema
        public static void GuardarError(Instancia instancia, string descripcion, params string[] detalles)
        {
            RegistroEvento.Registrar(instancia, TipoEvento.Error, descripcion, detalles);
        }

        public static void GuardarError(string descripcion, Exception exception)
        {
            Instancia instancia = Instancia.Predeterminada;
            RegistroEvento.RegistrarError(instancia, descripcion, exception);
        }

        public static void GuardarEvento(TipoEvento tipo, string descripcion, params string[] detalles)
        {
            Instancia instancia = Instancia.Predeterminada;
            RegistroEvento.Registrar(instancia, tipo, descripcion, detalles);
        }

        public static void GuardarEvento(Instancia instancia, TipoEvento tipo, string descripcion, params string[] detalles)
        {
            RegistroEvento.Registrar(instancia, tipo, descripcion, detalles);
        }
        #endregion


        #endregion

        #region Eventos
        /// <summary>
        /// Evento que se ejecutará antes de que el sistema se reinicie por cambio de algún archivo del sistema (.config, .dll, .asax, entre otros).
        /// </summary>
        public static event Action ReinicializarSistema;

        private static void EjecutarEventoReinicializarSistema()
        {
            if (ReinicializarSistema != null) ReinicializarSistema();
        }

        /// <summary>
        /// Evento que se ejecutará apenas el sistema complete su inicialización.
        /// </summary>
        public static event Action SistemaInicializado;

        private static void EjecutarEventoSistemaInicializado()
        {
            if (SistemaInicializado != null) SistemaInicializado();
        }
        #endregion
    }

    /// <summary>
    /// Colección de los esquemas que se usarán para validar los archivos XML. Su estructura es un diccionario cuya clave
    /// está pensada para ser un espacio de nombres dado; y su valor, una colección de los nombres de archivo que apuntan a los
    /// .xsd cuyo espacio de nombres coincide con la clave.
    /// </summary>
    [Serializable]
    public class ColeccionEsquemasXml : Dictionary<string, List<string>>
    {
        public ColeccionEsquemasXml() { }
        public ColeccionEsquemasXml(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Un simple diccionario en el que la clave y el valor son cadenas de tipo <see langword="string"/>.
    /// </summary>
    [Serializable]
    public class DiccionarioCadenas : Dictionary<string, string>
    {
        //private Dictionary<string, string> diccionarioDeValores = new Dictionary<string, string>();
        public DiccionarioCadenas() { }
        public DiccionarioCadenas(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public new string this[string nombre]
        {
            get
            {
                if (base.ContainsKey(nombre))
                    return base[nombre];

                return null;
            }
            set
            {
                base[nombre] = value;
            }
        }

        public T Obtener<T>(string nombre)
        {
            string valor = this[nombre];
            if (string.IsNullOrEmpty(valor)) return default(T);

            Type tipo = typeof(T);

            if (tipo == typeof(bool))
            {
                valor = valor.ToUpper();
                return (T)(object)(valor.Equals("TRUE") || valor.Equals("Y") || valor.Equals("1"));
            }
            else
            {
                return valor.CambiarATipo<T>(default(T));
            }
        }
    }
}
