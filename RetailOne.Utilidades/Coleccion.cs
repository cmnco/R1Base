using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace RetailOne.Utilidades
{
    /// <summary>
    /// Define una colección de objetos, identificados con el uso de una clave contenida en dichos objetos.
    /// </summary>
    /// <typeparam name="TClave">Type de la clave de los objetos</typeparam>
    /// <typeparam name="TValor">Type de los objetos almacenados. Sólo puede ser de tipo referencia y la clave debe poder extraerse del mismo con el uso de una propiedad o método</typeparam>
    [Serializable]
    public class Coleccion<TClave, TValor> : KeyedCollection<TClave, TValor>
    {
        private bool usaDiccionarioBusqueda;
        private Func<TValor, TClave> selectorID;
        /// <summary>
        /// Crea una colección de objetos con claves, asumiendo que los objetos implementarán la interfaz <see cref="IEntidad&lt;TClave&gt;"/>
        /// para la obtención de la clave (propiedad ID del interfaz). También se asume que las claves de los objetos no
        /// cambiarán luego de que estos hayan sido almacenados en la colección.
        /// </summary>
        public Coleccion()
        {
        }

        /// <summary>
        /// Crea una colección de objetos con claves, especificando la forma en que se obtendrán las claves de los objetos.
        /// </summary>
        /// <param name="selectorID">Función que, dado un objeto de tipo <typeparamref name="TValor"/>, extraerá la clave
        /// del tipo <typeparamref name="TClave"/></param>
        public Coleccion(Func<TValor, TClave> selectorID)
            : this()
        {
            this.selectorID = selectorID;
        }

        /// <summary>
        /// Función que determina cómo se extraerán las claves a partir de los objetos almacenados.
        /// </summary>
        /// <param name="item">Objeto almacenado</param>
        /// <returns>Clave extraída del objeto</returns>
        protected override TClave GetKeyForItem(TValor item)
        {
            if (selectorID != null)
                return selectorID(item);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Obtiene un diccionario con todos los objetos contenidos en la colección. Si al crearse la colección se indicó que sus claves no serían mutables,
        /// se retornará directamente el diccionario interno manejado por la colección; de lo contrario, se construirá en el momento de la invocación del método un nuevo diccionario con todos los elementos y sus claves.
        /// </summary>
        public IDictionary<TClave, TValor> Diccionario
        {
            get
            {
                if (usaDiccionarioBusqueda)
                    return Dictionary;
                if (selectorID != null)
                    return Items.ToDictionary(selectorID);
                
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Agrega varios elementos a la colección.
        /// </summary>
        /// <param name="coleccion">Objeto enumerable con elementos de tipo <typeparamref name="TValor"/>. El valor puede ser <c>null</c> (no lanzará excepción)</param>
        public void AgregarVarios(IEnumerable<TValor> coleccion)
        {
            if (coleccion == null) return;
            foreach (TValor item in coleccion)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Obtiene todas las claves concatenadas de los objetos almacenados, separadas con coma (',') y sin espacios.
        /// Es útil al querer almacenar referencias a varios objetos en una única cadena de texto (por ejemplo, en un sólo campo de una tabla en BD).
        /// </summary>
        public string ClavesConcatenadas
        {
            get
            {
                return this.EnumerarDadoMaximosCaracteres(x => GetKeyForItem(x).ToString(), int.MaxValue, false, ",");
            }
        }

        /// <summary>
        /// Concatena las representaciones en cadena de los objetos almacenados, separadas con coma (", ", incluyendo un espacio).
        /// </summary>
        /// <returns>Cadena con la representación predeterminada en cadena de los objetos almacenados (usa el llamado al método <c>ToString()</c> de todos los elementos)</returns>
        public override string ToString()
        {
            return this.EnumerarDadoMaximosCaracteres(x => x.ToString(), int.MaxValue, false, ", ");
        }

        /// <summary>
        /// Elimina todos los elementos que cumplen con la condición del parámetro <paramref name="predicado"/>.
        /// </summary>
        /// <param name="predicado">Función que determinará cuáles elementos serán eliminados</param>
        public void EliminarTodos(Func<TValor, bool> predicado)
        {
            this.Where(predicado).ToList().ForEach(
                x => this.Remove(x));
        }
    }

    /// <summary>
    /// Define una colección de objetos sin clave (tanto para tipos de referencia como tipos primitivos.
    /// </summary>
    /// <typeparam name="Type">Type de los objetos almacenados. Puede ser de tipo referencia o de valor (tipos primitivos, como <c>System.Int32</c> o <c>System.DateTime</c>)</typeparam>
    [Serializable]
    public class Coleccion<Type> : Collection<Type>
    {
        /// <summary>
        /// Crea una colección vacía de objetos.
        /// </summary>
        public Coleccion() { }

        /// <summary>
        /// Crea una colección de objetos, especificando los elementos que contendrá inicialmente.
        /// </summary>
        /// <param name="coleccion">Objeto enumerable con los elementos iniciales de la colección</param>
        public Coleccion(IEnumerable<Type> coleccion) { AgregarVarios(coleccion); }


        /// <summary>
        /// Agrega varios elementos a la colección.
        /// </summary>
        /// <param name="coleccion">Objeto enumerable con elementos de tipo <typeparamref name="Type"/>. El valor puede ser <c>null</c> (no lanzará excepción)</param>
        public void AgregarVarios(IEnumerable<Type> coleccion)
        {
            if (coleccion == null) return;
            foreach (Type item in coleccion)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Concatena las representaciones en cadena de los objetos almacenados, separadas con coma (", ", incluyendo un espacio).
        /// </summary>
        /// <returns>Cadena con la representación predeterminada en cadena de los objetos almacenados (usa el llamado al método <c>ToString()</c> de todos los elementos)</returns>
        public override string ToString()
        {
            return this.EnumerarDadoMaximosCaracteres(x => x.ToString(), int.MaxValue, false, ", ");
        }

        /// <summary>
        /// Elimina todos los elementos que cumplen con la condición del parámetro <paramref name="predicado"/>.
        /// </summary>
        /// <param name="predicado">Función que determinará cuáles elementos serán eliminados</param>
        public void EliminarTodos(Func<Type, bool> predicado)
        {
            this.Where(predicado).ToList().ForEach(
                x => this.Remove(x));
        }
    }
}