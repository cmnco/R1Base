using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using RetailOne.Datos;
using RetailOne.Utilidades;

namespace RetailOne.Seguridad
{
    public class RegistroEvento
    {
        #region Atributos y Propiedades
        public Instancia Instancia { get; protected set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Componente { get; set; }
        public string Descripcion { get; set; }
        public string Detalle { get; set; }
        public string Referencia { get; set; }
        public DateTime Fecha { get; set; }
        public short Hora { get; set; }
        public DateTime FechaHora { get; private set; }
        public TipoEvento TipoEvento { get; set; }
        public string Usuario { get; set; }
        private static DateTime? ultimaEliminacion;

        private static bool? registrarEnArchivo = null;
        /// <summary>
        /// Esta propiedad quedará descontinuada para próximas versiones.
        /// </summary>
        [Obsolete]
        public bool ForzarRegistroErroresEnArchivo
        {
            get
            {
                if (registrarEnArchivo == null)
                {
                    var valor = Ejecucion.Sistema.VariablesConfiguracion["ForzarRegistroErroresEnArchivo"];
                    registrarEnArchivo = false;
                    if (!string.IsNullOrEmpty(valor))
                    {
                        valor = valor.ToUpper();
                        if (valor.Equals("TRUE") || valor.Equals("SI") || valor.Equals("Y") || valor.Equals("1"))
                            registrarEnArchivo = true;
                    }
                }
                return registrarEnArchivo.Value;
            }
        }
        
        public static string Ruta
        {
            get
            {
                return RetailOne.Depuracion.Log.Ruta;
            }
        }
        private static readonly object _padlock = new object();
        private static bool tablaCreada = ValidarTabla();
        #endregion

        #region Constructor
        private RegistroEvento(Instancia instancia)
        {
            this.Instancia = instancia;
        }
        #endregion

        #region Metodos
        public static void RegistrarError(Instancia instancia, string titulo, Exception error)
        {
            Registrar(instancia, null, null, null, null, TipoEvento.Error, titulo, error.Detalles());
        }

        public static void RegistrarError(Instancia instancia, string componente, string titulo, Exception error)
        {
            Registrar(instancia, null, componente, null, null, TipoEvento.Error, titulo, error.Detalles());
        }

        public static void Registrar(Instancia instancia, TipoEvento tipo, string descripcion, params string[] detalles)
        {
            Registrar(instancia, null, null, descripcion, null, tipo, detalles);
        }

        public static void Registrar(Instancia instancia, string code, string componente, string descripcion, string referencia, TipoEvento tipo, params string[] detalles)
        {
            RegistroEvento ev = new RegistroEvento(instancia)
            {
                Code = code,
                Componente = string.IsNullOrEmpty(componente) ? Ejecucion.Sistema.Abreviatura : componente,
                TipoEvento = tipo,
                Descripcion = descripcion,
                Referencia = referencia
            };

            if (detalles != null)
            {
                if (detalles.Count() > 1)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var detalle in detalles)
                    {
                        sb.AppendLine(detalle);
                    }
                    ev.Detalle = sb.ToString();
                }
                else
                {
                    ev.Detalle = detalles[0];
                }
            }
            ev.Registrar();
        }

