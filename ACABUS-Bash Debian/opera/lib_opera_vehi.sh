# ----------------------------------------------------
# ----------------------------------------------------
# ||            Script de verificación              ||
# ||                de vehículos                    ||
# ||                                                ||
# ||    2017/02/01                          v1.2    ||
# ||    Javier de Jesús Flores Mondragón            ||
# ||    Operadora de transporte integral            ||
# ----------------------------------------------------
# ----------------------------------------------------
# Este script es una herramienta que permite verificar
# el funcionamiento de sicoft, crear backups y validar
# el número de registros pendientes por envíar a través
# del proceso de réplica.

VERSION="v1.2"

# Validando dependencias
if [ "$BLACKLIST_LIB_LOADED" != "1" ]; then
    echo "       No se cargó la librería de listas negras"
    exit 2
fi

# ------------------------------------------------------
# Funciones especifica de operación
# ------------------------------------------------------

# Obtiene el numero economico de la base de datos
function getNoEcon() {
    log "Obteniendo número económico"
    noecon=$(query "SELECT NO_ECON FROM SITM_DISP.SFVH_VEHI" $DB_NAME | grep -o "A[ACP]\{1\}-[0-9]\{3\}")
    if [ "$noecon" == "" ]; then
        read -p "       No se obtuvo el número económico del vehículo, ingréselo por favor: " -e noecon
        noecon=$(toUpperCase $noecon)
        noecon=${noecon/-/}
        noecon=$(echo $noecon | grep -o "A[ACP]\{1\}[0-9]\{3\}")
        if [ "$noecon" == "" ]; then
            return 1
        fi
        read -p "       El número económico: $noecon es correcto? (s/n): " -e response
        if [[ "$response" == "s" || "$response" == "S" ]]; then
            if [ "$noecon" == "" ]; then
                return 1
            fi
        else
            return 1
        fi
    fi
    noecon=${noecon/-/}
    echo $noecon
    return 0
}

FILE_UPDATE_ENVI="update_envi.sql"

# Crea un back up de la base de datos de un vehiculos
# con solo ingresar el nombre del dispositivo
# @param $1 Nombre de la base de datos
# @param $2 Nombre del equipo
function createBackup() {
    title "Backup de base de datos $2"
    echo "       Deteniendo RN GPS..."
    killall -s KILL rn_gps &>/dev/null
    sleep 1
    reportAcceCont $2
    echo "       Espere, preparando información..."
    lastdaysTrans=$(showAllTrans | grep "[Val|Cont|Loc|Asgn_turn|Turn|Asgn_Ruta]")
    lineToLog "$lastdaysTrans" "Ultimos 10 días de transacciones"
    backup $2 $1
    if [ $? -ne 1 ]; then
        echo "$2,$(ifconfig | grep 'wlan[0-9]\{1\}' | grep -o '[0-9a-f:]\{3\}\{5\}[0-9a-f]\{2\}')" > $DIR_RESULTS/mac_$2.info
        read -p "       Desea actualizar las tablas de sitm_envi ? (s/n): " response
        if [[ "$response" == "" ]]; then return 1; fi
        if [[ "$response" == "s" || "$response" == "S" ]]; then
            echo "       Actualizando sitm_envi, espere..."
            result=$(queryFromFile "$FILE_UPDATE_ENVI" $DB_NAME)
            if [[ $? -eq 0 ]]; then
                echo "       Actualización de sitm_envi correcta"
                log "<< Fin de actualización de sitm_envi.*.bol_envi=true"
            else
                echo "       Ocurrió un error al actualizar"
                log "<< Error al actualizar sitm_envi"
            fi
        fi
        createTar $2
    fi
    here="$PWD"
    echo  "       Iniciando RN GPS..."
    cd /home/teknei/.teknei_startup/rn
    sudo -u tkn_gsm ./rn_gps_startup.sh &>/tmp/tkn_rn_gps.log &
    cd "$here"
    echo "       Listo..."
    pause
}

