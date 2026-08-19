namespace ControlLicencias.Models;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }

    public string EstadoTexto => Activo ? "Activo" : "Inactivo";
    public string EstadoColor => Activo ? "#22C55E" : "#94A3B8";
    public string EstadoBg => Activo ? "#1A22C55E" : "#1A64748B";
}
