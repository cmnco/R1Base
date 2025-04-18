using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace RetailOne.Utilidades
{
    /// <summary>
    /// Provee métodos de utilidad para la validación, parseo y manejo de archivos y cadenas XML.
    /// </summary>
    public static class UtilidadesXml
    {
        /// <summary>
        /// Valida un archivo XML en contra de los esquemas definidos para el espacio de nombres del mismo y devuelve el XML
        /// parseado.
        /// </summary>
        /// <param name="direcciónArchivoXml">Dirección del archivo XML</param>
        /// <param name="espacioDeNombres">Espacio de nombres principal que tiene definido el archivo XML</param>
        /// <param name="archivosEsquema">Direcciones de los archivos de esquema que definen las características del espacio de nombres indicado</param>
        /// <returns>Objeto de tipo <see cref="System.Xml.Linq.XElement"/> con el contenido del archivo XML ya parseado</returns>
        public static XElement ValidarXml(string direcciónArchivoXml, string espacioDeNombres, IEnumerable<string> archivosEsquema)
        {
            if (!File.Exists(direcciónArchivoXml))
            {
                throw new Exception(Recursos.Excepciones.ArchivoNoExisteNoAccesible[direcciónArchivoXml]);
            }
            if(archivosEsquema != null && archivosEsquema.Count() > 0)
            {
                return ValidarXmlLiteral(File.ReadAllText(direcciónArchivoXml), espacioDeNombres, archivosEsquema);
            }
            else
            {
                using (XmlReader Reader = XmlReader.Create(direcciónArchivoXml))
                {
                    return XElement.Load(Reader);
                }
            }
        }

        /// <summary>
        /// Valida un texto literal XML en contra de los esquemas definidos para el espacio de nombres del mismo y devuelve el XML
        /// parseado.
        /// </summary>
        /// <param name="descriptor">Cadena con el contenido literal XML</param>
        /// <param name="espacioDeNombres">Espacio de nombres principal que tiene definido el archivo XML</param>
        /// <param name="archivosEsquema">Direcciones de los archivos de esquema que definen las características del espacio de nombres indicado</param>
        /// <returns>Objeto de tipo <see cref="System.Xml.Linq.XElement"/> con el contenido XML ya parseado</returns>
        public static XElement ValidarXmlLiteral(string descriptor, string espacioDeNombres, IEnumerable<string> archivosEsquema)
        {
            if (string.IsNullOrEmpty(descriptor))
            {
                throw new IOException(Recursos.Excepciones.DescriptorXmlNuloOVacío);
            }
            else
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                try
                {
                    XmlDocument config = new XmlDocument();
                    config.LoadXml(descriptor);
                    foreach (string archivoEsquema in archivosEsquema)
                    {
                        config.Schemas.Add(espacioDeNombres, archivoEsquema);
                    }
                    config.Validate(new System.Xml.Schema.ValidationEventHandler(ErrorValidacionXml), config.DocumentElement);
                }
                catch (Exception ex)
                {
                    throw new Exception(Recursos.Excepciones.DescriptorXmlFormatoIncorrecto, ex);
                }

                foreach (string archivoEsquema in archivosEsquema)
                {
                    settings.Schemas.Add(espacioDeNombres, archivoEsquema);
                }
                settings.Schemas.Compile();
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags = System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ConformanceLevel = ConformanceLevel.Document;

                using (XmlReader Reader = XmlReader.Create(new StringReader(descriptor), settings))
                {
                    return XElement.Load(Reader);
                }
            }
        }

        /// <summary>
        /// Parsea una estructura XML y la materializa con el uso de reflexión en objetos de código administrado, especificando
        /// los espacios de nombre de código que contienen las posibles clases en las que se materializarán los objetos.
        /// Generalmente, la clase en la que se materializará un elemento del XML se inferirá del nombre local de dicho
        /// elemento.
        /// </summary>
        /// <param name="origen">Estructura XML que se materializará</param>
        /// <param name="tipoObjeto">Opcional. Define la clase que se usará para materializar el elemento raíz. Si no se especifica,
        /// se inferirá por su nombre</param>
        /// <param name="espaciosNombre">Colección de espacios de nombre de código administrado en los que se encuentren
        /// las posibles clases en las que se materializarán los objetos</param>
        /// <param name="parámetrosConstructor">Parameters que podría requerir la clase en que se materializará el objeto al ser instanciada.</param>
        /// <returns>Objeto instanciado con la información del XML <paramref name="origen"/> ya parseada</returns>
        public static object ParsearXml(XElement origen, Type tipoObjeto, IEnumerable<string> espaciosNombre, params object[] parámetrosConstructor)
        {
            return ParsearXml(origen, tipoObjeto, espaciosNombre, null, null, false, parámetrosConstructor);
        }

        /// <summary>
        /// Parsea una estructura XML y la materializa con el uso de reflexión en objetos de código administrado, especificando
        /// los espacios de nombre de código que contienen las posibles clases en las que se materializarán los objetos.
        /// Generalmente, la clase en la que se materializará un elemento del XML se inferirá del nombre local de dicho
        /// elemento.
        /// </summary>
        /// <param name="origen">Estructura XML que se materializará</param>
        /// <param name="tipoObjeto">Opcional. Define la clase que se usará para materializar el elemento raíz. Si no se especifica,
        /// se inferirá por su nombre</param>
        /// <param name="espaciosNombre">Colección de espacios de nombre de código administrado en los que se encuentren
        /// las posibles clases en las que se materializarán los objetos</param>
        /// <param name="proveedorNúmeros">Proveedor de formato usado para interpretar los números asignados a los atributos</param>
        /// <param name="proveedorFechas">Proveedor de formato usado para interpretar las fechas asignadas a los atributos</param>
        /// <param name="sóloPropiedadesSimples">Si se especifica <see langword="true"/>, sólo se parsearán los atributos del XML (generalmente,
        /// asociadas a tipos de datos primitivos), omitiendo los elementos descendientes</param>
        /// <param name="parámetrosConstructor">Parameters que podría requerir la clase en que se materializará el objeto al ser instanciada.</param>
        /// <returns>Objeto instanciado con la información del XML <paramref name="origen"/> ya parseada</returns>
        public static object ParsearXml(XElement origen, Type tipoObjeto, IEnumerable<string> espaciosNombre, IFormatProvider proveedorNúmeros, IFormatProvider proveedorFechas, bool sóloPropiedadesSimples, params object[] parámetrosConstructor)
        {
            if (tipoObjeto == null)
            {
                tipoObjeto = ParsearXmlObtenerTipoObjeto(origen, espaciosNombre);
                if (tipoObjeto == null)
                    throw new Exception(Recursos.Excepciones.ClaseNoEncontradaEnNS[origen.Name.LocalName]);
            }
            else if (tipoObjeto.IsAbstract)
            {
                tipoObjeto = Type.GetType(tipoObjeto.Namespace + "." + origen.Name.LocalName + ", " + tipoObjeto.Assembly.FullName, false, false);
                if (tipoObjeto == null)
                {
                    tipoObjeto = ParsearXmlObtenerTipoObjeto(origen, espaciosNombre);
                    if (tipoObjeto == null)
                        throw new Exception(Recursos.Excepciones.ClaseNoEncontradaEnNS[origen.Name.LocalName]);
                }
            }
            object instancia;
            try
            {
                instancia = Activator.CreateInstance(tipoObjeto, parámetrosConstructor);
            }
            catch
            {
                try
                {
                    instancia = Activator.CreateInstance(tipoObjeto);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + " — tipoObjeto: " + tipoObjeto.FullName, ex.InnerException);
                }
            }
            
            ParsearXmlPropiedadesSimples(origen, instancia, espaciosNombre, proveedorNúmeros, proveedorFechas);
            if (!sóloPropiedadesSimples) ParsearXmlPropiedadesComplejas(origen, instancia, espaciosNombre, proveedorNúmeros, proveedorFechas);
            return instancia;
        }

        private static void ErrorValidacionXml(object sender, System.Xml.Schema.ValidationEventArgs e)
        {
            throw new Exception(e.Message);
        }

        private static Type ParsearXmlObtenerTipoObjeto(XElement origen, IEnumerable<string> espaciosNombre)
        {
            Type tipoObjeto = null;
            foreach (string espacioNombre in espaciosNombre)
            {
                string[] segmentos = espacioNombre.Split(',');
                if (segmentos.Length == 1)
                {
                    tipoObjeto = Type.GetType(espacioNombre.Replace("<clase>", origen.Name.LocalName), false, false);
                }
                else if (segmentos.Length > 1)
                {
                    tipoObjeto = Type.GetType(segmentos[0].Trim() + "." + origen.Name.LocalName + ", " + segmentos[1], false, false);
                }
                if (tipoObjeto != null)
                    break;
            }
            return tipoObjeto;
        }

        private static void ParsearXmlPropiedadesSimples(XElement origen, object instancia, IEnumerable<string> espaciosNombre, IFormatProvider proveedorNúmeros, IFormatProvider proveedorFechas)
        {
            Type tipoObjeto = instancia.GetType();
            foreach (XAttribute atributo in origen.Attributes())
            {
                PropertyInfo propiedad = tipoObjeto.GetProperty(atributo.Name.LocalName);
                if (propiedad != null && propiedad.CanWrite)
                {
                    if (propiedad.PropertyType.Equals(typeof(DateTime)))
                    {
                        propiedad.SetValue(instancia, atributo.Value.CambiarATipo(propiedad.PropertyType, proveedorFechas), null);
                    }
                    else
                    {
                        propiedad.SetValue(instancia, atributo.Value.CambiarATipo(propiedad.PropertyType, proveedorNúmeros), null);
                    }
                }
                else
                {
                }
            }
        }

        private static void ParsearXmlPropiedadesComplejas(XElement origen, object instancia, IEnumerable<string> espaciosNombre, IFormatProvider proveedorNúmeros, IFormatProvider proveedorFechas)
        {
            Type tipoObjeto = instancia.GetType();

            foreach (XElement elemento in origen.Elements())
            {
                PropertyInfo propiedad = tipoObjeto.GetProperty(elemento.Name.LocalName);
                if (propiedad.PropertyType.GetInterface("IList") != null && propiedad.CanRead)
                {
                    System.Collections.IList lista = (System.Collections.IList)propiedad.GetValue(instancia, null);
                    foreach (var objeto in elemento.Elements())
                    {
                        lista.Add(ParsearXml(objeto, null, espaciosNombre));
                    }

                    ParsearXmlPropiedadesSimples(elemento, lista, espaciosNombre, proveedorNúmeros, proveedorFechas);
                }
                else if (propiedad.CanWrite)
                {
                    if (propiedad.PropertyType.Equals(typeof(DateTime)))
                    {
                        propiedad.SetValue(instancia, elemento.Value.CambiarATipo(propiedad.PropertyType, proveedorFechas), null);
                    }
                    else if (propiedad.PropertyType.IsValueType || propiedad.PropertyType == typeof(string))
                    {
                        propiedad.SetValue(instancia, elemento.Value.CambiarATipo(propiedad.PropertyType, proveedorNúmeros), null);
                    }
                    else
                    {
                        propiedad.SetValue(instancia, ParsearXml(elemento, propiedad.PropertyType, espaciosNombre), null);
                    }
                }
            }
        }
    }
}