using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.infrastructure.DependencyInjection
{
    public static class MassTransitRegistration
    {
        public static IServiceCollection AddAppMassTransit(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<IBusRegistrationConfigurator> registerConsumers,
            Action<IRabbitMqBusFactoryConfigurator, IBusRegistrationContext> configureEndpoints)
        {
            services.AddMassTransit(x =>
            {
                // تسجيل جميع الـ Consumers القادمة من الخدمات المختلفة
                registerConsumers(x);

                // ربط مع RabbitMQ
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri(
                        $"amqp://{configuration["RabbitMQ:Username"]}:{configuration["RabbitMQ:Password"]}@{configuration["RabbitMQ:Host"]}:{configuration["RabbitMQ:Port"]}"),
                        h =>
                        {
                            h.Username(configuration["RabbitMQ:Username"]);
                            h.Password(configuration["RabbitMQ:Password"]);
                        });

                    // استدعاء الكونفيج الخاص بكل خدمة لتحديد أسماء الـ Queues
                    configureEndpoints(cfg, context);
                });
            });

            return services;
        }
    }
}
