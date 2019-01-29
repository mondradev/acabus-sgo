using System;

namespace Opera.Acabus.Core.Models.Base
{
    /// <summary>
    /// Provee de funciones utilizadas para la persistencia.
    /// </summary>
    public static class AcabusEntityExtension
    {
        /// <summary>
        /// Marca a la entidad actual como eliminada.
        /// </summary>
        /// <param name="entity"></param>
        public static void Delete(this AcabusEntityBase entity)
        {
            entity.Active = false;
            entity.ModifyUser = "SISTEMA";
            entity.ModifyTime = DateTime.Now;
        }

        /// <summary>
        /// Confirma los cambios de la entidad.
        /// </summary>
        /// <param name="entity"></param>
        public static void Commit(this AcabusEntityBase entity)
        {
            entity.ModifyTime = DateTime.Now;
            entity.ModifyUser = "SISTEMA";
        }
    }
}