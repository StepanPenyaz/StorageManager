using Microsoft.EntityFrameworkCore;
using Storage.Data.Contexts;
using Storage.Data.Interfaces;
using Storage.Data.Repositories;
using Storage.Services.Interfaces;
using Storage.Services.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("LocalDev", policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
}

builder.Services.AddDbContext<StorageContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StorageDatabase")));

builder.Services.AddScoped<IStorageRepository, StorageRepository>();
builder.Services.AddScoped<IStorageViewService, StorageViewService>();
builder.Services.AddScoped<IStorageInitializationService, StorageInitializationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

// UseCors must come before UseHttpsRedirection so that the CORS middleware
// can process preflight (OPTIONS) requests and return 204 with the correct
// Access-Control-Allow-Origin header before the HTTPS redirect fires.
if (app.Environment.IsDevelopment())
    app.UseCors("LocalDev");

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
