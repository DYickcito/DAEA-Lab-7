using Biblioteca.Entidades;

namespace Biblioteca.Datos.Interfaces;

public interface ILibroRepositorio
{
    Task<List<Libro>> BuscarAsync(string texto);
    Task<Libro> ObtenerPorIdAsync(int libroId);
    Task<bool> ExisteIsbnAsync(string isbn, int excluirLibroId);
    Task InsertarAsync(Libro libro);
    Task ActualizarAsync(Libro libro);
    Task DarDeBajaAsync(int libroId);
}
