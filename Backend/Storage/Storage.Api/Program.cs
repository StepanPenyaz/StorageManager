using Microsoft.EntityFrameworkCore;
using Storage.Data.Contexts;
using Storage.Data.Interfaces;
using Storage.Data.Repositories;
using Storage.Services.Interfaces;
using Storage.Services.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<StorageContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StorageDatabase")));

builder.Services.AddScoped<IStorageRepository, StorageRepository>();
builder.Services.AddScoped<IStorageViewService, StorageViewService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
