using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Services;

namespace DungeonExplorerBackend.Extensions
    {
    public static class ServiceCollectionExtensions
        {
        public static IServiceCollection AddDungeonServices(this IServiceCollection services)
            {
            services.AddScoped<IDataSeeder, AdminSeeder>();
            services.AddScoped<DungeonSolverService>();
            services.AddScoped<DungeonMapper>();
            services.AddScoped<IInputSanitizer, InputSanitizer>();
            services.AddScoped<IPathfindingService, AStarPathfindingService>();
            services.AddScoped<IDungeonService, DungeonService>();
            services.AddScoped<IResponseHandler, ResponseHandler>();
            services.AddSingleton<IJwtService, JwtService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IDungeonRepository, DungeonRepository>();
            return services;
            }
        }
    }