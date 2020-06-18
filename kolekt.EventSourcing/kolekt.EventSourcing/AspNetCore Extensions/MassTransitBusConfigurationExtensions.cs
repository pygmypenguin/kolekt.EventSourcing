using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Linq;
using MassTransit;
using kolekt.EventSourcing.Aggregates;
using kolekt.EventSourcing.Providers;
using kolekt.EventSourcing.Queries;
using kolekt.EventSourcing.Consumers;
using MassTransit.MultiBus;

namespace kolekt.EventSourcing.Extensions
{
    public static class MassTransitBusConfigurationExtensions
    {
        public static IServiceCollection AddCommandValidator<TValidator>(this IServiceCollection services) where TValidator : ICommandValidator
        {
            services.AddScoped(typeof(ICommandValidator), typeof(TValidator));
            return services;
        }

        public static IServiceCollection ConfigureMessageServicesWithRabbitMQ(this IServiceCollection services, RabbitMqHostOptions rabbitMqHostOptions, string assemblyName = null, bool includeHealthChecks = false)
        {
            var asm = string.IsNullOrEmpty(assemblyName) ? Assembly.GetCallingAssembly() : Assembly.Load(assemblyName);

            return services.ConfigureMessageServicesWithRabbitMQ(rabbitMqHostOptions, asm, includeHealthChecks);
        }

        public static IServiceCollection ConfigureMessageServicesWithRabbitMQ(this IServiceCollection services, RabbitMqHostOptions rabbitMqHostOptions, Assembly assembly, bool includeHealthChecks = false)
        {
            if (assembly == null)
            {
                throw new NullReferenceException($"Parameeter cannot be null: {nameof(assembly)}");
            }

            if (string.IsNullOrEmpty(rabbitMqHostOptions.Url) || rabbitMqHostOptions.Url.StartsWith("rabbitmq://") == false)
            {
                throw new ArgumentException($"Parameter {nameof(rabbitMqHostOptions)} must contain a valid URL (rabbitmq://hostname)");
            }

            if (includeHealthChecks)
            {
                services.AddHealthChecks();
            }

            services.AddMemoryCache();

            services.AddScoped(typeof(IAggregateRepository<>), typeof(AggregateRepository<>));
            services.AddScoped(typeof(IEventStore), typeof(EventStore));
            services.AddScoped(typeof(IMessageBus), typeof(MessageBus));
            services.AddHostedService<BusService>();

            services.AddMassTransit(options =>
            {
                options.AddConsumersFromNamespaceContaining(assembly.ExportedTypes.First());
                options.AddBus(context => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    if (includeHealthChecks)
                    {
                        cfg.UseHealthCheck(context);
                    }
                    cfg.Host(rabbitMqHostOptions.Url, host =>
                    {
                        host.Username(string.IsNullOrWhiteSpace(rabbitMqHostOptions.Username) ? "guest" : rabbitMqHostOptions.Username);
                        host.Password(string.IsNullOrWhiteSpace(rabbitMqHostOptions.Password) ? "guest" : rabbitMqHostOptions.Password);
                    });
                    cfg.ConfigureEndpoints(context);
                }));
            });

            services.AddMassTransitHostedService();

            var queryHandlers = assembly.GetTypes()
                .Where(a => a.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)))
                .Where(a => a.IsAbstract == false && a.IsInterface == false);

            foreach (var handler in queryHandlers)
            {
                var registrationType = handler.GetInterfaces().Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));
                services.AddScoped(registrationType, handler);
            }

            return services;
        }
    }
}
