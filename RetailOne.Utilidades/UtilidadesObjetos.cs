using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;

namespace RetailOne.Utilidades
{
    /// <summary>
    /// Provee métodos de extensión generales para cualquier tipo de objetos.
    /// </summary>
    public static class UtilidadesObjetos
    {
        /// <summary>
        /// Evalúa un objeto de referencia y retorna una expresión basada en él si el objeto no es nulo.
        /// Es muy útil para simplificar el acceso a propiedades o métodos de objetos que pueden ser nulos.
        /// <para>En lugar de escribir:</para>
        /// <code>string NombreParticipante = Participante != null ? Participante.Nombre : null;</code>
        /// <para>Se podrá representar así:</para>
        /// <code>string NombreParticipante = Participante.SiNoEsNulo(x => x.Nombre);</code>
        /// </summary>
        /// <typeparam name="TObjeto">Type del objeto; debe ser un tipo de referencia</typeparam>
        /// <typeparam name="TRetorno">Type que retorna la expresión o dato extraído del objeto</typeparam>
        /// <param name="objeto">Objeto que se evaluará</param>
        /// <param name="seleccion">Función que extraerá un dato del objeto</param>
        /// <returns>Si el objeto es nulo, retorna el valor por defecto del tipo de retorno (<typeparamref name="TRetorno"/>); de lo contrario,
        /// se evalúa la función <paramref name="seleccion"/> sobre el objeto y se retorna el resultado de dicha evaluación</returns>
        public static TRetorno SiNoEsNulo<TObjeto, TRetorno>(this TObjeto objeto, Func<TObjeto, TRetorno> seleccion)
            where TObjeto : class
        {
            return objeto == null ? default(TRetorno) : seleccion(objeto);
        }


        /// <summary>
        /// Evalúa un objeto de referencia y ejecuta una acción basada en él si el objeto no es nulo.
        /// Es muy útil para simplificar el acceso a métodos de objetos que pueden ser nulos.
        /// <para>En lugar de escribir:</para>
        /// <code>if (ElementoExterno != null) ElementoExterno.Actualizar(referencia); //pudiendo ser nulo ElementoExterno</code>
        /// <para>Se podrá representar así:</para>
        /// <code>ElementoExterno.SiNoEsNulo(x => x.Actualizar(referencia));</code>
        /// </summary>
        /// <typeparam name="TObjeto">Type del objeto; debe ser un tipo de referencia</typeparam>
        /// <param name="objeto">Objeto que se evaluará</param>
        /// <param name="seleccion">Acción que se ejecutará</param>
        public static void SiNoEsNulo<TObjeto>(this TObjeto objeto, Action<TObjeto> seleccion)
            where TObjeto : class
        {
            if (objeto != null) seleccion(objeto);
        }

        private static JavaScriptSerializer serializadorJson = new JavaScriptSerializer();

        /// <summary>
        /// Serializa un objeto obteniendo su representación simple en la sintaxis JSON (JavaScript Object Notation).
        /// Es útil para la serialización de objetos que serán enviados a código cliente a través de respuestas a solicitudes AJAX.
        /// </summary>
        /// <param name="objeto">Objeto que se desea serializar</param>
        /// <returns>Cadena con la representación JSON del objeto</returns>
        public static string SerializarObjetoJson(object objeto)
        {
            if (objeto == null) return null;
            //return Newtonsoft.Json.JsonConvert.SerializeObject(objeto);
            return serializadorJson.Serialize(objeto);
        }

        /// <summary>
        /// Deserializa un objeto (sin tipo de datos explícito) a partir de su representación JSON (JavaScript Object Notation), 
        /// obteniéndose un "árbol de diccionarios" de tipo <c>Dictionary&lt;string, object&gt;</c>.
        /// </summary>
        /// <param name="objetoSerializado">Cadena con la representación JSON del objeto a deserializar</param>
        /// <returns>TreeView de diccionarios de tipo <c>Dictionary&lt;string, object&gt;</c> con la deserialización no-tipificada
        /// de la cadena de entrada</returns>
        public static object DeserializarObjetoJson(string objetoSerializado)
        {
            if (objetoSerializado == null) return null;
            return serializadorJson.DeserializeObject(objetoSerializado);
        }

        /// <summary>
        /// Deserializa un objeto a partir de su representación JSON (JavaScript Object Notation). 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objetoSerializado"></param>
        /// <returns></returns>
        public static T DeserializarObjetoJson<T>(string objetoSerializado)
        {
            if (objetoSerializado == null) return default(T);
            //return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(objetoSerializado);
            return serializadorJson.Deserialize<T>(objetoSerializado);
        }

        /// <summary>
        /// Serializa un objeto en formato XML, manteniendo información de su tipo de datos y permitiendo su posterior reconstrucción
        /// bajo la misma clase.
        /// </summary>
        /// <param name="objeto">Objeto que se desea serializar</param>
        /// <returns>Cadena con la representación serializada del objeto</returns>
        public static string SerializarObjeto(object objeto)
        {
            if (objeto == null) return null;
            return SerializarObjeto(objeto, objeto.GetType());
        }