        private void Registrar()
        {
            bool ocurrioException = false;
            using (Conexion conn = new Conexion(Instancia.OrigenDatos))
            {
                try
                {
                    ValidarCampos();
                    lock (_padlock)
                    {
                        DateTime fechaActual = DateTime.Now;
                        if (!ultimaEliminacion.HasValue || fechaActual.Subtract(ultimaEliminacion.Value).TotalHours >= 24)
                        {
                            try
                            {
                                EliminarRegistrosAntiguos(conn, null);
                                ultimaEliminacion = fechaActual;
                            }
                            catch { }
                        }

                        bool actualizarRegistro = false;

                        if (!string.IsNullOrEmpty(this.Code))
                        {
                            conn.CargarConsulta(ConsultaControlEventos.ExisteEvento);
                            conn.AgregarParametro("pCode", Code);
                            actualizarRegistro = conn.EjecutarEscalar<string>().Equals("Y");
                            conn.LimpiarParametros();
                        }

                        this.Code = string.IsNullOrEmpty(this.Code) ? Guid.NewGuid().ToString() : this.Code;
                        Name = Code;

                        if (actualizarRegistro)
                            conn.CargarConsulta(ConsultaControlEventos.ActualizarEventos);
                        else
                            conn.CargarConsulta(ConsultaControlEventos.RegistrarEvento);

                        conn.AgregarParametro("pComponente", Componente);
                        conn.AgregarParametro("pDescripcion", Descripcion);
                        conn.AgregarParametro("pDetalle", Detalle);
                        conn.AgregarParametro("pFecha", fechaActual);
                        conn.AgregarParametro("pHora", Utilidades.UtilidadesTipos.HoraANumero(fechaActual.TimeOfDay, false));
                        conn.AgregarParametro("pTipoEvento", TipoEvento);
                        conn.AgregarParametro("pUsuario", string.IsNullOrEmpty(Usuario) ? "SYSTEM" : Usuario);
                        conn.AgregarParametro("pReferencia", Referencia);
                        conn.AgregarParametro("pCode", Code);
                        conn.AgregarParametro("pName", Name);
                        conn.EjecutarComando();
                    }
                }
                catch (Exception err)
                {
                    ocurrioException = true;
                    Detalle = $"Error: {err.Message}. Descripción original: {Descripcion}. Detalle: {Detalle}";
                    Descripcion = "Error al intentar registrar en el log de eventos.";
                }
                finally
                {
                    if (ocurrioException || (ForzarRegistroErroresEnArchivo && TipoEvento == TipoEvento.Error))
                    {
                        RetailOne.Depuracion.Log.Registrar(Descripcion, new Exception(Detalle));
                    }
                }
            }
        }

        private void ValidarCampos()
        {
            if (string.IsNullOrEmpty(Descripcion))
                Descripcion = "No especificado.";
            else if (Descripcion.Length >= 200)
                Descripcion = Descripcion.Substring(0, 199);

            if (string.IsNullOrEmpty(Componente))
                Componente = Ejecucion.Sistema.Abreviatura;
            else if (Componente.Length >= 200)
                Componente = Componente.Substring(0, 199);

            if (string.IsNullOrEmpty(Detalle))
                Detalle = "No especificado.";
        }

        public bool Consultar(string code, bool cargarDetalle = true)
        {
            using (Conexion conn = new Conexion(Instancia.OrigenDatos))
            {
                conn.CargarConsulta(ConsultaControlEventos.Consultar);
                conn.AgregarParametro("pCode", code);
                conn.EjecutarLector();
                bool encontrado = false;
                if (conn.LeerFila())
                {
                    Materializar(conn, this);
                    if (cargarDetalle)
                        Detalle = conn.DatoLector<string>("U_SO1_DETALLE");
                    encontrado = true;
                }
                conn.CerrarLector();
                return encontrado;
            }
        }

        public static RegistroEvento Consultar(Instancia instancia, string code)
        {
            RegistroEvento e = new RegistroEvento(instancia);
            return e.Consultar(code, true) ? e : null;
        }

        protected static RegistroEvento Materializar(ConexionDatos conn, RegistroEvento obj)
        {
            obj.Code = conn.DatoLector<string>("Code");
            obj.Name = conn.DatoLector<string>("Name");
            obj.Usuario = conn.DatoLector<string>("U_SO1_USUARIO");
            obj.Componente = conn.DatoLector<string>("U_SO1_COMPONENTE");
            obj.Descripcion = conn.DatoLector<string>("U_SO1_DESCRIPCION");
            obj.Referencia = conn.DatoLector<string>("U_SO1_REFERENCIA");
            obj.Detalle = conn.DatoLector<string>("U_SO1_DETALLE", true);
            obj.Fecha = conn.DatoLector<DateTime>("U_SO1_FECHA");
            obj.Hora = conn.DatoLector<short>("U_SO1_HORA");
            obj.FechaHora = obj.Fecha.Date + Utilidades.UtilidadesTipos.NumeroAHora(obj.Hora, false);
            string tipo = conn.DatoLector<string>("U_SO1_TIPO");
            obj.TipoEvento = (TipoEvento)Enum.Parse(typeof(TipoEvento), tipo);
            return obj;
        }

        public static IEnumerable<string> Componentes(Instancia instancia)
        {
            using (Conexion conn = new Conexion(instancia.OrigenDatos))
            {
                conn.CargarConsulta(ConsultaControlEventos.ListarComponentes);
                conn.EjecutarLector();
                List<string> componentes = new List<string>();
                while (conn.LeerFila())
                {
                    componentes.Add(conn.DatoLector<string>("U_SO1_COMPONENTE"));
                }
                conn.CerrarLector();
                return componentes;
            }
        }

