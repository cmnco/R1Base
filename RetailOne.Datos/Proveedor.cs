using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RetailOne.Datos
{
    /// <summary>
    /// Indica la plataforma de una base de datos.
    /// </summary>
    public enum Proveedor : short
    {
        SQLServer = 0,
        SQLServer2005 = 10,
        SQLServer2008 = 20,
        SQLServer2012 = 30,
        SQLServer2014 = 40,
        Hana = 100,
        MySQL = 200,
        Oracle = 300,
        PostgreSQL = 400
    }
}
