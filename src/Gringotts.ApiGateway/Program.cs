using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.OpenApi.Models;
using Prometheus;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

[assembly: InternalsVisibleTo("Gringotts.ApiGateway.Tests")]

var builder = WebApplication.CreateBuilder(args);

var isTesting = builder.Environment.IsEnvironment("Testing");
var jwtKey = builder.Configuration["Jwt:Key"] ?? "lyowRyNSr9p2iS1r4aU2bslYFCu/Udu/cfrX7SRa3ps=";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "GringottsAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "GringottsFrontend";

// Controllers, Swagger, etc.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();

// Configure CORS (ALLOW ONLY SPECIFIC ORIGINS)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
                "http://localhost:8100",
                "http://gringottsproject.zapto.org:8100",
                "http://161.97.92.174:8100"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(t =>
    {
        t.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ApiGateway"))
         .AddAspNetCoreInstrumentation()
         .AddHttpClientInstrumentation();

        if (!isTesting)
        {
            t.AddJaegerExporter(o =>
            {
                o.AgentHost = Environment.GetEnvironmentVariable("JAEGER_AGENT_HOST") ?? "jaeger";
                o.AgentPort = int.Parse(Environment.GetEnvironmentVariable("JAEGER_AGENT_PORT") ?? "6831");
            });
        }
    });

// Swagger configuration (after authentication setup)
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.\n\nEnter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// === HTTP REQUEST PIPELINE ===

// Routing must come first
app.UseRouting();

// CORS must come AFTER Routing but BEFORE Authentication
app.UseCors("AllowFrontend");

// Swagger UI (optional - dev environment)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Prometheus Metrics
app.UseMetricServer();
app.UseHttpMetrics();

// Controllers
app.MapControllers();

app.Run();

// For integration tests
public partial class Program { }
