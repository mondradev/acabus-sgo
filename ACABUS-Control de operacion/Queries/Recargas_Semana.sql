SELECT FECHA, SUM(RECARGAS), SUM(MONTO)
FROM (
SELECT
DATE(TRAN.FCH_TRAN) AS FECHA,
EQUI.id_equi,
COUNT(TRAN.ID_TRAN) AS RECARGAS,
    CASE
        WHEN UPPER(TRAN.TIPO_TRAN) = 'V' AND TRAN.IMPO_TRAN > 10 THEN SUM(TRAN.IMPO_TRAN - 10)
        WHEN UPPER(TRAN.TIPO_TRAN) = 'C' THEN SUM(TRAN.IMPO_TRAN + TRAN.Sald_ini)
        ELSE SUM(TRAN.IMPO_TRAN)
    END AS MONTO
FROM SITM.SBOP_TRAN AS TRAN
JOIN SITM.SBEQ_EQUI AS EQUI ON TRAN.ID_EQUI = EQUI.ID_EQUI
JOIN SITM.CCTM_INV AS INV ON EQUI.ID_INV = INV.ID_INV
JOIN SITM.CCTM_PROD AS PROD ON INV.ID_PROD = PROD.ID_PROD
WHERE DATE(TRAN.FCH_TRAN) BETWEEN DATE('{paramFchIni}') AND (DATE('{paramFchIni}') + 6)
and TRAN.id_equi not in (337,338,339,340,341,342,343,344,345,346,197,196,198,199, 471)
AND UPPER(TRAN.TIPO_TRAN) IN ('R', 'V','C')
GROUP BY FECHA, EQUI.id_equi, TIPO_TRAN, IMPO_TRAN
)
GROUP BY FECHA
ORDER BY FECHA