# DAEA-Lab-7

## Integrantes
- santiago salas
- jesus flores
  
## Activación

**Abrir:** `Biblioteca.sln` (Visual Studio 2022) → **F5**.
Se abre la aplicación **Biblioteca** (Libros, Socios, Préstamo, Devolución y Reporte).
Si pide proyecto de inicio, elegir **Biblioteca.WPF**.

## Base de datos

SQL Server LocalDB: `(localdb)\MSSQLLocalDB`.
Carpeta `Base-de-Datos\`, ejecutar en este orden:

| Script | Crea |
|---|---|
| `1-Script-BibliotecaDB.sql` | `BibliotecaDB` (datos del enunciado) |
| `2-Script-BibliotecaDB-DatosExtra.sql` | datos ficticios adicionales (opcional) |

La cadena de conexión está en el `App.config` de `Biblioteca.WPF`.
