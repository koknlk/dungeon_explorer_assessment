using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Data;
using DungeonExplorerBackend.Extensions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using DotNetEnv;

Env.Load();

SQLitePCL.Batteries.Init();

var builder = WebApplication.CreateBuilder(args);

//database
builder.Services.AddDbContext<DungeonContext>(options =>
    options.UseSqlite("Data Source=/app/data/dungeons.db"));

//services
builder.Services.AddDungeonServices();

//auth
builder.Services.AddJwtAuthentication();

//authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanCreateDungeon", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CanGetDungeon", policy => policy.RequireRole("Admin"));
});

//cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

//controller
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
        context.ModelState.ToApiResponse();
});

//swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dungeon API", Version = "v1" });
    c.AddJwtSwagger();
});

var app = builder.Build();

//dev env
if (app.Environment.IsDevelopment())
    {
    app.UseSwagger();
    app.UseSwaggerUI();
    }

//middleware
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

//routes
app.MapControllers();

//Seed data
using (var scope = app.Services.CreateScope())
    {
    var context = scope.ServiceProvider.GetRequiredService<DungeonContext>();
    await context.Database.EnsureCreatedAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    await seeder.SeedAsync();
    }

app.Run();