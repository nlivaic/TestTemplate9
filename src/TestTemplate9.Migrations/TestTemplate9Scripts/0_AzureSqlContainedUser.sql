-- Executed only on Azure SQL
IF SERVERPROPERTY('EngineEdition') > 4
BEGIN
	CREATE USER [sqlusers] FROM EXTERNAL PROVIDER;
    EXEC sp_addrolemember 'db_datareader', 'sqlusers'
    EXEC sp_addrolemember 'db_datawriter', 'sqlusers'
END
