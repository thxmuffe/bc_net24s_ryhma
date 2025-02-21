using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Adds controller support to the application
// This allows the application to process API requests using controllers
builder.Services.AddControllers();

var app = builder.Build();

app.UseRouting(); // Enables request routing to the appropriate controllers

app.UseAuthorization(); // Ensures authorization policies are enforced before processing requests

app.MapControllers(); // Maps the API endpoints to their respective controller actions

app.Run(); // Starts the application and begins listening for requests
