using System;
using System.Net.Http;
using System.Text;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Interfaces;
using CalorieMonitor.Data.Implementations;
using CalorieMonitor.Data.Interfaces;
using CalorieMonitor.Logic.Implementations;
using CalorieMonitor.Logic.Interfaces;
using CalorieMonitor.Utils;
using DbUp;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace CalorieMonitor
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration.GetSection("DatabaseSettings")["ConnectionString"];

            SetUpJWTAuthentication(services);

            services.AddControllers().AddNewtonsoftJson();

            services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
            services.AddHangfireServer();

            services.Configure<CalorieProviderConfig>(
                Configuration.GetSection(nameof(CalorieProviderConfig)));

            services.AddSingleton<ICalorieProviderConfig>(sp =>
                sp.GetRequiredService<IOptions<CalorieProviderConfig>>().Value);

            services.AddSingleton<ILogger>(sp => new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger());
            services.AddSingleton<IDbConnectionProvider>(sp => new SqlConnectionProvider(connectionString));
            services.AddTransient<IDbFilterQueryHandler, SqlFilterQueryHandler>();
            services.AddTransient<IMealEntryDAO, MealEntryDAO>();
            services.AddTransient<IMealItemDAO, MealItemDAO>();
            services.AddTransient<IUserDAO, UserDAO>();
            services.AddScoped<HttpClient, HttpClient>();

            services.AddTransient<IMealItemLogic, MealItemLogic>();
            services.AddTransient<IMealEntryLogic, MealEntryLogic>();
            services.AddSingleton<IPasswordHashProvider, PasswordHashProvider>();
            services.AddSingleton<ILoginHandler, LoginHandler>();
            services.AddSingleton<ILogManager, LogManager>();
            services.AddTransient<ISearchFilterHandler, SearchFilterHandler>();
            services.AddTransient<IUserLogic, UserLogic>();
            services.AddTransient<ICalorieProviderService>(sp => new CalorieProviderService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogManager>(), sp.GetRequiredService<ICalorieProviderConfig>()));

            //setUp DB tables
            SetUpDb(connectionString);
        }

        private void SetUpJWTAuthentication(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                            .AddJwtBearer(options =>
                            {
                                options.RequireHttpsMetadata = false;
                                options.SaveToken = true;
                                options.TokenValidationParameters = new TokenValidationParameters
                                {
                                    ValidateIssuer = true,
                                    ValidateAudience = true,
                                    ValidateLifetime = true,
                                    ValidateIssuerSigningKey = true,
                                    ValidIssuer = Configuration["Jwt:Issuer"],
                                    ValidAudience = Configuration["Jwt:Audience"],
                                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:SecretKey"])),
                                    ClockSkew = TimeSpan.Zero
                                };
                            });

            //Set up access policies
            services.AddAuthorization(config =>
            {
                config.AddPolicy(Policies.ManageUsers, Policies.ManageUsersPolicy());
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        public bool SetUpDb(string connectionString)
        {
            if (connectionString == "Data Source=:memory:") return false;

            var upgrader = DeployChanges.To.SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(typeof(UserDAO).Assembly)
                .Build();
            var result = upgrader.PerformUpgrade();
            return result.Successful;
        }
    }
}
