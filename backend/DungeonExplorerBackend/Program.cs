using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Data;
using DungeonExplorerBackend.Extensions;
using DungeonExplorerBackend.Models.ApiResponseM;
using DungeonExplorerBackend.Models.AuthLayer;
using DungeonExplorerBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

DotNetEnv.Env.Load();

// Fix for SQLite in Docker
SQLitePCL.Batteries.Init();

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<DungeonContext>(options =>
    options.UseSqlite("Data Source=/app/data/dungeons.db"));

// Services
builder.Services.AddScoped<IDataSeeder, AdminSeeder>();
builder.Services.AddScoped<DungeonSolverService>();
builder.Services.AddScoped<DungeonMapper>();
builder.Services.AddScoped<IInputSanitizer, InputSanitizer>();
builder.Services.AddScoped<IPathfindingService, AStarPathfindingService>();
builder.Services.AddScoped<IDungeonService, DungeonService>();
builder.Services.AddScoped<IResponseHandler, ResponseHandler>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDungeonRepository, DungeonRepository>();

// Auth
builder.Services.AddJwtAuthentication();

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanCreateDungeon", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CanGetDungeon", policy => policy.RequireRole("Admin"));
});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") // your Aurelia dev server
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Controllers
builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var response = new ApiResponse<object>
            {
            Success = false,
            Message = "Validation failed",
            Error = new DungeonErrorResponse
                {
                Message = "Validation failed",
                Details = System.Text.Json.JsonSerializer.Serialize(errors)
                }
            };

        return new BadRequestObjectResult(response);
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Dungeon API", Version = "v1" });

    // Add JWT bearer auth
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
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

// Enable CORS
app.UseCors("AllowFrontend");

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
    {
    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    await seeder.SeedAsync();
    }

app.Run();