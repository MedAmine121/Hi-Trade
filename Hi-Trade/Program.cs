using FluentValidation;
using Hi_Trade;
using Hi_Trade.BLL.DI;
using Hi_Trade.BLL.Interfaces;
using Hi_Trade.DAL;
using Hi_Trade.DAL.DI;
using Hi_Trade.Models.Common;
using Hi_Trade.Models.Requests;
using Hi_Trade.Models.Validators;
using Hi_Trade.Services.DI;
using Hi_Trade.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true);
builder.Services.AddDbContext<HiTradeContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("HiTradeConnection")));


builder.Services.AddCors(options =>
{
    options.AddPolicy("MyAllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("*")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("JWTOptions"));
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
builder.Services.Configure<AdminSeedOptions>(builder.Configuration.GetSection("AdminSeed"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWTOptions:Issuer"],
            ValidAudience = builder.Configuration["JWTOptions:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTOptions:Secret"]!)),
            RoleClaimType = "role",
            NameClaimType = JwtRegisteredClaimNames.Sub
        };
    });
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
builder.Services
    .AddDALServices()
    .AddBLLServices()
    .RegisterValidators()
    .AddServices()
    .AddTransient<RequestLoggingMiddleware>();
builder.Services.AddHostedService<PriceBackgroundService>();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token directly in the text input below."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();
if (!EF.IsDesignTime)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var seedOptions = scope.ServiceProvider.GetRequiredService<IOptions<AdminSeedOptions>>().Value;
        var seedRequest = new AdminSeedOptions
        {
            Email = seedOptions.Email,
            Password = string.IsNullOrWhiteSpace(seedOptions.Password) && app.Environment.IsDevelopment()
                ? "Admin@123456!"
                : seedOptions.Password,
            FullName = seedOptions.FullName,
            Address = seedOptions.Address
        };
        var hiTradeBLL = scope.ServiceProvider.GetRequiredService<IHiTradeBLL>();
        await hiTradeBLL.SeedAdminAsync(seedRequest, CancellationToken.None);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        logger.LogWarning(ex, "Skipped startup admin seeding.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("MyAllowSpecificOrigins");

app.UseAuthentication();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.UseStaticFiles();
await app.RunAsync();
