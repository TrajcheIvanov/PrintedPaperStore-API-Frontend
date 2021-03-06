using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PrintedPaperStore.Custom;
using PrintedPaperStore.Data;
using PrintedPaperStore.Data.Interfaces;
using PrintedPaperStore.Services;
using PrintedPaperStore.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace PrintedPaperStore
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
            services.AddDbContext<PrintedPaperStoreDbContext>(options => 
            { 
                options.UseSqlServer(Configuration.GetConnectionString("PrintedPaperStoreDB"));
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
                options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
            }).AddApiKeySupport(options => { });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyMethod()
                           .AllowAnyOrigin()
                           .AllowAnyHeader();
                });
            });

            //essential swagger configuration
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    //additional swagger configuraiton
                    Version = "v1",
                    Title = "Printed Papaer Store API",
                    Description = "Web API for working with printed papaer store",
                    Contact = new OpenApiContact
                    {
                        Name = "Trajche Ivanov",
                        Email = "trajcheivanov@hotmail.com",
                    }
                });

                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme()
                {
                    Description = "ApiKey Authorization header. Format: \"ApiKey {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "ApiKey"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme, Id = "ApiKey"
                            },
                            Scheme = "ApiKey",
                            Name = "ApiKey",
                            In = ParameterLocation.Header
                        },
                        new List<string> { }
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddControllers();
            services.AddTransient<IBooksService, BooksService>();
            services.AddTransient<IOrdersService, OrdersService>();


            services.AddTransient<IBooksRepository, BooksRepository>();
            services.AddTransient<IOrdersRepository, OrdersRepository>();
            services.AddTransient<IApplicationsRepository, ApplicationsRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("AllowAll");

            app.UseHttpsRedirection();
            //after this you need to make wwwroot folder
            app.UseStaticFiles();
            //essential swagger configuration
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                //this sets swagger as starting route
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
