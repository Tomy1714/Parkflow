using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;
using LibParkFlow.Nucleo;
using Microsoft.EntityFrameworkCore;

namespace LibParkFlow.Implementaciones
{
    public class Conexion : DbContext, IConexion
    {
        public string? string_conexion { get; set; }

        public Conexion()
        {
            string_conexion = Configuraciones.Obtener("string_conexion");
        }

        public Conexion(DbContextOptions<Conexion> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var conn = string_conexion ?? Configuraciones.Obtener("string_conexion");
                optionsBuilder.UseSqlServer(conn, sqlOptions =>
                {
                    sqlOptions.CommandTimeout(60);
                });
            }
        }

        public DbSet<Usuario>? Usuarios { get; set; }
        public DbSet<RolEntidad>? Roles { get; set; }
        public DbSet<AsignacionRol>? AsignacionesRol { get; set; }
        public DbSet<Vehiculo>? Vehiculos { get; set; }
        public DbSet<Celda>? Celdas { get; set; }
        public DbSet<Caja>? Cajas { get; set; }
        public DbSet<RegistroParqueadero>? RegistrosParqueadero { get; set; }
        public DbSet<Reserva>? Reservas { get; set; }
        public DbSet<ServicioLavado>? ServiciosLavado { get; set; }
        public DbSet<DetalleServicio>? DetallesServicio { get; set; }
        public DbSet<ComprobantePago>? ComprobantesPago { get; set; }
        public DbSet<AuditoriaReserva>? AuditoriasReserva { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Conversión de enums a strings para consistencia con BD SQL Server
            modelBuilder.Entity<Celda>()
                .Property(c => c.Estado)
                .HasConversion<string>();

            modelBuilder.Entity<RegistroParqueadero>()
                .Property(r => r.EstadoRegistro)
                .HasConversion<string>();

            modelBuilder.Entity<Reserva>()
                .Property(r => r.EstadoReserva)
                .HasConversion<string>();

            modelBuilder.Entity<ServicioLavado>()
                .Property(s => s.EstadoLavado)
                .HasConversion<string>();

            modelBuilder.Entity<ComprobantePago>()
                .Property(c => c.Canal)
                .HasConversion<string>();

            modelBuilder.Entity<Caja>()
                .Property(c => c.EstadoCaja)
                .HasConversion<string>();

            // Relaciones
            modelBuilder.Entity<RegistroParqueadero>()
                .HasOne(r => r.Vehiculo)
                .WithMany(v => v.HistorialRegistros)
                .HasForeignKey(r => r.Placa)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RegistroParqueadero>()
                .HasOne(r => r.Celda)
                .WithMany()
                .HasForeignKey(r => r.NumeroCelda)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Vehiculo)
                .WithMany(v => v.HistorialReservas)
                .HasForeignKey(r => r.Placa)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Celda)
                .WithMany()
                .HasForeignKey(r => r.NumeroCelda)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServicioLavado>()
                .HasOne(s => s.Registro)
                .WithMany(r => r.ServiciosLavado)
                .HasForeignKey(s => s.IdRegistro)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ComprobantePago>()
                .HasOne(c => c.Registro)
                .WithOne(r => r.Comprobante)
                .HasForeignKey<ComprobantePago>(c => c.IdRegistro)
                .OnDelete(DeleteBehavior.Restrict);

            // DetalleServicio: evitar múltiples rutas de cascada en SQL Server (error 1785)
            modelBuilder.Entity<DetalleServicio>()
                .HasOne(d => d.Registro)
                .WithMany()
                .HasForeignKey(d => d.IdRegistro)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleServicio>()
                .HasOne(d => d.Servicio)
                .WithMany()
                .HasForeignKey(d => d.IdServicio)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
