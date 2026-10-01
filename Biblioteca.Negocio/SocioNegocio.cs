using System.Net.Mail;
using System.Text.RegularExpressions;
using Biblioteca.Datos;
using Biblioteca.Datos.Interfaces;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio;

public class SocioNegocio
{
    private static readonly Regex DniValido = new(@"^\d{8}$", RegexOptions.Compiled);

    private readonly ISocioRepositorio _socios;
    private readonly IPrestamoRepositorio _prestamos;

    public SocioNegocio() : this(new SocioRepositorio(), new PrestamoRepositorio()) { }

    public SocioNegocio(ISocioRepositorio socios, IPrestamoRepositorio prestamos)
    {
        _socios = socios;
        _prestamos = prestamos;
    }

    public Task<List<Socio>> BuscarAsync(string texto) => _socios.BuscarAsync(texto?.Trim() ?? "");

    public async Task InsertarAsync(Socio socio)
    {
        await ValidarAsync(socio, esNuevo: true);
        await _socios.InsertarAsync(socio);
    }

    public async Task ActualizarAsync(Socio socio)
    {
        if (socio.SocioId <= 0)
            throw new ReglaNegocioException("Debe seleccionar el socio que desea actualizar.");

        var existente = await _socios.ObtenerPorIdAsync(socio.SocioId);
        if (existente == null || !existente.Activo)
            throw new ReglaNegocioException("El socio seleccionado ya no existe o está dado de baja.");

        await ValidarAsync(socio, esNuevo: false);
        await _socios.ActualizarAsync(socio);
    }

    public async Task EliminarAsync(int socioId)
    {
        var socio = await _socios.ObtenerPorIdAsync(socioId);
        if (socio == null || !socio.Activo)
            throw new ReglaNegocioException("El socio seleccionado ya no existe o está dado de baja.");

        int pendientes = await _prestamos.ContarLibrosPendientesPorSocioAsync(socioId);
        if (pendientes > 0)
            throw new ReglaNegocioException(
                $"No se puede dar de baja a {socio.Nombre}: tiene {pendientes} libro(s) pendiente(s) de devolución.");

        await _socios.DarDeBajaAsync(socioId);
    }

    private async Task ValidarAsync(Socio socio, bool esNuevo)
    {
        socio.DNI = socio.DNI?.Trim();
        socio.Nombre = socio.Nombre?.Trim();
        socio.Email = string.IsNullOrWhiteSpace(socio.Email) ? null : socio.Email.Trim();

        if (string.IsNullOrWhiteSpace(socio.DNI))
            throw new ReglaNegocioException("El DNI es obligatorio.");
        if (!DniValido.IsMatch(socio.DNI))
            throw new ReglaNegocioException("El DNI debe tener exactamente 8 dígitos.");

        if (string.IsNullOrWhiteSpace(socio.Nombre))
            throw new ReglaNegocioException("El nombre es obligatorio.");
        if (socio.Nombre.Length > 100)
            throw new ReglaNegocioException("El nombre no puede superar los 100 caracteres.");

        if (socio.Email != null && !EmailValido(socio.Email))
            throw new ReglaNegocioException("El email no tiene un formato válido.");

        int excluir = esNuevo ? 0 : socio.SocioId;
        if (await _socios.ExisteDniAsync(socio.DNI, excluir))
            throw new ReglaNegocioException($"Ya existe un socio con el DNI {socio.DNI}.");
    }

    private static bool EmailValido(string email)
    {
        if (email.Length > 100) return false;
        try
        {
            return new MailAddress(email).Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
