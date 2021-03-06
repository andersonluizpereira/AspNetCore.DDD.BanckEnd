﻿using Integration.CrossCuting.Tools.Jwt;
using Integration.Domain.Entities.Models;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using Integration.Ioc;

namespace Integration.Api.Produto
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
         
        // Configurando o serviço de documentação do Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                             new Info
                             {
                                 Title = "Integrador de produtos",
                                 Version = "v1",
                                 Description = "API REST criada com o ASP.NET Core, para integração de produtos",
                                 Contact = new Contact
                                 {
                                     Name = "Andy",
                                     Url = "https://github.com/andersonluizpereira/AspNetCore.DDD.BanckEnd"
                                 }
                             });

                string caminhoAplicacao =
                    PlatformServices.Default.Application.ApplicationBasePath;
                string nomeAplicacao =
                    PlatformServices.Default.Application.ApplicationName;
                string caminhoXmlDoc =
                    Path.Combine(caminhoAplicacao, $"{nomeAplicacao}.xml");

                c.IncludeXmlComments(caminhoXmlDoc);
            });

            var signingConfigurations = new SigningConfigurations();
            services.AddSingleton(signingConfigurations);

            var tokenConfigurations = new TokenConfigurations();
            new ConfigureFromConfigurationOptions<TokenConfigurations>(
                    Configuration.GetSection("TokenConfigurations"))
                .Configure(tokenConfigurations);
            services.AddSingleton(tokenConfigurations);


            services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearerOptions =>
            {
                var paramsValidation = bearerOptions.TokenValidationParameters;
                paramsValidation.IssuerSigningKey = signingConfigurations.Key;
                paramsValidation.ValidAudience = tokenConfigurations.Audience;
                paramsValidation.ValidIssuer = tokenConfigurations.Issuer;

                // Valida a assinatura de um token recebido
                paramsValidation.ValidateIssuerSigningKey = true;

                // Verifica se um token recebido ainda é válido
                paramsValidation.ValidateLifetime = true;

                // Tempo de tolerância para a expiração de um token (utilizado
                // caso haja problemas de sincronismo de horário entre diferentes
                // computadores envolvidos no processo de comunicação)
                paramsValidation.ClockSkew = TimeSpan.Zero;
            });

            // Ativa o uso do token como forma de autorizar o acesso
            // a recursos deste projeto
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                                   .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                                   .RequireAuthenticatedUser().Build());
            });


            services.AddMvc();
            services.AddCors();
            // .NET Native DI Abstraction
            RegisterServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json",
                                  "Integrador de produtos");
            });

         

            app.UseCors(x =>
            {
                x.AllowAnyHeader();
                x.AllowAnyMethod();
                x.AllowAnyOrigin();
            });
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddCors(
                options => options.AddPolicy("AllowCors",
                                             builder => {
                                                 builder
                                                     //.WithOrigins("http://localhost:4456") //AllowSpecificOrigins;  
                                                     //.WithOrigins("http://localhost:4456", "http://localhost:4457") //AllowMultipleOrigins;  
                                                     .AllowAnyOrigin() //AllowAllOrigins;  

                                                     //.WithMethods("GET") //AllowSpecificMethods;  
                                                     //.WithMethods("GET", "PUT") //AllowSpecificMethods;  
                                                     //.WithMethods("GET", "PUT", "POST") //AllowSpecificMethods;  
                                                     .WithMethods("GET", "PUT", "POST", "DELETE") //AllowSpecificMethods;  
                                                     //.AllowAnyMethod() //AllowAllMethods;  

                                                     //.WithHeaders("Accept", "Content-type", "Origin", "X-Custom-Header"); //AllowSpecificHeaders;  
                                                     .AllowAnyHeader(); //AllowAllHeaders;  
                                             })
            );
            // Adding dependencies from another layers (isolated from Presentation)
            SimpleInjectorBootStrapper.RegisterServices(services);
        }
    }
}
