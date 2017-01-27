DO $$
DECLARE
	TABLAS_ENVI RECORD;
BEGIN
	FOR TABLAS_ENVI IN (select  'update '||table_schema||'.'||table_name||' set bol_envi=true;' QUERY_UPDATE
			from information_schema.tables where table_schema = 'sitm_envi')
	LOOP
		EXECUTE TABLAS_ENVI.QUERY_UPDATE;
	END LOOP;
END $$
