using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RetailOne.Utilidades;

namespace RetailOne.Datos
{
    /// <summary>
    /// Representa un origen de datos relacional (base de datos).
    /// Engloba una cadena de conexión y la especificación del proveedor del origen,
    /// adicional de un nombre para identificarlo.
    /// </summary>
    [Serializable]
    public struct Origen
    {
        #region Propiedades
        private string id;
        /// <summary>
        /// Obtiene el identificador del origen.
        /// </summary>
        public string ID { get { return id; } }

        private string cadenaConexion;
        /// <summary>
        /// Obtiene la cadena de conexión (formateada de acuerdo al soporte de ADO.NET) del origen de datos.
        /// </summary>
        public string CadenaConexion { get { return cadenaConexion; } }

        private Proveedor proveedor;
        /// <summary>
        /// Indica la plataforma de la base de datos.
        /// </summary>
        public Proveedor Proveedor { get { return this.proveedor; } private set { this.proveedor = value; } }

        private string _usuarioDB;
        /// <summary>
        /// Usuario de base de datos.
        /// </summary>
        public string UsuarioDB { get { return this._usuarioDB; } private set { this._usuarioDB = value; } }

        private string _contraseñaDB;
        /// <summary>
        /// Contrasela de base de datos.
        /// </summary>
        public string ContraseñaDB { get { return this._contraseñaDB; } private set { this._contraseñaDB = value; } }

        private string _nombreBD;
        /// <summary>
        /// Nombre de la base de datos.
        /// </summary>
        public string NombreBD { get { return this._nombreBD; } private set { this._nombreBD = value; } }

        private string _servidorBD;
        /// <summary>
        /// Servidor de base de datos.
        /// </summary>
        public string ServidorBD { get { return this._servidorBD; } private set { this._servidorBD = value; } }

        private string _tenant;
        /// <summary>
        /// Tenant Hana.
        /// </summary>
        public string Tenant { get { return this._tenant; } private set { this._tenant = value; } }

        private static string _fabrica;
        /// <summary>
        /// Nombre fábrica ocupada por el proveedor seleccionado.
        /// </summary>
        public static string Fabrica { get { return _fabrica; } private set { _fabrica = value; } }
        #endregion

        /// <summary>
        /// Construye un origen de datos a partir de sus datos básicos.
        /// </summary>
        /// <param name="cadenaConexión">Cadena de conexión (formateada de acuerdo al soporte de ADO.NET) del origen de datos.</param>
        /// <param name="proveedor">Indica bajo qué plataforma se provee la base de datos.</param>
        //public Origen(string cadenaConexión, Proveedor proveedor) 
        //    : this(cadenaConexión, proveedor, true)
        //{

        //}
        /// <summary>
        /// Construye un origen de datos a partir de sus datos básicos.
        /// </summary>
        /// <param name="servidor">Nombre del servidor de base de datos</param>
        /// <param name="nombre">Nombre de la base de datos. Puede tener explícito el Tenant (NDB o HDB), prefijando en el nombre y separando con una @. Ejem: HDB@NombreBD.</param>
        /// <param name="usuario">Usuario</param>
        /// <param name="contraseña">Contraseña</param>
        /// <param name="proveedor">Indica bajo qué plataforma se provee la base de datos.</param>
        public Origen(string servidor, string nombre, string usuario, string contraseña, Proveedor proveedor)
            : this(servidor, nombre, usuario, contraseña, null, proveedor)
        {

        }

