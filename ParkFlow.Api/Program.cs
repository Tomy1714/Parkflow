using LibParkFlow.Implementaciones;
using LibParkFlow.Implementaciones.Patrones.FacadeLiquidacion;
using LibParkFlow.Interfaces;
using LibParkFlow.Nucleo;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configuración de cadena de conexión (prioriza appsettings o LibParkFlow.Nucleo.Configuraciones)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? Configuraciones.Obtener("string_conexion");

builder.Services.AddDbContext<Conexion>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<IConexion>(sp => sp.GetRequiredService<Conexion>());

// Inyección de dependencias para capas de negocio y patrones GoF
builder.Services.AddScoped<IUsuariosNegocio, UsuariosNegocio>();
builder.Services.AddScoped<ICeldasNegocio, CeldasNegocio>();
builder.Services.AddScoped<IRegistrosNegocio, RegistrosNegocio>();
builder.Services.AddScoped<IReservasNegocio, ReservasNegocio>();
builder.Services.AddScoped<IServiciosLavadoNegocio, ServiciosLavadoNegocio>();
builder.Services.AddScoped<ICajasNegocio, CajasNegocio>();
builder.Services.AddScoped<IVehiculosNegocio, VehiculosNegocio>();
builder.Services.AddScoped<IComprobantesNegocio, ComprobantesNegocio>();
builder.Services.AddScoped<ILiquidacionFacade, LiquidacionFacade>();

// Soporte para controladores y serialización JSON sin ciclos y con Enums como cadenas legibles
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p =>
    {
        p.AllowAnyOrigin()
         .AllowAnyMethod()
         .AllowAnyHeader();
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Inicialización de base de datos con datos semilla de demostración
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Conexion>();
    try
    {
        context.Database.EnsureCreated();
        InicializadorDatos.Inicializar(context);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[BD CONECTADA]: Base de datos ParkFlow conectada e inicializada en: {connectionString}");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[AVISO BD]: Advertencia al verificar/inicializar BD: {ex.Message}");
        Console.ResetColor();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");

// app.UseHttpsRedirection(); // Deshabilitado para desarrollo local en HTTP (puerto 5007)

app.UseAuthorization();

app.MapControllers();

app.Run();
