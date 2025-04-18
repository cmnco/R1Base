using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using RetailOne.Utilidades;


namespace RetailOne.Configuracion
{
    /// <summary>
    /// Establece las características de una cultura. Estas características ajustarán la manera de presentar datos numéricos, textuales
    /// y cronológicos en las interfaces de usuario, permitiéndole desarrollar aplicaciones globalizadas (que automáticamente ajusten su
    /// visualización a distintas culturas).
    /// </summary>
    [Serializable]
    public class Cultura 
    {
        #region Constantes
        private const string _formatoFechaNumerica = "yyyyMMdd";
        private const string _formatoFechaCorta = "dd/MM/yyyy";
        private const string _formatoFechaLarga = "dddd, d 'de' MMMM 'de' yyyy";
        private const string _formatoHoraNumerica = "HHmm";
        private const string _formatoHora = "hh:mm tt";
        private const string _separadorDecimal = ".";
        private const string _separadorMiles = ",";
        private const int _cantidadDecimales = 2;
        #endregion

        #region Fields y propiedades

        /// <summary>
        /// Obtiene o establece el identificador de la cultura. Esta propiedad encapsula a la propiedad <see cref="Codigo"/>,
        /// por lo que su valor siempre coincide con el de dicha propiedad.
        /// </summary>
        public string ID { get { return Codigo; } set { Codigo = value; } }

        /// <summary>
        /// Obtiene o establece el código de la cultura en el formato '<c>ii-LL</c>' (donde '<c>ii</c>' es la abreviatura en dos caracteres minúsculas del idioma y donde
        /// '<c>LL</c>' es la abreviatura en dos caracteres mayúsculas de la localización). '<c>en-US</c>' y '<c>es-VE</c>' son ejemplos de códigos de cultura.
        /// </summary>
        public string Codigo { get; private set; }

        /// <summary>
        /// Obtiene el nombre con que se identificará la cultura.
        /// </summary>
        public string Nombre { get; private set; }
        
        
        private CultureInfo infoCultura;

        /// <summary>
        /// Obtiene el objeto de información de cultura propio de la plataforma .NET Framework, inicializado dados los valores
        /// provistos para este objeto <see cref="Cultura"/>.
        /// </summary>
        
        private static Cultura culturaNeutra;
        /// <summary>
        /// Obtiene la definición de cultura neutra (es-VE), útil para uniformar los cambios de tipo entre números, fechas y cadenas de texto 
        /// en funciones de capas inferiores (que no llegan a mostrarse a nivel de interfaz de usuario).
        /// </summary>
        public static Cultura CulturaNeutra
        {
            get
            {
                if (culturaNeutra == null)
                {
                    if (Ejecucion.Sistema.Globalizacion.Count > 0)
                        culturaNeutra = Ejecucion.Sistema.Globalizacion.First();
                    else
                        culturaNeutra = new Cultura("es-MX", "Español (Mexico)", _formatoFechaCorta, _formatoFechaLarga, _formatoHora, _separadorDecimal, _separadorMiles, _cantidadDecimales);
                }
                return culturaNeutra;
            }
        }


        public CultureInfo InfoCultura
        {
            get
            {
                if (infoCultura == null)
                    InicializarCultura();
                return infoCultura;
            }
            private set
            {
                infoCultura = value;
            }
        }

        /// <summary>
        /// Obtiene el proveedor de formato de fechas propio de la plataforma .NET Framework, inicializado dados los valores
        /// provistos para este objeto <see cref="Cultura"/>.
        /// </summary>
        
        public IFormatProvider ProveedorFechas { get { return InfoCultura.DateTimeFormat; } }

        /// <summary>
        /// Obtiene el proveedor de formato de números propio de la plataforma .NET Framework, inicializado dados los valores
        /// provistos para este objeto <see cref="Cultura"/>.
        /// </summary>
        
        public IFormatProvider ProveedorNumeros { get { return InfoCultura.NumberFormat; } }

        /// <summary>
        /// Obtiene el formato en que se presentarán las fechas cortas (usando patrones tales como "<c>dd/MM/yyyy</c>", aceptados por los métodos de
        /// formato de fechas de .NET Framework).
        /// </summary>
        public string FormatoFechaCorta { get; private set; }

        /// <summary>
        /// Obtiene el formato en que se presentarán las fechas largas (usando patrones tales como "<c>dddd, d 'de' MMMM 'de' yyyy</c>", aceptados por los métodos de
        /// formato de fechas de .NET Framework).
        /// </summary>
        public string FormatoFechaLarga { get; private set; }

        /// <summary>
        /// Obtiene el formato en que se presentarán las horas (usando patrones tales como "hh:mm tt").
        /// </summary>
        public string FormatoHora { get; private set; }

