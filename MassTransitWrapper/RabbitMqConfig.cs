using Microsoft.Extensions.Configuration;

namespace MassTransitWrapper
{
    public class RabbitMqConfig
    {
        private readonly IConfiguration Configuration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public RabbitMqConfig(IConfiguration configuration) { Configuration = configuration; }

        public string RabbitMqUri => Configuration["BusConnection"];
        public string RabbitMqUserName => Configuration["BusUserName"];
        public string RabbitMqPassword => Configuration["BusPassword"];
        public string RabbitMqVirtualHost => Configuration["VirtualHost"];
    }
}