		public static IEnumerable<RegistroEvento> Filtrar(Instancia instancia, TipoEvento? tipoEvento, string referencia, DateTime? desde, DateTime? hasta, string criterio)
        {
            return Filtrar(instancia, null, null, null, tipoEvento, desde, hasta, criterio);
        }

        public static IEnumerable<RegistroEvento> Filtrar(Instancia instancia, TipoEvento? tipoEvento, DateTime? desde, DateTime? hasta, string criterio)
        {
            return Filtrar(instancia, null, null, null, tipoEvento, desde, hasta, criterio);
        }

        public static IEnumerable<RegistroEvento> Filtrar(Instancia instancia, TipoEvento? tipoEvento, string referencia, DateTime? desde, DateTime? hasta)
        {
            return Filtrar(instancia, null, referencia, null, tipoEvento, desde, hasta, null);
        }

        public static IEnumerable<RegistroEvento> Filtrar(Instancia instancia, string codigo, string referencia, string componente, TipoEvento? tipoEvento, DateTime? desde, DateTime? hasta, string criterio)
        {
            using (Conexion conn = new Conexion(instancia.OrigenDatos))
            {
                conn.CargarConsulta(ConsultaControlEventos.FiltradoDeEventos);
                conn.AgregarParametro("pTipoEvento", tipoEvento);
                conn.AgregarParametro("pTipoEvento", tipoEvento);
                conn.AgregarParametro("pComponente", string.IsNullOrEmpty(componente) ? null : componente);
                conn.AgregarParametro("pComponente", string.IsNullOrEmpty(componente) ? null : componente);
                conn.AgregarParametro("pCode", codigo);
                conn.AgregarParametro("pCode", codigo);
                conn.AgregarParametro("pName", codigo);
                conn.AgregarParametro("pName", codigo);
                conn.AgregarParametro("pReferencia", referencia);
                conn.AgregarParametro("pReferencia", referencia);
                conn.AgregarParametro("pDesde", desde);
                conn.AgregarParametro("pDesde", desde);
                conn.AgregarParametro("pHasta", hasta);
                conn.AgregarParametro("pHasta", hasta);
                criterio = string.IsNullOrEmpty(criterio) ? null : conn.ReemplazarComodines("*" + criterio + "*");
                conn.AgregarParametro("pCriterio", criterio);
                conn.AgregarParametro("pCriterio", criterio);
                conn.AgregarParametro("pCriterio", criterio);
                Func<ConexionDatos, RegistroEvento> crearInstancia = delegate (ConexionDatos c)
                {
                    return new RegistroEvento(instancia);
                };
                return conn.EjecutarObjetos<RegistroEvento>(Materializar, crearInstancia); //new object[] { instancia }
            }
        }

        /// <summary>
        /// Elimina los registros antiguos del log de eventos. 
        /// Por defecto elimina los que tiene mas de dos meses de antiguedad.
        /// </summary>
        /// <returns></returns>
        public static bool EliminarRegistrosAntiguos()
        {
            return EliminarRegistrosAntiguos(null);
        }

