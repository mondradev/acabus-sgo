#!/bin/bash

FILENAME_EXEC="exec.sh"

function include() {
    cat $1 >> $FILENAME_EXEC
    echo "" >> $FILENAME_EXEC
    echo "" >> $FILENAME_EXEC
}

# -----------------------------------------------------
# Creación del archivo ejecutable
# -----------------------------------------------------
rm -rf /home/teknei/opera_bus*
rm -rf exec.sh # Eliminamos si existe
echo "#!/bin/bash" >> $FILENAME_EXEC

# -----------------------------------------------------
# Archivos a incluir
# -----------------------------------------------------

include lib_opera_util.sh # Libería de utilería
include db_connection.properties # Propiedades de conexión a base de datos
include lib_opera_db.sh # Libería de base de datos
include lib_opera_blacklist.sh # Librería de listas negras
include lib_opera_vehi.sh # Libería de vehículos

# ----------------------------------------------------
# Configuración de permisos y ejecución
# ----------------------------------------------------
chmod +x exec.sh # Autorizamos la ejecución
./exec.sh # Ejecutamos
rm -rf exec.sh # Eliminamos para finalizar

