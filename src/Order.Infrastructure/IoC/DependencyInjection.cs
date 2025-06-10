using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Behaviors;
using Order.Application.Commands.RegisterOrder;
using Order.Application.Interfaces;
using Order.Application.Services;
using Order.Domain.Interfaces;
using Order.Infrastructure.Data.Context;
using Order.Infrastructure.Data.Repositories;
using Order.Infrastructure.Messaging;
using System.Reflection;

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
