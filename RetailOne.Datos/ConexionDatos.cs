using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Linq;
using System.Data.SqlClient;
using RetailOne.Utilidades;
using System.Text;

namespace RetailOne.Datos
{
    /// <summary>
    /// Simplifica la tarea de conexión a base de datos y ejecución de consultas y comandos.
    /// </summary>
    public partial class ConexionDatos : IDisposable
    {
        #region Constantes

        private const string CadenaMars = "MultipleActiveResultSets";

        #endregion

        #region Fields y propiedades

        private DbProviderFactory fabrica;
        private DbConnection conexion;
        private DbTransaction transaccion;
        private DbCommand comando;
        private DbDataReader lector;
        private Origen origen;
        private bool mantenerConexionAbierta;
        private bool transaccionIniciada;
        private bool comandoValidado;
        public bool TransaccionIniciada
        {
            get
            {
                return transaccion != null && transaccionIniciada;
            }
        }

        /// <summary>
        /// Obtiene el origen de datos con que se inició la conexión.
        /// </summary>
        /// <value>
        /// Objeto de la clase <see cref="RetailOne.Datos.Origen"/> con la cadena de conexión y proveedor del origen de datos.
        /// </value>
        public Origen Origen
        {
            get
            {
                return origen;
            }
        }

        /// <summary>
        /// Obtiene o establece el texto del comando que se correrá en la base de datos.
        /// </summary>
        /// <value>Texto que define el comando o consulta a ejecutar.</value>
        public string TextoComando
        {
            get { return comando.CommandText; }
            set { comando.CommandText = value; }
        }

        /// <summary>
        /// Habilita el manejo de conexiones compartidas entre distintos comandos asociados, para mejorar el rendimiento.
        /// Esto permitirá utilizar, en los casos en que se pueda, una misma conexión del Connection Pool para los objetos
        /// de tipo <see cref="ConexionDatos"/> que se creen a partir de la ejecución del método <see cref="NuevaConexionAsociada"/>.
        /// </summary>
        public bool OptimizarConexionesAsociadas { get; set; }


        private static Dictionary<string, bool> estadoTextoCompleto = new Dictionary<string, bool>();
        /// <summary>
        /// Obtiene un booleano especificando si la base de datos correspondiente a la conexión tiene habilitada las funciones de
        /// búsqueda de texto completo.
        /// </summary>
        public bool HabilitadoParaTextoCompleto
        {
            get
            {
                if (conexion == null || conexion.ConnectionString.EsNuloOVacio())
                    return false;
                if (!estadoTextoCompleto.ContainsKey(conexion.ConnectionString))
                {
                    ConexionDatos conn = new ConexionDatos(this);
                    conn.CargarConsulta(ConsultasUtiles.VerificarActivacionTextoCompleto);
                    estadoTextoCompleto[conexion.ConnectionString] = conn.EjecutarEscalar<int>() == 1;
                }
                return estadoTextoCompleto[conexion.ConnectionString];
            }
        }

        /// <summary>
        /// Activa o inactiva el cierre automático de la conexión posterior a la ejecución de cada comando.
        /// </summary>
        /// <value>Si el valor es <see langword="false"> (predeterminado), luego de ejecutar cada comando se cerrará la conexión. Si
        /// el valor es <see langword="true">, se mantendrá abierta la conexión al finalizar cada comando. Este último
        /// valor se recomienda sólo cuando se está consciente de la ejecución consecutiva de gran cantidad de comandos.
        /// </value>
        /// <remarks>
        /// Los métodos <see cref="EjecutarLector"/> son los únicos que no cierran la conexión, independientemente
        /// del valor de esta propiedad. Sin embargo, este sí afecta el comportamiento del método <see cref="CerrarLector"/>,
        /// el cual es necesario ejecutar apenas se termine de consultar la información del lector.
        /// <para>Cuando a la propiedad se establece un valor <see langword="false"/>, inmediatamente
        /// se intenta cerrar la conexión (si esta estaba activa).</para>
        /// </remarks>
        public bool MantenerConexionAbierta
        {
            get
            {
                return mantenerConexionAbierta;
            }
            set
            {
                mantenerConexionAbierta = value;
                if (!value)
                {
                    CerrarConexion();
                }
            }
        }

        /// <summary>
        /// Obtiene el objeto de comando internamente creado por la conexión.
        /// </summary>
        /// <value>Objeto de tipo <see cref="System.Data.Common.DbCommand"/> subyacente.</value>
        public DbCommand Comando { get { return comando; } }

        /// <summary>
        /// Obtiene el objeto lector con que se podrán acceder e iterar en los resultados de la consulta.
        /// </summary>
        /// <value>Objeto de tipo <see cref="System.Data.Common.DbDataReader"/> subyacente.</value>
        public DbDataReader Lector { get { return lector; } }

        /// <summary>
        /// Colección de los parámetros que usará el comando.
        /// </summary>
        /// <value>Colección de tipo <see cref="System.Data.Common.DbParameterCollection"/> con la colección de todos los parámetros del comando.</value>
        //public DbParameterCollection Parametros
        //{
        //    get { return comando.Parameters; }
        //}

        /// <summary>
        /// Obtiene el texto del comando junto con las declaraciones de las variables con los valores de los parámetros que se le hayan
        /// especificado al comando. Con el comando completo podrá ejecutarlo en otra herramienta (como el SQL Server Management Studio)
        /// con propósitos de revisión y depuración.
        /// </summary>
        public string TextoComandoCompleto
        {
            get
            {
                return ConstruirTextoComando();
            }
        }
        #endregion

        #region Constructores

        /// <summary>
        /// Crea un objeto <see cref="RetailOne.Datos.ConexionDatos"/> dado un origen de datos específico.
        /// </summary>
        /// <param name="origen">Objeto del tipo <see cref="RetailOne.Datos.Origen"/> que especifica la cadena de conexión y proveedor de la base de datos.</param>
        public ConexionDatos(Origen origen)
        {
            this.origen = origen;
            
            fabrica = Origen.ObtenerFabrica(origen.Proveedor);
            if (fabrica == null)
                throw new Exception("Proveedor de datos no encontrado o no está soportado. Nombre proveedor: " + origen.Proveedor.ToString());

            conexion = fabrica.CreateConnection();
            conexion.ConnectionString = origen.CadenaConexion;
            comando = conexion.CreateCommand();
        }
        
