using RetailOne.Datos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RetailOne.Seguridad
{
    public class ConsultaControlEventos
    {
        public static Consulta VerificarTablaEventos = new Consulta(Proveedor.SQLServer,
            new ComandoConsulta(Proveedor.SQLServer, @"SELECT COUNT(TABLE_NAME) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = @pSchema AND TABLE_NAME = '@SO1_01REGEVENTOS'"),
            new ComandoConsulta(Proveedor.Hana, @"SELECT COUNT(TABLE_NAME) FROM TABLES WHERE schema_name = :pSchema AND table_name = '@SO1_01REGEVENTOS'"));

        public static Consulta CrearTablaEventos = new Consulta(Proveedor.SQLServer,
            new ComandoConsulta(Proveedor.SQLServer, @"CREATE TABLE [dbo].[@SO1_01REGEVENTOS](
                                                           [Code] [nvarchar](50) NOT NULL,
                                                           [Name] [nvarchar](100) NOT NULL,
                                                           [U_SO1_COMPONENTE] [nvarchar](200) NOT NULL,
                                                           [U_SO1_DESCRIPCION] [nvarchar](200) NOT NULL,
                                                           [U_SO1_DETALLE] [nvarchar](max) NOT NULL,
                                                           [U_SO1_REFERENCIA] [nvarchar](200) NULL,
                                                           [U_SO1_FECHA] [datetime] NOT NULL,
                                                           [U_SO1_HORA] [smallint] NOT NULL,
                                                           [U_SO1_TIPO] [tinyint] NOT NULL,
                                                           [U_SO1_USUARIO] [nvarchar](48) NOT NULL,
                                                           ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]"),
            new ComandoConsulta(Proveedor.Hana, @"CREATE TABLE [@SO1_01REGEVENTOS] (
                                                      [Code] NVARCHAR(50) PRIMARY KEY NOT NULL,
                                                      [Name] NVARCHAR(100) NOT NULL,
                                                      [U_SO1_COMPONENTE] NVARCHAR(200) NOT NULL,
                                                      [U_SO1_DESCRIPCION] NVARCHAR(200) NOT NULL,
                                                      [U_SO1_DETALLE] NCLOB NULL,
                                                      [U_SO1_REFERENCIA] NVARCHAR(200) NULL,
                                                      [U_SO1_FECHA] TIMESTAMP NOT NULL,
                                                      [U_SO1_HORA] SMALLINT NOT NULL,
                                                      [U_SO1_TIPO] SMALLINT NOT NULL,
                                                      [U_SO1_USUARIO] NVARCHAR(48) NOT NULL
                                                      )"));

        internal static Consulta VerificarColumnas = new Consulta(Proveedor.SQLServer,
            new ComandoConsulta(Proveedor.SQLServer, @"IF NOT EXISTS(SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_CATALOG = @pSchema AND TABLE_NAME = '@SO1_01REGEVENTOS' AND COLUMN_NAME = 'U_SO1_REFERENCIA')
BEGIN
	ALTER TABLE [@SO1_01REGEVENTOS] ADD [U_SO1_REFERENCIA] NVARCHAR(200) NULL;
END;"));


        private static string obtenerProximoID = @"SELECT(ISNULL(MAX(ID),0)+1) FROM[@SO1_01REGEVENTOS]";
        public static Consulta ObtenerProximoID = new Consulta(Proveedor.SQLServer,
            new ComandoConsulta(Proveedor.SQLServer, obtenerProximoID),
            new ComandoConsulta(Proveedor.Hana, obtenerProximoID));

        private static string registrarEventos = @"INSERT INTO  [@SO1_01REGEVENTOS] 
                                                                ([U_SO1_COMPONENTE],
                                                                [U_SO1_DESCRIPCION],
                                                                [U_SO1_DETALLE],
                                                                [U_SO1_FECHA],
                                                                [U_SO1_HORA],
                                                                [U_SO1_TIPO],
                                                                [U_SO1_USUARIO],
                                                                [U_SO1_REFERENCIA],
                                                                [Code],
                                                                [Name]
                                                            ) VALUES (
                                                                @pComponente,
                                                                @pDescripcion,
                                                                @pDetalle,
                                                                @pFecha,
                                                                @pHora,
                                                                @pTipoEvento,
                                                                @pUsuario,
                                                                @pReferencia, 
                                                                @pCode,
                                                                @pName)";

        public static Consulta RegistrarEvento = new Consulta(Proveedor.SQLServer,
            new ComandoConsulta(Proveedor.SQLServer, registrarEventos),
            new ComandoConsulta(Proveedor.Hana, registrarEventos));

        private static string actualizarEventos = @"UPDATE [@SO1_01REGEVENTOS] SET
                                                            [U_SO1_COMPONENTE] = @pComponente, 
                                                            [U_SO1_DESCRIPCION] = @pDescripcion, 
                                                            [U_SO1_DETALLE] = @pDetalle, 
                                                            [U_SO1_FECHA] = @pFecha, 
                                                            [U_SO1_HORA] = @pHora, 
                                                            [U_SO1_TIPO] = @pTipoEvento,
                                                            [U_SO1_USUARIO] = @pUsuario,
                                                            [U_SO1_REFERENCIA] = @pReferencia
                                                            WHERE [Code] = @pCode AND [Name] = @pName";

        public static Consulta ActualizarEventos = new Consulta(Proveedor.SQLServer,
            new ComandoConsulta(Proveedor.SQLServer, actualizarEventos),
            new ComandoConsulta(Proveedor.Hana, actualizarEventos));

        public static Consulta ExisteEvento = new Consulta(Proveedor.SQLServer,
            new ComandoConsulta(Proveedor.SQLServer, "SELECT (CASE WHEN EXISTS(SELECT [Code] FROM [@SO1_01REGEVENTOS] WHERE [Code] = @pCode) THEN 'Y' ELSE 'N' END) AS [Existe]"),
            new ComandoConsulta(Proveedor.Hana, "SELECT (CASE WHEN EXISTS(SELECT [Code] FROM [@SO1_01REGEVENTOS] WHERE [Code] = @pCode) THEN 'Y' ELSE 'N' END) AS [Existe] FROM DUMMY"));

        private static string consultar = @"SELECT  [Code],
                                                    [Name],
                                                    [U_SO1_COMPONENTE],
                                                    [U_SO1_DESCRIPCION],
                                                    [U_SO1_DETALLE],
                                                    [U_SO1_FECHA],
                                                    [U_SO1_HORA],
                                                    [U_SO1_TIPO],
                                                    [U_SO1_USUARIO],
                                                    [U_SO1_REFERENCIA]
                                                    FROM [@SO1_01REGEVENTOS] WHERE [Code] = @pCode";

        public static Consulta Consultar = new Consulta(Proveedor.SQLServer,
            new ComandoConsulta(Proveedor.SQLServer, consultar),
            new ComandoConsulta(Proveedor.Hana, consultar));

        //private static string filtradoDeEventos = @"
        //    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
        //    BEGIN TRANSACTION;  
        //    SELECT TOP 50 [Code],
        //                                               [Name],
        //                                               [U_SO1_COMPONENTE],
        //                                               [U_SO1_DESCRIPCION],
        //                                               [U_SO1_FECHA],
        //                                               [U_SO1_HORA],
        //                                               [U_SO1_TIPO],
        //                                               [U_SO1_USUARIO] 
        //                                         FROM [@SO1_01REGEVENTOS] WHERE 
        //                                               (@pTipoEvento IS NULL OR [U_SO1_TIPO] = @pTipoEvento) 
        //                                               AND (@pComponente IS NULL OR [U_SO1_COMPONENTE] = @pComponente) 
        //                                               AND (@pDesde IS NULL OR [U_SO1_FECHA] >= @pDesde) 
        //                                               AND (@pHasta IS NULL OR [U_SO1_FECHA] < @pHasta)
        //                                               AND (@pCriterio IS NULL OR [U_SO1_DESCRIPCION] LIKE @pCriterio) ORDER BY U_SO1_FECHA DESC;
        //    COMMIT TRANSACTION;"; //OR [U_SO1_DETALLE] LIKE @pCriterio 

        public static Consulta FiltradoDeEventos = new Consulta(Proveedor.SQLServer,
            new ComandoConsulta(Proveedor.SQLServer, @"SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
                                                       BEGIN TRANSACTION;  
                                                           SELECT TOP 50 [Code],
                                                           [Name],
                                                           [U_SO1_COMPONENTE],
                                                           [U_SO1_DESCRIPCION],
                                                           [U_SO1_FECHA],
                                                           [U_SO1_HORA],
                                                           [U_SO1_TIPO],
                                                           [U_SO1_USUARIO],
                                                           [U_SO1_REFERENCIA]
		                                                   FROM [@SO1_01REGEVENTOS] WHERE 
                                                           (@pTipoEvento IS NULL OR [U_SO1_TIPO] = @pTipoEvento) 
                                                           AND (@pComponente IS NULL OR [U_SO1_COMPONENTE] = @pComponente)
                                                           AND (@pCode IS NULL OR [Code] = @pCode) 
                                                           AND (@pName IS NULL OR [Name] = @pName)
                                                           AND (@pReferencia IS NULL OR [U_SO1_REFERENCIA] = @pReferencia) 
                                                           AND (@pDesde IS NULL OR [U_SO1_FECHA] >= @pDesde) 
                                                           AND (@pHasta IS NULL OR [U_SO1_FECHA] < @pHasta)
                                                           AND (@pCriterio IS NULL OR [U_SO1_DESCRIPCION] LIKE @pCriterio) ORDER BY U_SO1_FECHA DESC;
                                                       COMMIT TRANSACTION;"),
            new ComandoConsulta(Proveedor.Hana, @"SELECT TOP 50 [Code],
                                                       [Name],
                                                       [U_SO1_COMPONENTE],
                                                       [U_SO1_DESCRIPCION],
                                                       [U_SO1_FECHA],
                                                       [U_SO1_HORA],
                                                       [U_SO1_TIPO],
                                                       [U_SO1_USUARIO],
                                                       [U_SO1_REFERENCIA]
		                                               FROM [@SO1_01REGEVENTOS] WHERE 
                                                       (@pTipoEvento IS NULL OR [U_SO1_TIPO] = @pTipoEvento) 
                                                       AND (@pComponente IS NULL OR [U_SO1_COMPONENTE] = @pComponente) 
                                                       AND (@pCode IS NULL OR [Code] = @pCode) 
                                                       AND (@pName IS NULL OR [Name] = @pName) 
                                                       AND (@pReferencia IS NULL OR [U_SO1_REFERENCIA] = @pReferencia) 
                                                       AND (@pDesde IS NULL OR [U_SO1_FECHA] >= @pDesde) 
                                                       AND (@pHasta IS NULL OR [U_SO1_FECHA] < @pHasta)
                                                       AND (@pCriterio IS NULL OR [U_SO1_DESCRIPCION] LIKE @pCriterio) ORDER BY U_SO1_FECHA DESC;"));

        public static Consulta EliminarRegistrosAntiguos = new Consulta(Proveedor.SQLServer,
            new ComandoConsulta(Proveedor.SQLServer, "DELETE FROM [@SO1_01REGEVENTOS] WHERE [U_SO1_FECHA] <= @pFechaHora"),
            new ComandoConsulta(Proveedor.Hana, "DELETE FROM [@SO1_01REGEVENTOS] WHERE [U_SO1_FECHA] <= @pFechaHora"));

        public static Consulta ListarComponentes = new Consulta(Proveedor.SQLServer,
            new ComandoConsulta(Proveedor.SQLServer, "SELECT DISTINCT [U_SO1_COMPONENTE] FROM [@SO1_01REGEVENTOS]"),
            new ComandoConsulta(Proveedor.Hana, "SELECT DISTINCT [U_SO1_COMPONENTE] FROM [@SO1_01REGEVENTOS]"));
    }
}
