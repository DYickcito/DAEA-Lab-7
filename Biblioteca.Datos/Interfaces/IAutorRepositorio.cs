using Biblioteca.Entidades;

namespace Biblioteca.Datos.Interfaces;

public interface IAutorRepositorio
{
    Task<List<Autor>> ListarActivosAsync();
    Task<Autor> ObtenerPorIdAsync(int autorId);
}
