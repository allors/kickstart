﻿// <copyright file="Startup.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Allors.Database.Server.Controllers
{
    using System.Text;
    using Adapters;
    using Domain;
    using Allors.Security;
    using Allors.Server;
    using Allors.Services;
    using Configuration;
    using Configuration.Derivations.Default;
    using JSNLog;
    using Meta;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;
    using ObjectFactory = ObjectFactory;
    using User = Domain.User;

    public class Startup
    {
        public Startup(IConfiguration configuration) => this.Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(this.Configuration);

            // Allors
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IPolicyService, PolicyService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton(new WorkspaceConfig(new Dictionary<HostString, string>
            {
                {new HostString("localhost", 5000), "Default"},
                {new HostString("server.aviation.com"), "Default"},
                {new HostString("server-test.aviation.com"), "Default"}
            }));
            services.AddScoped<IWorkspaceService, WorkspaceService>();
            services.AddScoped<ITransactionService, TransactionService>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins(
                                "http://localhost:4000", "http://localhost:4200", "http://localhost:8000",
                                "https://aviation.dipu.com", "https://aviation-test.dipu.com")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });

            services.AddDefaultIdentity<IdentityUser>()
                .AddAllorsStores();

            services.AddAuthentication(option => option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(this.Configuration.GetSection("JwtToken:Key").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                    };
                });

            services.AddResponseCaching();
            services.AddControllers();

            services.PostConfigure<ApiBehaviorOptions>(options =>
            {
                var builtInFactory = options.InvalidModelStateResponseFactory;

                options.InvalidModelStateResponseFactory = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Startup>>();

                    var problemDetails = new ValidationProblemDetails(context.ModelState);
                    var message = string.Join("; ", problemDetails.Errors.Select(v => $"{string.Join(",", v.Value)}"));
                    logger.LogError(problemDetails.Title, message);

                    return builtInFactory(context);
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IConfiguration configuration, IHostingEnvironment env, IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory)
        {
            // Allors
            var metaPopulation = new MetaBuilder().Build();
            var engine = new Engine(Rules.Create(metaPopulation));
            var objectFactory = new ObjectFactory(metaPopulation, typeof(User));
            var databaseScope = new DefaultDatabaseServices(engine, configuration, httpContextAccessor);
            var databaseBuilder = new DatabaseBuilder(databaseScope, this.Configuration, objectFactory);

            var databaseService = app.ApplicationServices.GetRequiredService<IDatabaseService>();
            databaseService.Build = () => databaseBuilder.Build();
            databaseService.Database = databaseService.Build();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors();

            var jsnlogConfiguration = new JsnlogConfiguration
            {
                defaultAjaxUrl = "allors/jsnlog.logger",
                corsAllowedOriginsRegex = ".*",
                serverSideMessageFormat = env.IsDevelopment() ?
                                            "%requestId | %url | %message" :
                                            "%requestId | %url | %userHostAddress | %userAgent | %message",
            };

            app.UseJSNLog(loggerFactory, jsnlogConfiguration);

            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions { ServeUnknownFileTypes = true });

            // app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.ConfigureExceptionHandler(env);

            app.UseResponseCaching();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "allors/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllers();
            });
        }
    }
}
