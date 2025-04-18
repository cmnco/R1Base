using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RetailOne.Utilidades.Recursos
{
    /// <summary>
    /// Definición de las distintas cadenas usadas al lanzar excepciones.
    /// </summary>
    internal static class Excepciones
    {
        /// <summary>
        /// El archivo '{0}' no existe o no es accesible.
        /// </summary>
        public static Cadenas ArchivoNoExisteNoAccesible = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "El archivo '{0}' no existe o no es accesible."),
            new Cadena(Cadenas.English, "The file '{0}' doesn't exists or isn't accesible."));

        /// <summary>
        /// El descriptor Xml es nulo o vacío.
        /// </summary>
        public static Cadenas DescriptorXmlNuloOVacío = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "El descriptor Xml es nulo o vacío."),
            new Cadena(Cadenas.English, "The XML descriptor is null or empty."));

        /// <summary>
        /// El descriptor Xml no tiene un formato correcto.
        /// </summary>
        public static Cadenas DescriptorXmlFormatoIncorrecto = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "El descriptor Xml no tiene un formato correcto."),
            new Cadena(Cadenas.English, "The XML descriptor hasn't a correct format."));

        /// <summary>
        /// El archivo no tiene un formato correcto.
        /// </summary>
        public static Cadenas ArchivoFormatoIncorrecto = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "El archivo no tiene un formato correcto."),
            new Cadena(Cadenas.English, "The file hasn't a correct format."));
        
        /// <summary>
        /// No se puede hallar la clase '{0}' en ninguno de los espacios de nombre identificados.
        /// </summary>
        public static Cadenas ClaseNoEncontradaEnNS = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "No se puede hallar la clase '{0}' en ninguno de los espacios de nombre identificados."),
            new Cadena(Cadenas.English, "Can't find the class '{0}' in any of the identified namespaces."));

        /// <summary>
        /// Error al intentar recuperar el objeto de la secuencia.
        /// </summary>
        public static Cadenas ErrorAlRecuperarObjetoDeSecuencia = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "Error al intentar recuperar el objeto de la secuencia."),
            new Cadena(Cadenas.English, "Failed to retrieve the object from the sequence."));

        /// <summary>
        /// Error al intentar serializar el objeto. Tipo: '{0}'.
        /// </summary>
        public static Cadenas ErrorAlSerializarObjeto = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "Error al intentar serializar el objeto. Tipo: '{0}'."),
            new Cadena(Cadenas.English, "Error trying to serialize the object. Type: '{0}'."));


        /// <summary>
        /// No se ha proporcionado una lista de correos.
        /// </summary>
        public static Cadenas NoSeHaProporcionadoListaCorreos = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "No se ha proporcionado una lista de correos."),
            new Cadena(Cadenas.English, "No mailing list provided."));

        /// <summary>
        /// El correo '{0}' no tiene un formato válido.
        /// </summary>
        public static Cadenas ElCorreoNotieneFormatoValido = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "El correo '{0}' no tiene un formato válido."),
            new Cadena(Cadenas.English, "The email '{0}' is not in a valid format."));

        /// <summary>
        /// Valor no válido.
        /// </summary>
        public static Cadenas ValorNoValido = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "Valor no válido. Referencia: '{0}."),
            new Cadena(Cadenas.English, "Invalid value. Reference: '{0}.'"));



        /// <summary>
        /// El archivo comprimido no es valido o está encriptado. Archivo:'{0}'.
        /// </summary>
        public static Cadenas ArchivoComprimidoNoValido = new Cadenas(Cadenas.CulturaNeutra,
            new Cadena(Cadenas.Español, "El archivo comprimido no es valido o está encriptado. Archivo:'{0}'."),
            new Cadena(Cadenas.English, "The compressed file is invalid or encrypted. File:'{0}'."));

    }
}