        /// <summary>
        /// Crea un objeto <see cref="RetailOne.Datos.ConexionDatos"/> a partir de otro objeto de conexión.
        /// Si la conexión de origen tiene habilitada la propiedad <c>OptimizarConexionesAsociadas</c>,
        /// hará uso de la optimización de múltiples conjuntos de resultados para una misma conexión.
        /// De lo contrario, simplemente creará una nueva conexión copiando la información de origen de datos
        /// (cadena de conexión y proveedor de base de datos).
        /// </summary>
        /// <param name="conexionOrigen">ConexiónDatos original</param>
        private ConexionDatos(ConexionDatos conexionOrigen)
        {
            origen = conexionOrigen.Origen;
            OptimizarConexionesAsociadas = conexionOrigen.OptimizarConexionesAsociadas;
            if (OptimizarConexionesAsociadas && (short)conexionOrigen.Origen.Proveedor < 100) 
            {
                //Sólo para SqlServer
                conexion = conexionOrigen.conexion;
                if (!conexion.ConnectionString.Contains(CadenaMars))
                {
                    conexion.ConnectionString += ";" + CadenaMars + "=True";
                }
            }
            else
            {
                fabrica = Origen.ObtenerFabrica(origen.Proveedor);
                if (fabrica == null)
                    throw new Exception("Proveedor de datos no encontrado o no está soportado. Nombre proveedor: " + origen.Proveedor.ToString());

                conexion = fabrica.CreateConnection();
                conexion.ConnectionString = origen.CadenaConexion;
            }
            comando = conexion.CreateCommand();
        }

        #endregion

        #region Métodos

        /// <summary>
        /// Verifica que se haya especificado el texto del comando.
        /// </summary>
        private void ValidarComando()
        {
            if (string.IsNullOrEmpty(comando.CommandText))
            {
                throw new Exception(Recursos.Excepciones.TextoComandoNoEspecificado);
            }
            //RedistribuciónDeParámetros();
            ValidarComandoHana();
        }

        /// <summary>
        /// Prepara el texto comando en caso de ser proveedor Hana.
        /// </summary>
        public void ValidarComandoHana()
        {
            if (comandoValidado) return;

            if (this.Origen.Proveedor != Proveedor.Hana) return;

            comando.CommandText = ComandoConsulta.ValidarComandoHana(comando.CommandText);

            if (Parametros.Count > 0 && Parametros[0] is System.Data.Odbc.OdbcParameter)
            {
                foreach (DbParameter parametro in Parametros)
                {
                    TextoComando = TextoComando.Replace(parametro.ParameterName, "?");
                }
            }
            comandoValidado = true;
        }
        
        /// <summary>
        /// Redistribución de lista de parámetros según orden de aparición en el texto de la consulta.
        /// Útil sólo para Hana.
        /// </summary>
        public void RedistribucionDeParametros()
        {
            if (origen.Proveedor != Proveedor.Hana || comando.Parameters.Count <= 0) return;

            var list = new[] { new { Nombre = "", Valor = new object(), Indice = 0 } }.ToList();
            list.Clear();

            foreach (DbParameter param in comando.Parameters)
            {
                int index = 0;
                while ((index = TextoComando.IndexOf(param.ParameterName, index)) > 0)
                {
                    list.Add(new
                    {
                        Nombre = param.ParameterName,
                        Valor = param.Value,
                        Indice = index
                    });
                    index += 1;
                }
            }

            if (list.Count > 0)
            {
                LimpiarParametros();
                foreach (var param in list.OrderBy(x => x.Indice))
                {
                    AgregarParametro(param.Nombre, param.Valor);
                }
            }
        }

        /// <summary>
        /// Si la conexión está cerrada, intenta abrirla para la posterior ejecución de un comando.
        /// </summary>
        private void AbrirConexion()
        {
            if (conexion.State == ConnectionState.Closed)
            {
                if ((short)origen.Proveedor < 100)
                {
                    //Sólo para SqlServer
                    if (OptimizarConexionesAsociadas && !conexion.ConnectionString.Contains(CadenaMars))
                    {
                        conexion.ConnectionString += ";" + CadenaMars + "=True";
                    }
                    else if (!OptimizarConexionesAsociadas && conexion.ConnectionString.Contains(CadenaMars))
                    {
                        conexion.ConnectionString = conexion.ConnectionString.Replace(";" + CadenaMars + "=True", "");
                    }
                }
                conexion.Open();
            }
        }

        /// <summary>
        /// Si la conexión está abierta y no hay una transacción iniciada, intenta cerrarla.
        /// </summary>
        private void CerrarConexion()
        {
            MultiConsulta = false;
            if (conexion.State != ConnectionState.Closed && !transaccionIniciada && !mantenerConexionAbierta && !OptimizarConexionesAsociadas)
            {
                conexion.Close();
            }
        }

