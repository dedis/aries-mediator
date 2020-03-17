using Hyperledger.Aries.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MediatorAgent
{
    public class Startup
    {
        public IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<HubConnectionSubscriberManager>();
            services.AddSingleton<MessageQueue<InboxItemRecord>>();
            services.AddSingleton<HubMethods>();

            services.AddAriesFramework(builder =>
            {
                _ = builder.RegisterMediatorAgent<CustomMediatorAgent>(options =>
                  {
                      options.EndpointUri = _configuration.GetValue<string>("agentPublicEndpoint");
                      options.WalletConfiguration.StorageConfiguration = new Hyperledger.Aries.Storage.WalletConfiguration.WalletStorageConfiguration
                      {
                          Path = _configuration.GetValue<string>("walletPath")
                      };
                      var url = new System.Uri(options.EndpointUri);
                      options.WalletConfiguration.Id = url.Host;
                  });
                services.AddHostedService<ForwardMessageSubscriber>();
                services.AddSignalR();
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
            app.UseAriesFramework();
            app.UseMediatorDiscovery();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
                endpoints.MapHub<MediatorHub>("/hub");
            });
        }
    }
}