        /// <summary>
        /// Obtiene el símbolo que se usará como separador de espacios decimales.
        /// </summary>
        public string SeparadorDecimal { get; private set; }

        /// <summary>
        /// Obtiene el símbolo que se usará como separador de miles.
        /// </summary>
        public string SeparadorMiles { get; private set; }

        /// <summary>
        /// Obtiene la cantidad predeterminada de posiciones decimales máximas a las que se redondearán las expresiones numéricas.
        /// </summary>
        public int CantidadDecimales { get; private set; }

        /// <summary>
        /// Obtiene un objeto <see cref="System.TimeSpan"/> que define la diferencia entre la hora coordinada UTC y el huso horario 
        /// al que se deban ajustar las horas manejadas por el sistema. Al especificar explícitamente un valor de huso horario,
        /// se evitan desincronías cuando la configuración regional del servidor no maneje el mismo huso horario que los
        /// terminales de los usuarios (por ejemplo, si el servidor fuera un servicio de hosting compartido).
        /// <para>Si el valor de esta propiedad es <see langword="null"/>, se asumirá el huso horario del servidor.</para>
        /// </summary>
        public TimeSpan HusoHorario { get; private set; }

        /// <summary>
        /// Obtiene un lapso de tiempo que se deberá restar a las expresiones de fecha del servidor para ajustarlas al huso horario
        /// especificado.
        /// <para>Por ejemplo, si el reloj del servidor está configurado en el huso horario -04:00 GMT y los usuarios
        /// del sistema están ubicados en el huso -04:30 GMT, la propiedad <see cref="HusoHorario"/> deberá ser configurada
        /// a <c>-04:30</c> y la propiedad <see cref="DiferenciaHoraria"/> retornará <c>00:30</c>; de manera que, al
        /// mostrar la hora de un instante de tiempo, en lugar de ejecutar sólo '<c>DateTime.Now</c>',
        /// se reste esta diferencia horaria para ajustarla al huso correspondiente: '<c>DateTime.Now - Sistema.CulturaActual.DiferenciaHoraria</c>'.</para>
        /// </summary>
        public TimeSpan DiferenciaHoraria { get; private set; }

        /// <summary>
        /// Obtiene la fecha y hora actuales del sistema, ajustadas al huso horario correspondiente a la propiedad <see cref="HusoHorario"/>.
        /// </summary>
        public DateTime Ahora { get { return DateTime.Now - DiferenciaHoraria; } }

        /// <summary>
        /// Obtiene la fecha actual del sistema, ajustada al huso horario correspondiente a la propiedad <see cref="HusoHorario"/>.
        /// </summary>
        public DateTime Hoy { get { return (DateTime.Now - DiferenciaHoraria).Date; } }

        #endregion

        #region Constructores

        /// <summary>
        /// Construye un objeto de cultura con la especificación de sus propiedades básicas.
        /// </summary>
        /// <param name="código">Código de la cultura en el formato '<c>ii-LL</c>' (donde '<c>ii</c>' es la abreviatura en dos caracteres minúsculas del idioma y donde
        /// '<c>LL</c>' es la abreviatura en dos caracteres mayúsculas de la localización). '<c>en-US</c>' y '<c>es-VE</c>' son ejemplos de códigos de cultura</param>
        /// <param name="nombre">Nombre con que se identificará la cultura</param>
        /// <param name="formatoFechaCorta">Formato en que se presentarán las fechas cortas (usando patrones tales como "<c>dd/MM/yyyy</c>", aceptados por los métodos de
        /// formato de fechas de .NET Framework)</param>
        /// <param name="formatoFechaLarga">Formato en que se presentarán las fechas largas (usando patrones tales como "<c>dddd, d 'de' MMMM 'de' yyyy</c>", aceptados por los métodos de
        /// formato de fechas de .NET Framework)</param>
        /// <param name="formatoHora">Formato en que se presentarán las horas (usando patrones tales como "hh:mm tt")</param>
        /// <param name="separadorDecimal">Símbolo que se usará como separador de espacios decimales</param>
        /// <param name="separadorMiles">Símbolo que se usará como separador de miles</param>
        /// <param name="cantidadDecimales">Cantidad predeterminada de decimales máximos a los que se redondearán las expresiones</param>
        public Cultura(string código, string nombre, string formatoFechaCorta, string formatoFechaLarga, string formatoHora, string separadorDecimal, string separadorMiles, int cantidadDecimales)
            : this(código, nombre, formatoFechaCorta, formatoFechaLarga, formatoHora, separadorDecimal, separadorMiles, cantidadDecimales, null)
        {
        }

