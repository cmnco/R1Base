using System;

namespace RetailOne.Utilidades
{
    [Serializable]
    public class ExcepcionControlada : Exception
    {
        public string Nombre { get; set; }
        public int Codigo { get; set; }
        public string Mensaje { get { return base.Message; } }
        public string Referencia { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param nombre="nombre"></param>
        /// <param nombre="mensaje"></param>
        /// <param nombre="codigo"></param>
        public ExcepcionControlada(int codigo, string mensaje, string referencia)
            : base(mensaje)
        {
            Referencia = referencia;
            Codigo = codigo;
            Nombre = "[EXCEPTION]";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param nombre="nombre"></param>
        /// <param nombre="mensaje"></param>
        /// <param nombre="codigo"></param>
        public ExcepcionControlada(int codigo, string nombre, string mensaje, string referencia)
            : base(mensaje)
        {
            Nombre = nombre;
            Codigo = codigo;
            Referencia = referencia;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param nombre="mensaje"></param>
        public ExcepcionControlada(string mensaje)
            : this(0, mensaje, null)
        {
            
        }

        public ExcepcionControlada(string mensaje, Exception innerException)
            : this(0, "[EXCEPTION]", mensaje, innerException)
        {

        }

        public ExcepcionControlada(int codigo, string mensaje)
            : this(codigo, "[EXCEPTION]", mensaje, (Exception)null)
        {

        }

        public ExcepcionControlada(int codigo, Exception innerException)
            : this(codigo, "[EXCEPTION]", innerException.SiNoEsNulo(x=>x.Message), innerException)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param nombre="nombre"></param>
        /// <param nombre="mensaje"></param>
        public ExcepcionControlada(string nombre, string mensaje)
            : this(0, mensaje, null)
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param nombre="nombre"></param>
        /// <param nombre="mensaje"></param>
        /// <param nombre="codigo"></param>
        /// <param nombre="innerException"></param>
        public ExcepcionControlada(int codigo, string nombre, string mensaje, Exception innerException) 
            : base(mensaje, innerException){
                Nombre = nombre;
                Codigo = codigo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param nombre="nombre"></param>
        /// <param nombre="mensaje"></param>
        /// <param nombre="codigo"></param>
        /// <param nombre="innerException"></param>
        public ExcepcionControlada(int codigo, string nombre, string mensaje, Exception innerException, string referencia)
            : base(mensaje, innerException)
        {
            Nombre = nombre;
            Codigo = codigo;
            Referencia = referencia;
        }
    }
}
