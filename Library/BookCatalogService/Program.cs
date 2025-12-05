using System.Text;
using BooksCatalogService.Data;
using BooksCatalogService.GrpcServices;
using BooksCatalogService.Repository;
using BooksCatalogService.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

//Config Connection String

var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPass = Environment.GetEnvironmentVariable("DB_PASSWORD");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(dbHost) && !string.IsNullOrWhiteSpace(dbPort) && !string.IsNullOrWhiteSpace(dbUser) && !string.IsNullOrWhiteSpace(dbName) &&
    !string.IsNullOrWhiteSpace(dbPass))
{
    connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPass}";
}

// Add services to the container.
builder.Services.AddDbContext<BookDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.WebHost.ConfigureKestrel(options =>
{
    // Cổng 5000: Dành riêng cho gRPC (Chạy HTTP/2 không bảo mật)
    options.ListenLocalhost(5000, o => o.Protocols = HttpProtocols.Http2);

    // Cổng 5001: Dành cho REST API & Swagger (Chạy HTTP/1)
    options.ListenLocalhost(5001, o => o.Protocols = HttpProtocols.Http1);
});


builder.Services.AddControllers();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBookRepository, BookRepository>();

// Add gRPC services
builder.Services.AddGrpc();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapControllers();
app.MapGrpcService<BookStockGrpcService>();

app.Run();