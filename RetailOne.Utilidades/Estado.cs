
using System.ComponentModel;

namespace RetailOne.Utilidades
{
    /// <summary>
    /// Representa los estados de activación de una entidad (para determinar si un objeto se considera activo o inactivo).
    /// El valor numérico encapsulado es de tipo <see cref="System.Byte"/>.
    /// </summary>
    public enum Estado : byte
    {
        /// <summary>
        /// Active (habilitado)
        /// </summary>
        [Description("Activo")]
        Activo = 1,

        /// <summary>
        /// Inactive (deshabilitado)
        /// </summary>
        [Description("Inactivo")]
        Inactivo = 0
    }
}