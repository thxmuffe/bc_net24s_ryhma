using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Todo.Models;
using todo;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

// Käytä Serilog nugettia konfiguroidaksesi
// Serilogin käyttämään appsettings.json-tiedostosta
// luettua konfiguraatiota
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();


// Use azure database
builder.Services.Configure<AzureAdOptions>(
    builder.Configuration.GetSection("AzureAd"));

builder.Services.AddDbContext<TodoContext>((sp, options) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var azureAd = sp.GetRequiredService<IOptions<AzureAdOptions>>().Value;
    var connectionString = config.GetConnectionString("SqlServer");

    var credential = new ClientSecretCredential(
        azureAd.TenantId,
        azureAd.ClientId,
        azureAd.ClientSecret);

    var token = credential.GetToken(
        new TokenRequestContext(new[] { "https://database.windows.net/.default" }));

    var conn = new SqlConnection(connectionString)
    {
        AccessToken = token.Token
    };

    options.UseSqlServer(conn);
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Create tables to database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoContext>();
    db.Database.Migrate();
}


logger.Warning("HEI!");

Log.Information("Program started!");

Log.Error("Example error");

app.Run();

