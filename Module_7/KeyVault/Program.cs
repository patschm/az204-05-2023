
using System;
using System.Threading.Tasks;

#region KeyVault
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Azure.Security.KeyVault.Secrets;
using Azure.Security.KeyVault;
#endregion

#region AppConfiguration
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Azure;
#endregion


namespace KeyVault
{
    class Program
    {
        static string tenentId = "030b09d5-7f0f-40b0-8c01-03ac319b2d71";
        static string clientId = "1fdcc3d5-c4a1-4c9d-b698-083395b6c825";
        static string clientSecret = "a0a8Q~HMfs-fBvK156CScfXKxpceqBgYKYVtIbJH";
        static string kvUri = "https://ps-sleutelbos.vault.azure.net/";
        
        static async Task Main(string[] args)
        {
           //await ReadKeyVault();
           await ReadAppConfigurationAsync();

            Console.WriteLine("Done");
            Console.ReadLine();
        }
        private static async Task ReadKeyVault()
        {
            ClientSecretCredential cred = new ClientSecretCredential(tenentId, clientId, clientSecret);
            SecretClient kvClient = new SecretClient(new Uri(kvUri), cred);
                
            var result = await kvClient.GetSecretAsync("mijngeheim");
            Console.WriteLine($"Hello: {result.Value?.Value}");
        }

        private static async Task ReadAppConfigurationAsync()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json")
                    .AddUserSecrets<KeyVault.Program>(true)
                   .AddEnvironmentVariables();
            IConfiguration configuration = builder.Build();

                      

            //ReadLocal();
            await ReadRemoteAsync();

            void ReadLocal()
            {
                Console.WriteLine(configuration["MySetings:hello"]);
                Console.WriteLine(configuration["KlantA:KeyVault:BackgroundColor"]);
                Console.WriteLine(configuration["ConnectionString"]);
            }

            async Task ReadRemoteAsync()
            {
                //ClientSecretCredential cred = new ClientSecretCredential(tenentId, clientId, clientSecret);
                //var env = Environment.GetEnvironmentVariable("Bla");
               // builder.AddAzureKeyVault(new Uri(kvUri), cred);
                builder.AddAzureAppConfiguration(opts => {
                    //opts.ConfigureKeyVault(kvopts =>
                    //{
                    //    kvopts.SetCredential(new ClientSecretCredential(tenentId, clientId, clientSecret));
                    //});

                    // From user-secrets
                    // dotnet init
                    // dotnet user-secrets set APPC "Endpoint=https://ps-config.azconfig.io;Id=LDpu;Secret=zE8JVpCLRSBwkKyE+sHBE1uPnexZTenYGpvY2eQoY04="
                    //var constr = configuration["APPC"];
                    //Console.WriteLine(  constr);
                    //opts.Connect(constr);
                    
                    opts.Connect("Endpoint=https://ps-config.azconfig.io;Id=LDpu;Secret=zE8JVpCLRSBwkKyE+sHBE1uPnexZTenYGpvY2eQoY04=")
                       .Select(KeyFilter.Any, "Production")
                       // .Select(KeyFilter.Any, "Prog") // When using labels in your configuration, import the appropriate keys for that label
                       .UseFeatureFlags(opts => {
                           opts.CacheExpirationInterval = TimeSpan.FromSeconds(1);
                           opts.Label = "Production";
                       });
                        // });
                    
                //builder.AddAzureAppConfiguration(opts => {
                //    opts.ConfigureKeyVault(kvopts =>
                //    {
                //        kvopts.SetCredential(new ClientSecretCredential(tenentId, clientId, clientSecret));
                //    })
                //    .UseFeatureFlags();
                //    opts.Connect(configuration["ConnectionString"]);    
                   
                });
                IConfiguration conf = builder.Build();

                //  Console.WriteLine($"{conf["KeyVault:Test:MyConfig"]}");
                // Console.WriteLine($"Hello {conf["ThaKey"]}");
                Console.WriteLine(conf["KlantA:KeyVault:BackgroundColor"]);

                IServiceCollection services = new ServiceCollection();
                services.AddSingleton<IConfiguration>(conf).AddFeatureManagement();

                using (var svcProvider = services.BuildServiceProvider())
                {
                    do
                    {
                        using (var scope = svcProvider.CreateScope())
                        {
                            var featureManager = scope.ServiceProvider.GetRequiredService<IFeatureManager>();

                            if (await featureManager.IsEnabledAsync("FeatureA"))
                            {
                                Console.WriteLine("We have a new feature");
                            }
                            Console.Write(".");
                            await Task.Delay(2000);
                        }
                    }
                    while (true);
                }
            }
        }
    }
}
