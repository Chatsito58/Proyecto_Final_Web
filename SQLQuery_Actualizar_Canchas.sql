-- Agregar la columna PrecioHora si no existe
IF COL_LENGTH('Canchas','PrecioHora') IS NULL
BEGIN
    ALTER TABLE [dbo].[Canchas] ADD [PrecioHora] DECIMAL(10,2) NOT NULL DEFAULT 0;
END
GO