# Indica cuantas transacciones estan pendientes por
# replicar.
function showAllTrans() {
    # Validaciones
    query "SELECT date(fch_acce_sali)||' Val_Disp: '||count(*)
        FROM sitm_disp.sbop_acce_sali
	    WHERE date(fch_acce_sali) BETWEEN (date(NOW())-10) AND date(NOW())
        GROUP BY date(fch_acce_sali)
        ORDER BY date(fch_acce_sali) ASC" $DB_NAME | grep 'Val_Disp:'
    query "SELECT date(fch_envi)||' Val_Envi: '||count(*)
        FROM sitm_envi.sbop_acce_sali
	    WHERE date(fch_envi) BETWEEN (date(NOW())-10) AND date(NOW())
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi) ASC" $DB_NAME | grep 'Val_Envi:'
    query "SELECT date(fch_envi)||' Val_Pend_Envi: '||count(*)
        FROM sitm_envi.sbop_acce_sali
	    WHERE date(fch_envi) BETWEEN (date(NOW())-10) AND date(NOW())
            AND bol_envi=false
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi) ASC" $DB_NAME | grep 'Val_Pend_Envi:'

    # Conteos
    query "SELECT date(fch_cont_acce)||' Cont_Disp: '||count(*)
        FROM sitm_disp.sbop_cont_acce
	    WHERE date(fch_cont_acce) BETWEEN (date(NOW())-10) AND date(NOW())
        GROUP BY date(fch_cont_acce)
        ORDER BY date(fch_cont_acce) ASC" $DB_NAME | grep 'Cont_Disp:'
    query "SELECT date(fch_envi)||' Cont_Envi: '||count(*)
        FROM sitm_envi.sbop_cont_acce
	    WHERE date(fch_envi) BETWEEN (date(NOW())-10) AND date(NOW())
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi) ASC" $DB_NAME | grep 'Cont_Envi:'
    query "SELECT date(fch_envi)||' Cont_Pend_Envi: '||count(*)
        FROM sitm_envi.sbop_cont_acce
	    WHERE date(fch_envi) BETWEEN (date(NOW())-10) AND date(NOW())
            AND bol_envi=false
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi) ASC" $DB_NAME | grep 'Cont_Pend_Envi:'

    # Localización
    query "SELECT date(fch_crea)||' Loca_Disp: '||count(*)
        FROM sitm_disp.sfmo_hist_rece_nave
	    WHERE date(fch_crea) BETWEEN (date(NOW())-10) AND date(NOW())
        GROUP BY date(fch_crea)
        ORDER BY date(fch_crea) ASC" $DB_NAME | grep 'Loca_Disp:'
    query "SELECT date(fch_envi)||' Loca_Envi: '||count(*)
        FROM sitm_envi.sfmo_hist_rece_nave
	    WHERE date(fch_envi) BETWEEN (date(NOW())-10) AND date(NOW())
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi) ASC" $DB_NAME | grep 'Loca_Envi:'
    query "SELECT date(fch_envi)||' Loca_Pend_Envi: '||count(*)
        FROM sitm_envi.sfmo_hist_rece_nave
	    WHERE date(fch_envi) BETWEEN (date(NOW())-10) AND date(NOW())
            AND bol_envi=false
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi) ASC" $DB_NAME | grep 'Loca_Pend_Envi:'

	# Asignación de turnos
    query "SELECT date(fch_crea)||' Asgn_turn_Disp: '||count(*)
        FROM sitm_disp.sbop_asgn_turn
	    WHERE date(fch_crea) BETWEEN (date(NOW())-10) AND date(NOW())
        GROUP BY date(fch_crea)
        ORDER BY date(fch_crea) ASC" $DB_NAME | grep 'Asgn_turn_Disp:'
    query "SELECT date(fch_envi)||' Asgn_turn_Envi: '||count(*)
        FROM sitm_envi.sbop_asgn_turn
	    WHERE date(fch_envi) BETWEEN (date(NOW())-10) AND date(NOW())
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi) ASC" $DB_NAME | grep 'Asgn_turn_Envi:'
    query "SELECT date(fch_envi)||' Asgn_turn_Pend_Envi: '||count(*)
        FROM sitm_envi.sbop_asgn_turn
	    WHERE date(fch_envi) BETWEEN (date(NOW())-10) AND date(NOW())
            AND bol_envi=false
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi) ASC" $DB_NAME | grep 'Asgn_turn_Pend_Envi:'

    # Turnos
    query "SELECT date(fch_crea)||' Turn_Disp: '||count(*)
        FROM sitm_disp.sbop_turn
	    WHERE date(fch_crea) BETWEEN (date(NOW())-10) AND date(NOW())
        GROUP BY date(fch_crea)
        ORDER BY date(fch_crea) ASC" $DB_NAME | grep 'Turn_Disp:'
    query "SELECT date(fch_envi)||' Turn_Envi: '||count(*)
        FROM sitm_envi.sbop_turn
	    WHERE date(fch_envi) BETWEEN (date(NOW())-10) AND date(NOW())
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi) ASC" $DB_NAME | grep 'Turn_Envi:'
    query "SELECT date(fch_envi)||' Turn_Pend_Envi: '||count(*)
        FROM sitm_envi.sbop_turn
	    WHERE date(fch_envi) BETWEEN (date(NOW())-10) AND date(NOW())
            AND bol_envi=false
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi) ASC" $DB_NAME | grep 'Turn_Pend_Envi:'

    # Asignación de ruta
    query "SELECT date(fch_crea)||' Asgn_Ruta_Disp: '||count(*)
        FROM sitm_disp.sfru_asgn
	    WHERE date(fch_crea) BETWEEN (date(NOW())-10) AND date(NOW())
        GROUP BY date(fch_crea)
        ORDER BY date(fch_crea) ASC" $DB_NAME | grep 'Asgn_Ruta_Disp:'
    query "SELECT date(fch_envi)||' Asgn_Ruta_Envi: '||count(*)
        FROM sitm_envi.sfru_asgn
	    WHERE date(fch_envi) BETWEEN (date(NOW())-10) AND date(NOW())
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi) ASC" $DB_NAME | grep 'Asgn_Ruta_Envi:'
    query "SELECT date(fch_envi)||' Asgn_Ruta_Pend_Envi: '||count(*)
        FROM sitm_envi.sfru_asgn
	    WHERE date(fch_envi) BETWEEN (date(NOW())-10) AND date(NOW())
            AND bol_envi=false
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi) ASC" $DB_NAME | grep 'Asgn_Ruta_Pend_Envi:'
}

