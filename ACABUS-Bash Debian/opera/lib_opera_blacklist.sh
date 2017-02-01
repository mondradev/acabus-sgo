# ----------------------------------------------------
# ----------------------------------------------------
# ||            Script de actualización de          ||
# ||            lista negra en vehículos            ||
# ||                                                ||
# ||    2017/01/26                          v0.4    ||
# ||    Javier de Jesús Flores Mondragón            ||
# ||    Operadora de transporte integral            ||
# ----------------------------------------------------
# ----------------------------------------------------
# Este script es una herramienta que permite actualizar
# la lista negra de credenciales en vehiculos.

# Validando dependencias
if [ "$DB_LIB_LOADED" != "1" ]; then
    echo "       No se cargó la librería de base de datos"
    exit 2
fi

BLACK_LIST_FILENAME="blacklist.list"

# Ejecución de la actualización de lista negra
# de un vehículo
function updateBackList() {
    clear
    title "Actualizacion de lista negra de la unidad $1"
    echo "       Actualizando, espere..."
    log ">> Actualizando lista negra"
    updated=0
    error=0
    for uidTarj in $(cat $BLACK_LIST_FILENAME); do
        exists=$(query "SELECT 'Exists: '||COUNT(*) FROM SITM_DISP.SBLN_LNEG WHERE UID_TARJ='$uidTarj'" $DB_NAME | grep 'Exists: ')
        exists=${exists/'Exists: '/}
        if [[ $exists -eq 0 ]]; then
            idTarj=$(query "SELECT 'id_tarj: '||(ID_TARJ + 1) FROM SITM_DISP.SBLN_LNEG ORDER BY ID_TARJ DESC LIMIT 1" $DB_NAME | grep 'id_tarj: ')
            idTarj=${idTarj/'id_tarj: '/}
            if [[ "$idTarj" == "" ]]; then
                idTarj=1
            fi
            log ">> Agregando tarjeta $uidTarj"
            result=$(query "INSERT INTO sitm_disp.sbln_lneg (id_tarj, desc_lneg, fch_alta, uid_tarj) VALUES ($idTarj, 'Carga masiva', NOW(), '$uidTarj')" $DB_NAME)
            if [[ $? -eq 0 ]]; then
                log "<< Se agregó tarjeta $uidTarj"
                updated=$((updated+1))
            else
                log "<< Error al agregar a la lista negra $uidTarj"
                error=$((error+1))
            fi
        fi
    done
    echo "       Fin de la actualización"
    echo "       Agregados: $updated"
    echo "       Errores: $error"
    log "<< Actualización terminada, actualizados: $updated, errores: $error"
    pause
}

BLACKLIST_LIB_LOADED=1
