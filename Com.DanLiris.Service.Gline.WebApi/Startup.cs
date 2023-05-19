using AutoMapper;
using Com.DanLiris.Service.Gline.Lib;
using Com.DanLiris.Service.Gline.Lib.Helpers;
using Com.DanLiris.Service.Gline.Lib.Serializers;
using Com.DanLiris.Service.Gline.Lib.Services;
using Com.DanLiris.Service.Gline.WebApi.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json.Serialization;
using System.Text;
using FluentScheduler;
using Com.DanLiris.Service.Gline.WebApi.SchedulerJobs;
using Com.DanLiris.Service.Gline.Lib.Utilities.CacheManager;
using Com.DanLiris.Service.Gline.Lib.Utilities.Currencies;
using Com.DanLiris.Service.Gline.Lib.Utilities;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.AspNetCore;
using Com.DanLiris.Service.Gline.Lib.Interfaces;
using Com.DanLiris.Service.Gline.Lib.Facades.ProsesFacades;
using Com.DanLiris.Service.Gline.Lib.Facades.SettingRoFacades;
using Com.DanLiris.Service.Gline.Lib.Facades.LineFacades;
using Com.DanLiris.Service.Gline.Lib.Facades.TransaksiFacades;
using Com.DanLiris.Service.Gline.Lib.Facades.ReportFacades;

namespace Com.DanLiris.Service.Gline.WebApi
{
    public class Startup
    {
        /* Hard Code */
        private string[] EXPOSED_HEADERS = new string[] { "Content-Disposition", "api-version", "content-length", "content-md5", "content-type", "date", "request-id", "response-time" };
        private string PURCHASING_POLICITY = "PurchasingPolicy";

        public IConfiguration Configuration { get; }

        public bool HasAppInsight => !string.IsNullOrEmpty(Configuration.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY") ?? Configuration.GetValue<string>("ApplicationInsights:InstrumentationKey"));

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        #region Register

        //private void RegisterEndpoints()
        //{
        //    APIEndpoint.Purchasing = Configuration.GetValue<string>(Constant.PURCHASING_ENDPOINT) ?? Configuration[Constant.PURCHASING_ENDPOINT];
        //}

        private void RegisterFacades(IServiceCollection services)
        {
            services
                .AddTransient<ICoreData, CoreData>()
                .AddTransient<ICoreHttpClientService, CoreHttpClientService>()
                .AddTransient<IMemoryCacheManager, MemoryCacheManager>()
                .AddTransient<ICurrencyProvider, CurrencyProvider>()
                .AddTransient<IProsesFacade, ProsesFacade>()
                .AddTransient<ISettingRoFacade, SettingRoFacade>()
                .AddTransient<ILineFacade, LineFacade>()
                .AddTransient<ITransaksiOperatorFacade, TransaksiOperatorFacade>()
                .AddTransient<ITransaksiQcFacade, TransaksiQcFacade>()
                .AddTransient<IReportFacade, ReportFacade>();
        }

        private void RegisterServices(IServiceCollection services, bool isTest)
        {
            services
                .AddScoped<IdentityService>()
                .AddScoped<ValidateService>()
                .AddScoped<IHttpClientService, HttpClientService>()
                .AddScoped<IValidateService, ValidateService>();

            //if (isTest == false)
            //{
            //    services.AddScoped<IHttpClientService, HttpClientService>();
            //}
        }

        private void RegisterSerializationProvider()
        {
            BsonSerializer.RegisterSerializationProvider(new SerializationProvider());
        }

        #endregion Register

        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration.GetConnectionString(Constant.DEFAULT_CONNECTION) ?? Configuration[Constant.DEFAULT_CONNECTION];
            string connectionStringSales = Configuration.GetConnectionString(Constant.SALES_CONNECTION) ?? Configuration[Constant.SALES_CONNECTION];
            string env = Configuration.GetValue<string>(Constant.ASPNETCORE_ENVIRONMENT);
            //string connectionStringLocalCashFlow = Configuration.GetConnectionString("LocalDbCashFlowConnection") ?? Configuration["LocalDbCashFlowConnection"];
            APIEndpoint.ConnectionString = Configuration.GetConnectionString("DefaultConnection") ?? Configuration["DefaultConnection"];

            /* Register */
            services.AddDbContext<GlineDbContext>(options => options.UseSqlServer(connectionString, sqlServerOptions => sqlServerOptions.CommandTimeout(1000 * 60 * 20)));
            services.AddTransient<ISalesDbContext>(s => new SalesDbContext(connectionStringSales));

            //RegisterEndpoints();
            RegisterFacades(services);
            RegisterServices(services, env.Equals("Test"));
            services.AddAutoMapper(typeof(BaseAutoMapperProfile));
            services.AddMemoryCache();

            RegisterSerializationProvider();

            /* Versioning */
            services.AddApiVersioning(options => { options.DefaultApiVersion = new ApiVersion(1, 0); });

            /* Authentication */
            string Secret = Configuration.GetValue<string>(Constant.SECRET) ?? Configuration[Constant.SECRET];
            SymmetricSecurityKey Key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Secret));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateLifetime = false,
                        IssuerSigningKey = Key
                    };
                });

            /* CORS */
            services.AddCors(options => options.AddPolicy(PURCHASING_POLICITY, builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .WithExposedHeaders(EXPOSED_HEADERS);
            }));

            /* API */
            services
               .AddMvcCore()
               .AddApiExplorer()
               .AddAuthorization()
               .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver())
               .AddJsonFormatters();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Gline API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme { In = "header", Description = "Please enter JWT with Bearer into field", Name = "Authorization", Type = "apiKey" });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", Enumerable.Empty<string>() },
                });
                c.OperationFilter<ResponseHeaderFilter>();

                c.CustomSchemaIds(i => i.FullName);
            });

            // App Insight
            if (HasAppInsight)
            {
                services.AddApplicationInsightsTelemetry();
                services.AddAppInsightRequestBodyLogging();
            }

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            /* Update Database */
            //using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            //{
            //    PurchasingDbContext context = serviceScope.ServiceProvider.GetService<PurchasingDbContext>();

            //    if (context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
            //    {
            //        context.Database.SetCommandTimeout(10 * 60 * 1000);
            //        context.Database.Migrate();
            //    }
            //}

            if(HasAppInsight){
                app.UseAppInsightRequestBodyLogging();
                app.UseAppInsightResponseBodyLogging();
            }

            app.UseAuthentication();
            app.UseCors(PURCHASING_POLICITY);
            app.UseMvc();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
            });

            //JobManager.Initialize(new MasterRegistry(app.ApplicationServices));
        }
    }
}
