# ----------------------------------------------------
# ----------------------------------------------------
# ||            Libería de Base de datos            ||
# ||                                                ||
# ||    2017/01/25                          v1.0    ||
# ||    Javier de Jesús Flores Mondragón            ||
# ||    Operadora de transporte integral            ||
# ----------------------------------------------------
# ----------------------------------------------------

# Validando dependencias
if [ "$UTIL_LIB_LOADED" != "1" ]; then
    echo "Libería de utilidades no cargada"
    exit 2
fi

if [ "$PROPERTIES_LOADED" != "1" ]; then
    echo "No se cargó las propiedades de la conexión a la base de datos"
    exit 2
fi

# Permite la creacion de un backup de
# la base de datos
# @param $1 Nombre del equipo a respaldar
# @param $2 Nombre de la base de datos
function backup() {
    rm -rf $DIR_RESULTS/*.backup
    datetime=$(date +"%Y%m%d")$(echo _)$(date +"%H%M")
    bkp_filename="$DIR_RESULTS/$2$(echo _)$1$(echo _)$datetime.backup"
    echo "Generando backup, espere por favor..."
    log ">> Generando backup $2 del $1"
    PGPASSWORD=$DB_PASS $PG_PATH/pg_dump -i -h $DB_HOST -p $DB_PORT -U $DB_USER -F c -b -v -f "$bkp_filename" $2 2> /dev/null
    if [[ $? -eq 0 ]]; then
        log "<< Backup creado y guardado en $bkp_filename"
        echo "Backup guardado en: $PWD"
        echo "Archivo backup: $bkp_filename"
        return 0
    else
        log "<< Ocurrio un error al generar el backup"
        echo "Error al generar Backup, intente de nuevo"
        return 1
    fi
}

# Funcion para exportar una consulta a la base de datos
# @param $1 Consulta a la base de datos, este es el contenido
# 			a exportar
# @param $2 Nombre del archivo a exportar
# @param $3 Nombre de la base de datos
function queryExport() {
    dbName=$3
    dateNow="$(date +"%Y%m%d")"
    filePathToExport="$DIR_RESULTS/$2$(echo "_")$dateNow$(echo ".rpt.csv")"
    psqlCommand="\copy ($1) TO '$filePathToExport' CSV HEADER"
    echo "Realizando consulta y exportando resultado -> $2$(echo "...")"
    PGPASSWORD=$DB_PASS \
    $PG_PATH/psql -U $DB_USER -h $DB_HOST -p $DB_PORT -d $dbName -c "$psqlCommand"
    if [[ $? -ne 0 ]]; then
        echo "Ocurrió un error al exportar"
        log "<< Error al generar el CSV de la consulta [ $1 ]"
        return 1
    else
        echo "$filePathToExport generado..."
        log "<< Consulta [ $(echo $1) ] ejecutada y resultado exportado a $filePathToExport"
        return 0
    fi
}

# Funcion que permite realizar una consulta a la base de datos
# y mostrar el resultado en pantalla
# @param $1 Nombre del archivo SQL
# @param $2 Nombre de la base de datos
function queryFromFile() {
    dbName=$2
    sqlFile="$1"
    log ">> Se ejecuta el script $1"
    PGPASSWORD=$DB_PASS \
    $PG_PATH/psql -U $DB_USER -h $DB_HOST -p $DB_PORT -d $dbName -f "$sqlFile" 2> /dev/null
}

# Funcion que permite realizar una consulta a la base de datos
# y mostrar el resultado en pantalla
# @param $1 Consulta a realizar
# @param $2 Nombre de la base de datos
function query() {
    dbName=$2
    psqlCommand="$1"
    log ">> Ejecución de la consulta [ $(echo $1) ]"
    PGPASSWORD=$DB_PASS \
    $PG_PATH/psql -U $DB_USER -h $DB_HOST -p $DB_PORT -d $dbName -c "$psqlCommand" 2> /dev/null
}

DB_LIB_LOADED=1