# Indica cuantas transacciones estan pendientes por
# replicar.
# @param $1 Nombre del equipo
function showTrans() {
    title "Transacciones pendientes $1"
    query "SELECT date(fch_envi)||'       Validaciones: '||count(*)
        FROM sitm_envi.sbop_acce_sali
        WHERE bol_envi=false
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi)" $DB_NAME | grep 'Validaciones:'
    query "SELECT date(fch_envi)||'       Conteos: '||count(*)
        FROM sitm_envi.sbop_cont_acce
        WHERE bol_envi=false
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi)" $DB_NAME | grep 'Conteos:'
    query "SELECT date(fch_envi)||'       Localización: '||count(*)
        FROM sitm_envi.sfmo_hist_rece_nave
        WHERE bol_envi=false
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi)" $DB_NAME | grep 'Localización:'
    query "SELECT date(fch_envi)||'       Asignación Turnos: '||count(*)
        FROM sitm_envi.sbop_asgn_turn
        WHERE bol_envi=false
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi)" $DB_NAME | grep 'Asignación Turnos:'
    query "SELECT date(fch_envi)||'       Turnos: '||count(*)
        FROM sitm_envi.sbop_turn
        WHERE bol_envi=false
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi)" $DB_NAME | grep 'Turnos:'
    query "SELECT date(fch_envi)||'       Asignación Ruta: '||count(*)
        FROM sitm_envi.sfru_asgn
        WHERE bol_envi=false
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi)" $DB_NAME | grep 'Asignación Ruta:'
}


# Muestra el ID de vehiculo y el ID de equipo
function showIDVehiEqui() {
    title 'ID de Vehículo y ID de Equipo'
    query "SELECT '       ID Vehículo: '||id_vehi FROM sitm_disp.sfvh_vehi " $DB_NAME | grep "ID Vehículo: "
    query "SELECT '       ID Equipo: '||id_equi FROM sitm_disp.sbeq_equi" $DB_NAME | grep "ID Equipo: "
    pause
}

# Muestra el ICCID de la SIM, que es el numero de
# serie de la tarjeta SIM
function showICCID() {
    title 'ICCID de SIM Card'
    echo " Obteniendo número de serie de la SIM"
    echo -e "AT+ICCID" > /dev/ttyUSB3
    echo "       $(head -n 8 /dev/ttyUSB3 | grep ICCID:)"
    pause
}

# Muestra el log de RN GPS, en el se visualiza el
# envio de las tramas de ubicacion del vehiculo
function validateRN() {
    viewLog /tmp/tkn_rn_gps.log 'Bitácora de tramas enviadas' $1
}