        /// <summary>
        /// Construye un origen de datos a partir de sus datos básicos.
        /// </summary>
        /// <param name="servidor">Nombre del servidor de base de datos</param>
        /// <param name="nombre">Nombre de la base de datos. Puede tener explícito el Tenant (NDB o HDB), prefijando en el nombre y separando con una @. Ejem: HDB@NombreBD.</param>
        /// <param name="usuario">Usuario</param>
        /// <param name="contraseña">Contraseña</param>
        /// <param name="tenant">tenant (o inquilino) se refiere a una base de datos independiente que opera dentro de una instancia de SAP HANA. Esto es parte de la arquitectura de base de datos multitenant.</param>
        /// <param name="proveedor">Indica bajo qué plataforma se provee la base de datos.</param>
        public Origen(string servidor, string nombre, string usuario, string contraseña, string tenant, Proveedor proveedor)
        {
            this.proveedor = proveedor;
            this.id = nombre;
            this._nombreBD = nombre;
            this._servidorBD = servidor;
            this._usuarioDB = usuario;
            this._contraseñaDB = contraseña;
            this._tenant = tenant;
            if (!string.IsNullOrEmpty(nombre))
            {
                string[] partes = (nombre ?? "").Split("@|;:".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (partes.Count() > 1)
                {
                    this._tenant = partes[0];
                    this._nombreBD = partes[1];
                }
            }
            this.cadenaConexion = ObtenerCadenaDeConexion(servidor, nombre, usuario, contraseña, proveedor);
        }

        /// <summary>
        /// Construye un origen de datos a partir de sus datos básicos.
        /// </summary>
        /// <param name="id">Identificador del origen.</param>
        /// <param name="cadenaConexion">Cadena de conexión (formateada de acuerdo al soporte de ADO.NET) del origen de datos.</param>
        /// <param name="proveedor">Indica bajo qué plataforma se provee la base de datos.</param>
        public Origen(string cadenaConexion, Proveedor proveedor) : this(cadenaConexion, proveedor, true)
        {

        }

        private Origen(string cadenaConexion, Proveedor proveedor, bool analizarCadena)
        {
            this.cadenaConexion = cadenaConexion;
            this.proveedor = proveedor;
            this.id = "";
            this._servidorBD = "";
            this._nombreBD = "";
            this._usuarioDB = "";
            this._contraseñaDB = "";
            this._tenant = "";
            if (analizarCadena)
                this = UtilidadesConexion.ObtenerDatosDeCadena(cadenaConexion, proveedor);
        }


        /// <summary>
        /// Obtiene la fábrica <see cref="System.Data.Common.DbProviderFactory"/> del proveedor especificado.
        /// </summary>
        /// <param name="proveedor">Proveedor del que se quiere obtener la fábrica de objetos.</param>
        /// <returns></returns>
        public static DbProviderFactory ObtenerFabrica(Proveedor proveedor)
        {
            try
            {
                if ((short)proveedor < 100)
                {
                    Fabrica = "System.Data.SqlClient";
                    return System.Data.SqlClient.SqlClientFactory.Instance; //DbProviderFactories.GetFactory("System.Data.SqlClient");
                }

                switch (proveedor)
                {
                    case Proveedor.Hana:
                        return ObtenerFabricaHana();
                    case Proveedor.MySQL:
                        return DbProviderFactories.GetFactory("MySQL"); // MySql.Data.MySqlClient.MySqlClientFactory.Instance;
                    case Proveedor.Oracle:
                        return DbProviderFactories.GetFactory("System.Data.Oracle");
                    case Proveedor.PostgreSQL:
                        return DbProviderFactories.GetFactory("Npgsql");
                    default: return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //public static string ConstruirCadenaConexion(string servidor, string nombreBD, string usuario, string contraseña, Proveedor proveedor)
        //{
        //    string tenant = "";
        //    if (proveedor == Proveedor.Hana)
        //    {
        //        string[] partes = (servidor ?? "").Split("@|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //        if (partes.Count() > 1)
        //        {
        //            tenant = partes[0];
        //            servidor = partes[1];
        //        }
        //    }
        //    return ConstruirCadenaConexion(servidor, nombreBD, usuario, contraseña, tenant, proveedor);
        //}

        //public static string ConstruirCadenaConexion(string servidor, string nombreBD, string usuario, string contraseña, string tenant, Proveedor proveedor)
        //{
        //    if (proveedor == Proveedor.Hana)
        //    {
        //        string drive = "{HDBODBC32}";
        //        if(Arquitectura == Plataforma.x64)
        //            drive = "{HDBODBC}";

        //        return ConstruirCadenaConexion(drive, servidor, nombreBD, usuario, contraseña, tenant, proveedor);
        //    }
        //    else
        //    {
        //        return ConstruirCadenaConexion(null, servidor, nombreBD, usuario, contraseña, null, proveedor);
        //    }
        //}

        //public static string ConstruirCadenaConexion(string driver, string servidor, string nombreBD, string usuario, string contraseña, string tenant, Proveedor proveedor)
        //{
        //    if (proveedor == Proveedor.Hana)
        //    {
        //        return $"DRIVER={driver};UID={usuario};PWD={contraseña};SERVERNODE={servidor};databaseName={tenant};DATABASE={tenant};CS={nombreBD}";
        //    }
        //    else
        //    {
        //        return $"Data Source={servidor};Initial Catalog={nombreBD};User ID={usuario};Password={contraseña};Persist Security Info=False;Integrated Security=False;";
        //    }
        //}

        //[Obsolete]
        //public static Origen ObtenerDatosDeCadena(Origen origen)
        //{
        //    Dictionary<string, string> campos = new Dictionary<string, string>();

        //    Func<string, bool> analizarCadena = delegate (string cadena)
        //    {
        //        foreach (var campo in campos.Keys.ToList())
        //        {
        //            int index = cadena.IndexOf(campo, StringComparison.InvariantCultureIgnoreCase);
        //            int maxLength = cadena.Length;
        //            if (index >= 0)
        //            {
        //                index += campo.Length;

        //                for (; index < maxLength && !cadena[index].Equals('='); index++) ;
        //                for (index++; index < maxLength && !cadena[index].Equals(';'); index++)
        //                    campos[campo] += cadena[index];
        //            }
        //            campos[campo] = campos[campo].Trim();
        //        }
        //        return true;
        //    };

        //    if ((short)origen.Proveedor < 100)
        //    {
        //        campos = new Dictionary<string, string>() { { "Source", "" }, { "ID", "" }, { "Password", "" }, { "Catalog", "" } };
        //        analizarCadena(origen.cadenaConexion);
        //        origen._usuarioDB = campos["ID"];
        //        origen._contraseñaDB = campos["Password"];
        //        origen._nombreBD = campos["Catalog"];
        //        origen._servidorBD = campos["Source"];
        //    }
        //    else if (origen.Proveedor == Proveedor.Hana)
        //    {
        //        campos = new Dictionary<string, string>() { { "DRIVER", "" }, { "UID", "" }, { "PWD", "" }, { "SERVERNODE", "" }, { "DATABASE", "" }, { "CS", "" }, { "currentSchema", "" }, { "databaseName", ""} };
        //        analizarCadena(origen.cadenaConexion);
        //        origen._usuarioDB = campos["UID"];
        //        origen._contraseñaDB = campos["PWD"];
        //        origen._nombreBD = (string.IsNullOrEmpty(campos["CS"]) ? campos["currentSchema"] : campos["CS"]).Replace("\"", "");
        //        origen._tenant = string.IsNullOrEmpty("DATABASE") ? campos["databaseName"] : campos["DATABASE"];
        //        origen._servidorBD = campos["SERVERNODE"];
        //    }
        //    return origen;
        //}

        /// <summary>
        /// Construye la cadena de conexión según los datos proporcionados.
        /// </summary>
        /// <param name="servidor">Nombre del servidor de base de datos</param>
        /// <param name="nombre">Nombre de la base de datos</param>
        /// <param name="usuario">Usuario</param>
        /// <param name="contraseña">Contraseña</param>
        /// <param name="proveedor">Indica bajo qué plataforma se provee la base de datos.</param>
        /// <returns></returns>
        public static string ObtenerCadenaDeConexion(string servidor, string nombre, string usuario, string contraseña, Proveedor proveedor)
        {
            return ObtenerCadenaDeConexion(servidor, nombre, usuario, contraseña, null, proveedor);
        }
        
        public static string ObtenerCadenaDeConexion(string servidor, string nombre, string usuario, string contraseña, string tenants, Proveedor proveedor)
        {
            if (string.IsNullOrEmpty(servidor))
                throw new ArgumentNullException("No se ha especificado el servidor");

            string cadenaConexion = string.Empty;

            if ((short)proveedor < 100)
            {
                if (!string.IsNullOrEmpty(nombre))
                {
                    cadenaConexion = $"Data Source={servidor};Initial Catalog={nombre};";
                }
                else
                {
                    cadenaConexion = $"Data Source={servidor};";
                }
                if (!string.IsNullOrEmpty(usuario) && !string.IsNullOrEmpty(contraseña))
                {
                    cadenaConexion += $"User ID={usuario};Password={contraseña};Persist Security Info=False;Integrated Security=False;";
                }
                else
                {
                    cadenaConexion += "Persist Security Info=False;Integrated Security=True;";
                }
            }
            else if (proveedor == Proveedor.Hana)
            {
                string drive = Environment.Is64BitProcess ? "{HDBODBC}" : "{HDBODBC32}";

                if (!string.IsNullOrEmpty(usuario) && !string.IsNullOrEmpty(contraseña))
                {
                    cadenaConexion = $"DRIVER={drive};SERVERNODE={servidor};UID={usuario};PWD={contraseña};";
                }
                else
                {
                    throw new ArgumentNullException("No se ha especificado el usuario o contraseña");
                }

                if (!string.IsNullOrEmpty(nombre))
                {
                    string[] partes = (nombre ?? "").Split("@|;:".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (partes.Count() > 1)
                    {
                        cadenaConexion += $"DATABASE={partes[0]};databaseName={partes[0]};CS=\"{partes[1]}\";currentSchema=\"{partes[1]}\"";
                    }
                    else
                    {
                        string db = Origen.BaseDeDatosHana(new Origen(cadenaConexion, Proveedor.Hana, false));
                        if (!string.IsNullOrEmpty(db))
                        {
                            cadenaConexion += $"DATABASE={db};CS=\"{nombre}\";currentSchema=\"{nombre}\"";
                        }
                        else
                        {
                            cadenaConexion += $"CS=\"{nombre}\"";
                        }
                    }
                }
            }
            return cadenaConexion;
        }

        //private static string[] DescomponerNombreBD(string nombre)
        //{
        //    return nombre.Split("|;:".ToCharArray());
        //}

        /// <summary>
        /// Lista el catálogo de bases de datos dispobibles
        /// </summary>
        /// <returns></returns>
        public List<string> ListaCatalogos()
        {
            return ListaCatalogos(this);
        }

        /// <summary>
        /// Lista el catálogo de bases de datos dispobibles en el origen
        /// </summary>
        /// <param name="origen"></param>
        /// <returns></returns>
        public static List<string> ListaCatalogos(Origen origen)
        {
            using (ConexionDatos conn = new ConexionDatos(origen))
            {
                List<string> lista = new List<string>();
                conn.CargarConsulta(ConsultasUtiles.ListaBaseDeDatos);
                conn.EjecutarLector();
                while (conn.LeerFila())
                {
                    lista.Add(conn.DatoLector<string>("Nombre"));
                }
                conn.CerrarLector();
                return lista;
            }
        }

        /// <summary>
        /// Retorna la base de datos habilitada en Hana (NDB o HDB)
        /// </summary>
        /// <param name="origen"></param>
        /// <returns></returns>
        public static string BaseDeDatosHana(Origen origen)
        {
            if (origen.Proveedor != Proveedor.Hana)
                return null;

            using (ConexionDatos conn = new ConexionDatos(origen))
            {
                try
                {
                    conn.CargarConsulta(ConsultasUtiles.ListaBaseDeDatosHana);
                    string dbName = conn.EjecutarEscalar<string>();
                    return dbName;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        //string servidor, string nombre, string usuario, string contraseña, Proveedor proveedor

        /// <summary>
        /// Prueba que se pueda abrir una conexión válida con el origen de datos especificado al construir el objeto.
        /// </summary>
        /// <returns>Devuelve <see langword="true"/> si la prueba de conexión fue satisfactoria; de lo contrario, retorna <see langword="false"/>.</returns>
        public bool ProbarConexion(bool liberarExcepcion = false)
        {
            try
            {
                using (ConexionDatos conn = new ConexionDatos(this))
                {
                    return conn.ProbarConexion(liberarExcepcion);
                }
            }
            catch(Exception ex)
            {
                if (liberarExcepcion)
                    throw ex;
                else
                    return false;
            }
        }

        #region Proveedor de Hana
        private static bool _proveedorDatosHanaInicializado = false;
        private static DbProviderFactory _proveedorDatosHana = null;
        private static string _fabricaHana = null;

        private static DbProviderFactory ObtenerFabricaHana()
        {
            if (_proveedorDatosHanaInicializado)
            {
                Fabrica = _fabricaHana;
                return _proveedorDatosHana;
            }

            try
            {
                DbProviderFactory fabrica = null;

                //En primera instancia, se intenta cargar la fábrica Sap.Data.Hana.
                #region Carga de fábrica
                try
                {
                    fabrica = DbProviderFactories.GetFactory("Sap.Data.Hana");
                    using (System.Data.Common.DbConnection conn = fabrica.CreateConnection())
                    {
                        _fabricaHana = "Sap.Data.Hana";
                        _proveedorDatosHana = fabrica;
                        Depuracion.Error("Fábrica Sap.Data.Hana cargada con éxito.");
                        return _proveedorDatosHana;
                    }
                }
                catch { Depuracion.Error("Fábrica Sap.Data.Hana no encontrado."); }
                #endregion

                //En segunda instancia se intenta cargar los ensamblados.
                #region Carga de ensamblados
                // Ruta de la carpeta "Program Files" de 64 bits
                string programFiles64 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

                // Ruta de la carpeta "Program Files (x86)" de 32 bits
                string programFiles32 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                string[] rutas;
                
                if (Environment.Is64BitProcess)
                {
                    rutas = new string[] {
                        System.IO.Path.Combine(programFiles64, @"sap\hdbclient\ado.net\v4.5\Sap.Data.Hana.v4.5.dll"),
                        System.IO.Path.Combine(programFiles64, @"sap\hdbclient\ado.net\v3.5\Sap.Data.Hana.v3.5.dll"),
                        System.IO.Path.Combine(programFiles32, @"sap\hdbclient\ado.net\v4.5\Sap.Data.Hana.v4.5.dll"),
                        System.IO.Path.Combine(programFiles32, @"sap\hdbclient\ado.net\v3.5\Sap.Data.Hana.v3.5.dll"),
                    };
                }
                else
                {
                    rutas = new string[] {
                        System.IO.Path.Combine(programFiles32, @"sap\hdbclient\ado.net\v3.5\Sap.Data.Hana.v3.5.dll"),
                        System.IO.Path.Combine(programFiles32, @"sap\hdbclient\ado.net\v4.5\Sap.Data.Hana.v4.5.dll"),
                    };
                }

                foreach(string ruta in rutas)
                {
                    fabrica = CargarEnsamblado(ruta);
                    if (fabrica != null)
                    {
                        _fabricaHana = "Sap.Data.Hana";
                        _proveedorDatosHana = fabrica;
                        return _proveedorDatosHana;
                    }
                }
                #endregion

                //En tercer lugar, si fallan todas las opciones anteriores, se carga Odbc.
                _proveedorDatosHana = DbProviderFactories.GetFactory("System.Data.Odbc"); //System.Data.Odbc.OdbcFactory.Instance;
                _fabricaHana = "System.Data.Odbc";
            }
            catch (Exception ex)
            {
                Depuracion.Error($"Error inesperado al cargar el proveedor de Hana.");
                _proveedorDatosHana = DbProviderFactories.GetFactory("System.Data.Odbc");
            }
            finally
            {
                Fabrica = _fabricaHana;
                _proveedorDatosHanaInicializado = true;
            }

            return _proveedorDatosHana;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ruta"></param>
        private static DbProviderFactory CargarEnsamblado(string ruta)
        {
            try
            {
                if (!System.IO.File.Exists(ruta))
                    return null;

                // Nombre completo del tipo del Factory de SAP HANA
                string factoryTypeName = "Sap.Data.Hana.HanaFactory";

                // Carga el ensamblado que contiene el Factory de SAP HANA en tiempo de ejecución
                Assembly assembly = Assembly.LoadFrom(ruta);

                // Obtiene el tipo del Factory de SAP HANA
                Type factoryType = assembly.GetType(factoryTypeName);

                if (factoryType != null)
                {
                    FieldInfo fieldInfo = factoryType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);

                    if (fieldInfo != null)
                    {
                        System.Data.Common.DbProviderFactory factory = (System.Data.Common.DbProviderFactory)fieldInfo.GetValue(null);
                        using (System.Data.Common.DbConnection conn = factory.CreateConnection())
                        {
                            Depuracion.Error($"La librería '{ruta}' se cargó correctamente.");
                        }

                        return factory;
                    }
                }

                return null;
            }
            catch(Exception ex)
            {
                Depuracion.Error($"No fue posible cargar la librería '{ruta}'.", ex);
                return null;
            }
        }
        #endregion

        #region SubTipo
        public enum Plataforma : byte
        {
            x86 = 0,
            x64 = 1
        }
        #endregion
    }
}