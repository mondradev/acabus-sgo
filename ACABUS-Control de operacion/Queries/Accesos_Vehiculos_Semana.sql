SELECT FECHA, 
	SUM(CASE WHEN CREDENCIAL LIKE 'MONEDERO%' AND COSTO=0 THEN VALIDACIONES ELSE 0 END) MONEDERO_COSTO_0,
	SUM(CASE WHEN CREDENCIAL LIKE 'MONEDERO%' AND COSTO=3 THEN VALIDACIONES ELSE 0 END) MONEDERO_COSTO_3,
	SUM(CASE WHEN CREDENCIAL LIKE 'MONEDERO%' AND COSTO=7 THEN VALIDACIONES ELSE 0 END) MONEDERO_COSTO_7,
	SUM(CASE WHEN CREDENCIAL LIKE 'MONEDERO%' AND COSTO=10 THEN VALIDACIONES ELSE 0 END) MONEDERO_COSTO_10,
	SUM(CASE WHEN CREDENCIAL LIKE '%ADMINISTRADOR%' THEN VALIDACIONES ELSE 0 END) SUPERVISOR
FROM (
SELECT 
    DATE(FCH_ACCE_SALI) AS FECHA,
    SCRE.DES_SCRE AS CREDENCIAL, 
    COST_VIAJ AS COSTO, 
    COUNT(ID_ACCE_SALI) AS VALIDACIONES,
    SUM(COST_VIAJ) AS COSTOS
FROM SITM.SBOP_ACCE_SALI AS ACCE
JOIN SITM.SBEQ_EQUI AS EQUI ON ACCE.ID_EQUI = EQUI.ID_EQUI
JOIN SITM.SFVH_VEHI AS VEHI ON VEHI.ID_VEHI = EQUI.ID_VEHI
JOIN SITM.SFVH_TIPO_VEHI AS TIPO_VEHI ON VEHI.ID_TIPO_VEHI = TIPO_VEHI.ID_TIPO_VEHI
JOIN SITM.COAC_SCRE AS SCRE ON UPPER(SCRE.COD_SCRE) = UPPER(ACCE.TCRE)
WHERE DATE(FCH_ACCE_SALI) BETWEEN DATE('{paramFchIni}') AND (DATE('{paramFchIni}') + 6)
AND ACCE.ID_TIPO_ACCE NOT IN (327,868,902,904)
GROUP BY FECHA, CREDENCIAL, COSTO
)
GROUP BY FECHA
ORDER BY FECHA