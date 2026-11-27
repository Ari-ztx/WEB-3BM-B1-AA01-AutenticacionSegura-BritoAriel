USE master;
GO


IF DB_ID(N'AutenticacionSeguraRiderDb') IS NULL
BEGIN
    CREATE DATABASE [AutenticacionSeguraRiderDb];
END;
GO

