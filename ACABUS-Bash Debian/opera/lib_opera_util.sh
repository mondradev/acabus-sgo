# ----------------------------------------------------
# ----------------------------------------------------
# ||            Libería de utilería                 ||
# ||                                                ||
# ||    2017/01/26                          v1.1    ||
# ||    Javier de Jesús Flores Mondragón            ||
# ||    Operadora de transporte integral            ||
# ----------------------------------------------------
# ----------------------------------------------------

# Ancho de la terminal de DEBIAN (caracteres)
LENGTH_SCREEN=80

# Directorio de resultados
DIR_RESULTS="results"

# ------------------------------------------------------
# Funciones comunes
# ------------------------------------------------------

# Muestra una linea separadora en la pantalla
function separator() {
    string='-'
    for i in $(seq 1 $LENGTH_SCREEN)
    do
        string="$string-"
    done
    echo $string
    echo ''
}

# Permite mostrar un texto centrado que es
# utilizado como titulo
# @param "$1" Texto a mostrar como titulo
function title() {
    length=$(echo "$1" | wc --chars)
    size=$(($LENGTH_SCREEN-$length))
    size=$(($size/2))
    strTitle=''
    for i in $(seq 1 $size)
    do
	strTitle=" $strTitle"
    done
    strTitle="$strTitle "$1""
    separator
    echo "$strTitle"
    separator
}

# Permite establecer una pausa hasta que el usuario
# presiona una tecla
function pause() {
    echo ''
    read -p "Presione [ENTER] para continuar..." pauseVar
}

# Permite mostrar en pantalla un menu
# @param $1..$n Opciones del menu, puedes usar la
# 				palabra [SALTO] para generar un espacio
#				entre las opciones del menú
function menu() {
	if (($# > 0)); then
		i=1
		for param in "$@"
		do
		if [[ "$param" == '[SALTO]' ]]; then
			echo ''
		else
            if [[ "$param" == *'['* ]]; then
                tempOpc=$param
                tempOpc=${tempOpc/[/}
                tempOpc=${tempOpc/]/}
                if (( ${#tempOpc} > 0 )); then
                    printf "\e[38;5;196m     $tempOpc\n"
                    printf "\e[0m"
                fi
            else
                if [[ $i -lt 10 ]]; then
                    echo "      $i$(echo "] ")$param"
                else
                    echo "     $i$(echo "] ")$param"
                fi
                i=$(($i+1))
            fi
		fi
		done
		echo ''
		echo "     $i$(echo "] Salir")"
		echo ''
	fi
}

# Permite crea un comprimido de un archivo y las bitacoras
# @param $1 Nombre del archivo
function createTar () {
    echo "Comprimiendo resultados y bitácoras, espere..."
    log ">> Comprimiendo resultados y las bitácoras en el $1"
    filename="$1$(echo '.tar.gz')"
    tar -cf $filename $DIR_RESULTS/*
    if [[ $? -ne 0 ]]; then
	    return 1
    fi
    log "<< comprimido completo"
    echo "Se terminá de crear el comprimido: $filename"
}

# Permite reiniciar la pc
function rebootPC() {
    echo ''
    echo "Reiniciando PC..."
    log ">> Reiniciando equipo abordo"
    reboot
}

# Permite reiniciar la pc
function shutdownNow() {
    echo ''
    echo "Apagando PC..."
    log ">> Apagando equipo abordo"
    shutdown now
}

# Indica que se puede utilizar el log
readyLog=0

# Permite llevar una bitacora de procesos
# @param $1 Mensaje a enviar a la bitacora
function log() {
    if [[ readyLog -eq 0 ]]; then return 0; fi
    dateNow="$(date +"%Y/%m/%d") $(date +"%H:%M:%S")"
    echo "$dateNow $1" >> "$DIR_RESULTS/$LOG_FILENAME"
}

# Permite enviar a bitacora cada linea leida en el argumento
# @param $1 Contenido multilinea
function lineToLog() {
    log "<< $2"
    bkpIFS=$IFS
    IFS=$'\n'
    for line in $1
    do
        log "<< $line"
    done
    IFS=$bkpIFS
}

# Verificar log
# @param $1 Archivo de la bitacora
# @param $2 Titulo del archivo
# @param $3 Lectura en tiempo real (1: para si)
function viewLog() {
    title "$2"
    if [ "$3" == "1" ]; then
	    echo "Presione [ENTER] para cerrar el log"
    fi
    echo ""
    tail -f "$1" &
    PID=$!
    if [ "$3" == "1" ]; then
        while (true); do
            if read -t 10 -n 1 key; then
                break
            fi
        done
    else
	sleep 1
    fi
    kill -9 $PID
}

# Convierte todo a mayusculas
function toUpperCase() {
    echo $1 | tr [a-z] [A-Z]
}

# Convierte todo a minusculas
function toLowerCase() {
    echo $1 | tr [A-Z] [a-z]
}

UTIL_LIB_LOADED=1