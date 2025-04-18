using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RetailOne.Datos
{
    public class ConsultasUtiles
    {
        public static Consulta SiguienteNumero = new Consulta(Proveedor.SQLServer,
            new ComandoConsulta(Proveedor.SQLServer, "select isnull(max($[Columna]), 0) + 1 from $[Tabla]"), 
            new ComandoConsulta(Proveedor.Hana, "select ifnull(max($[Columna]), 0) + 1 from $[Tabla]"));

        public static Consulta SiguienteNumeroDadoOtroCampo = new Consulta(Proveedor.SQLServer,
            new ComandoConsulta(Proveedor.SQLServer, "select isnull(max($[Columna]), 0) + 1 from $[Tabla] where $[Campo] = @Valor"),
            new ComandoConsulta(Proveedor.Hana, "select ifnull(max($[Columna]), 0) + 1 from $[Tabla] where $[Campo] = @Valor"));

        internal static Consulta VerificarActivacionTextoCompleto = new Consulta(Proveedor.SQLServer,
            new ComandoConsulta(Proveedor.SQLServer, "select case when fulltextserviceproperty('IsFullTextInstalled') = 1 and DATABASEPROPERTY(DB_NAME(), 'IsFullTextEnabled') = 1 then 1 else 0 end"));

        public static Consulta ListaBaseDeDatos = new Consulta(Proveedor.SQLServer,
            new ComandoConsulta(Proveedor.SQLServer, "SELECT NAME AS Nombre FROM SYS.DATABASES WHERE DATABASE_ID > 4 ORDER BY NAME"),
            new ComandoConsulta(Proveedor.Hana, "SELECT SCHEMA_NAME AS Nombre FROM [SYS].[SCHEMAS] WHERE [HAS_PRIVILEGES] = 'TRUE' ORDER BY SCHEMA_NAME"));

        public static Consulta ListaBaseDeDatosHana = new Consulta(Proveedor.SQLServer,
            new ComandoConsulta(Proveedor.Hana, "SELECT TOP 1 [DATABASE_NAME] FROM [SYS].[M_DATABASES] WHERE [ACTIVE_STATUS] = 'YES'"));
    }
}