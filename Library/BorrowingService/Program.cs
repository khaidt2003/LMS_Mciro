using System.Net;
using System.Text;
using BorrowingService.AsyncDataServices;
using BorrowingService.Data;
using BorrowingService.GrpcService;
using BorrowingService.QueueMessageService;
using BorrowingService.Repository;
using BorrowingService.Service;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
Env.Load();
var builder = WebApplication.CreateBuilder(args);

// 1. CẤU HÌNH CỔNG (PORTS)
// Tránh trùng với BookService (5000/5001)


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Config Connection String

var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPass = Environment.GetEnvironmentVariable("DB_PASSWORD");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine("avc");
if (!string.IsNullOrWhiteSpace(dbHost) && !string.IsNullOrWhiteSpace(dbPort) && !string.IsNullOrWhiteSpace(dbUser) && !string.IsNullOrWhiteSpace(dbName) &&
    !string.IsNullOrWhiteSpace(dbPass))
{
    connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username=postgres;Password={dbPass}";
}

// Add services to the container.
builder.Services.AddDbContext<BorrowingDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// 3. RABBITMQ (Async)
builder.Services.AddSingleton<IMessageProducer, MessageProducer>();

// 4. GRPC CLIENT (Sync)
builder.Services.AddScoped<IBookGrpcClient, BookGrpcClient>();

// 5. SERVICES & REPOSITORIES
builder.Services.AddScoped<IBorrowingRepository, BorrowingRepository>();
builder.Services.AddScoped<IBorrowingService, BorrowingService.Service.BorrowingService>();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});
// 6. AUTHENTICATION (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration["APP_SETTINGS_TOKEN"]!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Tắt HTTPS redirect để tránh lỗi SSL local

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();