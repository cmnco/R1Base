using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Sap.Data.Hana;
using RetailOne.Utilidades;

namespace RetailOne.Datos
{
    /// <summary>
    /// Simplifica la tarea de conexión a base de datos y ejecución de consultas y comandos.
    /// </summary>
    public class ConexionHana : IDisposable
    {
        #region Fields y propiedades

        private HanaFactory fabrica;
        private HanaConnection conexion;
        private HanaTransaction transaccion;
        private HanaCommand comando;
        private HanaDataReader lector;
        private Origen origen;
        private bool mantenerConexionAbierta;
        private bool transaccionIniciada;

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
        /// Activa o inactiva el cierre automático de la conexión posterior a la ejecución de cada comando.
        /// </summary>
        /// <value>Si el valor es <see langword="false"> (predeterminado), luego de ejecutar cada comando se cerrará la conexión. Si
        /// el valor es <see langword="true">, se mantendrá abierta la conexión al finalizar cada comando. Este último
        /// valor se recomienda sólo cuando se está consciente de la ejecución consecutiva de gran cantidad de comandos.
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
        /// <value>Objeto de tipo <see cref="Sap.Data.Hana.HanaCommand"/> subyacente.</value>
        public HanaCommand Comando { get { return comando; } }

        /// <summary>
        /// Obtiene el objeto lector con que se podrán acceder e iterar en los resultados de la consulta.
        /// </summary>
        /// <value>Objeto de tipo <see cref="Sap.Data.Hana.HanaDataReader"/> subyacente.</value>
        public HanaDataReader Lector { get { return lector; } }

        #endregion

        #region Constructores

        /// <summary>
        /// Crea un objeto <see cref="RetailOne.Datos.ConexionHana"/> dado un origen de datos específico.
        /// </summary>
        /// <param name="origen">Objeto del tipo <see cref="RetailOne.Datos.Origen"/> que especifica la cadena de conexión y proveedor de la base de datos.</param>
        public ConexionHana(Origen origen)
        {
            this.origen = origen;
            
            fabrica = (Sap.Data.Hana.HanaFactory)Origen.ObtenerFabrica(origen.Proveedor);

            conexion = (Sap.Data.Hana.HanaConnection)fabrica.CreateConnection();
            conexion.ConnectionString = origen.CadenaConexion;
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
        }

        /// <summary>
        /// Si la conexión está cerrada, intenta abrirla para la posterior ejecución de un comando.
        /// </summary>
        private void AbrirConexion()
        {
            if (conexion.State == ConnectionState.Closed)
            {
                conexion.Open();
            }
        }

