using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace RetailOne.Datos
{
    public partial class ConexionDatos
    {
        public bool MultiConsulta { get; private set; }
        private List<Consulta> _listaConsultas = new List<Consulta>();
        private bool _textoComandoPreparado = false;

        /////// <summary>
        /////// Devuelve el texto del comando con el texto que corresponda al proveedor de la conexión, dado un objeto de tipo <see cref="RetailOne.Datos.Consulta"/>,
        /////// y sobre-escribe la propiedad <see cref="TextoComando"/> con el valor encontrado.
        /////// Si no se especifica el texto para dicho proveedor, se asumirá el comando definido como base para la consulta.
        /////// </summary>
        /////// <param name="consulta">Consulta que se cargará.</param>
        /////// <returns>Texto del comando (de acuerdo al proveedor).</returns>
        public string AgregarConsulta(Consulta consulta)
        {
            if (MultiConsulta == false)
            {
                TextoComando = null;
            }
            if (!string.IsNullOrEmpty(TextoComando))
            {
                _listaConsultas.Add(new Consulta(origen.Proveedor, new ComandoConsulta(origen.Proveedor, TextoComando)));
            }

            MultiConsulta = true;
            _textoComandoPreparado = false;

            if (consulta == null) return null;
            return CargarConsulta(consulta);
        }

        ///// <summary>
        ///// Devuelve el texto del comando con el texto que corresponda al proveedor de la conexión, dado un objeto de tipo <see cref="RetailOne.Datos.Consulta"/>,
        ///// y sobre-escribe la propiedad <see cref="TextoComando"/> con el valor encontrado.
        ///// Si no se especifica el texto para dicho proveedor, se asumirá el comando definido como base para la consulta.
        ///// </summary>
        ///// <param name="consulta">Consulta que se cargará.</param>
        ///// <returns>Texto del comando (de acuerdo al proveedor).</returns>
        //public string AgregarConsulta(Consulta consulta)
        //{
        //    if (consulta == null) return null;

        //    if ((_listaConsultas == null || _listaConsultas.Count <= 0) && !string.IsNullOrEmpty(TextoComando))
        //    {
        //        _listaConsultas.Add(new Consulta(origen.Proveedor, new ComandoConsulta(origen.Proveedor, TextoComando)));
        //    }

        //    string textoComando = CargarConsulta(consulta);
        //    _listaConsultas.Add(consulta);
        //    _textoComandoPreparado = false;
        //    MultiConsulta = true;
        //    return textoComando;
        //}

        public void PrepararTextoComando()
        {
            if (_textoComandoPreparado) return;

            //if (!MultiConsulta || _listaConsultas == null || _listaConsultas.Count <= 0)
            //{
            //    MultiConsulta = false;
            //    if (_listaConsultas != null)
            //        _listaConsultas.Clear();
            //    return;
            //}

            AgregarConsulta(null);

            StringBuilder consultas = new StringBuilder();
            if (origen.Proveedor == Proveedor.Hana)
            {
                consultas.AppendLine("DO BEGIN");
                if (comando.Parameters != null && comando.Parameters.Count > 0)
                {
                    foreach (DbParameter parametro in Parametros)
                    {
                        string nombreParametro = parametro.ParameterName.StartsWith(":") ? parametro.ParameterName.Substring(1) : parametro.ParameterName;
                        if (parametro.Value == null || parametro.Value == DBNull.Value)
                        {
                            consultas.AppendLine(string.Format("DECLARE {0} VARCHAR(1) := NULL;", nombreParametro));
                        }
                        else if (parametro.DbType.Equals(DbType.String))
                        {
                            consultas.AppendLine(string.Format("DECLARE {0} VARCHAR({1}) := '{2}';", nombreParametro, parametro.Value.ToString().Length, parametro.Value));
                        }
                        else if (parametro.DbType.Equals(DbType.DateTime))
                        {
                            consultas.AppendLine(string.Format("DECLARE {0} TIMESTAMP :=  TO_TIMESTAMP ('{1}', 'DD-MM-YYYY HH24:MI:SS');", nombreParametro, ((DateTime)parametro.Value).ToString("dd-MM-yyyy HH:mm:ss")));
                        }
                        else if (tiposHana.ContainsKey(parametro.DbType))
                        {
                            consultas.AppendLine(string.Format("DECLARE {0} {1} := {2};", nombreParametro, tiposHana[parametro.DbType], parametro.Value));
                        }
                    }
                    LimpiarParametros();
                }
                foreach (var consulta in _listaConsultas)
                {
                    string textoComando = consulta.ObtenerTextoComando(origen.Proveedor);
                    textoComando = textoComando.Replace(":p", "p");
                    consultas.AppendLine(textoComando.EndsWith(";") ? textoComando : textoComando + ";");
                }
                //consultas.AppendLine("END;");
                consultas.Append("END;");
            }
            else
            {
                foreach (var consulta in _listaConsultas)
                {
                    string textoComando = consulta.ObtenerTextoComando(origen.Proveedor);
                    consultas.AppendLine(textoComando.EndsWith(";") ? textoComando : textoComando + ";");
                }
            }
            TextoComando = consultas.ToString();
            MultiConsulta = false;
            _textoComandoPreparado = true;
            _listaConsultas.Clear();
        }
    }
}
