using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using PAMrecert.Models;
using PAMrecert.Services;

namespace PAMrecert
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection ser)
        {

            ser.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            ser.AddDbContext<PAML01Context>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("PAMrecertDataBaseConnection")), ServiceLifetime.Transient
            );

            // Register NewtonsoftJson and set options for date and case conventions
            ser.AddControllers().AddNewtonsoftJson(options =>
            {
                options.UseMemberCasing();
            });

            // Add JSON patch support
            ser.AddControllers(opt =>
            {
                opt.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
            });

            ser.AddSwaggerGen(opt =>
            {
                opt.CustomSchemaIds(type => type.ToString());
                opt.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "PAM Recertification API",
                    Description = "Exposes various endpoints as part of the PAM recertification application",
                    Contact = new OpenApiContact
                    {
                        Name = "Anna Howell",
                        Email = string.Empty,
                    }
                });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                opt.IncludeXmlComments(xmlPath);
                opt.EnableAnnotations();
            });

            ser.AddTransient<ControllerService, ControllerService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // @TODO This must be configured securely for production THESE ARE PLACEHOLDER DEV SETTINGS
            app.UseCors(builder =>
                builder
                    .WithOrigins("http://localhost:8080")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
            );

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.SupportedSubmitMethods(); // Disable making API calls from the swagger UI
                c.DefaultModelsExpandDepth(-1); // Disable showing model info
                c.EnableFilter();
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
        {
            var builder = new ServiceCollection()
                .AddLogging()
                .AddMvc()
                .AddNewtonsoftJson()
                .Services.BuildServiceProvider();

            return builder
                .GetRequiredService<IOptions<MvcOptions>>()
                .Value
                .InputFormatters
                .OfType<NewtonsoftJsonPatchInputFormatter>()
                .First();
        }
    }
}
