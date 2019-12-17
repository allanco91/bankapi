using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BankAPI.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace BankAPI
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
            services.AddControllers();

            services.AddCors();

            services.AddScoped<ITransactionsRepository, TransactionsRepository>();

            services.AddScoped<IMongoDatabase>(s =>
            {

                string username = "root";
                string password = "123456";
                string mongoDbAuthMechanism = "SCRAM-SHA-1";
                MongoInternalIdentity internalIdentity =
                          new MongoInternalIdentity("admin", username);
                PasswordEvidence passwordEvidence = new PasswordEvidence(password);
                MongoCredential mongoCredential =
                     new MongoCredential(mongoDbAuthMechanism,
                             internalIdentity, passwordEvidence);
                List<MongoCredential> credentials =
                           new List<MongoCredential>() { mongoCredential };

                MongoClientSettings settings = new MongoClientSettings();
                // comment this line below if your mongo doesn't run on secured mode
                settings.Credentials = credentials;
                String mongoHost = Configuration["DOTNET_RUNNING_IN_CONTAINER"] != null ? "mongo" : "localhost"; // <== weblocal 'locahost', container 'mongo'
                MongoServerAddress address = new MongoServerAddress(mongoHost);
                settings.Server = address;

                MongoDB.Driver.MongoClient client = new MongoDB.Driver.MongoClient(settings);

                return client.GetDatabase("bank");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var enUS = new CultureInfo("en-US");
            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(enUS),
                SupportedCultures = new List<CultureInfo> { enUS },
                SupportedUICultures = new List<CultureInfo> { enUS }
            };

            app.UseRequestLocalization(localizationOptions);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseCors(option => option.AllowAnyOrigin().AllowAnyHeader());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
