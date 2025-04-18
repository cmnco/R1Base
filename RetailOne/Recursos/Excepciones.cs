using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RetailOne.Utilidades;

namespace RetailOne.Recursos
{
    public static class Excepciones
    {
        public static Cadenas NoDisponeDePermisosParaLecturaEscritura = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "No dispone de permisos suficientes para lectura/escritura de archivos. Ruta: '{0}'"),
            new Cadena(Cadenas.English, "You do not have sufficient permissions to read/write files. Path: '{0}'."));

        public static Cadenas NoSePudoCrearElDirectorio = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "No se pudo crear el directorio. Ruta: '{0}'"),
            new Cadena(Cadenas.English, "Could not create directory. Path: '{0}'."));
    }
}
