namespace ControlLicencias.Models;

public class UsuarioAdmin
{
    public int Id { get; set; }
    public string UserName { get; set; } = "";
    public string Nombre { get; set; } = "";
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }

    public string EstadoTexto => Activo ? "Activo" : "Inactivo";
    public string EstadoColor => Activo ? "#22C55E" : "#94A3B8";
    public string EstadoBg => Activo ? "#1A22C55E" : "#1A64748B";
}
