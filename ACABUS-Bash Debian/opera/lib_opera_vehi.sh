# ----------------------------------------------------
# ----------------------------------------------------
# ||            Script de verificación              ||
# ||                de vehículos                    ||
# ||                                                ||
# ||    2017/01/26                          v0.9    ||
# ||    Javier de Jesús Flores Mondragón            ||
# ||    Operadora de transporte integral            ||
# ----------------------------------------------------
# ----------------------------------------------------
# Este script es una herramienta que permite verificar
# el funcionamiento de sicoft, crear backups y validar
# el número de registros pendientes por envíar a través
# del proceso de réplica.

# Validando dependencias
if [ "$BLACKLIST_LIB_LOADED" != "1" ]; then
    echo "No se cargó la librería de listas negras"
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
        read -p "No se obtuvo el número económico del vehículo, ingréselo por favor: " -e noecon
        read -p "El número económico: $noecon es correcto? (s/n): " -e response
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

# Crea un back up de la base de datos de un vehiculos
# con solo ingresar el nombre del dispositivo
# @param $1 Nombre de la base de datos
# @param $2 Nombre del equipo
function createBackup() {
    title "Backup de base de datos $2"
    for line in $(showAllTrans)
    do
	    log "<< $line"
    done
    for line in $(showTrans)
    do
	    log "<< $line"
    done
    backup $2 $1
    if [ $? -ne 1 ]; then 
        createTar $2
    fi
    pause
}


# Indica cuantas transacciones estan pendientes por
# replicar.
function showAllTrans() {
    query "SELECT date(fch_acce_sali)||' Val: '||count(*)
        FROM sitm_disp.sbop_acce_sali
	    WHERE date(fch_acce_sali) BETWEEN (date(NOW())-7) AND date(NOW())
        GROUP BY date(fch_acce_sali)
        ORDER BY date(fch_acce_sali) DESC" $DB_NAME | grep 'Val:'

    query "SELECT date(fch_cont_acce)' Conteos: '||count(*)
        FROM sitm_disp.sbop_cont_acce
	    WHERE date(fch_cont_acce) BETWEEN (date(NOW())-7) AND date(NOW())
        GROUP BY date(fch_cont_acce)
        ORDER BY date(fch_cont_acce) DESC" $DB_NAME | grep 'Conteos:'

	query "SELECT date(fch_crea)||' Localización: '||count(*)
        FROM sitm_disp.sfmo_hist_rece_nave
	    WHERE date(fch_crea) BETWEEN (date(NOW())-7) AND date(NOW())
        GROUP BY date(fch_crea)
        ORDER BY date(fch_crea) DESC" $DB_NAME | grep 'Localización:'
}

# Indica cuantas transacciones estan pendientes por
# replicar.
# @param $1 Nombre del equipo
function showTrans() {
    title "Transacciones pendientes $1"
    query "SELECT date(fch_envi)||' Val: '||count(*)
        FROM sitm_envi.sbop_acce_sali
        WHERE bol_envi=false
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi)" $DB_NAME | grep 'Val:'
    query "SELECT date(fch_envi)||' Conteos: '||count(*)
        FROM sitm_envi.sbop_cont_acce
        WHERE bol_envi=false
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi)" $DB_NAME | grep 'Conteos:'
    query "SELECT date(fch_envi)||' Localización: '||count(*)
        FROM sitm_envi.sfmo_hist_rece_nave
        WHERE bol_envi=false
        GROUP BY date(fch_envi)
        ORDER BY date(fch_envi)" $DB_NAME | grep 'Localización:'
}


# Muestra el ID de vehiculo y el ID de equipo
function showIDVehiEqui() {
    title 'ID de Vehículo y ID de Equipo'
    query "SELECT 'ID Vehículo: '||id_vehi FROM sitm_disp.sfvh_vehi " $DB_NAME | grep "ID Vehículo: "
    query "SELECT 'ID Equipo: '||id_equi FROM sitm_disp.sbeq_equi" $DB_NAME | grep "ID Equipo: "
    pause
}

# Muestra el ICCID de la SIM, que es el numero de
# serie de la tarjeta SIM
function showICCID() {
    title 'ICCID de SIM Card'
    echo " Obteniendo número de serie de la SIM"
    echo -e "AT+ICCID" > /dev/ttyUSB3
    head -n 8 /dev/ttyUSB3 | grep ICCID:
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
    echo ' Realizando ping al servidor de SICOFT'
    sudo -u tkn_gsm ping 187.217.65.98 -c 5
    pause
}

