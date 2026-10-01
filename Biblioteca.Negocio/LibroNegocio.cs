using System.Text.RegularExpressions;
using Biblioteca.Datos;
using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio;

public class LibroNegocio
{
    private static readonly Regex IsbnValido = new(@"^(\d{13}|\d{9}[\dXx])$", RegexOptions.Compiled);

    private readonly ILibroRepositorio _libros;
    private readonly IAutorRepositorio _autores;
    private readonly IPrestamoRepositorio _prestamos;

    public LibroNegocio() : this(new LibroRepositorio(), new AutorRepositorio(), new PrestamoRepositorio()) { }

    public LibroNegocio(ILibroRepositorio libros, IAutorRepositorio autores, IPrestamoRepositorio prestamos)
    {
        _libros = libros;
        _autores = autores;
        _prestamos = prestamos;
    }

    public Task<List<Libro>> BuscarAsync(string texto) => _libros.BuscarAsync(texto?.Trim() ?? "");

    public Task<List<Autor>> ListarAutoresAsync() => _autores.ListarActivosAsync();

    public async Task InsertarAsync(Libro libro)
    {
        await ValidarAsync(libro, esNuevo: true);
        await _libros.InsertarAsync(libro);
    }

    public async Task ActualizarAsync(Libro libro)
    {
        if (libro.LibroId <= 0)
            throw new ReglaNegocioException("Debe seleccionar el libro que desea actualizar.");

        var existente = await _libros.ObtenerPorIdAsync(libro.LibroId);
        if (existente == null || !existente.Activo)
            throw new ReglaNegocioException("El libro seleccionado ya no existe o está dado de baja.");

        await ValidarAsync(libro, esNuevo: false);
        await _libros.ActualizarAsync(libro);
    }

    public async Task EliminarAsync(int libroId)
    {
        var libro = await _libros.ObtenerPorIdAsync(libroId);
        if (libro == null || !libro.Activo)
            throw new ReglaNegocioException("El libro seleccionado ya no existe o está dado de baja.");

        int pendientes = await _prestamos.ContarPendientesPorLibroAsync(libroId);
        if (pendientes > 0)
            throw new ReglaNegocioException(
                $"No se puede dar de baja \"{libro.Titulo}\": tiene {pendientes} préstamo(s) pendiente(s) de devolución.");

        await _libros.DarDeBajaAsync(libroId);
    }

    private async Task ValidarAsync(Libro libro, bool esNuevo)
    {
        libro.Titulo = libro.Titulo?.Trim();
        libro.ISBN = libro.ISBN?.Replace("-", "").Replace(" ", "").ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(libro.Titulo))
            throw new ReglaNegocioException("El título es obligatorio.");
        if (libro.Titulo.Length > 200)
            throw new ReglaNegocioException("El título no puede superar los 200 caracteres.");

        if (string.IsNullOrWhiteSpace(libro.ISBN))
            throw new ReglaNegocioException("El ISBN es obligatorio.");
        if (!IsbnValido.IsMatch(libro.ISBN))
            throw new ReglaNegocioException("El ISBN debe tener 13 dígitos (o 10 en el formato antiguo).");

        if (libro.Ejemplares < 0)
            throw new ReglaNegocioException("La cantidad de ejemplares no puede ser negativa.");

        if (libro.AutorId <= 0)
            throw new ReglaNegocioException("Debe seleccionar un autor.");
        var autor = await _autores.ObtenerPorIdAsync(libro.AutorId);
        if (autor == null || !autor.Activo)
            throw new ReglaNegocioException("El autor seleccionado no existe o está dado de baja.");

        int excluir = esNuevo ? 0 : libro.LibroId;
        if (await _libros.ExisteIsbnAsync(libro.ISBN, excluir))
            throw new ReglaNegocioException($"Ya existe un libro con el ISBN {libro.ISBN}.");
    }
}
