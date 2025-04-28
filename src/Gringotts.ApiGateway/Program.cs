using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

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

var app = builder.Build();

app.UseCors("AllowAngularDev");

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.UseMetricServer();
app.UseHttpMetrics();

app.MapControllers();
app.Run();

public partial class Program { }