        /// <summary>
        /// Si la conexión está abierta y no hay una transacción iniciada, intenta cerrarla.
        /// </summary>
        private void CerrarConexion()
        {
            if (conexion.State != ConnectionState.Closed && !transaccionIniciada && !mantenerConexionAbierta)
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
            try
            {
                conexion.Open();
                conexion.Close();
                return true;
            }
            catch
            {
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
        /// Ejecuta el comando y establece el lector resultante en la propiedad <see cref="Lector"/>.
        /// </summary>
        public void EjecutarLector()
        {
            ValidarComando();
            try
            {
                AbrirConexion();
                lector = comando.ExecuteReader();

                //definicionColumnasLector = new List<string>();
                //foreach (DataRow row in lector.GetSchemaTable().Rows)
                //{
                //    definicionColumnasLector.Add(row["ColumnName"].ToString());
                //}
            }
            catch (Exception ex)
            {
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
                else
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
                return registros;
            }
            catch (Exception ex)
            {
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
                return valor;
            }
            catch (Exception ex)
            {
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
                return valor;
            }
            catch (Exception ex)
            {
                throw new Exception(Recursos.Excepciones.ErrorComandoTabla, ex);
            }
        }

        /// <summary>
        /// Ejecuta un comando que devuelva los datos en un objeto de tipo <see cref="System.Data.DataSet"/>.
        /// </summary>
        public DataSet EjecutarConjuntoDeDatos()
        {
            ValidarComando();
            try
            {
                AbrirConexion();
                DataSet ds = new DataSet();
                using (DbDataAdapter Adapt = fabrica.CreateDataAdapter())
                {
                    Adapt.SelectCommand = comando;
                    Adapt.Fill(ds);
                }
                CerrarConexion();
                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception("Error de comando dataset, " + ex.Message, ex);
            }
        }

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
        public IEnumerable<TipoAMaterializar> EjecutarObjetos<TipoAMaterializar>(Func<ConexionHana, TipoAMaterializar, TipoAMaterializar> selectorValores, params object[] parámetrosConstructor)
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

        public IEnumerable<TipoAMaterializar> EjecutarObjetos<TipoAMaterializar>(Func<ConexionHana, TipoAMaterializar, TipoAMaterializar> selectorValores, Func<ConexionHana, TipoAMaterializar> creaciónInstancias)
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
        /// </summary>
        /// <typeparam name="T">Tipo del dato que se obtendrá. Para datos primitivos, puede hacerse uso de una construcción <see cref="Nullable"/>.</typeparam>
        /// <param name="nombreColumna">Nombre de la columna cuyo dato se obtendrá.</param>
        /// <returns>Devuelve el dato casteado al tipo especificado.</returns>
        public T DatoLector<T>(string nombreColumna)
        {
            if (lector == null || (lector != null && (lector[nombreColumna] == null || lector[nombreColumna] is DBNull)))
                return default(T);

            Type tipo = typeof(T);
            if (lector != null && lector[nombreColumna] != null && !(lector[nombreColumna] is DBNull) && (lector[nombreColumna] is T || (tipo.IsEnum)))
            {
                return (T)lector[nombreColumna];
            }
            else if (tipo == typeof(bool))
            {
                int? i = lector[nombreColumna] as int?;
                if (i.HasValue && i == 1)
                    return (T)(object)true;
                else return (T)(object)false;
            }
            else if (tipo.Name.StartsWith("Nullable") && lector[nombreColumna] != null)
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
            return default(T);
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
            if (omitirSiColumnaNoExiste && !EsquemaTablaContieneColumna(nombreColumna))
                return default(T);

            try
            {
                return DatoLector<T>(nombreColumna);
            }
            catch
            {
                return default(T);
            }
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

        public bool EsquemaTablaContieneColumna(string nombreColumna)
        {
            return DefinicionColumnasLector.Contains(nombreColumna);
        }

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
        /// Si no se especifica el texto para dicho proveedor, se asumirá el comando definido como base para la consulta.
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
            return textoComando;
        }

        public string ObtenerConsultaProveedor(Consulta consulta)
        {
            string textoComando = consulta.ObtenerTextoComando(origen.Proveedor);
            return textoComando;
        }

        /// <summary>
        /// Colección de los parámetros que usará el comando.
        /// </summary>
        /// <value>Colección de tipo <see cref="System.Data.Common.DbParameterCollection"/> con la colección de todos los parámetros del comando.</value>
        public DbParameterCollection Parámetros
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
            if (origen.Proveedor == Proveedor.Hana && !nombreParametro.StartsWith(":"))
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
        /// <returns>Retorna el objeto de parámetro recién creado.</returns>
        public DbParameter AgregarParametro(string nombre, object valor, DbType? tipo = null)
        {
            if (origen.Proveedor == Proveedor.Hana && !nombre.StartsWith(":"))
            {
                nombre = ":" + nombre;
            }
            DbParameter parámetro = comando.CreateParameter();
            parámetro.ParameterName = nombre;
            parámetro.Value = VerificarValor(valor);
            if (tipo != null)
                parámetro.DbType = tipo.Value;

            if (!comando.Parameters.Contains(nombre))
            {
                comando.Parameters.Add(parámetro);
                return parámetro;
            }
            else
            {
                int cont = 1;
                while (comando.Parameters.Contains(nombre + cont.ToString()))
                {
                    cont++;
                }
                parámetro.ParameterName = nombre + cont.ToString();
                comando.Parameters.Add(parámetro);
                return parámetro;
            }
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
        }

        #endregion
    }
}