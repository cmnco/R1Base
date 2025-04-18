using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace RetailOne.Utilidades
{
    [Serializable]
    public class Json
    {
        #region Propiedades
        public static JsonSerializerSettings settings = Inicializar();
        #endregion

        #region Métodos
        protected static JsonSerializerSettings Inicializar()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            settings.TypeNameHandling = TypeNameHandling.All;
            return settings;
        }

        /// <summary>
        /// Serializa el bojeto ignorando valores nulos y propiedades establecidas como JsonIgnore.
        /// </summary>
        /// <param name="objeto">Objeto a serializar.</param>
        /// <returns></returns>
        public static string Serializar(object objeto)
        {
            return Serializar(objeto, false, GestionValoresNulos.Ignorar, Formato.Normal);
        }

        /// <summary>
        /// Serializa el bojeto ignorando valores nulos.
        /// </summary>
        /// <param name="objeto">Objeto a serializar.</param>
        /// <param name="incluirTodasLasPropiedades">Indica si se deben serializar todas las propiedades incluyendo las marcadas como JsonIgnore.</param>
        /// <returns></returns>
        public static string Serializar(object objeto, bool incluirTodasLasPropiedades)
        {
            return Serializar(objeto, incluirTodasLasPropiedades, GestionValoresNulos.Ignorar, Formato.Normal);
        }

        /// <summary>
        /// Serializa el bojeto según los parámetros establecidos.
        /// </summary>
        /// <param name="objeto">Objeto a serializar.</param>
        /// <param name="incluirTodasLasPropiedades">Indica si se deben serializar las propiedades/atributos con valores nulos.</param>
        /// <param name="valoresNulos">Indica si se deben incluir o ignorar las propiedades con valores nulos.</param>
        /// <returns></returns>
        public static string Serializar(object objeto, bool incluirTodasLasPropiedades, GestionValoresNulos valoresNulos)
        {
            return Serializar(objeto, incluirTodasLasPropiedades, valoresNulos, Formato.Normal);
        }

        /// <summary>
        /// Serializa el bojeto según los parámetros establecidos.
        /// </summary>
        /// <param name="objeto">Objeto a serializar.</param>
        /// <param name="incluirTodasLasPropiedades">Indica si se deben serializar las propiedades/atributos con valores nulos.</param>
        /// <param name="valoresNulos">Indica si se deben incluir o ignorar las propiedades con valores nulos.</param>
        /// <param name="formato">Indica si el json se debe formatear con indentado.</param>
        /// <returns></returns>
        public static string Serializar(object objeto, bool incluirTodasLasPropiedades, GestionValoresNulos valoresNulos, Formato formato)
        {
            if(valoresNulos == GestionValoresNulos.Incluir)
                settings.NullValueHandling = NullValueHandling.Include;
            else
                settings.NullValueHandling = NullValueHandling.Ignore;

            if (incluirTodasLasPropiedades)
            {
                settings.ContractResolver = new ShouldSerializeContractResolver(true);
            }
            else
            {
                settings.ContractResolver = null;
            }
            settings.TypeNameHandling = TypeNameHandling.None;
            return JsonConvert.SerializeObject(objeto, (formato == Formato.Indentado ? Formatting.Indented : Formatting.None), settings);
        }

        /// <summary>
        /// Deserializa el objeto json al tipo indicado.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T Deserializar<T>(string json)
        {
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.ContractResolver = new ShouldSerializeContractResolver(true);
            settings.TypeNameHandling = TypeNameHandling.All;
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public static T ConvertirAObjeto<T>(JObject objeto)
        {
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.ContractResolver = new ShouldSerializeContractResolver(true);
            JsonSerializer js = JsonSerializer.Create(settings);
            return objeto.ToObject<T>(js);
        }

        /// <summary>
        /// Serializa, comprime y encripta el objeto.
        /// </summary>
        /// <param name="objeto">Objeto a serializar.</param>
        /// <returns></returns>
        public static byte[] ObjetoABytes(object objeto)
        {
            try
            {
                string json = Serializar(objeto, false, GestionValoresNulos.Ignorar, Formato.Normal);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                bytes = Compresor.Comprimir(bytes);
                return (new Encriptacion()).Encriptar(bytes);
            }
            catch (Exception ex) { throw new Exception(Recursos.Excepciones.ErrorAlSerializarObjeto, ex); }
        }

        /// <summary>
        /// decifra, descomprime y deserializa el arreglo de bytes.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto a deserializar.</typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static T BytesAObjeto<T>(byte[] bytes)
        {
            try
            {
                bytes = (new Encriptacion()).DesencriptarEnBytes(bytes);
                bytes = Compresor.Descomprimir(bytes);
                string json = Encoding.UTF8.GetString(bytes);
                return (T)Deserializar<T>(json);
            }
            catch (Exception ex) { throw new Exception(Recursos.Excepciones.ErrorAlRecuperarObjetoDeSecuencia, ex); }
        }

        #endregion

        #region Sub-tipos
        public class ShouldSerializeContractResolver : DefaultContractResolver
        {
            public bool ForceIncludeAllProperties { get; private set; }

            public ShouldSerializeContractResolver(bool forceIncludeAllProperties)
            {
                this.ForceIncludeAllProperties = forceIncludeAllProperties;
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);

                if (property.Ignored)
                {
                    //if (property.PropertyName != "Instancia")
                    //{
                    //    property.Ignored = !ForceIncludeAllProperties;
                    //}
                    property.Ignored = !ForceIncludeAllProperties;
                }
                return property;
            }
        }

        public enum GestionValoresNulos : short
        {
            Ignorar,
            Incluir
        }

        public enum Formato : short
        {
            Normal,
            Indentado
        }
        #endregion
    }
}
