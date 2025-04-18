using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailOne.Datos
{
    public static class Depuracion
    {
        private static readonly object _bloqueo = new object();
        private static bool _estaEjecutando = false;

        public static bool Activa { get; set; }

        public static void Error(ConexionDatos conn, Exception ex)
        {
            if (!Activa) return;

            if (conn == null)
                return;

            Error(conn.TextoComandoCompleto, ex);
        }

        public static void Error(string mensaje)
        {
            Error(mensaje, null);
        }

        public static void Error(string mensaje, Exception ex)
        {
            if (!Activa) return;

            // Almacenar la referencia del evento en una variable local
            var alNotificarHandler = AlNotificar;

            if (_estaEjecutando || alNotificarHandler == null)
                return;

            lock (_bloqueo)
            {
                try
                {
                    _estaEjecutando = true;
                    alNotificarHandler?.Invoke(mensaje, ex);
                }
                catch
                { 
                    // De momento no se considera el manejo de la excepción.
                }
                finally
                {
                    _estaEjecutando = false;
                }
            }
        }

        private static Task _tareaActual = Task.FromResult(0);

        public static void Registrar(ConexionDatos conn)
        {
            if (!Activa) return;
            
            // Almacenar la referencia del evento en una variable local
            var alEjecutarHandler = AlEjecutar;

            if (alEjecutarHandler == null || conn == null)
                return;

            string comando = conn.TextoComandoCompleto;

            lock (_bloqueo)
            {
                // Si la tarea actual ya ha terminado, inicia una nueva tarea
                if (_tareaActual.IsCompleted)
                {
                    _tareaActual = Task.Run(() => Ejecutar(comando));
                }
                else
                {
                    // Si la tarea actual aún no ha terminado, añade una continuación
                    _tareaActual = _tareaActual.ContinueWith(t => Ejecutar(comando),
                        TaskContinuationOptions.OnlyOnRanToCompletion);
                }
            }
        }

        static async Task Ejecutar(string comando)
        {
            lock (_bloqueo)
            {
                try
                {
                    _estaEjecutando = true;
                    AlEjecutar?.Invoke(comando);
                }
                catch (Exception ex)
                {
                    // De momento no se considera el manejo de la excepción.
                }
                finally
                {
                    _estaEjecutando = false;
                }
            }
        }

        public delegate void ManejadorErrorEvento(string descripcion, Exception ex);
        public static event ManejadorErrorEvento AlNotificar;

        public delegate void EjecutarComandoEvento(string comando);
        public static event EjecutarComandoEvento AlEjecutar;
    }
}