# Muestra el log de la tarjeta Sierra MC8090
function validateMC() {
    viewLog /tmp/tkn_sierra.log 'Validación de Conexión WWAN' $1
    if [ "$1" != "1" ]; then pause; fi
    viewLog /tmp/tkn_mc8090.log 'Estado de la tarjeta WWAN' $1
}

# Abre el fichero de propiedades del RN GPS
function openRNProperties() {
    title 'Propiedades de SICOFT'
    nano /home/teknei/.teknei_startup/rn/rn_gps.properties
    echo ' Fin de la edicion'
}

# Ejecuta un ping al servidor de base de datos
function validateConnection() {
    title 'Verificando conectividad a internet'
    echo '       Realizando ping al servidor de SICOFT'
    sudo -u tkn_gsm ping 187.217.65.98 -c 5
    pause
}

SQL_FILE_NAME="report_query.sql"

# Crea un reporte de las validaciones y conteos replicados
# @param $1 Fecha de inicio del reporte
# @param $2 Fecha final del reporte
# @param $3 Número economico del vehiculo
function reportAcceCont() {
    sqlFile=$(cat "$SQL_FILE_NAME")
    rm -rf $DIR_RESULTS/trx_cont_nave_*.rpt.csv
    filename="data_$1"
    queryExport "$sqlFile" "$filename" $DB_NAME
}

# Muestra el log del DB Backup
function showDBBackuplog() {
    viewLog /tmp/tkn_db_backup.log 'DB Backup LOG' $1
}

# Muestra el log del Looking
function showLookingLog() {
    viewLog /tmp/tkn_looking.log 'Looking LOG' $1
}

# Permite verificar si en la base de datos
# se estan guardando conteos de personas
function checkCounterDB() {
    while (true); do
        clear
        title "Conteos en base de datos"
        echo "       Presione [ENTER] para finalizar..."
        echo ""
        query "select '       Conteos: '||count(*) from sitm_disp.sbop_cont_acce" $DB_NAME | grep "Conteos:"
	if read -t 10 -n 1 key; then
	    break
	fi
    done
}

# ------------------------------------------------------
# Funcionamiento principal del programa
# ------------------------------------------------------
if [ "$(whoami)" != "root" ]; then
    echo "       Necesitas ser superusuario, ejecuta con sudo"
    exit
fi
NO_ECON=$(getNoEcon)
if [[ $? -ne 0 ]]; then
    echo "       Debes ingresar el número económico correctamente, adios! :)"
    exit
fi
LOG_FILENAME="log_$NO_ECON$(echo _)$(date +"%Y%m%d").log"
readyLog=1
log "Iniciando Opera Bus $VERSION $NO_ECON"
opcMain=0
while [ "$opcMain" != "16" ]
do
	# Menú de funcionamiento de consultas
	title "Opera Bus $VERSION | $NO_ECON"
	menu "[SICOFT]" \
        "Verificar sicoft" \
        "Estado de tarjeta WWAN" \
        "Validación de envío de tramas" \
        "Conectividad al servidor SICOFT" \
        "[Configuración de SICOFT]" \
        "Obtener número de serie del SIM" \
        "Propiedades del SICOFT" \
        "Ver ID de Vehículo y ID de Equipo" \
        "[Contadores]" \
        "Verificar contador en base de datos" \
        "[Replica]" \
        "Transacciones pendientes" \
        "DB Backup LOG" \
        "Looking LOG" \
        "[Otras funciones]" \
        "Crear backup SITM" \
        "Actualizar lista negra" \
        "Apagar PC" \
        "Reiniciar PC"
	read -p "      Opcion: " -e opcMain
	if [ "$opcMain" != "" ]; then
	    case $opcMain in
       	    1)
            	validateMC
            	pause
            	validateRN
            	pause
	            alidateConnection
            	;;
            2) validateMC 1 ;;
            3) validateRN 1 ;;
            4) validateConnection ;;
            5) showICCID ;;
            6) openRNProperties ;;
            7) showIDVehiEqui ;;
            8) checkCounterDB ;;
            9) showTrans $NO_ECON
                pause ;;
            10) showDBBackuplog 1 ;;
            11) showLookingLog 1 ;;
            12) createBackup $DB_NAME $NO_ECON ;;
            13) updateBackList ;;
            14) shutdownNow ;;
            15) rebootPC ;;
	        16) exit 0 ;;
	        *) ;;
	    esac
	else
	   opcMain=0
	fi
done
