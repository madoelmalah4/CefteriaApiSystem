using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CafeteriaProject.Models; // Your User model namespace
using Microsoft.EntityFrameworkCore;
using CafeteriaProject.Models.Data;
using CafeteriaProject.Services;
using Microsoft.OpenApi.Models; // <-- IMPORTANT: Add this namespace for Swagger

var builder = WebApplication.CreateBuilder(args);

// --- JWT Settings Retrieval ---
// Get JWT settings directly from configuration
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
// var jwtDurationInMinutes = int.Parse(builder.Configuration["Jwt:DurationInMinutes"] ?? "60"); // Not directly used here, but good to have

if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT:Key is not configured in appsettings.json. This is required for security.");
}
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => // <-- Modify AddSwaggerGen to configure security
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CafeteriaProject API", Version = "v1" });

    // Define the security scheme for JWT Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer", // The name of the HTTP Authorization scheme to be used in the Authorization header.
        BearerFormat = "JWT", // The format of the bearer token that is used.
        In = ParameterLocation.Header, // Where the parameter is located (in the header)
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\nEnter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>() // This means the 'Bearer' scheme applies to all operations
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // or .WithOrigins("https://yourfrontend.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("hostConn")));

builder.Services.AddScoped<AuthService>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = signingKey,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();