namespace ControlLicencias.Models;

public class UsuarioAutorizado
{
    public int Id { get; set; }
    public int LicenciaId { get; set; }
    public string UserName { get; set; } = "";
    public DateTime FechaAgregado { get; set; }
    public bool Activo { get; set; }

    public string EstadoTexto => Activo ? "Activo" : "Inactivo";
    public string EstadoColor => Activo ? "#22C55E" : "#94A3B8";
    public string EstadoBg => Activo ? "#1A22C55E" : "#1A64748B";
}
