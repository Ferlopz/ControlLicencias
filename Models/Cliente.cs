namespace ControlLicencias.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string RUC { get; set; } = "";
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public bool Activo { get; set; }

    public string EstadoTexto => Activo ? "Activo" : "Inactivo";
    public string EstadoColor => Activo ? "#22C55E" : "#94A3B8";
    public string EstadoBg => Activo ? "#1A22C55E" : "#1A64748B";
}
