using System.Text;
using AuthApi.Data;
using AuthApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // <-- add this

var builder = WebApplication.CreateBuilder(args);

// ---------- EF Core 9 + Pomelo 9 (MySQL) ----------
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
           ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(o =>
    o.UseMySql(conn, ServerVersion.AutoDetect(conn)));

// ---------- Identity (users/roles) ----------
builder.Services
    .AddIdentityCore<ApplicationUser>(opts =>
    {
        opts.User.RequireUniqueEmail = true;
        opts.Password.RequiredLength = 6;
        opts.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ---------- JWT ----------
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt");
var key = jwt["Key"] ?? throw new InvalidOperationException("Missing Jwt:Key");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// App services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddHostedService<RoleSeeder>();

builder.Services.AddControllers();

// ---------- Swagger (with JWT support) ----------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });

    // Define the Bearer scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    // Apply it globally so all endpoints send the header once you click Authorize
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"   // <-- must match the definition name above
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Apply migrations at startup (dev convenience)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// ---------- Use Swagger ----------
app.UseSwagger();      // serves /swagger/v1/swagger.json
app.UseSwaggerUI();    // serves interactive UI at /swagger

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
// These two lines make the API serve the Client’s static web assets
app.UseBlazorFrameworkFiles();   // serves _framework from the Client project
app.UseStaticFiles();            // serves wwwroot (images, css, etc.)

app.MapControllers();
// SPA fallback to Client/index.html
app.MapFallbackToFile("index.html");
app.Run();

// ---------- Optional role seeder ----------
class RoleSeeder : IHostedService
{
    private readonly IServiceProvider _sp;
    public RoleSeeder(IServiceProvider sp) => _sp = sp;

    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "User" })
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new IdentityRole(role));
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
