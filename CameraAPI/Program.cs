﻿using CameraAPI.Models;
using CameraCore.Models;
using CameraService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// Display Authorize button
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter your token",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
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
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Connect Database from SQL
builder.Services.AddDbContext<CameraAPIdbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("InternShop"));  
});
builder.Services.AddDbContext<WarehouseDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Warehouse"));
});

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

// Create JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    /*options.Authority = "https://localhost:7031";
    options.RequireHttpsMetadata = true;
    options.Audience = "api1";*/
    options.TokenValidationParameters = new TokenValidationParameters() 
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Issuer"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// Repository Pattern
builder.Services.AddDIServices(builder.Configuration);

builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
});

builder.Services.AddDistributedMemoryCache();

// Session
/*builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".CameraAPI.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});*/

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("MyPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

//app.UseSession();

app.MapControllers();


app.Run();
