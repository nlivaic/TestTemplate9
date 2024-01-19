USE master
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TestTemplate9Db')
BEGIN
  CREATE DATABASE TestTemplate9Db;
END;
GO

USE TestTemplate9Db;
GO

IF NOT EXISTS (SELECT 1
                 FROM sys.server_principals
                WHERE [name] = N'TestTemplate9Db_Login' 
                  AND [type] IN ('C','E', 'G', 'K', 'S', 'U'))
BEGIN
    CREATE LOGIN TestTemplate9Db_Login
        WITH PASSWORD = '<DB_PASSWORD>';
END;
GO  

IF NOT EXISTS (select * from sys.database_principals where name = 'TestTemplate9Db_User')
BEGIN
    CREATE USER TestTemplate9Db_User FOR LOGIN TestTemplate9Db_Login;
END;
GO  


EXEC sp_addrolemember N'db_datareader', N'TestTemplate9Db_User';
GO

EXEC sp_addrolemember N'db_datawriter', N'TestTemplate9Db_User';
GO

EXEC sp_addrolemember N'db_ddladmin', N'TestTemplate9Db_User';
GO
