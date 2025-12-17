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
using TaskManager.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

#region ================== Serilog Configuration ==================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Reads from appsettings.json
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();
builder.Logging.ClearProviders();
#endregion

#region ================== Env Variables Config ==================
DotNetEnv.Env.Load(); // Loads values from .env
builder.Configuration.AddEnvironmentVariables();

#endregion

#region ================== JWT Auth in Swagger ==================

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Manager APIs",
        Version = "v1",
        Description = "API for managing Task Manager"
    });

    // JWT Auth in Swagger
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT"
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
            Array.Empty<string>()
        }
    });
});
#endregion

#region ================== CONFIG REDIS ==================

var redisHost = builder.Configuration["REDIS_HOST"];
var redisPort = builder.Configuration["REDIS_PORT"];

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = $"{redisHost}:{redisPort}";
    options.InstanceName = "TaskManager_";
});

#endregion


#region ================== DI   Services & Repositories Registrations ==================

// AutoMapper
//builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// App Services
builder.Services.AddScoped<TaskManagerService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IAIChatService,AIChatService>();
builder.Services.AddScoped<IEmailService,EmailService>();
builder.Services.AddScoped<CurrentUserService>();

//register http client 
builder.Services.AddHttpClient();

// Repository Services
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<ITaskManagerRepo, TaskManagerRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Context
builder.Services.AddHttpContextAccessor();
#endregion

#region ================== DBContext & Identity ==================

builder.Services.AddDbContext<AuthDBContext>(options =>
    options.UseSqlServer(builder.Configuration["DB_CONNECTION_STRING"]
    ));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDBContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredUniqueChars = 1;
});
#endregion

#region ================== JWT Authentication Config==================

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
        ValidIssuer = builder.Configuration["JWT_ISSUER"],
        ValidAudiences = new[] { builder.Configuration["JWT_AUDIENCE"] },
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT_SECRET"]))
    };

    // Customize 401 Unauthorized Response
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse(); // Skip default behavior
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var response = ResponseHelper.Unauthorized();
            var json = JsonSerializer.Serialize(response);

            return context.Response.WriteAsync(json);
        }
    };
});
#endregion

#region ================== Configure CORS ==================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(
                    "http://localhost:4200",
                    "https://localhost:7208"  
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

#endregion

#region ================== App Build & Middleware ==================

var app = builder.Build();

// Use Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Custom Global Exception Handler Middleware
app.UseMiddleware<ExceptionHandleMiddleware>();

// Handle known HTTP status codes (e.g. 400, 403, 404)
app.UseStatusCodePages(async context =>
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
        await response.WriteAsync(result);
});



// Request pipeline
//app.UseHttpsRedirection();

app.UseCors("AllowFrontend");
app.UseAuthentication(); // Validates JWT
app.UseAuthorization();  // Applies role policies, [Authorize]

// Maps controller routes
app.MapControllers();

// Run the application
app.Run();

// Optional: Remove all default logging providers
#endregion





