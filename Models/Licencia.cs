namespace ControlLicencias.Models;

public class Licencia
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int ProductoId { get; set; }
    public string ClienteNombre { get; set; } = "";
    public string RUC { get; set; } = "";
    public string ProductoNombre { get; set; } = "";
    public string? CodigoActivacion { get; set; }
    public int CantidadPCs { get; set; }
    public int CantidadUsuarios { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public bool Activa { get; set; }

    public string EstadoLicencia => Activa ? "Activa" : "Inactiva";
    public string EstadoLicenciaColor => Activa ? "#22C55E" : "#94A3B8";
    public string EstadoLicenciaBg => Activa ? "#1A22C55E" : "#1A64748B";
}
