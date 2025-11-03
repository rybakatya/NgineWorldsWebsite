
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Services;

namespace frontend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            /* builder.Services.AddScoped(sp => new HttpClient
             { BaseAddress = new Uri("https://localhost:7156/") });*/
            builder.Services.AddScoped<AuthState>();
            builder.Services.AddScoped(sp => new HttpClient
            {
                // same-origin base (server hosts the client + API)
                BaseAddress = new Uri(new Uri(builder.HostEnvironment.BaseAddress), "/")
            });
            builder.Services.AddScoped<AuthService>();

            var host = builder.Build();

           
            

            await host.RunAsync();
        }
    }
}
