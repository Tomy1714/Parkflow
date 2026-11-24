using LibParkFlow.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace LibParkFlow.Interfaces
{
    public interface IConexion : IDisposable
    {
        string? string_conexion { get; set; }

        DbSet<Usuario>? Usuarios { get; set; }
        DbSet<RolEntidad>? Roles { get; set; }
        DbSet<AsignacionRol>? AsignacionesRol { get; set; }
        DbSet<Vehiculo>? Vehiculos { get; set; }
        DbSet<Celda>? Celdas { get; set; }
        DbSet<Caja>? Cajas { get; set; }
        DbSet<RegistroParqueadero>? RegistrosParqueadero { get; set; }
        DbSet<Reserva>? Reservas { get; set; }
        DbSet<ServicioLavado>? ServiciosLavado { get; set; }
        DbSet<DetalleServicio>? DetallesServicio { get; set; }
        DbSet<ComprobantePago>? ComprobantesPago { get; set; }
        DbSet<AuditoriaReserva>? AuditoriasReserva { get; set; }

        DatabaseFacade Database { get; }
        EntityEntry<T> Entry<T>(T entity) where T : class;
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
