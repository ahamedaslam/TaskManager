using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Text.Json;
using TaskManager.DBContext;
using TaskManager.Helper;
using TaskManager.Interface;
using TaskManager.InterfaceService;
using TaskManager.IRepository;
using TaskManager.IServices;
using TaskManager.Middleware;
using TaskManager.Models;
using TaskManager.Repository;
using TaskManager.Services;
using TaskManager.Utility;

var builder = WebApplication.CreateBuilder(args);


// Setup Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/TaskManager.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .CreateLogger();

// Remove default logging providers and use only Serilog
builder.Host.UseSerilog();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Manager APIs",
        Version = "v1",
        Description = "API for managing Task Manager"
    });

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                },
                Scheme = "oauth2",
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header
            },
            new string[] {}
        }
    });
});

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

//Register Services and Interfaces
builder.Services.AddScoped<TaskManagerService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITenantService,TenantService>();
builder.Services.AddScoped<CurrentUserService>();


// Register Repoitories and Interfaces
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<ITaskManagerRepo, TaskManagerRepo>();
builder.Services.AddScoped<ITenantRepository,TenantRepository>();

// Register AddHttpContextAccessor
builder.Services.AddHttpContextAccessor();



// Register DBContext with SQL Server
builder.Services.AddDbContext<AuthDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TaskManagerAuthDB")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDBContext>()
    .AddDefaultTokenProviders();


// Password Policy
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredUniqueChars = 1;
});

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})

    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
            AuthenticationType = "Jwt",
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudiences = new[] { builder.Configuration["Jwt:Audience"] },
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Custom error handling middleware
app.UseMiddleware<ExceptionHandleMiddleware>(); // Custom exception handling (top level)


app.UseStatusCodePages(async context =>        // Handles known status codes like 400/403/404
{
    var response = context.HttpContext.Response;
    response.ContentType = "application/json";

    var result = response.StatusCode switch
    {
        404 => JsonSerializer.Serialize(ResponseHelper.NotFound()),
        403 => JsonSerializer.Serialize(ResponseHelper.Unauthorized()),
        400 => JsonSerializer.Serialize(ResponseHelper.BadRequest()),
        409 => JsonSerializer.Serialize(ResponseHelper.Conflict()),
        422 => JsonSerializer.Serialize(ResponseHelper.Unprocessable()),
        _ => null
    };

    if (result != null)
    {
        await response.WriteAsync(result);
    }
});

app.UseHttpsRedirection(); // Enforces HTTPS

app.UseAuthentication(); // ? FIRST: Authenticate the user (sets the User principal)

//app.UseMiddleware<TaskManager.Middleware.CustomAuthorizationMiddleware>(); // Custom handling for 401/403

app.UseAuthorization(); // ? THEN: Apply role-based policies or [Authorize] checks

app.MapControllers(); // Route the request to the controller

app.Run();

builder.Logging.ClearProviders();
