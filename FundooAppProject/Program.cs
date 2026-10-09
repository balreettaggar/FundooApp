using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity; // gives us Password Hasher
using Microsoft.EntityFrameworkCore; // gives us DbContext, DbSet, UseNpsql
using Microsoft.IdentityModel.Tokens; // provides us SymmetricSecurityKey, TokenValidationParameters
using Microsoft.OpenApi.Models; // These are used to tell Swagger: "This API uses Bearer JWT authentication."
using RepositoryLayer.Context;
using RepositoryLayer.Entity; //gives us UserEntity which is needed for PasswordHasher<UserEntity>
using RepositoryLayer.Interface;
using RepositoryLayer.Service;
using System.Text; // for later UTF-8 encoding


var builder = WebApplication.CreateBuilder(args); // we assemble our application 
//We're telling ASP.NET Core:
//I want an application.
//Here's its configuration.
//Here's what services it needs.
//Here's how authentication works.
//Here's how database access works.
// it returns us WebApplicationBuilder object, args stands for command line arguments if passed any
// CreateBuilder(args) passes those arguments into ASP.NET Core's configuration/hosting system.

// builder contains 
//builder
// ├── Services
// ├── Configuration
// ├── Environment
// ├── Logging
// └── Host/Web server configuration , mostly we use Services and Configuration

builder.Services.AddControllers(); // this application uses controller based web apis

//builder.Services is essentially the application's service registration collection.
//You're telling ASP.NET Core:
//"These are the components my application needs. Please know how to create them."

builder.Services.AddEndpointsApiExplorer(); // Expose the controller endpoints

// AddControllers()
//      ↓
//Register / configure controller infrastructure

//MapControllers()
//      ↓
//Map controller endpoints into routing

builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUserBL , UserBL>(); // dependency injection
//IUserBL requested
//      ↓
//DI container 
//      ↓
//UserBL object
//      ↓
//pass it to FundooController
// interface is used for acheiving abstraction
//Controller
//    │
//    │ "I need something that follows IUserBL"
//    ▼
//DI Container
//    │
//    │ "I'll provide UserBL"
//    ▼
//UserBL   it follows loose coupling

builder.Services.AddScoped<IUserRL , UserRL>();
builder.Services.AddScoped<JwtService>(); // No interface here
builder.Services.AddScoped<PasswordHasher<UserEntity>>();
builder.Services.AddScoped<EmailClient>();
builder.Services.AddScoped<HttpClient>();

// Add Scoped is one of the 3 lifetimes transient - new instance when requested, Scoped - new instance per HTTP request and
// Singleton - one instance per application's lifetime

builder.Services.AddDbContext<FundooContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("FundooConnection"));
});

// "My application uses EF Core, and FundooContext is my database context." 
// EF Core is ORM (Object Relational Mapper) used to map c# code to database (EF core is
// framework), and DB Context is the concrete implememtation of it

//UserRL
// ↓
//asks DI for FundooContext (FundooContext extends DB Context)
// ↓
//DI creates/provides it

// builder.Configuration retreives information from appsettings.json
//appsettings.Development.json
//Environment variables
// Get Connection String retrives the value from connection string from app settings.json 

// EF Core is not itself a PostgreSQL driver.

// EF Core
//+
//Npgsql EF Core provider
//  ↓
//PostgreSQL support

//UseNpgsql() tells EF Core:
//"Use PostgreSQL as the database provider.", Alternatives UseSqlServer() is used for SQL Server, UseSqlLite() is used for SQL ite


// this says "My application uses an authentication system, and the default authentication mechanism is JWT Bearer."
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]!
                ) // this converts JWT signing key from string to bytes
            )
        };
    });

//"Bearer" is simply a hardcoded string literal inside your HTTP Authorization request header.
//http
//Authorization: Bearer<your_actual_token_here>
//Use code with caution.

//The Breakdown

//• Authorization: The standard HTTP header key for security.
//• Bearer: The Auth Scheme. It signals to the API gateway or backend server how it should parse the rest of the string.
// (Other common schemes include Basic, Digest, or API-Key).
//• <token>: The actual cryptographic credential (like a JWT) containing the access permissions.


builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();


//app.UseHttpsRedirection();

// Map Controllers
app.MapControllers();

app.Run();