using System.Configuration;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

internal static class ConexionBD
{
    private static string CadenaConexion =>
        ConfigurationManager.ConnectionStrings["BibliotecaDB"]?.ConnectionString
        ?? throw new InvalidOperationException(
            "No se encontró la cadena de conexión 'BibliotecaDB' en el App.config del proyecto de inicio.");

    public static SqlConnection Crear() => new SqlConnection(CadenaConexion);
}
