using AuctionService.Entities;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using AuctionService.Data;
using AuctionService.RequestHelpers;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();

builder.Services.AddDbContext<AuctionDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();
app.UseRouting();


try
{
    DbInitializer.InitDb(app);
}
catch (Exception)
{
    throw;
}

app.Run();

