using Biblioteca.Entidades;

namespace Biblioteca.Datos.Interfaces;

public interface ISocioRepositorio
{
    Task<List<Socio>> BuscarAsync(string texto);
    Task<Socio> ObtenerPorIdAsync(int socioId);
    Task<bool> ExisteDniAsync(string dni, int excluirSocioId);
    Task InsertarAsync(Socio socio);
    Task ActualizarAsync(Socio socio);
    Task DarDeBajaAsync(int socioId);
}
