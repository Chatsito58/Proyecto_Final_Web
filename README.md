# Proyecto Final Web

Este proyecto utiliza una base de datos SQL Server local (LocalDB). Para evitar el error `Invalid column name 'PrecioHora'` asegúrese de que la tabla **Canchas** tenga la columna `PrecioHora`.

Ejecute los scripts de base de datos incluidos en el repositorio para crear y poblar la base de datos. Si ya tiene una base existente y falta dicha columna, ejecute el script `SQLQuery_Actualizar_Canchas.sql`:

```sql
-- Agregar la columna PrecioHora si no existe
IF COL_LENGTH('Canchas','PrecioHora') IS NULL
BEGIN
    ALTER TABLE [dbo].[Canchas] ADD [PrecioHora] DECIMAL(10,2) NOT NULL DEFAULT 0;
END
GO
```

Después de actualizar la base de datos, reinicie la aplicación y el error debería resolverse.