        /// <summary>
        /// Construye un objeto de cultura con la especificación de sus propiedades básicas.
        /// </summary>
        /// <param name="código">Código de la cultura en el formato '<c>ii-LL</c>' (donde '<c>ii</c>' es la abreviatura en dos caracteres minúsculas del idioma y donde
        /// '<c>LL</c>' es la abreviatura en dos caracteres mayúsculas de la localización). '<c>en-US</c>' y '<c>es-VE</c>' son ejemplos de códigos de cultura</param>
        /// <param name="nombre">Nombre con que se identificará la cultura</param>
        /// <param name="formatoFechaCorta">Formato en que se presentarán las fechas cortas (usando patrones tales como "<c>dd/MM/yyyy</c>", aceptados por los métodos de
        /// formato de fechas de .NET Framework)</param>
        /// <param name="formatoFechaLarga">Formato en que se presentarán las fechas largas (usando patrones tales como "<c>dddd, d 'de' MMMM 'de' yyyy</c>", aceptados por los métodos de
        /// formato de fechas de .NET Framework)</param>
        /// <param name="formatoHora">Formato en que se presentarán las horas (usando patrones tales como "hh:mm tt")</param>
        /// <param name="separadorDecimal">Símbolo que se usará como separador de espacios decimales</param>
        /// <param name="separadorMiles">Símbolo que se usará como separador de miles</param>
        /// <param name="cantidadDecimales">Cantidad predeterminada de decimales máximos a los que se redondearán las expresiones</param>
        /// <param name="husoHorario">Objeto <see cref="System.TimeSpan"/> que define la diferencia la hora coordinada UTC y el huso horario 
        /// al que se deban ajustar las horas manejadas por el sistema. Al especificar explícitamente un valor de huso horario,
        /// se evitan desincronías cuando la configuración regional del servidor no maneje el mismo huso horario que los
        /// terminales de los usuarios (por ejemplo, si el servidor fuera un servicio de hosting compartido)</param>
        public Cultura(string código, string nombre, string formatoFechaCorta, string formatoFechaLarga, string formatoHora, string separadorDecimal, string separadorMiles, int cantidadDecimales, TimeSpan? husoHorario)
        {
            Codigo = código;
            Nombre = nombre;

            FormatoFechaCorta = formatoFechaCorta;
            FormatoFechaLarga = formatoFechaLarga;
            FormatoHora = formatoHora;

            var dif = (DateTime.Now - DateTime.UtcNow);
            dif = new TimeSpan(dif.Hours, dif.Minutes, 0);

            if (husoHorario.HasValue)
            {
                HusoHorario = husoHorario.Value;
                DiferenciaHoraria = dif - husoHorario.Value;
            }
            else
            {
                HusoHorario = dif;
                DiferenciaHoraria = new TimeSpan(0, 0, 0);
            }

            CantidadDecimales = cantidadDecimales;
            SeparadorDecimal = separadorDecimal;
            SeparadorMiles = separadorMiles;

            InicializarCultura();
        }

        #endregion

        #region Métodos

        private void InicializarCultura()
        {
            InfoCultura = new CultureInfo(Codigo);

            InfoCultura.NumberFormat.CurrencyDecimalDigits = CantidadDecimales;
            InfoCultura.NumberFormat.CurrencyDecimalSeparator = SeparadorDecimal;
            InfoCultura.NumberFormat.CurrencyGroupSeparator = SeparadorMiles;

            InfoCultura.NumberFormat.PercentDecimalDigits = CantidadDecimales;
            InfoCultura.NumberFormat.PercentDecimalSeparator = SeparadorDecimal;
            InfoCultura.NumberFormat.NumberGroupSeparator = SeparadorMiles;

            InfoCultura.NumberFormat.NumberDecimalDigits = CantidadDecimales;
            InfoCultura.NumberFormat.NumberDecimalSeparator = SeparadorDecimal;
            InfoCultura.NumberFormat.NumberGroupSeparator = SeparadorMiles;

            InfoCultura.DateTimeFormat.LongDatePattern = FormatoFechaLarga;
            InfoCultura.DateTimeFormat.ShortDatePattern = FormatoFechaCorta;
            InfoCultura.DateTimeFormat.ShortTimePattern = FormatoHora;
        }

        #endregion
    }

    /// <summary>
    /// Colección de culturas, identificadas por su código.
    /// </summary>
    [Serializable]
    public class ColeccionCulturas : Coleccion<string, Cultura>
    {
        /// <summary>
        /// Construye una colección de culturas, identificadas por su código.
        /// </summary>
        public ColeccionCulturas() : base(x => x.Codigo) { }
    }
}