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

//Request–Response Pipeline
//Request → Middleware → Controller
//Response ← Middleware ← Controller
//Order matters — middleware executes in the sequence it’s registered.

#region ================== Serilog Configuration ==================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Reads from appsettings.json
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();
builder.Logging.ClearProviders();
#endregion

#region ================== Env Variables Config ==================
if (builder.Environment.IsDevelopment())
{
    DotNetEnv.Env.Load();
}
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
        Title = "MultiTenant APIs",
        Version = "v1",
        Description = "APIs for managing Task Manager"
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

if (!string.IsNullOrEmpty(redisHost))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = $"{redisHost}:{redisPort}";
        options.InstanceName = "TaskManager_";
    });
}


#endregion

#region ========= DI Services & Repositories Registrations =========

// App Services
builder.Services.AddScoped<TaskManagerService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IAIChatService,AIChatService>();
builder.Services.AddScoped<IEmailService,EmailService>();
builder.Services.AddScoped<CurrentUserService>();

//to communicate with APIs over HTTP/HTTPS.
//register http client 
builder.Services.AddHttpClient();

//Redis Service
builder.Services.AddScoped<RedisService>();

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

//var dbProvider = builder.Configuration["DatabaseProvider"];
string conn = builder.Configuration["DB_PROD"];

if (string.IsNullOrWhiteSpace(conn))
    throw new Exception("DB_PROD is missing in App Service configuration!");


builder.Services.AddDbContext<AuthDBContext>(options =>
    options.UseSqlServer(conn));

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

#region ================== JWT Bearer Authentication Config ==================

//It tells ASP.NET how to validate incoming JWTs (issuer, audience, lifetime, signing key, and role claim type).
//it also customizes the 401 response body so clients get a JSON error instead of the default WWW-Authenticate header/html.
builder.Services.AddAuthentication(options =>
{
    //Use JWT Bearer Handler for authentication.
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    //Registers JwtBearerHandler.
    options.TokenValidationParameters = new TokenValidationParameters
    {
//1.  User logs in (POST /api/auth/login) with username/password.
//2.  Server validates credentials and issues a JWT that includes issuer, audience, expiry, and a role claim.
//3.  Client calls a protected endpoint (e.g., GET /api/tasks) and sets Authorization: Bearer<token>.
//4.  JwtBearer middleware validates the token using the configured TokenValidationParameters.If valid, request proceeds; if not, the custom OnChallenge returns a JSON 401 payload.

        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
        //ClaimsPrincipal is the authenticated user
        //it contains userdi, username, roles, and other claims from the JWT. 
        AuthenticationType = "Jwt",
        ValidateIssuer = true, //Compares iss claim with JWT_ISSUER
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
    options.AddPolicy("AllowClientFrom",
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

#region ================== Custom Global Exception Handler Middleware (e.g. 400, 403, 404) ==================

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

#endregion

#region ================== Request Middleware Pipeline ==================

//Registers Authentication Middleware

//middleware order matters
app.UseCors("AllowClientFrom");
app.UseAuthentication(); // Validates JWT
app.UseAuthorization();  // Reads [Authorize] attributes //Evaluates: Is user authenticated? Does user have required roles/claims?
app.MapControllers();// Maps controller routes
app.Run();// Run the application

#endregion









