using TODOAPI.Models;
using TODOAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using TodoApi.DatabaseSettings;
using TodoApi.Services;
using TodoApi.Models;
using MailKit;

var builder = WebApplication.CreateBuilder(args);





builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// builder.Services.AddMongo(builder.Configuration);
// builder.Services.AddMongoService<User>("users");
var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

builder.Services.Configure<ReservationDBSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<ReservationServices>();
builder.Services.AddScoped<UserServices>();

var jwtKey = configuration["JwtSettings:Key"];

if (string.IsNullOrEmpty(jwtKey))
{
    // Handle the case where the key is missing or empty
    throw new ApplicationException("JWT Key is missing or empty in configuration.");
}

Console.WriteLine($"JWT Key from configuration: {jwtKey}");

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["JwtSettings:Issuer"],
        ValidAudience = configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
