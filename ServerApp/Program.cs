// Creacion Front con blazor template
// dotnet new blazorwasm -n ClientApp

// Creacion Back con C# template
// dotnet new webapi -n ServerApp

// Agregar ambos proyectos a una solucion
// dotnet new sln -n FullStackSolution
// dotnet sln add ClientApp ServerApp

var builder = WebApplication.CreateBuilder(args);

// Opción A: permitir sólo el origen del cliente (más seguro para dev)
// Ajusta el origen al que usa tu ClientApp (ejemplo: http://localhost:5173 o http://localhost:5000)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:5077")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Opción B: permitir todos los orígenes (solo para pruebas rápidas; NO usar en producción)
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAll", policy =>
//     {
//         policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
//     });
// });

var app = builder.Build();

// Usar la política antes de mapear endpoints
app.UseCors("AllowClientOrigin");
// app.UseCors("AllowAll"); // descomenta si usas la opción B

app.MapGet("/api/productlist", () =>
{
    return new[]
    {
        new { Id = 1, Name = "Laptop", Price = 1200.50, Stock = 25,  Category = new { Id = 101, Name = "Electronics" } },
        new { Id = 2, Name = "Headphones", Price = 50.00, Stock = 100,   Category = new { Id = 102, Name = "Accessories" } }
    };
});

app.Run();

/* Sugerencias y estrategias para mejora de alamcenamiento en cache para minimizar la carga del servidor
// ...existing code...
using System.Net.Http.Headers;
using Microsoft.Net.Http.Headers;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Habilitar caché en memoria para datos
builder.Services.AddMemoryCache();

// Habilitar response caching (control de cabeceras para caché del lado cliente/proxy)
builder.Services.AddResponseCaching();

// Opcional: caché distribuida (Redis) — descomenta y ajusta si la necesitas
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = "localhost:6379";
//     options.InstanceName = "ServerApp_";
// });

var app = builder.Build();

// Usar CORS (ya configurado en tu proyecto si aplica)
// app.UseCors("AllowClientOrigin");

// Habilitar middleware de Response Caching
app.UseResponseCaching();

// Endpoint con caché en memoria para minimizar carga del servidor
app.MapGet("/api/products", (IMemoryCache cache, HttpContext http) =>
{
    const string cacheKey = "productList_v1";

    // 1) Intentar obtener del cache en memoria
    if (cache.TryGetValue(cacheKey, out object? cached) && cached is object[] productsFromCache)
    {
        // Ajustar cabeceras HTTP para permitir cache cliente/proxy
        http.Response.GetTypedHeaders().CacheControl =
            new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromSeconds(60) }; // TTL de respuesta
        http.Response.Headers[HeaderNames.Vary] = "Origin";
        return Results.Ok(productsFromCache);
    }

    // 2) Si no está en caché: crear/obtener datos (simulación)
    var products = new[]
    {
        new { Id = 1, Name = "Laptop", Price = 1200.50, Stock = 25 },
        new { Id = 2, Name = "Headphones", Price = 50.00, Stock = 100 }
    };

    // 3) Configurar políticas de caché en memoria
    var cacheOptions = new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5), // expira en 5 min
        SlidingExpiration = TimeSpan.FromMinutes(2)               // extiende si hay acceso frecuente
        // Puedes agregar Priority o Size si lo necesitas
    };

    cache.Set(cacheKey, products, cacheOptions);

    // Ajustar cabeceras HTTP para la respuesta actual
    http.Response.GetTypedHeaders().CacheControl =
        new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromSeconds(60) };
    http.Response.Headers[HeaderNames.Vary] = "Origin";

    return Results.Ok(products);
});

// Ejemplo alternativo usando IDistributedCache (Redis) — para entornos con múltiples instancias
// app.MapGet("/api/products-redis", async (IDistributedCache distCache, HttpContext http) =>
// {
//     const string cacheKey = "productList_v1";
//     var cachedJson = await distCache.GetStringAsync(cacheKey);
//     if (!string.IsNullOrEmpty(cachedJson))
//     {
//         var fromCache = JsonSerializer.Deserialize<object[]>(cachedJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
//         http.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromSeconds(60) };
//         http.Response.Headers[HeaderNames.Vary] = "Origin";
//         return Results.Ok(fromCache);
//     }
//
//     var products = new[]
//     {
//         new { Id = 1, Name = "Laptop", Price = 1200.50, Stock = 25 },
//         new { Id = 2, Name = "Headphones", Price = 50.00, Stock = 100 }
//     };
//
//     var json = JsonSerializer.Serialize(products);
//     var options = new DistributedCacheEntryOptions
//     {
//         AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
//     };
//     await distCache.SetStringAsync(cacheKey, json, options);
//
//     http.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromSeconds(60) };
//     http.Response.Headers[HeaderNames.Vary] = "Origin";
//     return Results.Ok(products);
// });

app.Run();
// ...existing code...
*/