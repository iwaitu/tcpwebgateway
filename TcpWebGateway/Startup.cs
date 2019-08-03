using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Reflection;
using TcpWebGateway.Services;
using TcpWebGateway.Tools;
using Hangfire.Mongo;

namespace TcpWebGateway
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
            services.AddHangfire(x => {
                var migrationOptions = new MongoMigrationOptions
                {
                    Strategy = MongoMigrationStrategy.Migrate,
                    BackupStrategy = MongoBackupStrategy.Collections
                };
                x.UseMongoStorage(Configuration["Mongodb:ConnectString"], "hangfire", new MongoStorageOptions { Prefix = "hangfire",MigrationOptions = migrationOptions });
            });
            services.AddHangfireServer();

            //注入配置信息(appsettings.json)
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddMemoryCache();
            ///注入顺序不能变
            services.AddSingleton<CurtainHelper>();
            services.AddSingleton<HvacHelper>();
            services.AddSingleton<LightHelper>();
            services.AddSingleton<SensorHelper>();

            services.AddHostedService<MqttHelper>();
            services.AddHostedService<CurtainListener>();
            services.AddHostedService<SwitchListener>();
            services.AddHostedService<SensorListener>();
            services.AddHostedService<HvacListener>();

            services.AddHostedService<SmartService>();


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Tcp Web Gateway", Version = "v1" , Description = "与tcp网关进行通信,并提供api控制设备,支持mqtt",
                    Contact = new Contact
                    {
                        Name = "Rafael Luo",
                        Email = "iwaitu@vip.qq.com",
                        Url = "https://www.ivilson.com",
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();
            app.UseMvc();

            app.UseHangfireDashboard("/jobs", new DashboardOptions()
            {
                Authorization = new[] { new HangFireAuthorizationFilter() }
            });
        }
    }
}
