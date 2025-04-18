using RetailOne.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailOne.Recursos
{
    public static class Mensajes
    {
        public static Cadenas EjecucionDeConsultasABaseDeDatos = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "Ejecución de consulta a base de datos."),
            new Cadena(Cadenas.English, "Database query execution."));

        public static Cadenas ErrorEnConsultaABaseDeDatos = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "Error en ejecución de consulta a base de datos."),
            new Cadena(Cadenas.English, "Database query execution error."));
    }
}