        /// <summary>
        /// Elimina los registros antiguos del log de eventos a partir de la fecha indicada hacia atrás. 
        /// Por defecto elimina los que tiene mas de dos meses de antiguedad.
        /// </summary>
        /// <returns></returns>
        public static bool EliminarRegistrosAntiguos(DateTime? desde)
        {
            try
            {
                using (Conexion conn = new Conexion())
                {
                    EliminarRegistrosAntiguos(conn, desde);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("No fue posible eliminar los registros. ", ex);
            }
        }

        /// <summary>
        /// Elimina los registros antiguos del log de eventos. Por defecto elimina los que tiene mas de dos meses de antiguedad.
        /// </summary>
        /// <returns></returns>
        private static void EliminarRegistrosAntiguos(Conexion conn, DateTime? desde)
        {
            conn.LimpiarParametros();
            conn.CargarConsulta(ConsultaControlEventos.EliminarRegistrosAntiguos);
            conn.AgregarParametro("pFechaHora", desde.HasValue ? desde.Value : DateTime.Now.AddMonths(-2));
            conn.EjecutarComando();
            conn.LimpiarParametros();
        }

        public static bool ValidarTabla()
        {
            using (Conexion conn = new Conexion())
            {
                if (conn.Origen.Proveedor == Proveedor.Hana || 
                    ((short)conn.Origen.Proveedor < 100 && !conn.Origen.NombreBD.ToLower().StartsWith("retail")))
                    return true;

                conn.CargarConsulta(ConsultaControlEventos.VerificarTablaEventos);
                conn.AgregarParametro("pSchema", conn.Origen.NombreBD);
                var valor = conn.EjecutarEscalar<object>();
                if (Convert.ToInt32(valor) <= 0)
                {
                    try
                    {
                        conn.CargarConsulta(ConsultaControlEventos.CrearTablaEventos);
                        conn.EjecutarComando();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    try
                    {
                        conn.CargarConsulta(ConsultaControlEventos.VerificarColumnas);
                        conn.LimpiarParametros();
                        conn.AgregarParametro("pSchema", conn.Origen.NombreBD);
                        conn.EjecutarComando();
                    }
                    catch (Exception ex) { }
                }
                return true;
            }
        }
        #endregion

        #region Métodos guardado en archivo
        /// <summary>
        /// Este método queda descontinuado para futuras versiones. Se reemplaza por el método: RetailOne.Depuracion.Log.Registrar.
        /// </summary>
        [Obsolete]
        public static void GuardarErrorEnArchivoLog(string descripcion, Exception exception)
        {
            RetailOne.Depuracion.Log.Registrar($"{TipoEvento.Error}: {descripcion}", exception);
        }

        /// <summary>
        /// Este método queda descontinuado para futuras versiones. Se reemplaza por el método: RetailOne.Depuracion.Log.Registrar.
        /// </summary>
        [Obsolete]
        public static void GuardarErrorEnArchivoLog(Exception exception)
        {
            RetailOne.Depuracion.Log.Registrar($"{TipoEvento.Error}: {exception.Message}", exception);
        }

        /// <summary>
        /// Este método queda descontinuado para futuras versiones. Se reemplaza por el método: RetailOne.Depuracion.Log.Registrar.
        /// </summary>
        /// <param name="descripcion"></param>
        /// <param name="detalles"></param>
        [Obsolete]
        public static void GuardarErrorEnArchivoLog(string descripcion, params string[] detalles)
        {
            RetailOne.Depuracion.Log.Registrar(descripcion, detalles);
        }

        /// <summary>
        /// Este método queda descontinuado para futuras versiones. Se reemplaza por el método: RetailOne.Depuracion.Log.Registrar.
        /// </summary>
        [Obsolete]
        public static void GuardarEnArchivoLog(TipoEvento tipo, string descripcion, params string[] detalles)
        {
            RetailOne.Depuracion.Log.Registrar($"{tipo}: {descripcion}", detalles);
        }

        /// <summary>
        /// Este método queda descontinuado para futuras versiones. Se reemplaza por el método: RetailOne.Depuracion.Log.Registrar.
        /// </summary>
        [Obsolete]
        public static void VerificarDirectorio()
        {
            RetailOne.Depuracion.Log.ValidarDirectorio();
        }
        #endregion
    }

    #region Enumerados
    public enum TipoEvento : short
    {
        [Description("General")]
        General = 0,

        [Description("Inicio Sesión")]
        InicioSecion = 5,

        [Description("Fin Sesión")]
        FinSecion = 10,

        [Description("Inicio Aplicación")]
        InicioAplicacion = 15,

        [Description("Fin Aplicación")]
        FinAplicacion = 20,

        [Description("Impresión")]
        Impresion = 25,

        [Description("Confirmación")]
        Confirmacion = 30,

        [Description("Evento")]
        Evento = 35,

        [Description("Documento Enviado")]
        DocumentoEnviado = 40,

        [Description("Documento Confirmado")]
        DocumentoConfirmado = 45,

        [Description("DocumentoProcesado")]
        DocumentoProcesado = 50,

        [Description("DocumentoRechazado")]
        DocumentoRechazado = 55,

        [Description("ErrorEnDocumento")]
        ErrorEnDocumento = 60,

        [Description("Notificación")]
        Notificacion = 65,

        [Description("Advertencia")]
        Advertencia = 70,

        [Description("Error")]
        Error = 255
    }
    #endregion
}
