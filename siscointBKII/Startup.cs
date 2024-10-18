using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace siscointBKII
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
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                    };
                })
                .AddJwtBearer("client2",options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key2"]))
                    };
                });
            services.AddMvc();
            //string sqlServerConennection = Configuration.GetConnectionString("conexion");
            string sqlServerConennection = Configuration.GetConnectionString("conexionDbPruebas");
            //services.AddDbContext<AplicationDbContext>(options => options.UseSqlServer(sqlServerConennection));
            services.AddDbContextPool<AplicationDbContext>(options => options.UseSqlServer(sqlServerConennection));
            services.AddControllers(options =>
            {
                var jsonInputFormatter = options.InputFormatters
                    .OfType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonInputFormatter>()
                    .Single();
                jsonInputFormatter.SupportedMediaTypes.Add("application/csp-report");
                jsonInputFormatter.SupportedMediaTypes.Add("application/json");
            });
            services.AddRazorPages();
            services.AddCors(options =>
            {
                //var origin = "http://siscointv2.sistemasoptecom.net";
                //var origin = "http://localhost:4200";
                options.AddPolicy("CorsPolicy", builder => builder.WithOrigins("http://localhost:4200", 
                                                                               "http://siscointv2.sistemasoptecom.net", 
                                                                               "https://siscointv2.sistemasoptecom.net",
                                                                               "http://sistemasop-001-site8.ctempurl.com",
                                                                               "https://sistemasop-001-site8.ctempurl.com")
                                                                  .AllowAnyMethod()
                                                                  .AllowAnyHeader()
                                                                  .SetPreflightMaxAge(TimeSpan.FromSeconds(3000)));
            });
            services.AddControllers();

            services.AddScoped<Interfaces.IRSAHelper, Helpers.RSAHelper>();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("CorsPolicy");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });


        }
    }
}
