using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Autofac;
using MarketingBox.AuthApi.Modules;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MyJetWallet.Sdk.GrpcSchema;
using MyJetWallet.Sdk.Service;
using Prometheus;
using SimpleTrading.ServiceStatusReporterConnector;

namespace MarketingBox.AuthApi
{
    public class Startup
    {
        private readonly string _allowAllOrigins = "Develop";
        public Startup()
        {
            ModelStateDictionaryResponseCodes = new HashSet<int>();

            ModelStateDictionaryResponseCodes.Add(StatusCodes.Status400BadRequest);
            ModelStateDictionaryResponseCodes.Add(StatusCodes.Status500InternalServerError);
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.BindCodeFirstGrpc();
            services.AddCors(options =>
            {
                options.AddPolicy(_allowAllOrigins,
                 builder =>
                 {
                     builder
                     .WithOrigins("http://localhost:3001", "http://localhost:3002")
                     //.AllowAnyOrigin()
                     //.WithMethods("GET", "POST")
                     .AllowAnyHeader()
                     .AllowAnyMethod()
                     .SetIsOriginAllowed((host) => true)
                     .AllowCredentials();
                 });
            });


            ///Access to XMLHttpRequest at 'https://mb-affiliate-api.mnftx.biz/api/affiliates' from origin 'http://localhost:3001' 
            ///has been blocked by CORS policy: Response to preflight request doesn't pass access control check: No 'Access-Control-Allow-Origin' 
            ///header is present on the requested resource.

            //services.AddAuthorization();
            services.AddControllers();
            services.SetupSwaggerDocumentation();

            services.AddHostedService<ApplicationLifetimeManager>();

            services
                .AddAuthentication(ConfigureAuthenticationOptions)
                .AddJwtBearer(ConfigureJwtBearerOptions);

            services.AddMyTelemetry("MB-", Program.Settings.ZipkinUrl);
        }

        protected virtual void ConfigureJwtBearerOptions(JwtBearerOptions options)
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Program.Settings.JwtSecret)),
                ValidateIssuer = false,
                ValidateAudience = true,
                ValidAudience = Program.Settings.JwtAudience,
                ValidateLifetime = true
            };
        }

        protected virtual void ConfigureAuthenticationOptions(AuthenticationOptions options)
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors(_allowAllOrigins);

            //app.UseAuthentication();
            //app.UseAuthorization();

            app.UseMetricServer();

            app.BindServicesTree(Assembly.GetExecutingAssembly());

            app.BindIsAlive();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcSchemaRegistry();

                endpoints.MapControllers();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            app.UseOpenApi(settings =>
            {
                settings.Path = $"/swagger/api/swagger.json";
            });

            app.UseSwaggerUi3(settings =>
            {
                settings.EnableTryItOut = true;
                settings.Path = $"/swagger/api";
                settings.DocumentPath = $"/swagger/api/swagger.json";
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<SettingsModule>();
            builder.RegisterModule<ServiceModule>();
        }
        public ISet<int> ModelStateDictionaryResponseCodes { get; }
    }
}
