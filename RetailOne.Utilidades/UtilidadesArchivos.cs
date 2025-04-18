using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace RetailOne.Utilidades
{
    public static class Directorio
    {
        /// <summary>
        /// Valida si existe el directorio y si cuenta con permisos de escritura.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool Existe(string path, bool validarPermisosEscritura = false)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    return false;
                }
                else if(validarPermisosEscritura)
                {
                    using (FileStream fs = File.Create(Path.Combine(path, "AccessTemp.txt"), 1, FileOptions.DeleteOnClose))
                    {
                        fs.Close();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Crea el directorio especificado en caso de no existir y establece los permisos de escritura en el mismo.
        /// </summary>
        /// <param name="path"></param>
        public static void Crear(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                DirectoryInfo dInfo = System.IO.Directory.CreateDirectory(path);

                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                dSecurity.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    FileSystemRights.FullControl,
                    InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                    PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                
                dInfo.SetAccessControl(dSecurity); 
            }
        }
    }

    public static class Archivo
    {
        private static object __lockObj = new object();

        /// <summary>
        /// Verifica si existe un archivo.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool Existe(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Crea el archivo especificado y escribe el contenido correspondiente.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="contenido"></param>
        /// <returns></returns>
        public static void Escribir(string path, string contenido)
        {
            lock (__lockObj)
            {
                System.IO.File.WriteAllText(path, contenido);
            }

            //UTF8Encoding noBOM = new UTF8Encoding(false, true);
            //using (StreamWriter sw = new StreamWriter(path, false, noBOM))
            //{
            //    sw.Write(contenido);
            //}

            //return true;
        }
    }
}
