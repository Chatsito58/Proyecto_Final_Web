# Proyecto Final Web

Este proyecto utiliza una base de datos SQL Server local (LocalDB). Para evitar el error `Invalid column name 'PrecioHora'` asegúrese de que la tabla **Canchas** tenga la columna `PrecioHora`.

Para inicializar la base de datos ejecute primero `SQLQuery_Crear_Tablas.sql` y luego `SQLQuery_Poblar_Tablas.sql`. Si la tabla **Canchas** ya existe y no contiene la columna `PrecioHora`, utilice `SQLQuery_Actualizar_Canchas.sql`:

```sql
-- Agregar la columna PrecioHora si no existe
IF COL_LENGTH('Canchas','PrecioHora') IS NULL
BEGIN
    ALTER TABLE [dbo].[Canchas] ADD [PrecioHora] DECIMAL(10,2) NOT NULL DEFAULT 0;
END
GO
```


Después de actualizar la base de datos, reinicie la aplicación y el error debería resolverse.
