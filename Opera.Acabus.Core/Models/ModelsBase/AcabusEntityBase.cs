using InnSyTech.Standard.Mvvm;
using System;

namespace Opera.Acabus.Core.Models.ModelsBase
{

    /// <summary>
    /// Provee de una base para la definición de entidades.
    /// </summary>
    public abstract class AcabusEntityBase : NotifyPropertyChanged
    {
        /// <summary>
        /// Obtiene el estado actual de la instancia persistida.
        /// </summary>
        public Boolean Active { get; internal set; } = true;

        /// <summary>
        /// Obtiene la fecha/hora de creación de la instancia.
        /// </summary>
        public DateTime CreateTime { get; internal set; } = DateTime.Now;

        /// <summary>
        /// Obtiene el nombre de usuario que creó la instancia.
        /// </summary>
        public String CreateUser { get; internal set; } = "SISTEMA";

        /// <summary>
        /// Obtiene el identificador único de la entidad.
        /// </summary>
        public abstract UInt64 ID { get; protected set; }

        /// <summary>
        /// Obtiene la fecha/hora de la última modificación de la instancia.
        /// </summary>
        public DateTime ModifyTime { get; internal set; } = DateTime.Now;

        /// <summary>
        /// Obtiene el nombre del último usuario que modificó la instancia.
        /// </summary>
        public String ModifyUser { get; internal set; } = "SISTEMA";

        /// <summary>
        /// Asigna la información de la entidad.
        /// </summary>
        public static void AssignData(AcabusEntityBase entity, String createUser,
            DateTime createTime, String modifyUser, DateTime modifyTime, Boolean active)
        {
            entity.CreateUser = createUser;
            entity.CreateTime = createTime;
            entity.ModifyUser = modifyUser;
            entity.ModifyTime = modifyTime;
            entity.Active = active;
        }
    }
}