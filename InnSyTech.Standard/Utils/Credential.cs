using System;

namespace InnSyTech.Standard.Utils
{
    /// <summary>
    /// Permisos básicos de una credencial de acceso a sistemas informáticos.
    /// </summary>
    public enum Permissions
    {
        NONE,
        READ,
        WRITE,
        ROOT
    }

    /// <summary>
    /// Define una estructura que almacena las credenciales de acceso a un dispositivo.
    /// </summary>
    public sealed class Credential
    {
        /// <summary>
        /// Campo que provee a la propiedad 'Password'.
        /// </summary>
        private String _password;

        /// <summary>
        /// Campo que provee a la propiedad <see cref="Permission"/>.
        /// </summary>
        private Permissions _permission;

        /// <summary>
        /// Campo que provee a la propiedad 'Username'.
        /// </summary>
        private String _username;

        /// <summary>
        /// Crea una instancia nueva de una credencial de acceso.
        /// </summary>
        /// <param name="username">Nombre de usuario.</param>
        /// <param name="password">Clave de acceso.</param>
        /// <param name="permission">Permisos de acceso.</param>
        public Credential(String username, String password, Permissions permission)
        {
            this._username = username;
            this._password = password;
            _permission = permission;
        }

        /// <summary>
        /// Crea una instancia nueva de una credencial de acceso.
        /// </summary>
        public Credential()
        {
            _username = String.Empty;
            _password = String.Empty;
        }

        /// <summary>
        /// Obtiene la clave de acceso.
        /// </summary>
        public String Password => _password;

        /// <summary>
        /// Obtiene los privilegios con los que cuenta el dispositivo.
        /// </summary>
        public Permissions Permission => _permission;

        /// <summary>
        /// Obtiene el nombre de usuario.
        /// </summary>
        public String Username => _username;

        /// <summary>
        /// Determina si la credencial cuenta con el permiso especificado.
        /// </summary>
        /// <param name="permission">Permiso a verificar.</param>
        /// <returns>Un true si la credencial tiene el permiso.</returns>
        public bool HasPermission(Permissions permission)
            => permission >= _permission;
    }
}