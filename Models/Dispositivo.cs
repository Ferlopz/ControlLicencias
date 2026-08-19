namespace ControlLicencias.Models;

public class Dispositivo
{
    public int Id { get; set; }
    public int LicenciaId { get; set; }
    public string MachineId { get; set; } = "";
    public string NombrePC { get; set; } = "";
    public string Tipo { get; set; } = "";
    public DateTime FechaRegistro { get; set; }
    public DateTime UltimoAcceso { get; set; }
    public bool Activo { get; set; }

    public string EstadoTexto => Activo ? "Activo" : "Inactivo";
    public string EstadoColor => Activo ? "#22C55E" : "#94A3B8";
    public string EstadoBg => Activo ? "#1A22C55E" : "#1A64748B";
}
