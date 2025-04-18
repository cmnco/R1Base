using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace RetailOne.Utilidades
{
    [Serializable]
    public static class Compresor
    {
        //public delegate void NotificacionDeProgreso(string sMessage);

        private const int MAX_LENGHT_ARRAY = 4096;
        /// <summary>
        /// Comprime la secuencia (<typeparamref name="datos"/>) mediante <see cref="System.IO.Compression.GZipStream"/> 
        /// </summary>
        /// <param name="datos"></param>
        /// <returns></returns>
        public static byte[] Comprimir(byte[] datos)
        {
            using (var msi = new MemoryStream(datos))
            {
                byte[] bytes = new byte[MAX_LENGHT_ARRAY];
                int cnt;
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(mso, CompressionMode.Compress))
                        while ((cnt = msi.Read(bytes, 0, bytes.Length)) != 0)
                            gs.Write(bytes, 0, cnt);
                    bytes = mso.ToArray();
                }
                return bytes;
            }
        }

        /// <summary>
        /// Descomprime la secuencia (<typeparamref name="datosComprimidos"/>) mediante <see cref="System.IO.Compression.GZipStream"/> 
        /// </summary>
        /// <param name="datosComprimidos"></param>
        /// <returns></returns>
        public static byte[] Descomprimir(byte[] datosComprimidos)
        {
            byte[] bytes = new byte[MAX_LENGHT_ARRAY];
            int cnt;
            using (var msi = new MemoryStream(datosComprimidos))
            using (var mso = new MemoryStream())
            using (var gs = new GZipStream(msi, CompressionMode.Decompress))
            {
                while ((cnt = gs.Read(bytes, 0, bytes.Length)) != 0)
                    mso.Write(bytes, 0, cnt);
                return mso.ToArray();
            }
        }

        #region Archivos
        /// <summary>
        /// Comprimir un archivo individual
        /// </summary>
        /// <param name="archivoEntrada">Path del archivo de entrada</param>
        /// <param name="archivoSalida">Path del archivo de salida</param>
        public static void ComprimirArchivo(string archivoEntrada, string archivoSalida)
        {
            using (FileStream archivoOriginal = new FileStream(archivoEntrada, FileMode.Open, FileAccess.Read))
            using (FileStream archivoComprimido = new FileStream(archivoSalida, FileMode.Create, FileAccess.Write))
            using (GZipStream compresor = new GZipStream(archivoComprimido, CompressionMode.Compress))
            {
                archivoOriginal.CopyTo(compresor);
            }
        }

        /// <summary>
        /// Descomprime un archivo individual.
        /// </summary>
        /// <param name="archivoComprimido">Nombre (path) del archivo de comprimido</param>
        /// <param name="archivoSalida">Nombre (path) del archivo descomprimido</param>
        public static void DescomprimirArchivo(string archivoComprimido, string archivoSalida)
        {
            using (FileStream archivoZip = new FileStream(archivoComprimido, FileMode.Open, FileAccess.Read))
            using (GZipStream descompresor = new GZipStream(archivoZip, CompressionMode.Decompress))
            using (FileStream archivoDescomprimido = new FileStream(archivoSalida, FileMode.Create, FileAccess.Write))
            {
                descompresor.CopyTo(archivoDescomprimido);
            }
        }

        /// <summary>
        /// Comprimir múltiples archivos en un archivo zip. 
        /// </summary>
        /// <param name="archivosEntrada">Arreglo de archivos (paths) a comprimir</param>
        /// <param name="archivoSalida">Nombre (path) del archivo de salida .Zip</param>
        public static void ComprimirArchivos(string[] archivosEntrada, string archivoSalida)
        {
            using (FileStream fsOutput = new FileStream(archivoSalida, FileMode.Create))
            using (GZipStream zipStream = new GZipStream(fsOutput, CompressionMode.Compress))
            using (BinaryWriter writer = new BinaryWriter(zipStream, Encoding.UTF8, true))
            {
                writer.Write(archivosEntrada.Length); // Escribir la cantidad de archivos

                foreach (string archivo in archivosEntrada)
                {
                    byte[] nombreBytes = Encoding.UTF8.GetBytes(Path.GetFileName(archivo));
                    writer.Write(nombreBytes.Length);
                    writer.Write(nombreBytes);

                    byte[] contenidoBytes = File.ReadAllBytes(archivo);
                    writer.Write(contenidoBytes.Length);
                    writer.Write(contenidoBytes);
                }
            }
        }

        /// <summary>
        /// Descomprimir múltiples archivos en una carpeta de salida.
        /// </summary>
        /// <param name="archivoComprimido">Nombre (path) del archivo de salida .Zip</param>
        /// <param name="carpetaDestino">Carpeta dónde se guardaran los archivos descomprimidos.</param>
        /// <exception cref="Exception"></exception>
        public static void DescomprimirArchivos(string archivoComprimido, string carpetaDestino)
        {
            if (string.IsNullOrEmpty(archivoComprimido) || string.IsNullOrEmpty(carpetaDestino)) return;
            if(!System.IO.Directory.Exists(carpetaDestino))
                System.IO.Directory.CreateDirectory(carpetaDestino);

            using (FileStream fsInput = new FileStream(archivoComprimido, FileMode.Open))
            using (GZipStream zipStream = new GZipStream(fsInput, CompressionMode.Decompress))
            using (BinaryReader reader = new BinaryReader(zipStream, Encoding.UTF8, true))
            {
                try
                {
                    int cantidadArchivos = reader.ReadInt32(); // Leer cantidad de archivos

                    for (int i = 0; i < cantidadArchivos; i++)
                    {
                        int nombreLength = reader.ReadInt32();
                        string nombreArchivo = Encoding.UTF8.GetString(reader.ReadBytes(nombreLength));

                        int contenidoLength = reader.ReadInt32();
                        byte[] contenido = reader.ReadBytes(contenidoLength);

                        string rutaArchivo = Path.Combine(carpetaDestino, nombreArchivo);
                        File.WriteAllBytes(rutaArchivo, contenido);
                    }
                }
                catch(System.IO.InvalidDataException ex)
                {
                    throw new Exception(Recursos.Excepciones.ArchivoComprimidoNoValido[archivoComprimido], ex);
                }
            }
        }

        private static byte[] DeriveKey(string password, int keySize = 32)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                Array.Resize(ref key, keySize);
                return key;
            }
        }
        /// <summary>
        /// Comprimir múltiples archivos en un archivo zip con contraseña.
        /// </summary>
        /// <param name="archivosEntrada">Arreglo de archivos (paths) a comprimir</param>
        /// <param name="archivoSalida">Nombre (path) del archivo de salida .Zip</param>
        /// <param name="password">Contraseña para la compresión de los archivos</param>
        public static void ComprimirArchivos(string[] archivosEntrada, string archivoSalida, string password)
        {
            if (archivosEntrada == null || archivosEntrada.Length <= 0 || string.IsNullOrEmpty(archivoSalida)) return;
            if(string.IsNullOrEmpty(password))
            {
                ComprimirArchivos(archivosEntrada, archivoSalida);
                return;
            }

            byte[] key = DeriveKey(password);

            using (FileStream fsOutput = new FileStream(archivoSalida, FileMode.Create))
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();
                fsOutput.Write(aes.IV, 0, aes.IV.Length);

                using (CryptoStream cryptoStream = new CryptoStream(fsOutput, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (GZipStream zipStream = new GZipStream(cryptoStream, CompressionMode.Compress))
                using (BinaryWriter writer = new BinaryWriter(zipStream, Encoding.UTF8, true))
                {
                    writer.Write(archivosEntrada.Length);

                    foreach (string archivo in archivosEntrada)
                    {
                        byte[] nombreBytes = Encoding.UTF8.GetBytes(Path.GetFileName(archivo));
                        writer.Write(nombreBytes.Length);
                        writer.Write(nombreBytes);

                        byte[] contenidoBytes = File.ReadAllBytes(archivo);
                        writer.Write(contenidoBytes.Length);
                        writer.Write(contenidoBytes);
                    }
                }
            }
        }
        /// <summary>
        /// Descomprimir múltiples archivos en una carpeta de salida.
        /// </summary>
        /// <param name="archivoComprimido">Nombre (path) del archivo de salida .Zip</param>
        /// <param name="carpetaDestino">Carpeta dónde se guardaran los archivos descomprimidos.</param>
        /// <param name="password">Contraseña para descomprimir los archivos</param>
        public static void DescomprimirArchivos(string archivoComprimido, string carpetaDestino, string password)
        {
            if (string.IsNullOrEmpty(archivoComprimido) || string.IsNullOrEmpty(carpetaDestino)) return;
            if (string.IsNullOrEmpty(password))
            {
                DescomprimirArchivos(archivoComprimido, carpetaDestino);
                return;
            }

            byte[] key = DeriveKey(password);

            using (FileStream fsInput = new FileStream(archivoComprimido, FileMode.Open))
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                byte[] iv = new byte[aes.IV.Length];
                fsInput.Read(iv, 0, iv.Length);
                aes.IV = iv;

                using (CryptoStream cryptoStream = new CryptoStream(fsInput, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (GZipStream zipStream = new GZipStream(cryptoStream, CompressionMode.Decompress))
                using (BinaryReader reader = new BinaryReader(zipStream, Encoding.UTF8, true))
                {
                    int cantidadArchivos = reader.ReadInt32();

                    for (int i = 0; i < cantidadArchivos; i++)
                    {
                        int nombreLength = reader.ReadInt32();
                        string nombreArchivo = Encoding.UTF8.GetString(reader.ReadBytes(nombreLength));

                        int contenidoLength = reader.ReadInt32();
                        byte[] contenido = reader.ReadBytes(contenidoLength);

                        string rutaArchivo = Path.Combine(carpetaDestino, nombreArchivo);
                        File.WriteAllBytes(rutaArchivo, contenido);
                    }
                }
            }
        }
        #endregion

        private static void ComprimirArchivo(string sDir, string sRelativePath, GZipStream zipStream)
        {
            try
            {
                // Convertir nombre del archivo a bytes UTF-8
                byte[] nameBytes = Encoding.UTF8.GetBytes(sRelativePath);
                zipStream.Write(BitConverter.GetBytes(nameBytes.Length), 0, sizeof(int));
                zipStream.Write(nameBytes, 0, nameBytes.Length);

                // Leer el contenido del archivo
                string fullPath = Path.Combine(sDir, sRelativePath);
                byte[] fileBytes = File.ReadAllBytes(fullPath);

                // Escribir tamaño del archivo y contenido
                zipStream.Write(BitConverter.GetBytes(fileBytes.Length), 0, sizeof(int));
                zipStream.Write(fileBytes, 0, fileBytes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al comprimir: {ex.Message}");
            }
        }

        private static bool DescomprimirArchivo(string sDir, GZipStream zipStream, Action<string> progress = null)
        {
            try
            {
                // Leer longitud del nombre del archivo
                byte[] buffer = new byte[sizeof(int)];
                if (zipStream.Read(buffer, 0, sizeof(int)) < sizeof(int)) return false;
                int nameLength = BitConverter.ToInt32(buffer, 0);

                // Leer el nombre del archivo
                buffer = new byte[nameLength];
                if (zipStream.Read(buffer, 0, nameLength) < nameLength) return false;
                string fileName = Encoding.UTF8.GetString(buffer);

                progress?.Invoke(fileName);

                // Leer longitud del contenido del archivo
                buffer = new byte[sizeof(int)];
                if (zipStream.Read(buffer, 0, sizeof(int)) < sizeof(int)) return false;
                int fileLength = BitConverter.ToInt32(buffer, 0);

                // Leer contenido del archivo en partes
                buffer = new byte[fileLength];
                int totalRead = 0;
                while (totalRead < fileLength)
                {
                    int bytesRead = zipStream.Read(buffer, totalRead, fileLength - totalRead);
                    if (bytesRead == 0) throw new EndOfStreamException("Archivo incompleto o corrupto.");
                    totalRead += bytesRead;
                }

                // Crear directorio si no existe
                string fullPath = Path.Combine(sDir, fileName);
                string dir = Path.GetDirectoryName(fullPath);
                if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                // Escribir archivo descomprimido
                using (FileStream outFile = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    outFile.Write(buffer, 0, buffer.Length);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al descomprimir: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Comprime un directorio completo. 
        /// </summary>
        /// <param name="directorioAComprimir">Nombre (path) del directorio a comprimir</param>
        /// <param name="nombreArchivoComprimido">Nombre (path) del archivo de salida .Zip</param>
        /// <param name="progreso">Action para notificación de progreso de compresión</param>
        public static void ComprimirDirectorio(string directorioAComprimir, string nombreArchivoComprimido, Action<string> progreso = null)
        {
            string[] sFiles = Directory.GetFiles(directorioAComprimir, "*.*", SearchOption.AllDirectories);
            int iDirLen = directorioAComprimir[directorioAComprimir.Length - 1] == Path.DirectorySeparatorChar ? directorioAComprimir.Length : directorioAComprimir.Length + 1;

            using (FileStream outFile = new FileStream(nombreArchivoComprimido, FileMode.Create, FileAccess.Write, FileShare.None))
            using (GZipStream str = new GZipStream(outFile, CompressionMode.Compress))
                foreach (string sFilePath in sFiles)
                {
                    string sRelativePath = sFilePath.Substring(iDirLen);
                    progreso?.Invoke(sRelativePath);
                    ComprimirArchivo(directorioAComprimir, sRelativePath, str);
                }
        }

        /// <summary>
        /// Descomprime un directorio.
        /// </summary>
        /// <param name="nombreArchivoComprimido">Nombre (path) del archivo de comprimido .Zip</param>
        /// <param name="carpetaDestino">Nombre (path) del directorio destino</param>
        /// <param name="progreso">Action para notificación de progreso de descompresión</param>
        public static void DescomprimirADirectorio(string nombreArchivoComprimido, string carpetaDestino, Action<string> progreso = null)
        {
            using (FileStream inFile = new FileStream(nombreArchivoComprimido, FileMode.Open, FileAccess.Read, FileShare.None))
            using (GZipStream zipStream = new GZipStream(inFile, CompressionMode.Decompress, true))
                while (DescomprimirArchivo(carpetaDestino, zipStream, progreso)) ;
        }
    }
}
