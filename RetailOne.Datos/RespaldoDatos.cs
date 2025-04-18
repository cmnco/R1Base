using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace RetailOne.Datos
{
    public partial class RespaldoDatos
    {
        #region Propiedades
        public Origen Origen { get; private set; }
        private string directorioBackups;
        public string DirectorioBackups {
            get
            {
                if (string.IsNullOrEmpty(directorioBackups))
                {
                    directorioBackups = ObtenerDirectorioBackups();
                }
                return directorioBackups;
            }
        }
        #endregion

        #region Constructor
        public RespaldoDatos(Origen origen)
        {
            Origen = new Origen(origen.ServidorBD, null, origen.UsuarioDB, origen.ContraseñaDB, origen.Proveedor);
        }
        #endregion

        #region Métodos
        /// <summary>
        /// Verifica si el directorio existe en caso contrario lo crea.
        /// </summary>
        /// <param name="directorio">Directorio donde se creará el archivo de respaldo, en caso de ser nulo se asigna por defecto Backups en el directorio actual.</param>
        /// <returns></returns>
        private string PrepararDirectorioRespaldos(string directorio)
        {
            if (directorio == null)
                directorio = DirectorioBackups;

            if (!ExisteRuta(directorio))
            {
                //Directory.CreateDirectory(directorio);
                throw new Exception(string.Format("No se encontró o no es accesible el directorio \"{0}\"", directorio));
            }
            return directorio;

            //try
            //{

            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(string.Format("No se encontró o no fue posible crear el directorio \"{0}\"", directorio), ex);
            //}
        }

        public string ObtenerDirectorioBackups()
        {
            using (ConexionDatos conn = new ConexionDatos(this.Origen))
            {
                conn.CargarConsulta(ObtenerDirectorioData);
                var directorio = conn.EjecutarEscalar<string>();
                DirectoryInfo dirInfo = new DirectoryInfo(directorio);
                return Path.Combine(dirInfo.Parent.FullName, "Backup");
            }
        }

        /// <summary>
        /// Realiza un backup de la base de datos especificada.
        /// </summary>
        /// <param name="databaseName">Nombre de base de datos a realizar el backup.</param>
        /// <returns>Ruta absoluta del archivo .bak resultante.</returns>
        public string Backup(string databaseName)
        {
            return Backup(databaseName, null);
        }
        
        /// <summary>
        /// Realiza un backup de la base de datos especificada.
        /// </summary>
        /// <param name="databaseName">Nombre de base de datos a realizar el backup.</param>
        /// <param name="directorioRespaldo">Ruta destino del archivo .bak</param>
        /// <returns>Ruta absoluta del archivo .bak resultante.</returns>
        public string Backup(string databaseName, string directorioRespaldo)
        {
            Validar();

            directorioRespaldo = PrepararDirectorioRespaldos(directorioRespaldo);
            string rutaArchivo = BuildBackupPathWithFilename(databaseName, directorioRespaldo);

            using (ConexionDatos conn = new ConexionDatos(this.Origen))
            {
                conn.CargarConsulta(InstruccionBackup);
                conn.AgregarLiteral("NOMBRE_DB", databaseName);
                conn.AgregarParametro("pRuta", rutaArchivo);
                conn.Comando.CommandTimeout = 500;
                conn.EjecutarComando();
            }

            return rutaArchivo;
        }

        /// <summary>
        /// Restaura la base de datos especificada a partir de un backup previamente creado según el nombre y la fecha actual.
        /// </summary>
        /// <param name="databaseName">Nombre de base de datos a restaurar</param>
        /// <param name="rutaArchivoBak">Ruta absoluta del archivo de respaldo .bak</param>
        public void Restore(string databaseName, string rutaArchivoBak)
        {
            Validar();

            if(string.IsNullOrEmpty(databaseName))
                throw new Exception("No se ha proporcionado el nombre de la base de datos a restaurar.");

            if (string.IsNullOrEmpty(rutaArchivoBak) || !ExisteRuta(rutaArchivoBak))
                throw new Exception(string.Format("No se encontró el archivo de respaldo \"{0}\"", rutaArchivoBak));
            
            using (ConexionDatos conn = new ConexionDatos(this.Origen))
            {
                conn.CargarConsulta(InstruccionRestore);
                conn.AgregarLiteral("NOMBRE_DB", databaseName);
                conn.AgregarParametro("pRuta", rutaArchivoBak);
                conn.Comando.CommandTimeout = 500;
                conn.EjecutarComando();
            }
        }

        /// <summary>
        /// Valida si existe un directorio o archivo en el host donde se aloja la base de datos.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool ExisteRuta(string path)
        {
            using (ConexionDatos conn = new ConexionDatos(this.Origen))
            {
                conn.CargarConsulta(ValidarExistenciaRuta);
                if (!Path.HasExtension(path))
                    path = Path.Combine(path, "nul");
                conn.AgregarParametro("pDestino", path);
                return conn.EjecutarEscalar<int>() != 0;
            }
        }

        /// <summary>
        /// Validaciones previas
        /// </summary>
        private void Validar()
        {
            if (((short)this.Origen.Proveedor) >= 100)
                throw new Exception("Proveedor de datos no soportado.");
        }

        /// <summary>
        /// Construye la ruta completa del archivo .bak en función del directorio destino, nombre de BD y fecha actual.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="backupFolderFullPath"></param>
        /// <returns></returns>
        private string BuildBackupPathWithFilename(string databaseName, string backupFolderFullPath)
        {
            string filename = string.Format("{0}-{1}.bak", databaseName, DateTime.Now.ToString("yyyy-MM-dd"));

            return Path.Combine(backupFolderFullPath, filename);
        }
        #endregion

        #region Consultas
        private static Consulta InstruccionBackup = new Consulta(Proveedor.SQLServer, new ComandoConsulta(Proveedor.SQLServer,
            @"BACKUP DATABASE [${NOMBRE_DB}] TO DISK = @pRuta WITH NOFORMAT, INIT, SKIP, NOREWIND, NOUNLOAD, STATS = 10"));

        private static Consulta InstruccionRestore = new Consulta(Proveedor.SQLServer, new ComandoConsulta(Proveedor.SQLServer,
            @"ALTER DATABASE [${NOMBRE_DB}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
              RESTORE DATABASE [${NOMBRE_DB}] FROM DISK = @pRuta WITH FILE = 1, NOUNLOAD, REPLACE, STATS = 5;
              ALTER DATABASE [${NOMBRE_DB}] SET MULTI_USER;"));

        private static Consulta ObtenerDirectorioData = new Consulta(Proveedor.SQLServer, new ComandoConsulta(Proveedor.SQLServer,
            @"SELECT serverproperty('InstanceDefaultDataPath')")); //'${RUTA}'

        private static Consulta CrearDirectorio = new Consulta(Proveedor.SQLServer, new ComandoConsulta(Proveedor.SQLServer,
            @"DECLARE @BackupDestination nvarchar(500) = N'C:\Backups';
              DECLARE @DirectoryExists int;
              SET NOCOUNT ON;
              EXEC master.dbo.xp_fileexist @BackupDestination, @DirectoryExists OUT;
              IF @DirectoryExists = 0
              EXEC master.sys.xp_create_subdir @BackupDestination;
              SET NOCOUNT OFF;"));

        private static Consulta ValidarExistenciaRuta = new Consulta(Proveedor.SQLServer, new ComandoConsulta(Proveedor.SQLServer,
            @"DECLARE @pDirectorioExiste int;
              EXEC master.dbo.xp_fileexist @pDestino, @pDirectorioExiste OUT;
              SELECT @pDirectorioExiste;"));

        #endregion
    }
}
