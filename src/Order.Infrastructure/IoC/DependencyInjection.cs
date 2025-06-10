using MediatR;
using FluentValidation;
using System.Reflection;
using Order.Domain.Interfaces;
using Order.Application.Services;
using Order.Application.Behaviors;
using Order.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Order.Infrastructure.Messaging;
using Order.Infrastructure.Data.Context;
using Microsoft.Extensions.Configuration;
using Order.Infrastructure.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Commands.RegisterOrder;

namespace Order.Infrastructure.IoC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<OrderDbContext>(options => options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName)));

            // Repositories
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            // Event Publisher (Mocked)
            services.AddScoped<IEventPublisher, MockEventPublisher>();

            // Application Services
            services.AddScoped<IOrderService, OrderService>();

            // AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            return services;
        }

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterOrderCommand).Assembly));
            services.AddValidatorsFromAssembly(typeof(RegisterOrderValidator).Assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
