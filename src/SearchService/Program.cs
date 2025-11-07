using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionServiceHttpClient>()
                .AddPolicyHandler(GetPolicy());

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

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