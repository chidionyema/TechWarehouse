﻿using Microsoft.OpenApi.Models;
using TechMastery.MarketPlace.Application;
using TechMastery.MarketPlace.Application.Contracts;
using TechMastery.MarketPlace.Application.Services;
using TechMastery.MarketPlace.Identity;
using TechMastery.MarketPlace.Infrastructure;
using TechMastery.MarketPlace.Persistence;
using TechMastery.Messaging.Consumers;
using TechMastery.MarketPlace.Api.Middleware;
using TechMastery.MarketPlace.Api.Utility;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;

namespace TechMastery.MarketPlace.Api
{
    public static class StartupExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            // Configure services
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Open", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            builder.Services.AddHealthChecks();

            AddSwagger(builder.Services);

            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddElasticsearchClient(builder.Configuration);
            builder.Services.AddStorageProvider(builder.Configuration, StorageProviderType.AzureBlobStorage);
            builder.Services.AddPersistenceServices(builder.Configuration);
            builder.Services.AddIdentityServices(builder.Configuration);
            builder.Services.AddMessagingServices(builder.Configuration);
            builder.Services.AddStripePaymentService(builder.Configuration);
            builder.Services.AddScoped<ILoggedInUserService, LoggedInUserService>();
            builder.Services.AddHttpContextAccessor();



            builder.Services.AddControllers();

             return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            var env = app.Services.GetRequiredService<IWebHostEnvironment>();

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechMastery API");
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("Open");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCustomExceptionHandler();

            app.MapControllers();

            return app;
        }

        private static Task WriteHealthCheckResponse(HttpContext httpContext, HealthReport result)
        {
            httpContext.Response.ContentType = "application/json";
            var response = new
            {
                Status = result.Status.ToString(),
                Checks = result.Entries.Select(entry => new
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description
                })
            };
            return httpContext.Response.WriteAsync(JsonConvert.SerializeObject(response, Formatting.Indented));
        }

        private static void AddSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "TechMastery MarketPlace API",
                });

                c.OperationFilter<FileResultContentTypeOperationFilter>();
            });
        }
    }
}
