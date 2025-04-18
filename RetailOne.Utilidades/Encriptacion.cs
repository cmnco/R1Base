
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System;

namespace RetailOne.Utilidades
{
    /// <summary>
    /// Provee funciones para la encriptación y decriptación de cadenas de texto.
    /// </summary>
    public class Encriptacion
    {
        #region Atributos internos
        //internal static byte[] clavePredeterminada1 = { 83, 48, 108, 117, 43, 48, 110, 51, 46, 50, 48, 48, 57, 47, 82, 51, 116, 52, 73, 108, 43, 79, 110, 101 };
        //internal static byte[] clavePredeterminada2 = { 83, 52, 80, 95, 66, 85, 83, 49, 78, 51, 83, 83, 95, 48, 78, 51 };

        internal static byte[] clavePredeterminada1 = { 239, 191, 189, 239, 191, 189, 239, 191, 189, 42, 37, 239, 191, 189, 239, 191 };
        internal static byte[] clavePredeterminada2 = { 239, 191, 189, 239, 191, 189, 239, 191 };

        private byte[] clave1;
        private byte[] clave2;
        #endregion

        #region Constructores
        public Encriptacion()
        {
            //this.clave1 = clavePredeterminada1;
            //this.clave2 = clavePredeterminada2;

            //Inicializando llave TripleDES                                        
            this.clave1 = Convert.FromBase64String("+++++Solu/1+++++");
            Array.Resize(ref this.clave1, 16);

            //Inicializanfo vector de inicialización para encripción TripleDES
            this.clave2 = Convert.FromBase64String("+/+/+/+/");
            Array.Resize(ref this.clave2, 8);
        }

        /// <summary>
        /// Crea un objeto <see cref="Encriptacion"/> con el uso de la clave secreta y el vector de inicialización.
        /// </summary>
        /// <param name="clave1">Clave secreta con que se realizará la encriptación/decriptación. Debe poseer 16 ó 24 bytes (128 ó 192 bits)</param>
        /// <param name="clave2">Vector de inicialización. Debe poseer al menos 8 bytes (64 bits)</param>
        public Encriptacion(byte[] clave1, byte[] clave2)
        {
            this.clave1 = clave1;
            this.clave2 = clave2;
        }
        #endregion

        #region Métodos
        /// <summary>
        /// Encripta un texto, devolviendo la cadena de bytes correspondiente
        /// </summary>
        /// <param name="texto">Texto a encriptar</param>
        /// <returns>Arreglo de bytes con la información encriptada</returns>
        public byte[] Encriptar(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return null;
            UTF8Encoding utf8encoder = new UTF8Encoding();
            byte[] inputInBytes = utf8encoder.GetBytes(texto);
            return Encriptar(inputInBytes);
        }

        /// <summary>
        /// Encripta un arreglo de bytes, devolviendo la secuencia de bytes correspondiente. 
        /// Útil para encriptar los bytes provenientes de un archivo o de una compresión (<see cref="System.IO.Compression.GZipStream"/> y <see cref="Compresor"/>).
        /// </summary>
        /// <param name="datos">Arreglo de bytes a encriptar</param>
        /// <returns>Arreglo de bytes con la información encriptada</returns>
        public byte[] Encriptar(byte[] datos)
        {
            if (datos.Length == 0) return null;

            TripleDESCryptoServiceProvider tdesProvider = new TripleDESCryptoServiceProvider();

            ICryptoTransform cryptoTransform = tdesProvider.CreateEncryptor(this.clave1, this.clave2);

            using (MemoryStream encryptedStream = new MemoryStream())
            {
                using (CryptoStream cryptStream = new CryptoStream(encryptedStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptStream.Write(datos, 0, datos.Length);
                    cryptStream.FlushFinalBlock();
                    encryptedStream.Position = 0;

                    byte[] result = new byte[(encryptedStream.Length)];
                    int totalOfBytes = encryptedStream.Read(result, 0, Convert.ToInt32(encryptedStream.Length));
                    cryptStream.Close();
                    encryptedStream.Close();
                    return result;
                }
            }
        }

        /// <summary>
        /// Descifra una cadena de bytes, devolviendo el texto original
        /// </summary>
        /// <param name="datos">Arreglo de bytes con la información encriptada</param>
        /// <returns>Texto descifrado</returns>
        public string Desencriptar(byte[] datos)
        {
            if (datos == null) return null;
            UTF8Encoding myutf = new UTF8Encoding();
            return myutf.GetString(DesencriptarEnBytes(datos));
        }

        /// <summary>
        /// Descifra una cadena de bytes 
        /// </summary>
        /// <param name="datos">Arreglo de bytes con la información encriptada</param>
        /// <returns>Arreglo de bytes descifrado</returns>
        public byte[] DesencriptarEnBytes(byte[] datos)
        {
            if (datos == null) return null;
            UTF8Encoding utf8encoder = new UTF8Encoding();

            TripleDESCryptoServiceProvider tdesProvider = new TripleDESCryptoServiceProvider();

            ICryptoTransform cryptoTransform = tdesProvider.CreateDecryptor(this.clave1, this.clave2);

            using (MemoryStream decryptedStream = new MemoryStream())
            {
                using (CryptoStream cryptStream = new CryptoStream(decryptedStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptStream.Write(datos, 0, datos.Length);
                    cryptStream.FlushFinalBlock();
                    decryptedStream.Position = 0;

                    byte[] result = new byte[decryptedStream.Length];
                    int totalOfBytes = decryptedStream.Read(result, 0, Convert.ToInt32(decryptedStream.Length));
                    cryptStream.Close();
                    decryptedStream.Close();
                    return result;
                }
            }
        }

        /// <summary>
        /// Encripta el texto de entrada, devolviendo una cadena en formato base64
        /// </summary>
        /// <param name="texto">Texto a encriptar</param>
        /// <returns>Cadena de texto encriptada</returns>
        public static string ConvertirABase64(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return texto;
            Encriptacion encriptacion = new Encriptacion();
            byte[] result = encriptacion.Encriptar(texto);
            return Convert.ToBase64String(result, Base64FormattingOptions.None);
        }

        /// <summary>
        /// Descifra una cadena encriptada y en formato base64, devolviendo el texto original
        /// </summary>
        /// <param name="base64String">Cadena base 64 con la información encriptada</param>
        /// <returns>Texto descifrado</returns>
        public static string ObtenerDeBase64(string base64String)
        {
            if (string.IsNullOrEmpty(base64String)) return base64String;
            byte[] bytes = Convert.FromBase64String(base64String);
            Encriptacion encriptacion = new Encriptacion();
            return encriptacion.Desencriptar(bytes);
        }
        #endregion
    }
}