using AuctionService.Entities;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using AuctionService.Data;
using AuctionService.RequestHelpers;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using AuctionService.Consumers;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();

builder.Services.AddDbContext<AuctionDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(10);
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
        {
            h.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            h.Username(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });
        cfg.ConfigureEndpoints(context);
    });
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer(options =>
                        {
                            options.Authority = builder.Configuration["IdentityServiceUrl"];
                            options.RequireHttpsMetadata = false;
                            options.TokenValidationParameters.ValidateAudience = false;
                            options.TokenValidationParameters.NameClaimType = "username";
                        });


var app = builder.Build();


app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.MapControllers();
// app.UseRouting();


try
{
    DbInitializer.InitDb(app);
}
catch (Exception)
{
    throw;
}

app.Run();

