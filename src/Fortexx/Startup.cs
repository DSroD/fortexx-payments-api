using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using Microsoft.EntityFrameworkCore;

using Pomelo.EntityFrameworkCore.MySql;
using Pomelo.EntityFrameworkCore;

using Fortexx.Data;
using Fortexx.Services;

namespace Fortexx
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCors(options => {
                options.AddPolicy(
                    name: "_allowSpecificOrigins",
                    builder => {
                        builder
                            .WithOrigins(Configuration.GetSection("CorsOrigins").Get<string[]>())
                            .AllowAnyHeader()
                            .WithMethods("GET", "POST", "DELETE", "PUT");
                    }
                );
            });

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IAuthorizationService>(new AuthorizazionService(Configuration));

            var connectionString = String.Format("server={0};user={1};password={2};database={3}",
                    Configuration["Database:Server"],
                    Configuration["Database:User"],
                    Configuration["Database:Password"],
                    Configuration["Database:Database"]);

            services.AddDbContext<PaymentContext>(dbContextOptions => dbContextOptions
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                );

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddScoped<IPaymentContext>(provider => provider.GetService<PaymentContext>());

            services.AddDataDransferObjectServices();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
                    Title = "Fortexx",
                    Version = "v1",
                    Description = "API for payment processing for FortexxGaming servers",
                    Contact = new OpenApiContact {
                        Name = "Daniel Rod",
                        Email = "rodd@lab.ms.mff.cuni.cz"
                    } 
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("_allowSpecificOrigins");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseSwagger();
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "fortexx v1");
                    });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
