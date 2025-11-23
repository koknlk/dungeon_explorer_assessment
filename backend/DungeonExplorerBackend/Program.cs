using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Data;
using DungeonExplorerBackend.Extensions;
using DungeonExplorerBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using DotNetEnv;

Env.Load();

SQLitePCL.Batteries.Init();

var builder = WebApplication.CreateBuilder(args);

//DATABASE
builder.Services.AddDbContext<DungeonContext>(options =>
    options.UseSqlite("Data Source=/app/data/dungeons.db"));

//SERVICES
builder.Services.AddDungeonServices();

//AUTH
builder.Services.AddJwtAuthentication();

//AUTHORIZATION POLICIES
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanCreateDungeon", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CanGetDungeon", policy => policy.RequireRole("Admin"));
});

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

//CONTROLLERS
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
        context.ModelState.ToApiResponse();
});

//SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dungeon API", Version = "v1" });
    c.AddJwtSwagger();
});

var app = builder.Build();

//DEV ENV
if (app.Environment.IsDevelopment())
    {
    app.UseSwagger();
    app.UseSwaggerUI();
    }

//MIDDLEWARE
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

//ROUTES
app.MapControllers();

//SEED DATA
using (var scope = app.Services.CreateScope())
    {
    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    await seeder.SeedAsync();
    }

app.Run();