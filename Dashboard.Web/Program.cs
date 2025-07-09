using Dashboard.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// MVC
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // JSON serialization options
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep original property names
        options.JsonSerializerOptions.WriteIndented = true;
    });

// HTTP Client for API communication - Sadece bu kaydı kullan
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    var apiBaseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "http://localhost:5100";
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);

    // Default headers
    client.DefaultRequestHeaders.Add("User-Agent", "Dashboard.Web/1.0");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


// Session support (optional - for user preferences)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "Dashboard.Session";
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Anti-forgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "Dashboard.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck("api-health", () =>
    {
        // API health check burada yapılabilir
        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API connection available");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // HSTS header ekle (production için)
    app.UseHsts();
}

// Global exception handling - bu kısmı düzelt
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// HTTPS redirection (production için)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Static files
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files for 1 day
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=86400");
    }
});

// Routing
app.UseRouting();

// Session
app.UseSession();

// Anti-forgery
app.UseAntiforgery();

// Authorization (if needed)
// app.UseAuthorization();

// Custom 404 handling
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
    {
        context.Request.Path = "/Home/NotFound";
        await next();
    }
});

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// API routes
app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller}/{action=Index}/{id?}");

// Health check endpoint
app.MapHealthChecks("/health");

// Robots.txt
app.MapGet("/robots.txt", async context =>
{
    var robotsTxt = "User-agent: *\nDisallow: /api/\nDisallow: /health/";
    context.Response.ContentType = "text/plain";
    await context.Response.WriteAsync(robotsTxt);
});

// Sitemap.xml (basic)
app.MapGet("/sitemap.xml", async context =>
{
    var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
    var sitemap = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
    <url>
        <loc>{baseUrl}/</loc>
        <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>
        <changefreq>daily</changefreq>
        <priority>1.0</priority>
    </url>
    <url>
        <loc>{baseUrl}/Home/About</loc>
        <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>
        <changefreq>monthly</changefreq>
        <priority>0.5</priority>
    </url>
</urlset>";

    context.Response.ContentType = "application/xml";
    await context.Response.WriteAsync(sitemap);
});

// Startup'ta API bağlantısını test et
using (var scope = app.Services.CreateScope())
{
    var apiService = scope.ServiceProvider.GetRequiredService<IApiService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Testing API connection...");
        var isHealthy = await apiService.HealthCheckAsync();
        logger.LogInformation("API Health Check Result: {IsHealthy}", isHealthy);

        if (!isHealthy)
        {
            logger.LogWarning("⚠️  API is not responding. Please ensure Dashboard.Api is running on http://localhost:5100");
            logger.LogWarning("   You can start the API project separately or check if it's running");
        }
        else
        {
            logger.LogInformation("✅ API connection successful");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Failed to connect to API. Please check if Dashboard.Api is running on http://localhost:5100");
    }
}

app.Logger.LogInformation("Dashboard.Web started on {Environment} environment", app.Environment.EnvironmentName);
app.Logger.LogInformation("Web application available at: http://localhost:5000 and https://localhost:5001");

app.Run();