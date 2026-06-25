using System.Reflection;
using System.Globalization;
using System.Threading.RateLimiting;
using System.Text;
using Application;
using Application.Common;
using Application.Common.Abstractions;
using Contracts.DataAccess;
using DataAccess.Context;
using DataAccess.Repository;
using DTO.DataAccess.DTO;
using DTO.DataAccess.Mapper;
using Web.Filters;
using Web.Services;
using Web.Swagger;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);
var tokenLifetimeConfiguration = new Dictionary<string, string?>();
if (Environment.GetEnvironmentVariable("ACCESS_TOKEN_SECONDS") is { } accessTokenSeconds)
{
    tokenLifetimeConfiguration[$"{TokenLifetimeOptions.SectionName}:AccessTokenSeconds"] = accessTokenSeconds;
}

if (Environment.GetEnvironmentVariable("REFRESH_TOKEN_SECONDS") is { } refreshTokenSeconds)
{
    tokenLifetimeConfiguration[$"{TokenLifetimeOptions.SectionName}:RefreshTokenSeconds"] = refreshTokenSeconds;
}

if (tokenLifetimeConfiguration.Count > 0)
{
    builder.Configuration.AddInMemoryCollection(tokenLifetimeConfiguration);
}

builder.Host.UseDefaultServiceProvider((context, options) =>
{
    options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
    options.ValidateOnBuild = false;
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AppUserEntity>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<AppRoleEntity>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddApplication();
builder.Services.Configure<TokenLifetimeOptions>(
    builder.Configuration.GetSection(TokenLifetimeOptions.SectionName));
builder.Services.AddScoped<AppUserEntityMapper>();
builder.Services.AddScoped<AppRoleEntityMapper>();
builder.Services.AddScoped<AppUserClientEntityMapper>();
builder.Services.AddScoped<ClientEntityMapper>();
builder.Services.AddScoped<SecurityEventEntityMapper>();
builder.Services.AddScoped<IAppUserRepository, AppUserRepository>();
builder.Services.AddScoped<IAppRoleRepository, AppRoleRepository>();
builder.Services.AddScoped<IAppUserClientRepository, AppUserClientRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<ISecurityEventRepository, SecurityEventRepository>();
builder.Services.AddScoped<IEmailService, LoggingEmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ISecurityEventService, SecurityEventService>();
builder.Services.AddScoped<MainClientResolver>();
builder.Services.AddScoped<AdminProvisioningService>();
builder.Services.AddScoped<ClientAuthenticationFilter>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddLocalization();
builder.Services.AddControllers();
builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(Web.Resources));
    });
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminArea");
}).AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(Web.Resources));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminArea", policy =>
    {
        policy.AuthenticationSchemes.Add(IdentityConstants.ApplicationScheme);
        policy.RequireRole("Admin");
    });
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { error = "TooManyRequests" },
            cancellationToken);
    };

    options.AddPolicy("default", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromSeconds(60),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.AddPolicy("auth-strict", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromSeconds(60),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromSeconds(60),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

var authenticationBuilder = builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSection = builder.Configuration.GetSection("Jwt");
        var signingKey = jwtSection["SigningKey"] ?? jwtSection["Key"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = !string.IsNullOrWhiteSpace(jwtSection["Issuer"]),
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = !string.IsNullOrWhiteSpace(jwtSection["Audience"]),
            ValidAudience = jwtSection["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = !string.IsNullOrWhiteSpace(signingKey),
            IssuerSigningKey = string.IsNullOrWhiteSpace(signingKey)
                ? null
                : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
        };
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = builder.Configuration["Authentication:AdminCookie:Name"] ?? "AuthService.Admin";
        options.LoginPath = builder.Configuration["Authentication:AdminCookie:LoginPath"] ?? "/admin/login";
        options.AccessDeniedPath = builder.Configuration["Authentication:AdminCookie:AccessDeniedPath"] ?? "/admin/access-denied";
    });

var googleClientId = builder.Configuration["Authentication:Google:ClientId"] ?? Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authenticationBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
    });
}

var microsoftClientId = builder.Configuration["Authentication:Microsoft:ClientId"] ?? Environment.GetEnvironmentVariable("MICROSOFT_CLIENT_ID");
var microsoftClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"] ?? Environment.GetEnvironmentVariable("MICROSOFT_CLIENT_SECRET");
if (!string.IsNullOrWhiteSpace(microsoftClientId) && !string.IsNullOrWhiteSpace(microsoftClientSecret))
{
    authenticationBuilder.AddMicrosoftAccount(options =>
    {
        options.ClientId = microsoftClientId;
        options.ClientSecret = microsoftClientSecret;
    });
}

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth Service API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });
    options.AddSecurityDefinition("ClientCredentials", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-Client-Id",
        Description = "Client application credentials. Also requires X-Client-Secret header."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document, null),
            []
        }
    });
    options.OperationFilter<ClientCredentialsOperationFilter>();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

var supportedCultures = new[]
{
    new CultureInfo("en-US"),
    new CultureInfo("et-EE"),
    new CultureInfo("fi-FI")
};

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth Service API v1");
});

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();
