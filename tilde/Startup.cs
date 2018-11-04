// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using Tilde.Core;
using Tilde.Host;
using Tilde.Core.Projects;
using Tilde.Runtime.Dotnet;

namespace tilde
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                //.UseAuthentication()
                .UseSwagger()
                .UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/1.0/swagger.json", "Zero Core API"); })
                .UseMvcWithDefaultRoute()
                .UseCors("CorsPolicy")
                .UseSignalR(
                    routes =>
                    {
                        routes.MapHub<NotifyHub>("/api/notify");
                        routes.MapHub<ModuleHub>("/api/module");
                    }
                );

            appLifetime.ApplicationStopping.Register(
                () => app.ApplicationServices.GetService<IRuntime>()
                    .Dispose()
            );

            app.Run(
                async context =>
                {
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
                                //.WithOrigins("http://localhost:5000")
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
                    //c.AddSecurityDefinition("Bearer", new ApiKeyScheme() { In = "header", Description = "Please insert JWT with Bearer into field", Name = "Authorization", Type = "apiKey" });
                }
            );

            services.AddSingleton(new ProjectManager(Configuration.GetValue("ProjectRoot", "projects")));
            services.AddSingleton<IRuntime>(new DotnetRuntime());

            services.AddHostedService<NotifyService>();
        }
    }
}