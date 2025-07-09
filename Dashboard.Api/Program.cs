using Dashboard.Api.Configuration;
using Dashboard.Api.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // ✅ Bu ayarları kontrol edin
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.AllowTrailingCommas = true;
        options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
    });

// API Explorer for Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Dashboard API",
        Version = "v1",
        Description = "Dashboard Analytics API - Elasticsearch tabanlı sepet analiz sistemi"
    });

    // XML comments for Swagger documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// CORS - Frontend ile iletişim için
builder.Services.AddCors(options =>
{
    options.AddPolicy("DashboardPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "https://localhost:5001", "http://localhost:5002") // MVC app URLs
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Elasticsearch configuration
builder.Services.AddElasticsearch(builder.Configuration);

// Services
builder.Services.AddScoped<IElasticsearchService, ElasticsearchService>();
builder.Services.AddScoped<ILocationService, LocationService>();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dashboard API v1");
        c.RoutePrefix = "swagger"; // Swagger UI'ı /swagger endpoint'inde aç
        c.DocumentTitle = "Dashboard API Documentation";
        c.DefaultModelsExpandDepth(-1); // Model açıklamalarını gizle
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // Endpoint'leri kapalı göster
    });
}

// Global exception handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            message = "Sunucu hatası oluştu",
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    });
});

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});

// CORS
app.UseCors("DashboardPolicy");

// Routing
app.UseRouting();

// Controllers
app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// Default route - API bilgisi
app.MapGet("/", () => new
{
    Message = "Dashboard API is running",
    Version = "1.0.0",
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName,
    Documentation = "/swagger", // Swagger URL'ini göster
    Health = "/health"
});

// API endpoints listesi
app.MapGet("/api", () => new
{
    Message = "Dashboard API Endpoints",
    Endpoints = new
    {
        Documentation = "/swagger",
        Health = "/health",
        Cart = "/api/cart",
        Customer = "/api/customer",
        Dashboard = "/api/dashboard",
        Autocomplete = "/api/autocomplete",
        Dropdown = "/api/dropdown"
    }
});

// Ensure Data directory exists for sabitler.json
var dataPath = Path.Combine(app.Environment.ContentRootPath, "Data");
if (!Directory.Exists(dataPath))
{
    Directory.CreateDirectory(dataPath);
}

app.Logger.LogInformation("Dashboard API started on {Environment} environment", app.Environment.EnvironmentName);
app.Logger.LogInformation("Swagger UI available at: /swagger");
app.Logger.LogInformation("API Health check available at: /health");

app.Run();