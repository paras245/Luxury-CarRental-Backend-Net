using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using CarRental_Backend_Net.Data;
using CarRental_Backend_Net.DTOs;
using CarRental_Backend_Net.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Dependency Injection
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

// 4. Swagger & API Explorer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Luxury Vehicle API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'YOUR_TOKEN ONLY' below"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// 5. Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Log.Error("Authentication failed: {Message}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Log.Information("Token validated successfully.");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddAntiforgery();

var app = builder.Build();

// 6. Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// ----------------------------------------------------------------------------------
// AUTH ENDPOINT
// ----------------------------------------------------------------------------------
app.MapPost("/login", (UserLogin request, IConfiguration config) =>
{
    if (request.UserName != "Paras" || request.Password != "123")
    {
        Log.Warning("Failed login attempt for user: {User}", request.UserName);
        return Results.Ok(ApiResponse<object>.Error("Invalid credentials", 401));
    }

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, request.UserName),
        new Claim(ClaimTypes.Role, "Admin")
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: config["Jwt:Issuer"],
        audience: config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(30),
        signingCredentials: creds
    );

    var jwt = new JwtSecurityTokenHandler().WriteToken(token);
    Log.Information("User {User} logged in successfully.", request.UserName);

    return Results.Ok(ApiResponse<object>.Ok(new { token = jwt }, "Login Successful"));
});

// ----------------------------------------------------------------------------------
// CATEGORY ENDPOINTS
// ----------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------
// CATEGORY ENDPOINTS
// ----------------------------------------------------------------------------------

// GET ALL
app.MapGet("/api/categories", async (ICategoryService service) => 
    Results.Ok(ApiResponse<object>.Ok(await service.GetAllAsync()))
).WithTags("Categories").RequireAuthorization();

// GET BY ID
app.MapGet("/api/categories/{id}", async (int id, ICategoryService service) => 
    await service.GetByIdAsync(id) is var cat && cat != null 
        ? Results.Ok(ApiResponse<object>.Ok(cat)) 
        : Results.Ok(ApiResponse<object>.NotFound("Category not found"))
).WithTags("Categories").RequireAuthorization();

// POST
app.MapPost("/api/categories", async ([FromForm] CategoryCreateDto dto, ICategoryService service) =>
{
    var cat = await service.CreateAsync(dto);
    return Results.Created($"/api/categories/{cat.Id}", ApiResponse<object>.Created(cat));
}).WithTags("Categories").RequireAuthorization().DisableAntiforgery();

// PUT
app.MapPut("/api/categories/{id}", async (int id, [FromBody] CategoryUpdateDto dto, ICategoryService service) =>
{
    var cat = await service.UpdateAsync(id, dto);
    return cat != null 
        ? Results.Ok(ApiResponse<object>.Ok(cat, "Category updated")) 
        : Results.Ok(ApiResponse<object>.NotFound("Category not found"));
}).WithTags("Categories").RequireAuthorization();

// DELETE
app.MapDelete("/api/categories/{id}", async (int id, ICategoryService service) =>
{
    return await service.DeleteAsync(id) 
        ? Results.Ok(ApiResponse<object>.Ok(null, "Category deleted")) 
        : Results.Ok(ApiResponse<object>.NotFound("Category not found"));
}).WithTags("Categories").RequireAuthorization();

// ----------------------------------------------------------------------------------
// VEHICLE ENDPOINTS
// ----------------------------------------------------------------------------------

// GET ALL
app.MapGet("/api/vehicles", async (IVehicleService service) => 
    Results.Ok(ApiResponse<object>.Ok(await service.GetAllAsync()))
).WithTags("Vehicles").RequireAuthorization();

// GET BY ID
app.MapGet("/api/vehicles/{id}", async (int id, IVehicleService service) => 
    await service.GetByIdAsync(id) is var v && v != null 
        ? Results.Ok(ApiResponse<object>.Ok(v)) 
        : Results.Ok(ApiResponse<object>.NotFound("Vehicle not found"))
).WithTags("Vehicles").RequireAuthorization();

// GET OFFERS
app.MapGet("/api/vehicles/offers", async (IVehicleService service) => 
    Results.Ok(ApiResponse<object>.Ok(await service.GetOffersAsync()))
).WithTags("Vehicles").RequireAuthorization();

// GET TOP SELLING
app.MapGet("/api/vehicles/top-selling", async (IVehicleService service) => 
    Results.Ok(ApiResponse<object>.Ok(await service.GetTopSellingAsync()))
).WithTags("Vehicles").RequireAuthorization();

// POST
app.MapPost("/api/vehicles", async ([FromForm] VehicleCreateDto dto, IVehicleService service) =>
{
    try 
    {
        var vehicle = await service.CreateAsync(dto);
        return Results.Created($"/api/vehicles/{vehicle.Id}", ApiResponse<object>.Created(vehicle));
    }
    catch (Exception ex)
    {
         return Results.Ok(ApiResponse<object>.Error(ex.Message));
    }
}).WithTags("Vehicles").RequireAuthorization().DisableAntiforgery();

// PATCH
app.MapPatch("/api/vehicles/{id}", async (int id, [FromBody] VehiclePatchDto dto, IVehicleService service) =>
{
    var v = await service.UpdateAsync(id, dto);
    return v != null 
        ? Results.Ok(ApiResponse<object>.Ok(v, "Vehicle updated")) 
        : Results.Ok(ApiResponse<object>.NotFound("Vehicle not found"));
}).WithTags("Vehicles").RequireAuthorization();

// DELETE
app.MapDelete("/api/vehicles/{id}", async (int id, IVehicleService service) =>
{
    return await service.DeleteAsync(id) 
        ? Results.Ok(ApiResponse<object>.Ok(null, "Vehicle deleted")) 
        : Results.Ok(ApiResponse<object>.NotFound("Vehicle not found"));
}).WithTags("Vehicles").RequireAuthorization();

app.Run();
