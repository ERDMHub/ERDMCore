using ERDM.Core.Interfaces;
using ERDMCore.Infrastructure.MongoDB.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;


namespace ERDMCore.Infrastructure.MongoDB.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDB(this IServiceCollection services, IConfiguration configuration)
        {
            // Register settings
            services.Configure<MongoDbSettings>(configuration.GetSection("MongoDB"));

            // Register MongoDB client
            services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                var client = new MongoClient(settings.GetClientSettings());
                return client;
            });

            // Register database
            services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(settings.DatabaseName);
            });

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, MongoUnitOfWork>();

            return services;
        }

        public static IServiceCollection AddMongoDBWithHealthChecks(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddMongoDB(configuration);

            services.AddHealthChecks()
                .AddCheck<MongoHealthCheck>("mongodb");

            return services;
        }
    }
}
