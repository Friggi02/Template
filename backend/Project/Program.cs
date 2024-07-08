using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using Project.DAL;
using Project.DAL.DTOs;
using Project.DAL.Entities;
using Project.DAL.Jwt;
using Project.DAL.Permit;
using Project.DAL.Repositories;
using Project.DAL.Repositories.Permission;
using Project.DAL.Repositories.UnitOfWork;
using Project.DAL.Utils;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
ODataConventionModelBuilder modelBuilder = new();
modelBuilder.EntitySet<User>("User");
EntityTypeConfiguration<User> EntityType = modelBuilder.EntityType<User>();
EntityType.Ignore(ui => ui.PasswordHash);
EntityType.Ignore(ui => ui.RefreshToken);
modelBuilder.EnableLowerCamelCase();

string defaultCors = "defaultCors";
builder.Services.AddCors(options =>
    options.AddPolicy(name: defaultCors, policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    })
);
builder.Services.AddControllers(options =>
{
    options.InputFormatters.Insert(0, MyJPIF.GetJsonPatchInputFormatter());
}).AddNewtonsoftJson().AddOData(
    options => options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(null).AddRouteComponents(
        "odata",
        modelBuilder.GetEdmModel()));
builder.Services.AddSqlServer<ProjectDbContext>(builder.Configuration.GetConnectionString("Default"));
builder.Services.AddSingleton<JwtOptions>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();
    JwtOptions jwtOptions = serviceProvider.GetService<JwtOptions>()!;
    options.SaveToken = jwtOptions.SaveToken;
    options.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;
    options.TokenValidationParameters = jwtOptions.TokenParameters;
});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<Mapper>();
builder.Services.AddEndpointsApiExplorer();

// Add Swagger and Swagger UI to the service collection
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swagger =>
{
    // Generate the default UI of Swagger documentation
    swagger.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ASP.NET 7 Web API",
        Description = "Authentication and Authorization in ASP.NET 7 with JWT and Swagger"
    });

    // Enable authorization using Swagger (JWT)
    swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
    });
    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

WebApplication app = builder.Build();

// Use Swagger and SwaggerUI in development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ASP.NET 7 Web API v1"));
}

app.UseHttpsRedirection();
app.UseCors(defaultCors);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();