        /// <summary>
        /// Prueba que se pueda abrir una conexión válida con el origen de datos especificado al construir el objeto.
        /// </summary>
        /// <returns>Devuelve <see langword="true"/> si la prueba de conexión fue satisfactoria; de lo contrario, retorna <see langword="false"/>.</returns>
        public bool ProbarConexion()
        {
            return ProbarConexion(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ProbarConexion(bool liberarExcepcion)
        {
            try
            {
                conexion.Open();
                conexion.Close();
                return true;
            }
            catch (Exception ex)
            {
                if (liberarExcepcion)
                    throw ex;
                else
                    return false;
            }
        }
        
        /// <summary>
        /// Si el proveedor de base de datos lo soporta, inicializa una transacción asociada
        /// a la conexión, permitiendo la ejecución segura de múltiples comandos y garantizando la coherencia e integridad de los datos.
        /// </summary>
        /// <exception cref="System.Exception">Se produce una excepción si el origen de datos no soporta transacciones.</exception>
        /// <remarks>Al iniciar una transacción, se omite el valor de la propiedad <see cref="MantenerConexionAbierta"/> pues
        /// se hace necesario mantener siempre activa la conexión asociada a la transacción, hasta que se ejecuten los métodos
        /// <see cref="ConfirmarTransaccion"/> ó <see cref="RestaurarTransaccion"/>.</remarks>
        public void IniciarTransaccion()
        {
            try
            {
                if (conexion.State == ConnectionState.Closed)
                {
                    conexion.Open();
                }
                transaccion = conexion.BeginTransaction();
                comando.Transaction = transaccion;
                transaccionIniciada = true;
            }
            catch (Exception ex)
            {
                transaccionIniciada = false;
                throw new Exception(Recursos.Excepciones.NoPudoIniciarTransacción, ex);
            }
        }

        /// <summary>
        /// Al ejecutar el método, se envían todos los comandos al manejador para su ejecución.
        /// </summary>
        /// <exception cref="System.Exception">Se produce una excepción si no se pudo confirmar la transacción por problemas en la ejecución
        /// de al menos un comando.</exception>
        /// <example>
        /// Se recomienda envolver la ejecución de este método en un bloque <see langword="try/catch"/>,
        /// ubicando en el bloque <see langword="catch"/> un llamado al método <see cref="RestaurarTransaccion"/>:
        /// <code>
        /// using (ConexiónDatos transacciónSegura = new ConexiónDatos(origenDatos))
        /// {
        ///     transacciónSegura.IniciarTransacción();
        ///     // Ejecutar todos los comandos de la transacción.
        ///     try
        ///     {
        ///         transacciónSegura.ConfirmarTransacción();
        ///     }
        ///     catch (Exception ex)
        ///     {
        ///         transacciónSegura.RestaurarTransacción();
        ///     }
        /// }
        /// </code></example>
        public void ConfirmarTransaccion()
        {
            try
            {
                transaccion.Commit();
                transaccionIniciada = false;
            }
            catch (Exception ex)
            {
                transaccionIniciada = false;
                throw new Exception(Recursos.Excepciones.NoPudoConfirmarTransacción, ex);
            }
        }

        /// <summary>
        /// Si por alguna razón se quieren invalidar los comandos almacenados en la transacción, se ejecuta este
        /// método, restableciendo así el estado original de los datos.
        /// </summary>
        /// <exception cref="System.Exception">Se produce una excepción si no se pudo realizar la recuperación de los datos.</exception>
        public void RestaurarTransaccion()
        {
            try
            {
                if(transaccionIniciada)
                    transaccion.Rollback();
                transaccionIniciada = false;
            }
            catch (Exception ex)
            {
                transaccionIniciada = false;
                throw new Exception(Recursos.Excepciones.NoPudoRestaurarTransacción, ex);
            }
        }

        /// <summary>
        /// Crea una nueva conexión asociada a la actual. Dependiendo del valor del parámetro <see cref="OptimizarConexionesAsociadas"/>,
        /// se hará uso de la optimización de múltiples conjuntos de resultados o simplemente se creará una nueva conexión con los
        /// mismos datos de origen.
        /// </summary>
        /// <returns>Objeto de tipo <see cref="ConexionDatos"/> con el mismo origen de datos de la conexión actual.</returns>
        public ConexionDatos NuevaConexionAsociada()
        {
            return new ConexionDatos(this);
        }

        /// <summary>
        /// Ejecuta el comando y establece el lector resultante en la propiedad <see cref="Lector"/>.
        /// </summary>
        public void EjecutarLector()
        {
            ValidarComando();
            try
            {
                AbrirConexion();
                lector = comando.ExecuteReader();
                Depuracion.Registrar(this);
            }
            catch (Exception ex)
            {
                Depuracion.Error(this, ex);
                throw new Exception(Recursos.Excepciones.ErrorComandoLector + ", " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Cierra el lector e intenta cerrar la conexión (si la propiedad <see cref="MantenerConexionAbierta"/>
        /// está desactivada y si no hay una transacción iniciada).
        /// </summary>
        public void CerrarLector()
        {
            if (!lector.IsClosed)
            {
                if (lector.Read())
                    comando.Cancel();
                //else
                lector.Close();
            }
            BorrarDefinicionColumnasLector();
            CerrarConexion();
        }

        /// <summary>
        /// Ejecuta un comando que no requiera devolución de datos (por ejemplo, <c>insert</c>, <c>update</c> o <c>delete</c>).
        /// </summary>
        public int EjecutarComando()
        {
            ValidarComando();
            try
            {
                AbrirConexion();
                int registros;
                registros = comando.ExecuteNonQuery();
                CerrarConexion();
                Depuracion.Registrar(this);
                return registros;
            }
            catch (Exception ex)
            {
                Depuracion.Error(this, ex);
                throw new Exception(Recursos.Excepciones.ErrorComando, ex);
            }
        }

        /// <summary>
        /// Ejecuta un comando que devuelve un único dato escalar.
        /// <typeparam name="T">Tipo de datos al que se casteará el valor escalar devuelto.</typeparam>
        /// </summary>
        /// <returns>Objeto de tipo <typeparamref name="T"/> con el valor devuelto por el comando.</returns>
        public T EjecutarEscalar<T>()
        {
            ValidarComando();
            try
            {
                AbrirConexion();
                T valor;
                valor = (T)comando.ExecuteScalar();
                CerrarConexion();
                Depuracion.Registrar(this);
                return valor;
            }
            catch (Exception ex)
            {
                Depuracion.Error(this, ex);
                throw new Exception(Recursos.Excepciones.ErrorComandoEscalar, ex);
            }
        }
        
        /// <summary>
        /// Ejecuta un comando que devuelva los datos en un objeto de tipo <see cref="System.Data.DataTable"/>.
        /// </summary>
        public DataTable EjecutarTabla()
        {
            ValidarComando();
            try
            {
                AbrirConexion();
                DataTable valor = new DataTable();
                DbDataAdapter Adapt = fabrica.CreateDataAdapter();
                Adapt.SelectCommand = comando;
                Adapt.Fill(valor);
                CerrarConexion();
                Depuracion.Registrar(this);
                return valor;
            }
            catch (Exception ex)
            {
                Depuracion.Error(this, ex);
                throw new Exception(Recursos.Excepciones.ErrorComandoTabla, ex);
            }
        }

        /// <summary>
        /// Ejecuta un comando que devuelva los datos en un objeto de tipo <see cref="System.Data.DataSet"/>.
        /// </summary>
        public DataSet EjecutarConjuntoDeDatos()
        {
            return EjecutarConjuntoDeDatos(null, null);
        }

        /// <summary>
        /// Ejecuta un comando que devuelva los datos en un objeto de tipo <see cref="System.Data.DataSet"/>.
        /// </summary>
        public DataSet EjecutarConjuntoDeDatos(string nombreTabla)
        {
            return EjecutarConjuntoDeDatos(nombreTabla, null);
        }

        /// <summary>
        /// Ejecuta un comando que devuelva los datos en un objeto de tipo <see cref="System.Data.DataSet"/>.
        /// </summary>
        public DataSet EjecutarConjuntoDeDatos(string nombreTabla, DataSet dataSet)
        {
            ValidarComando();
            try
            {
                AbrirConexion();
                if (dataSet == null)
                    dataSet = new DataSet();

                using (DbDataAdapter Adapt = fabrica.CreateDataAdapter())
                {
                    Adapt.SelectCommand = comando;
                    if (string.IsNullOrEmpty(nombreTabla))
                        Adapt.Fill(dataSet);
                    else
                        Adapt.Fill(dataSet, nombreTabla);
                }
                CerrarConexion();
                return dataSet;
            }
            catch (Exception ex)
            {
                Depuracion.Error(this, ex);
                throw new Exception("Error de comando dataset, " + ex.Message, ex);
            }
        }

        #region
        private SqlBulkCopy _bulkCopy;
        protected SqlBulkCopy BulkCopy
        {
            get
            {
                if(_bulkCopy == null)
                {
                    var options = SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.TableLock;
                    _bulkCopy = new SqlBulkCopy((SqlConnection)conexion, options, (SqlTransaction)transaccion);
                }
                return _bulkCopy;
            }
            private set
            {
                _bulkCopy = value;
            }
        }

        public void AplicarCopiaMasiva(DataTable tablaDatos)
        {
            AplicarCopiaMasiva(null, tablaDatos, null);
        }

        public void AplicarCopiaMasiva(string nombreTablaDestino, DataTable tablaDatos)
        {
            AplicarCopiaMasiva(nombreTablaDestino, tablaDatos, null);
        }

        public void AplicarCopiaMasiva(string nombreTablaDestino, DataTable tablaDatos, Dictionary<string, string> mapeoColumnas)
        {
            if ((short)origen.Proveedor >= 100)
                throw new Exception("Comando no permitido para el proveedor de base de datos actual");

            AbrirConexion();
            try
            {
                if (!string.IsNullOrEmpty(nombreTablaDestino))
                {
                    BulkCopy.DestinationTableName = nombreTablaDestino;
                }
                else
                {
                    BulkCopy.DestinationTableName = tablaDatos.TableName;
                }
                
                if(mapeoColumnas != null && mapeoColumnas.Count > 0)
                {
                    BulkCopy.ColumnMappings.Clear();

                    foreach (var columna in mapeoColumnas)
                        BulkCopy.ColumnMappings.Add(columna.Key, columna.Value);
                }

                BulkCopy.WriteToServer(tablaDatos);
            }
            catch (Exception ex)
            {
                //RestaurarTransaccion();
                throw new Exception("Error al ejecutar la carga masiva: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tablaDatos"></param>
        [Obsolete]
        public void AplicarConjuntoDeDatos(DataTable tablaDatos)
        {
            AplicarCopiaMasiva(null, tablaDatos, null);
            //if ((short)origen.Proveedor >= 100)
            //    throw new Exception("Comando no permitido para el proveedor de base de datos actual");

            //if(bulkCopy == null)
            //{
            //    var options = SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.TableLock;
            //    bulkCopy = new SqlBulkCopy((SqlConnection)conexión, options, (SqlTransaction)transacción);
            //}
            //AbrirConexion();
            //try
            //{
            //    if(bulkCopy.DestinationTableName != tablaDatos.TableName)
            //    {
            //        bulkCopy.DestinationTableName = tablaDatos.TableName;
            //        //bulkCopy.ColumnMappings.Clear();
            //        //foreach (DataColumn columna in tablaDatos.Columns)
            //        //{
            //        //    bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(columna.ColumnName, columna.ColumnName));
            //        //}
            //    }

            //    bulkCopy.WriteToServer(tablaDatos);
            //}
            //catch (Exception ex)
            //{
            //    //RestaurarTransaccion();
            //    throw new Exception("Error al ejecutar la carga masiva: " + ex.Message, ex);
            //}
        }
        #endregion

        /// <summary>
        /// Ejecuta un comando que devuelva los datos materializados en objetos de una clase en particular, con el uso
        /// de una función selectora de valores.
        /// </summary>
        /// <typeparam name="TipoAMaterializar">Tipo (clase) en que se materializarán los datos devueltos por la consulta.</typeparam>
        /// <param name="selectorValores">Función que, con el uso de una variable de conexión y un objeto del tipo a materializar,
        /// realizará la carga de los datos del lector para una fila en dicho objeto.
        /// <para>Comúnmente, esta función deberá retornar el mismo objeto obtenido en el segundo parámetro de la función; sin embargo,
        /// está abierta la posibilidad de retornar otra instancia de esa misma clase (por ejemplo, si se programa una política de caché de instancias).</para></param>
        /// <param name="parámetrosConstructor">Lista de parámetros con que se construirán las instancias de la clase del <see cref="TipoAMaterializar"/>.</param>
        /// <returns>Enumerado de objetos materializados.</returns>
        public IEnumerable<TipoAMaterializar> EjecutarObjetos<TipoAMaterializar>(Func<ConexionDatos, TipoAMaterializar, TipoAMaterializar> selectorValores, params object[] parámetrosConstructor)
            where TipoAMaterializar : class
        {
            List<TipoAMaterializar> lista = new List<TipoAMaterializar>();
            EjecutarLector();
            while (lector.Read())
            {
                TipoAMaterializar fila = (TipoAMaterializar)Activator.CreateInstance(typeof(TipoAMaterializar), parámetrosConstructor);
                lista.Add(selectorValores(this, fila));
            }
            CerrarLector();
            return lista;
        }

        public IEnumerable<TipoAMaterializar> EjecutarObjetos<TipoAMaterializar>(Func<ConexionDatos, TipoAMaterializar, TipoAMaterializar> selectorValores, Func<ConexionDatos, TipoAMaterializar> creaciónInstancias)
            where TipoAMaterializar : class
        {
            List<TipoAMaterializar> lista = new List<TipoAMaterializar>();
            EjecutarLector();
            while (lector.Read())
            {
                TipoAMaterializar fila = creaciónInstancias(this);
                if (fila != null)
                {
                    lista.Add(selectorValores(this, fila));
                }
            }
            CerrarLector();
            return lista;
        }

        /// <summary>
        /// Ejecuta un comando que devuelva los datos materializados en objetos de una clase en particular, con el uso de reflexión.
        /// <para>En este caso, el método intentará establecer los valores de todos los campos devueltos por la base de datos en
        /// propiedades que tengan exactamente el mismo nombre y tipo de datos, así como estén activados para escritura.</para>
        /// </summary>
        /// <typeparam name="TipoAMaterializar">Tipo (clase) en que se materializarán los datos devueltos por la consulta.</typeparam>
        /// <param name="parámetrosConstructor">Lista de parámetros con que se construirán las instancias de la clase del <see cref="TipoAMaterializar"/>.</param>
        /// <returns>Enumerado de objetos materializados.</returns>
        public IEnumerable<TipoAMaterializar> EjecutarObjetos<TipoAMaterializar>(params object[] parámetrosConstructor)
        {
            Type tipo = typeof(TipoAMaterializar);
            bool sencillo = false;
            if (tipo.EsTipoNulable())
            {
                Type tipoInterno = new NullableConverter(tipo).UnderlyingType;
                if (tipoInterno.IsValueType)
                {
                    sencillo = true;
                }
            }
            else if (tipo.IsValueType)
            {
                sencillo = true;
            }
            else if (tipo == typeof(string))
            {
                sencillo = true;
            }
            List<TipoAMaterializar> lista = new List<TipoAMaterializar>();
            List<PropertyInfo> propiedades = new List<PropertyInfo>();
            bool primeraVez = true;
            EjecutarLector();
            while (lector.Read())
            {
                if (sencillo)
                {
                    lista.Add((TipoAMaterializar)lector.GetValue(0));
                    continue;
                }

                TipoAMaterializar fila = (TipoAMaterializar)Activator.CreateInstance(tipo, parámetrosConstructor);
                for (int i = 0; i < lector.FieldCount; i++)
                {
                    string nombre = lector.GetName(i);
                    if (primeraVez)
                    {
                        propiedades.Add(tipo.GetProperty(nombre));
                    }
                    PropertyInfo propiedad = propiedades[i];
                    if (propiedad != null && propiedad.CanWrite)
                    {
                        propiedad.SetValue(fila, lector[i] is DBNull ? null : lector[i], null);
                    }
                }
                primeraVez = false;
                lista.Add(fila);
            }
            CerrarLector();
            return lista;
        }

        /// <summary>
        /// Intenta avanzar el lector de datos a la siguiente fila del set de registros devueltos por la consulta.
        /// </summary>
        /// <returns>Devuelve <see langword="true"/> si realizó la lectura correcta de la siguiente fila y se posicionó en ella; <see langword="false"/>, si el lector llegó a su fin y no hay más filas que leer.</returns>
        public bool LeerFila()
        {
            return Lector.Read();
        }

        /// <summary>
        /// Obtiene el dato de una columna para la fila actual del lector, especificando el tipo de datos al que se casteará.
        /// Si en la consulta no fue seleccionada una columna con el nombre del parámetro <paramref name="nombreColumna"/>,
        /// se omitirá la consulta del dato y no se lanzará ninguna excepción.
        /// </summary>
        /// <typeparam name="T">Tipo del dato que se obtendrá. Para datos primitivos, puede hacerse uso de una construcción <see cref="Nullable"/>.</typeparam>
        /// <param name="nombreColumna">Nombre de la columna cuyo dato se obtendrá.</param>
        /// <param name="omitirSiColumnaNoExiste">Si vale <c>true</c>, se omitirá la consulta del dato cuando no exista una columna devuelta con el nombre descrito en el parámetro <paramref name="nombreColumna"/></param>
        /// <returns>Devuelve el dato casteado al tipo especificado.</returns>
        public T DatoLector<T>(string nombreColumna, bool omitirSiColumnaNoExiste)
        {
            return DatoLector(nombreColumna, omitirSiColumnaNoExiste, default(T));
        }

        /// <summary>
        /// Obtiene el dato de una columna para la fila actual del lector, especificando el tipo de datos al que se casteará.
        /// Si en la consulta no fue seleccionada una columna con el nombre del parámetro <paramref name="nombreColumna"/>,
        /// se omitirá la consulta del dato y no se lanzará ninguna excepción.
        /// </summary>
        /// <typeparam name="T">Tipo del dato que se obtendrá. Para datos primitivos, puede hacerse uso de una construcción <see cref="Nullable"/>.</typeparam>
        /// <param name="nombreColumna">Nombre de la columna cuyo dato se obtendrá.</param>
        /// <param name="omitirSiColumnaNoExiste">Si vale <c>true</c>, se omitirá la consulta del dato cuando no exista una columna devuelta con el nombre descrito en el parámetro <paramref name="nombreColumna"/></param>
        /// <param name="valorPorDefecto">Valor a retornar en caso de que la columna no exista o el dato sea nulo.</param>
        /// <returns>Devuelve el dato casteado al tipo especificado.</returns>
        public T DatoLector<T>(string nombreColumna, bool omitirSiColumnaNoExiste, T valorPorDefecto)
        {
            if (omitirSiColumnaNoExiste && !DefinicionColumnasLector.Contains(nombreColumna))
                return valorPorDefecto;

            try
            {
                return DatoLector<T>(nombreColumna, valorPorDefecto);
            }
            catch
            {
                return valorPorDefecto;
            }
            //if (omitirSiColumnaNoExiste && !lector.GetSchemaTable().Columns.Contains(nombreColumna)) 
        }

        /// <summary>
        /// Obtiene el dato de una columna para la fila actual del lector, especificando el tipo de datos al que se casteará.
        /// </summary>
        /// <typeparam name="T">Tipo del dato que se obtendrá. Para datos primitivos, puede hacerse uso de una construcción <see cref="Nullable"/>.</typeparam>
        /// <param name="nombreColumna">Nombre de la columna cuyo dato se obtendrá.</param>
        /// <returns>Devuelve el dato casteado al tipo especificado.</returns>
        public T DatoLector<T>(string nombreColumna)
        {
            return DatoLector(nombreColumna, default(T));
        }

        /// <summary>
        /// Obtiene el dato de una columna para la fila actual del lector, especificando el tipo de datos al que se casteará.
        /// </summary>
        /// <typeparam name="T">Tipo del dato que se obtendrá. Para datos primitivos, puede hacerse uso de una construcción <see cref="Nullable"/>.</typeparam>
        /// <param name="nombreColumna">Nombre de la columna cuyo dato se obtendrá.</param>
        /// <param name="valorPorDefecto">Valor a retornar si el dato es nulo.</param>
        /// <returns>Devuelve el dato casteado al tipo especificado.</returns>
        public T DatoLector<T>(string nombreColumna, T valorPorDefecto)
        {
            if (lector == null || (lector != null && (lector[nombreColumna] == null || lector[nombreColumna] is DBNull)))
                return valorPorDefecto;

            Type tipo = typeof(T);
            if (lector[nombreColumna] is T || (tipo.IsEnum))
            {
                return (T)lector[nombreColumna];
            }
            else if(tipo == typeof(char) || tipo == typeof(char?))
            {
                string c = lector[nombreColumna] as string;
                if (c != null && c != "")
                    return (T)(object)c[0];
            }
            else if (tipo == typeof(bool))
            {
                int? i = lector[nombreColumna] as int?;
                if (i.HasValue && i == 1)
                    return (T)(object)true;
                else return (T)(object)false;
            }
            else if (tipo.Name.StartsWith("Nullable"))
            {
                var tipoNulable = System.Nullable.GetUnderlyingType(tipo);
                if (tipoNulable != null && tipoNulable.IsValueType && (tipoNulable.IsAssignableFrom(lector[nombreColumna].GetType()) || tipoNulable.IsEnum))
                {
                    return (T)Enum.ToObject(tipoNulable, lector[nombreColumna]);
                }
            }
            else
            {
                return (T)Convert.ChangeType(lector[nombreColumna], tipo);
            }
            return valorPorDefecto;
        }

        

        public static bool ContieneColumna(IDataRecord r, string nombreColumna)
        {
            try
            {
                return r.GetOrdinal(nombreColumna) >= 0;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }

        private List<string> definicionColumnasLector;
        private List<string> DefinicionColumnasLector
        {
            get
            {
                if (definicionColumnasLector == null)
                {
                    definicionColumnasLector = new List<string>();
                    foreach (DataRow row in lector.GetSchemaTable().Rows)
                    {
                        definicionColumnasLector.Add(row["ColumnName"].ToString());
                    }
                }
                return definicionColumnasLector;
            }
        }

        //public bool EsquemaTablaContieneColumna(string nombreColumna)
        //{
        //    return DefinicionColumnasLector.Contains(nombreColumna);
        //}

        private void BorrarDefinicionColumnasLector()
        {
            if (definicionColumnasLector != null)
            {
                definicionColumnasLector.Clear();
                definicionColumnasLector = null;
            }
        }

        /// <summary>
        /// Devuelve el texto del comando con el texto que corresponda al proveedor de la conexión, dado un objeto de tipo <see cref="RetailOne.Datos.Consulta"/>,
        /// y sobre-escribe la propiedad <see cref="TextoComando"/> con el valor encontrado.
        /// Si no se especifica el texto para dicho proveedor, se asumirá el comando definido como base para la consulta.
        /// </summary>
        /// <param name="consulta">Consulta que se cargará.</param>
        /// <returns>Texto del comando (de acuerdo al proveedor).</returns>
        public string CargarConsulta(Consulta consulta)
        {
            return CargarConsulta(consulta, null);
        }


        /// <summary>
        /// Devuelve el texto del comando con el texto que corresponda al proveedor de la conexión, dado un objeto de tipo <see cref="RetailOne.Datos.Consulta"/>.
        /// y sobre-escribe la propiedad <see cref="TextoComando"/> con el valor encontrado.
        /// Si no se especifica el texto para dicho proveedor, se asumirá el comando del proveedor base definido en la consulta.
        /// </summary>
        /// <param name="consulta">Consulta que se cargará.</param>
        /// <param name="tipoComando">Establece el tipo de comando a ejecutar según la clasificación de <see cref="System.Data.CommandType"/> (Text , TableDirect o StoredProcedure)
        /// <returns>Texto del comando (de acuerdo al proveedor).</returns>
        public string CargarConsulta(Consulta consulta, CommandType? tipoComando)
        {
            string textoComando = consulta.ObtenerTextoComando(origen.Proveedor);
            BorrarDefinicionColumnasLector();
            TextoComando = textoComando;
            if (tipoComando.HasValue)
                comando.CommandType = tipoComando.Value;
            comandoValidado = false;
            return textoComando;
        }

        /// <summary>
        /// Obtiene el texto de la consulta para el proveedor definido en el origen.
        /// </summary>
        /// <param name="consulta"></param>
        /// <returns>String de la consulta</returns>
        public string ObtenerConsultaProveedor(Consulta consulta)
        {
            return ObtenerConsultaProveedor(consulta, origen.Proveedor);
        }

        /// <summary>
        /// Obtiene el texto de la consulta para el proveedor especificado.
        /// </summary>
        /// <param name="consulta">Consulta</param>
        /// <param name="proveedor">Proveedor</param>
        /// <returns>String de la consulta</returns>
        public static string ObtenerConsultaProveedor(Consulta consulta, Proveedor proveedor)
        {
            string textoComando = consulta.ObtenerTextoComando(proveedor);
            return textoComando;
        }

        /// <summary>
        /// Colección de los parámetros que usará el comando.
        /// </summary>
        /// <value>Colección de tipo <see cref="System.Data.Common.DbParameterCollection"/> con la colección de todos los parámetros del comando.</value>
        public DbParameterCollection Parametros
        {
            get { return comando.Parameters; }
        }

        /// <summary>
        /// Obtiene un objeto de parámetro dado su nombre.
        /// </summary>
        /// <param name="nombreParametro">Nombre del parámetro a obtener.</param>
        /// <returns>Devuelve el objeto de tipo <see cref="System.Data.Common.DbParameter"/>.</returns>
        public DbParameter Parametro(string nombreParametro)
        {
            if ((short)origen.Proveedor < 100 && !nombreParametro.StartsWith("@"))
            {
                nombreParametro = "@" + nombreParametro;
            }
            else if (origen.Proveedor == Proveedor.Hana && !nombreParametro.StartsWith(":"))
            {
                nombreParametro = ":" + nombreParametro;
            }
            return comando.Parameters[nombreParametro];
        }

        /// <summary>
        /// Agrega un parámetro al comando.
        /// </summary>
        /// <param name="nombre">Nombre del parámetro.</param>
        /// <param name="valor">Value del parámetro.</param>
        /// <returns></returns>
        public DbParameter AgregarParametro(string nombre, object valor)
        {
            return AgregarParametro(nombre, valor, null);
        }

        /// <summary>
        /// Agrega un parámetro al comando.
        /// </summary>
        /// <param name="nombre">Nombre del parámetro.</param>
        /// <param name="valor">Value del parámetro.</param>
        /// <returns>Retorna el objeto de parámetro recién creado.</returns>
        public DbParameter AgregarParametro(string nombre, object valor, DbType? tipo)
        {
            DbParameter parametro = comando.CreateParameter();

            if ((short)origen.Proveedor < 100)
            {
                if (nombre.StartsWith(":"))
                    nombre = nombre.Replace(":", "@");
                else if (!nombre.StartsWith("@"))
                    nombre = "@" + nombre;
            }
            else if(origen.Proveedor == Proveedor.Hana)
            {
                if (nombre.StartsWith("@"))
                    nombre = nombre.Replace("@", ":");
                else if (!nombre.StartsWith(":"))
                    nombre = ":" + nombre;

                //if (Origen.Fabrica.Equals("System.Data.Odbc"))
                if (!tipo.HasValue && parametro is System.Data.Odbc.OdbcParameter)
                {
                    tipo = InferirTipoDato(valor);
                }
            }

            parametro.ParameterName = nombre;
            parametro.Value = VerificarValor(valor);
            if (tipo != null)
                parametro.DbType = tipo.Value;

            if (!comando.Parameters.Contains(nombre))
            {
                comando.Parameters.Add(parametro);
                return parametro;
            }
            else
            {
                int cont = 1;
                while (comando.Parameters.Contains(nombre + cont.ToString()))
                {
                    cont++;
                }
                parametro.ParameterName = nombre + cont.ToString();
                comando.Parameters.Add(parametro);
                return parametro;
            }
        }

        private DbType InferirTipoDato(object valor)
        {
            if (valor is string)
            {
                return DbType.String;
            }
            else if (valor is int)
            {
                return DbType.Int32;
            }
            else if (valor is long)
            {
                return DbType.Int64;
            }
            else if (valor is short)
            {
                return DbType.Int16;
            }
            else if (valor is byte)
            {
                return DbType.Byte;
            }
            else if (valor is decimal)
            {
                return DbType.Decimal;
            }
            else if (valor is double)
            {
                return DbType.Double;
            }
            else if (valor is float)
            {
                return DbType.Single;
            }
            else if (valor is bool)
            {
                return DbType.Boolean;
            }
            else if (valor is DateTime)
            {
                return DbType.DateTime;
            }
            else if (valor is DateTimeOffset)
            {
                return DbType.DateTimeOffset;
            }
            else if (valor is TimeSpan)
            {
                return DbType.Time;
            }
            else if (valor is byte[])
            {
                return DbType.Binary;
            }
            //else if (valor is Guid)
            //{
            //    return DbType.Guid;
            //}
            else if (valor == null)
            {
                //return DbType.Object;
                return DbType.String;
            }

            // Por defecto, retorna DbType.Object
            //return DbType.Object;
            return DbType.String;
        }


        /// <summary>
        /// Elimina los parámetros registrados para el comando.
        /// </summary>
        public void LimpiarParametros()
        {
            comando.Parameters.Clear();
        }

        /// <summary>
        /// Agrega al texto de comando actual una cadena en reemplazo de todas las apariciones de una variable literal.
        /// </summary>
        /// <param name="nombreLiteral">Nombre de la variable literal.</param>
        /// <param name="valor">Texto con que se reemplazará la variable</param>
        public void AgregarLiteral(string nombreLiteral, string valor)
        {
            comando.CommandText = ReemplazarLiteral(comando.CommandText, nombreLiteral, valor);
        }

        /// <summary>
        /// Reemplaza todas las apariciones de una variable literal con un valor específico, en un texto de comando dado. No afecta al <see cref="TextoComando"/> del objeto.
        /// </summary>
        /// <param name="texto">Texto de comando base en el que se encuentran las variables a reemplazar.</param>
        /// <param name="nombreLiteral">Nombre de la variable a reemplazar.</param>
        /// <param name="valor">Value que se establecerá a las variables literales definidas.</param>
        /// <returns>Texto de comando con los reemplazos aplicables.</returns>
        public string ReemplazarLiteral(string texto, string nombreLiteral, string valor)
        {
            return texto.Replace("${" + nombreLiteral + "}", valor);
        }

        /// <summary>
        /// Realiza una verificación del objeto de entrada para reemplazarlo por el valor <see cref="DBNull"/> si este
        /// valía <see langword="null"/> originalmente. Útil al momento de asignar un valor a la propiedad <c>Value</c> de un parámetro (sin el uso de <see cref="AgregarParametro"/>).
        /// </summary>
        /// <param name="entrada">Objeto a verificar.</param>
        /// <returns>Objeto verificado. Si la entrada era <see langword="null"/>, retorna <see cref="DBNull"/>.</returns>
        public object VerificarValor(object entrada)
        {
            if (entrada == null)
                return DBNull.Value;
            return entrada;
        }

        /// <summary>
        /// Reemplaza el carácter asterisco comodín (*) por aquél usado específicamente en el proveedor del origen de datos.
        /// </summary>
        /// <param name="entrada">Cadena de entrada con los caracteres comodín.</param>
        /// <returns>Cadena con comodines reemplazados.</returns>
        public string ReemplazarComodines(string entrada)
        {
            return entrada.Replace("*", "%");
        }
        
        private string ConstruirTextoComando()
        {
            if (string.IsNullOrEmpty(comando.CommandText))
                return null;

            StringBuilder salida = new StringBuilder();

            if(Origen.Proveedor == Proveedor.Hana)
            {
                bool incluirCierre = false;

                if(!comando.CommandText.StartsWith("DO BEGIN", StringComparison.OrdinalIgnoreCase))
                {
                    salida.AppendLine("DO BEGIN");
                    incluirCierre = true;
                }

                foreach (DbParameter parametro in comando.Parameters)
                {
                    string nombreParametro = parametro.ParameterName.StartsWith(":") ? parametro.ParameterName.Substring(1) : parametro.ParameterName;

                    if (parametro.DbType == DbType.String)
                    {
                        if (parametro.Value == null || parametro.Value is DBNull)
                        {
                            salida.AppendLine($"DECLARE {nombreParametro} VARCHAR(1) := NULL;");
                        }
                        else
                        {
                            salida.AppendLine($"DECLARE {nombreParametro} VARCHAR({parametro.Value.ToString().Length}) := '{parametro.Value.ToString().Replace("'", "''")}';");
                        }
                    }
                    else if (parametro.DbType == DbType.Decimal || parametro.DbType == DbType.Double || parametro.DbType == DbType.Currency)
                    {
                        salida.AppendLine($"DECLARE {nombreParametro} DECIMAL(19,4) := {(parametro.Value == null || parametro.Value is DBNull ? "NULL" : Convert.ToDecimal(parametro.Value).ToString("0.0", new System.Globalization.CultureInfo("en-US")))};");
                    }
                    else if (parametro.DbType == DbType.Date || parametro.DbType == DbType.DateTime || parametro.DbType == DbType.DateTime2 || parametro.DbType == DbType.DateTimeOffset)
                    {
                        salida.AppendLine($"DECLARE {nombreParametro} TIMESTAMP :=  TO_TIMESTAMP ('{((DateTime)parametro.Value).ToString("dd-MM-yyyy HH:mm:ss")}', 'DD-MM-YYYY HH24:MI:SS');");
                    }
                    else if (parametro.Value is Enum)
                    {
                        salida.AppendLine($"DECLARE {nombreParametro} INTEGER := {(parametro.Value == null || parametro.Value is DBNull ? "NULL" : ((Enum)parametro.Value).ToString("d"))};");
                    }
                    else
                    {
                        salida.AppendLine($"DECLARE {nombreParametro} {TipoParametro(parametro)} := {(parametro.Value == null || parametro.Value is DBNull ? "NULL" : parametro.Value)};");
                    }
                }

                //salida.AppendLine(TextoComando.Replace(":p", "p").Replace("@p", "p") + ";");
                string texto = ComandoConsulta.ValidarComandoHana(TextoComando).Replace(":p", "p").Replace("@p", "p");
                salida.AppendLine(texto.EndsWith(";") ? texto : texto + ";");
                if(incluirCierre)
                    salida.AppendLine("END;");

                return salida.ToString();
            }
            else if ((short)Origen.Proveedor < 100)
            {
                foreach (DbParameter parametro in comando.Parameters)
                {
                    SqlParameter paramSql = (SqlParameter)parametro;

                    if (paramSql.SqlDbType == SqlDbType.NVarChar || paramSql.SqlDbType == SqlDbType.NChar
                         || paramSql.SqlDbType == SqlDbType.Char || paramSql.SqlDbType == SqlDbType.VarChar)
                    {
                        salida.AppendLine($"declare {paramSql.ParameterName} {paramSql.SqlDbType.ToString()}({(paramSql.Size == 0 ? 1 : paramSql.Size)}) = {(paramSql.Value == null || paramSql.Value is DBNull ? "null" : "'" + paramSql.SqlValue.ToString().Replace("'", "''") + "'")};");
                    }

                    else if (paramSql.SqlDbType == SqlDbType.Decimal)
                    {
                        if (paramSql.SqlValue is System.Data.SqlTypes.SqlDecimal)
                        {
                            System.Data.SqlTypes.SqlDecimal valorDecimal = (System.Data.SqlTypes.SqlDecimal)paramSql.SqlValue;
                            salida.AppendLine($"declare {paramSql.ParameterName} Decimal({valorDecimal.Precision},{valorDecimal.Scale}) = {(paramSql.Value == null || paramSql.Value is DBNull ? "null" : (valorDecimal.Value).ToString("0.0", new System.Globalization.CultureInfo("en-US")))};");
                        }
                        else
                        {
                            salida.AppendLine($"declare {paramSql.ParameterName} Decimal({paramSql.Precision},{paramSql.Scale}) = {(paramSql.Value == null || paramSql.Value is DBNull ? "null" : ((decimal)paramSql.SqlValue).ToString("0.0", new System.Globalization.CultureInfo("en-US")))};");
                        }
                    }
                    else if (paramSql.SqlDbType == SqlDbType.Float || paramSql.SqlDbType == SqlDbType.Money
                        || paramSql.SqlDbType == SqlDbType.Real || paramSql.SqlDbType == SqlDbType.SmallMoney)
                    {
                        if (paramSql.Value == null || paramSql.Value is DBNull)
                        {
                            salida.AppendLine($"declare {paramSql.ParameterName} {paramSql.SqlDbType.ToString()} = null");
                        }
                        else
                        {
                            salida.AppendLine($"declare {paramSql.ParameterName} {paramSql.SqlDbType.ToString()} = {Convert.ToDecimal(paramSql.Value, new System.Globalization.CultureInfo("en-US"))};");
                        }
                    }
                    else if (paramSql.SqlDbType == SqlDbType.DateTime)
                    {
                        salida.AppendLine($"declare {paramSql.ParameterName} DateTime = {(paramSql.Value == null || paramSql.Value is DBNull ? "null" : "convert(datetime, '" + ((DateTime)paramSql.Value).ToString("yyyy-MM-dd HH:mm:ss") + "', 120)")};");
                    }
                    else if (paramSql.Value is Enum)
                    {
                        salida.AppendLine($"declare {paramSql.ParameterName} Int = {(paramSql.Value == null || paramSql.Value is DBNull ? "null" : ((Enum)paramSql.Value).ToString("d"))} ");
                    }
                    else if (paramSql.Value is bool)
                    {
                        salida.AppendLine($"declare {paramSql.ParameterName} Bit = {(paramSql.Value == null || paramSql.Value is DBNull ? "null" : ("'" + ((bool)paramSql.Value).ToString() + "'"))} ");
                    }
                    else
                    {
                        salida.AppendLine($"declare {paramSql.ParameterName} {paramSql.SqlDbType.ToString()} = {(paramSql.Value == null || paramSql.Value is DBNull ? "null" : paramSql.Value.ToString())} ");
                    }
                }

                salida.AppendLine(TextoComando);
                return salida.ToString();
            }

            return TextoComando;
        }
        
        private string TipoParametro(DbParameter parametro)
        {
            if(Origen.Proveedor == Proveedor.Hana && tiposHana.ContainsKey(parametro.DbType))
            {
                return tiposHana[parametro.DbType];
            }
            else if((short)Origen.Proveedor < 100)
            {
                return ((SqlParameter)parametro).SqlDbType.ToString();
            }

            return parametro.DbType.ToString();
        }

        private static Dictionary<DbType, string> tiposHana = new Dictionary<DbType, string>()
        {
            { DbType.String, "VARCHAR" },
            { DbType.Int16, "SMALLINT" },
            { DbType.Int32, "INTEGER" },
            { DbType.Int64, "BIGINT" },
            { DbType.Double, "DOUBLE" },
            { DbType.Decimal, "DECIMAL(19,4)" },
            { DbType.DateTime, "TIMESTAMP" }
        };

        /// <summary>
        /// Libera los recursos de la conexión, primeramente, cerrando el lector si se encontraba abierto.
        /// Posteriormente cierra la conexión si no estaba cerrada.
        /// </summary>
        public void Dispose()
        {
            if (lector != null && !lector.IsClosed) lector.Close();
            if (conexion != null && conexion.State == ConnectionState.Open)
            {
                conexion.Close();
            }
            try
            {
                if (BulkCopy != null)
                {
                    BulkCopy.Close();
                    BulkCopy = null;
                }
                    
            }
            catch (Exception ex)
            {
                //throw new Exception("Error al intentar cerrar bulkCopy", ex);
            }
        }
        #endregion

        /// <summary>
        /// Método ejemplo
        /// </summary>
        protected void PruebaConexion()
        {
            DbProviderFactory fabrica = DbProviderFactories.GetFactory("Sap.Data.Hana");
            using (DbConnection conn = fabrica.CreateConnection())
            {
                conn.ConnectionString = "DRIVER={HDBODBC32};UID=SYSTEM;PWD=Soluone.2015; SERVERNODE=192.168.0.208:30015;DATABASE=NDB; CS=SBODEMOMX";
                DbCommand command = conn.CreateCommand();
                command.CommandText = "SELECT NOW() FROM DUMMY";
                conn.Open();
                object fecha = command.ExecuteScalar();
                conn.Close();
            }
        }
        
        private static void NotificarError(ConexionDatos conn, Exception ex)
        {
            if(conn != null)
            {
                //Depuracion.NotificarEvento(conn.TextoComandoCompleto, ex);
            }
        }
    }
}