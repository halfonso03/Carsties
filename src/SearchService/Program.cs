using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Services;
using MassTransit;
using AutoMapper;
using SearchService.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionServiceHttpClient>()
                .AddPolicyHandler(GetPolicy());
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));
builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search"));
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
        {
            h.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            h.Username(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });

        cfg.ReceiveEndpoint("search-auction-created", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, 5));
            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("search-auction-updated", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, 5));
            e.ConfigureConsumer<AuctionUpdatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("search-auction-deleted", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, 5));
            e.ConfigureConsumer<AuctionDeletedConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseHttpsRedirection();


app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.InitDb(app);

    }
    catch (System.Exception)
    {

        throw;
    }
});



app.Run();




static IAsyncPolicy<HttpResponseMessage> GetPolicy()
 => HttpPolicyExtensions.HandleTransientHttpError()
    .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));