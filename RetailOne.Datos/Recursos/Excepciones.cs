using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RetailOne.Utilidades;

namespace RetailOne.Datos.Recursos
{
    public static class Excepciones
    {
        public static Cadenas NoPudoIniciarTransacción = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "No se pudo iniciar la transacción."),
            new Cadena(Cadenas.English, "The transaction failed to start."));

        public static Cadenas NoPudoConfirmarTransacción = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "No se pudo confirmar la transacción."),
            new Cadena(Cadenas.English, "The transaction failed to commit."));

        public static Cadenas NoPudoRestaurarTransacción = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "No se pudo restaurar la transacción."),
            new Cadena(Cadenas.English, "The transaction failed to rollback."));

        public static Cadenas TextoComandoNoEspecificado = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "Debe especificar un texto de comando para poder ejecutar este método."),
            new Cadena(Cadenas.English, "A text command must be specified to run this method."));

        public static Cadenas ErrorComando = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "Error al ejecutar comando."),
            new Cadena(Cadenas.English, "Error executing command."));

        public static Cadenas ErrorComandoLector = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "Error al ejecutar comando para lector."),
            new Cadena(Cadenas.English, "Error executing command for obtaining a data reader."));

        public static Cadenas ErrorComandoEscalar = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "Error al ejecutar comando escalar."),
            new Cadena(Cadenas.English, "Error executing command for obtaining scalar."));

        public static Cadenas ErrorComandoTabla = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "Error al ejecutar comando para devolución de tabla."),
            new Cadena(Cadenas.English, "Error executing command for obtaining a data table."));
    }
}
