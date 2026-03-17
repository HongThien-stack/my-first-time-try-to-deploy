using Microsoft.EntityFrameworkCore;
using PromotionService.Application.Interfaces;
using PromotionService.Application.Services;
using PromotionService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Add DbContext
builder.Services.AddDbContext<PromotionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPromotionDbContext>(provider => provider.GetRequiredService<PromotionDbContext>());

// 2. Add Application Services
builder.Services.AddScoped<IPromotionEngineService, PromotionEngineService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Promotion Service API",
        Version = "v1",
        Description = "Promotion and Loyalty Program Service"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Promotion Service API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
