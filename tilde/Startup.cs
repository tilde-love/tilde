// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using Tilde.Core;
using Tilde.Core.Projects;
using Tilde.Core.Templates;
using Tilde.Core.Work;
using Tilde.Host;
using Tilde.Host.Hubs.Client;
using Tilde.Host.Hubs.Module;

namespace Tilde
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // ReSharper disable once UnusedMember.Global
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            IApplicationLifetime appLifetime,
            TemplateSources templateSources)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app .UseDefaultFiles()
                .UseStaticFiles()
                //.UseAuthentication()
                .UseSwagger()
                .UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/1.0/swagger.json", "Tilde Love API"); })
                .UseMvcWithDefaultRoute()
                .UseCors("CorsPolicy")
                .UseSignalR(
                    routes =>
                    {
                        routes.MapHub<ClientHub>("/api/notify");
                        routes.MapHub<ModuleHub>("/api/module");
                    }
                );

//            var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
//
//            app.ApplicationServices.GetService<IRuntime>()
//                    .ServerUri = new Uri(serverAddressesFeature.Addresses.FirstOrDefault() ?? "http://localhost:5678", UriKind.RelativeOrAbsolute);
//            

            appLifetime.ApplicationStopping.Register(
                () =>
                {
                    // app.ApplicationServices.GetService<Boss>().Dispose();
                    templateSources.Write();
                }
            );
            
            PathString apiPath = new PathString("/api");

            app.Run(
                async context =>
                {
                    if (context.Request.Path.StartsWithSegments(apiPath, StringComparison.InvariantCulture))
                    {
                        context.Response.StatusCode = 404;
                        return;
                    }

                    context.Response.ContentType = "text/html";
                    await context.Response.SendFileAsync(Path.Combine(env.WebRootPath, "index.html"));
                }
            );
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // PluginsConfig pluginsConfig = Configuration.GetSection("Plugins").Get<PluginsConfig>();

            services.AddCors(
                options =>
                {
                    options.AddPolicy(
                        "CorsPolicy",
                        builder =>
                            builder
                                .AllowAnyMethod()
                                .AllowAnyHeader()
                                .AllowCredentials()
                                //.WithOrigins("http://localhost:4200")
                                //.WithOrigins("http://localhost:5678")
                                .AllowAnyOrigin()
                    );
                }
            );

            services
                .AddSignalR()
                .AddJsonProtocol(
                    options =>
                    {
                        // Don't alter my data in transit please 
                        options.PayloadSerializerSettings.ContractResolver = new DefaultContractResolver();
                    }
                );

            services.AddMvc();

            services.AddSwaggerGen(
                c =>
                {
                    c.SwaggerDoc("1.0", new Info {Title = "Tilde Love", Version = "1.0"});
                    //c.AddSecurityDefinition("Bearer", new ApiKeyScheme() { In = "header", Description = "Please insert JWT with Bearer into field", Name = "Authorization", ItemTypes = "apiKey" });
                }
            );

            Uri moduleConnectionUri = new Uri(
                Configuration.GetValue($"Tilde.ServerUri", "http://localhost:5678/")
                    .Replace("://0.0.0.0/", "://localhost/")
                    .Replace("://0.0.0.0:", "://localhost:")
                    .Replace("://*/", "://localhost/")
                    .Replace("://*:", "://localhost:"),
                UriKind.Absolute
            );

            string projectsRoot = Configuration.GetValue($"Tilde.ProjectFolder", "./");
            string wwwRoot = Configuration.GetValue($"Tilde.WwwRoot", "./wwwroot");
            string templatesRoot = Configuration.GetValue($"Tilde.Templates", "./templates");

            DirectoryInfo projectsRootInfo = new DirectoryInfo(projectsRoot);
            DirectoryInfo wwwRootInfo = new DirectoryInfo(wwwRoot);
            DirectoryInfo templatesRootInfo = new DirectoryInfo(templatesRoot);

            Console.WriteLine($"Project root path: {projectsRootInfo.FullName}");
            Console.WriteLine($"Templates root path: {templatesRootInfo.FullName}");
            Console.WriteLine($"WWW root path: {wwwRootInfo.FullName}");
            Console.WriteLine($"Module connection uri: {moduleConnectionUri}");

            TemplateSources templateSources = new TemplateSources(templatesRootInfo); 
            
            //templateSources.Add(new Uri("file:///D:/code/tilde-love/packing-test/index.json", UriKind.RelativeOrAbsolute));
            
            templateSources.Cache();
            
            services.AddSingleton(new ProjectManager(projectsRoot));
            services.AddSingleton(templateSources);
            //services.AddSingleton<IRuntime>(new DotnetRuntime(moduleConnectionUri));
            services.AddSingleton<Boss>(new Boss());
            
            services.AddHostedService<BossService>();
            services.AddHostedService<ClientService>();
            services.AddHostedService<ModuleService>();
        }
    }
}