        /// <summary>
        /// Serializa un objeto en formato XML, manteniendo información de su tipo de datos y permitiendo su posterior reconstrucción
        /// bajo la misma clase.
        /// </summary>
        /// <param name="objeto">Objeto que se desea serializar</param>
        /// <param name="tipo">Type de datos del objeto</param>
        /// <returns>Cadena con la representación serializada del objeto</returns>
        public static string SerializarObjeto(object objeto, Type tipo)
        {
            if (objeto == null) return null;

            System.Runtime.Serialization.DataContractSerializer serial = new System.Runtime.Serialization.DataContractSerializer(tipo, null, int.MaxValue, false, true, null);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(ms))
                {
                    serial.WriteObject(ms, objeto);
                    ms.Position = 0;
                    return sr.ReadToEnd();
                }
            }
        }

        public static void SerializarObjeto(object objeto, Type tipo, string nombreArchivo)
        {
            if (objeto == null || nombreArchivo.EsNuloOVacio()) return;
            System.Runtime.Serialization.DataContractSerializer serial = new System.Runtime.Serialization.DataContractSerializer(tipo, null, int.MaxValue, false, true, null);
            using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(nombreArchivo, new System.Xml.XmlWriterSettings { Indent = true }))
            {
                serial.WriteObject(writer, objeto);
                //writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }
        }

        /// <summary>
        /// Deserializa una expresión de cadena que posee la serialización XML del objeto, especificando el tipo de datos en el que
        /// se reconstruirá.
        /// </summary>
        /// <param name="objetoSerializado">Cadena con la expresión XML de la serialización previa</param>
        /// <param name="tipo">Type de datos en que se materializará el objeto deserializado</param>
        /// <returns>Objeto deserializado</returns>
        public static object DeserializarObjeto(string objetoSerializado, Type tipo)
        {
            if (objetoSerializado == null) return null;
            System.Runtime.Serialization.DataContractSerializer serial = new System.Runtime.Serialization.DataContractSerializer(tipo); //, null, int.MaxValue, false, true, null); 
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(ms))
                {
                    sw.Write(objetoSerializado);
                    sw.Flush();
                    ms.Position = 0;
                    return serial.ReadObject(ms);
                }
            }
        }

        public static object DeserializarObjetoDeArchivo(string nombreArchivo, Type tipo)
        {
            if (nombreArchivo.EsNuloOVacio()) return null;
            System.Runtime.Serialization.DataContractSerializer serial = new System.Runtime.Serialization.DataContractSerializer(tipo); //, null, int.MaxValue, false, true, null); 

            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(nombreArchivo))
            {
                return serial.ReadObject(reader);
            }
        }

        /// <summary>
        /// Enumera todos los descendientes de un objeto cuya clase posee una estructura jerárquica.
        /// </summary>
        /// <typeparam name="TipoObjeto">CssClass que define a todos los objetos de la jerarquía</typeparam>
        /// <param name="objeto">Objeto raíz del cual se quieren enumerar sus descendientes</param>
        /// <param name="funcionHijos">Función que, dado un objeto de la clase <typeparamref name="TipoObjeto"/>, enumere sus hijos directos.</param>
        /// <returns>Enumerable con todos los descendientes del objeto en su estructura jerárquica</returns>
        public static IEnumerable<TipoObjeto> Descendientes<TipoObjeto>(this TipoObjeto objeto, Func<TipoObjeto, IEnumerable<TipoObjeto>> funcionHijos)
        {
            List<TipoObjeto> lista = new List<TipoObjeto>();
            foreach (TipoObjeto item in funcionHijos(objeto))
            {
                lista.Add(item);
                lista.AddRange(item.Descendientes(funcionHijos));
            }
            return lista;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="knownTypes"></param>
        /// <returns></returns>
        public static T BytesAObjeto<T>(byte[] bytes, IEnumerable<Type> knownTypes)
        {
            try
            {
                T objReturn;
                var serial = new DataContractSerializer(typeof(T), knownTypes);
                bytes = (new Encriptacion()).DesencriptarEnBytes(bytes);
                bytes = Compresor.Descomprimir(bytes);
                using (MemoryStream des = new MemoryStream(bytes))
                {
                    var o = serial.ReadObject(des);
                    des.Close();
                    objReturn = (T)o;
                }
                return objReturn;
            }
            catch (Exception ex) { throw new Exception(Recursos.Excepciones.ErrorAlRecuperarObjetoDeSecuencia, ex); } 
        }

        /// <summary>
        /// Serializa, comprime y encripta el objeto (<typeparamref name="objeto"/>).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objeto">Objeto que se desea serializar</param>
        /// <param name="tiposConocidos">Tipos que pueden estar presentes en el objeto principal</param>
        /// <returns></returns>
        public static byte[] ObjetoABytes<T>(T objeto, IEnumerable<Type> tiposConocidos)
        {
            try
            {
                byte[] bytes;
                using (var outStream = new MemoryStream())
                {
                    var serial = new DataContractSerializer(typeof(T), tiposConocidos);
                    serial.WriteObject(outStream, objeto);
                    outStream.Position = 0;
                    bytes = outStream.ToArray();
                    bytes = Compresor.Comprimir(bytes);
                }
                return (new Encriptacion()).Encriptar(bytes);
            }
            catch (Exception ex) { throw new Exception(Recursos.Excepciones.ErrorAlSerializarObjeto[typeof(T)], ex); }
        }

        /// <summary>
        /// Clona objetos con atributo "Serializable".
        /// </summary>
        /// <param name="oObjetoOrigen">Objeto origen</param>
        /// <returns>Objeto clonado</returns>
        public static object Clonar(object oObjetoOrigen)
        {
            object oObjetoDestino;

            using (MemoryStream oSecuenciaMemoria = new MemoryStream())
            {
                IFormatter oFormatoBinario = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                oFormatoBinario.Serialize(oSecuenciaMemoria, oObjetoOrigen);

                oSecuenciaMemoria.Seek(0, SeekOrigin.Begin);
                oObjetoDestino = oFormatoBinario.Deserialize(oSecuenciaMemoria);

                return oObjetoDestino;
            }
        }

        /// <summary>
        /// Indica al compilador que ignore las referencias no utilizadas. Esto elimina el "warning" por las declaraciones no utilizadas.
        /// </summary>
        /// <param name="o">Referencia no utilizada.</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void SinReferencia(params object[] o)
        {
            return;
        }
    }
}