# Crea un reporte de las validaciones y conteos replicados
# @param $1 Fecha de inicio del reporte
# @param $2 Fecha final del reporte
# @param $3 Número economico del vehiculo
function reportAcceCont() {
    title "Reporte de conteos y transacciones $1 al $2 $3"
    rm -rf $DIR_RESULTS/trx_cont_nave_*.rpt.csv
    filename="trx_cont_nave_$3$(echo _)$1_$2"
    queryExport "SELECT FECHA, SUM(VAL_PEND) AS VAL_PEND, SUM(VAL_TOTAL) AS VAL_TOTAL,
                    SUM(CONT_PEND) AS CONT_PEND, SUM(CONT_TOTAL) AS CONT_TOTAL
            FROM (
                SELECT DATE(FCH_ENVI) AS FECHA, COUNT(*) AS VAL_PEND, 0 AS VAL_TOTAL,
                    0 AS CONT_PEND, 0 AS CONT_TOTAL
                FROM SITM_ENVI.SBOP_ACCE_SALI
                WHERE BOL_ENVI=FALSE
                GROUP BY DATE(FCH_ENVI)
                UNION
                SELECT DATE(FCH_ACCE_SALI) AS FECHA, 0 AS VAL_PEND, COUNT(*) AS VAL_TOTAL,
                    0 AS CONT_PEND, 0 AS CONT_TOTAL
                FROM SITM_DISP.SBOP_ACCE_SALI
                GROUP BY DATE(FCH_ACCE_SALI)
                UNION
                SELECT DATE(FCH_ENVI) AS FECHA, 0 AS VAL_PEND, 0 AS VAL_TOTAL,
                    COUNT(*) AS CONT_PEND, 0 AS CONT_TOTAL
                FROM SITM_ENVI.SBOP_CONT_ACCE
                WHERE BOL_ENVI=FALSE
                GROUP BY DATE(FCH_ENVI)
                UNION
                SELECT DATE(FCH_CONT_ACCE) AS FECHA, 0 AS VAL_PEND, 0 AS VAL_TOTAL,
                    0 AS CONT_PEND, COUNT(*) AS CONT_TOTAL
                FROM SITM_DISP.SBOP_CONT_ACCE
                GROUP BY DATE(FCH_CONT_ACCE)
            ) AS V
            WHERE FECHA BETWEEN DATE('$1') AND DATE('$2')
            GROUP BY DATE(V.FECHA)
            ORDER BY V.FECHA" "$filename" $DB_NAME
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
        echo "Presione [ENTER] para finalizar..."
        echo ""
        query "select 'Conteos: '||count(*) from sitm_disp.sbop_cont_acce" $DB_NAME | grep "Conteos:"
	if read -t 10 -n 1 key; then
	    break
	fi
    done
}

# ------------------------------------------------------
# Funcionamiento principal del programa
# ------------------------------------------------------
if [ "$(whoami)" != "root" ]; then
    echo "Necesitas ser superusuario, ejecuta con sudo"
    exit
fi
NO_ECON=$(getNoEcon)
if [[ $? -ne 0 ]]; then
    echo "Debes ingresar el número económico, adios! :)"
    exit
fi
LOG_FILENAME="log_$NO_ECON$(echo _)$(date +"%Y%m%d").log"
readyLog=1
log "Iniciando Opera Bus v0.9 $NO_ECON"
opcMain=0
while [ "$opcMain" != "15" ]
do
	# Menú de funcionamiento de consultas
	title "Opera script para Vehiculos $NO_ECON"
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
        "Valicaciones y conteos pendientes" \
        "DB Backup LOG" \
        "Looking LOG" \
	    "[SALTO]" \
        "[Otras funciones]" \
        "Crear backup SITM" \
        "Apagar PC" \
        "Reiniciar PC"
	read -p "Opcion: " -e opcMain
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
            12)
                updateBackList
                now=$(date +"%Y-%m-%d")
                dateIni="2016-01-01"
                dateFin="$now"
            	reportAcceCont $dateIni $dateFin $NO_ECON
                createBackup $DB_NAME $NO_ECON ;;
            13) shutdownNow ;;
            14) rebootPC ;;
	        15) exit 0 ;;
	        *) ;;
	    esac
	else
	   opcMain=0
	fi
done
