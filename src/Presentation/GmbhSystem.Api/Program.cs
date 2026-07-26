using GmbhSystem.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GmbhSystem.Api.Middlewares;
using GmbhSystem.Application.Interfaces;
using GmbhSystem.Application.Services;
using GmbhSystem.Infrastructure.Services;
using GmbhSystem.Persistence.Authentication;
using GmbhSystem.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPersistence(builder.Configuration);

var jwtKey = builder.Configuration["Jwt:Key"] ?? builder.Configuration["Jwt:Secret"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? throw new InvalidOperationException("JWT Key is not configured in appsettings.json")))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new Microsoft.OpenApi.OpenApiComponents();
        
        document.Components.SecuritySchemes = new Dictionary<string, Microsoft.OpenApi.IOpenApiSecurityScheme>
        {
            ["Bearer"] = new Microsoft.OpenApi.OpenApiSecurityScheme
            {
                Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            }
        };

        document.Security = new List<Microsoft.OpenApi.OpenApiSecurityRequirement>
        {
            new Microsoft.OpenApi.OpenApiSecurityRequirement
            {
                [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = []
            }
        };

        return Task.CompletedTask;
    });
});

builder.Services.AddScoped<IMediaService, CloudflareR2Service>();
builder.Services.AddScoped<IContentRepository, ContentRepository>(); 
builder.Services.AddScoped<IUserRepository, UserRepository>(); 
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();

var app = builder.Build();

try
{
    await GmbhSystem.Persistence.DbInitializer.SeedAsync(app.Services);
    Console.WriteLine("Database seeded successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "GmbhSystem API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.Run();