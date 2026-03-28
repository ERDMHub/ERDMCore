using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Net.Sockets;


namespace ERDM.Infrastructure.Health
{
    public class RabbitMQHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMQHealthCheck> _logger;

        public RabbitMQHealthCheck(IConfiguration configuration, ILogger<RabbitMQHealthCheck> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var hostName = _configuration["RabbitMQ:HostName"];
                var port = _configuration.GetValue<int>("RabbitMQ:Port", 5672);

                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(hostName, port, cancellationToken);

                var connectionFactory = new ConnectionFactory
                {
                    HostName = hostName,
                    Port = port,
                    UserName = _configuration["RabbitMQ:UserName"],
                    Password = _configuration["RabbitMQ:Password"],
                    VirtualHost = _configuration["RabbitMQ:VirtualHost"]
                };

                using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
                using var channel = await connection.CreateChannelAsync();

                return HealthCheckResult.Healthy("RabbitMQ is healthy");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ health check failed");
                return HealthCheckResult.Unhealthy("RabbitMQ is unhealthy", ex);
            }
        }
    }
}
