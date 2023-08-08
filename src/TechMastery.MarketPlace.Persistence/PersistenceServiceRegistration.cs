﻿using TechMastery.MarketPlace.Application.Contracts.Persistence;
using TechMastery.MarketPlace.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TechMastery.MarketPlace.Application.Contracts;
using TechMastery.MarketPlace.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TechMastery.MarketPlace.Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Replace the connection string with your PostgreSQL connection string
                options.UseNpgsql(configuration.GetConnectionString("GloboTicketTicketManagementConnectionString"));
            });

            services.AddScoped(typeof(IAsyncRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICartItemRepository, CartItemRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
            services.AddScoped<ILoggedInUserService, LoggedInUserService>();

            return services;
        }
    }
}
