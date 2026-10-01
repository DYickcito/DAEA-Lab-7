using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

internal static class LectorExtensiones
{
    public static string TextoONulo(this SqlDataReader r, int i) => r.IsDBNull(i) ? null : r.GetString(i);

    public static DateTime? FechaONula(this SqlDataReader r, int i) => r.IsDBNull(i) ? null : r.GetDateTime(i);
}
