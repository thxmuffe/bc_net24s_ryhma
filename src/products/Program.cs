using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using AspNetWebAPI.Data;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;



var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("ProductsDb"));



var googleClientId = "490439995551-n1pm61l6p43f4uqq31rro48oimt7sj7h.apps.googleusercontent.com"; // Replace with your actual Google Client ID

// ✅ Configure authentication with Google OAuth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // 1️⃣ **Set the Authority (Who issued the token?)**
        options.Authority = "https://accounts.google.com"; // Google’s authentication server

        // 2️⃣ **Set the Audience (Who is the token for?)**
        options.Audience = googleClientId; // Must match your Google Client ID

        // 3️⃣ **Define Token Validation Rules**
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Ensure the token is from Google
            ValidIssuer = "https://accounts.google.com",

            ValidateAudience = true, // Ensure the token is for this API
            ValidAudience = googleClientId,

            ValidateLifetime = true, // Ensure the token is not expired
            ValidateIssuerSigningKey = true // Ensure the signature is valid
        };

                // 🔍 Debugging: Log token errors
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine("Unauthorized request - Token missing or invalid");
                return Task.CompletedTask;
            }
        };
    });

// ✅ Enable Authorization Middleware
builder.Services.AddAuthorization();

// ✅ Add API Controllers
builder.Services.AddControllers();

var app = builder.Build();

// ✅ Enable Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// ✅ Map API Controllers
app.MapControllers();

// ✅ Run the application
app.Run();