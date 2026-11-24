using LibParkFlow.Implementaciones;
using LibParkFlow.Implementaciones.Patrones.FacadeLiquidacion;
using LibParkFlow.Interfaces;
using LibParkFlow.Nucleo;
using LibPresentaciones.ImplementacionesPresentaciones;
using LibPresentaciones.InterfacesPresentaciones;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuración de base de datos y servicios
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? Configuraciones.CadenaConexionPorDefecto;

builder.Services.AddDbContext<Conexion>(options =>
{
    try
    {
        options.UseSqlServer(connectionString);
    }
    catch
    {
        options.UseInMemoryDatabase("ParkFlowDb_Web");
    }
});

builder.Services.AddScoped<IConexion>(sp => sp.GetRequiredService<Conexion>());
builder.Services.AddScoped<IUsuariosNegocio, UsuariosNegocio>();
builder.Services.AddScoped<ICeldasNegocio, CeldasNegocio>();
builder.Services.AddScoped<IRegistrosNegocio, RegistrosNegocio>();
builder.Services.AddScoped<IReservasNegocio, ReservasNegocio>();
builder.Services.AddScoped<IServiciosLavadoNegocio, ServiciosLavadoNegocio>();
builder.Services.AddScoped<ICajasNegocio, CajasNegocio>();
builder.Services.AddScoped<IVehiculosNegocio, VehiculosNegocio>();
builder.Services.AddScoped<IComprobantesNegocio, ComprobantesNegocio>();
builder.Services.AddScoped<ILiquidacionFacade, LiquidacionFacade>();
builder.Services.AddScoped<IComunicaciones, Comunicaciones>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpClient("ParkFlowApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5007");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddRazorPages();

var app = builder.Build();

// Inicialización de datos en SQL Server si aún no existen
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Conexion>();
    try
    {
        context.Database.EnsureCreated();
        InicializadorDatos.Inicializar(context);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[BD WEB CONECTADA]: Inicialización verificada en {connectionString}");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[AVISO BD Web]: {ex.Message}");
        Console.ResetColor();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

// Proxy transparente de /api hacia ParkFlow.Api (http://localhost:5007)
// Esto unifica el origen para el navegador en https://localhost:7213 o http://localhost:5188,
// eliminando Mixed Content (HTTPS -> HTTP), bloqueos de CORS y discrepancias de puertos.
app.Map("/api/{**catch-all}", async (HttpContext context, IHttpClientFactory clientFactory) =>
{
    var client = clientFactory.CreateClient("ParkFlowApi");
    var targetPath = context.Request.Path.Value ?? "";
    var queryString = context.Request.QueryString.Value ?? "";
    var targetUri = new Uri($"http://localhost:5007{targetPath}{queryString}");

    using var requestMessage = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUri);

    if (context.Request.ContentLength > 0 || !string.IsNullOrEmpty(context.Request.ContentType))
    {
        requestMessage.Content = new StreamContent(context.Request.Body);
        if (!string.IsNullOrEmpty(context.Request.ContentType))
        {
            requestMessage.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(context.Request.ContentType);
        }
    }

    foreach (var header in context.Request.Headers)
    {
        if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)) continue;
        requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
    }

    try
    {
        using var responseMessage = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
        context.Response.StatusCode = (int)responseMessage.StatusCode;

        foreach (var header in responseMessage.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }
        foreach (var header in responseMessage.Content.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }
        context.Response.Headers.Remove("transfer-encoding");

        await responseMessage.Content.CopyToAsync(context.Response.Body);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 503;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
        {
            ok = false,
            error = $"Error de comunicación con ParkFlow.Api en http://localhost:5007: {ex.Message}. Asegúrese de que ParkFlow.Api esté ejecutándose."
        }));
    }
});

app.MapRazorPages();

app.Run();
