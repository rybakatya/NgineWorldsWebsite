using frontend.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace frontend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient
            { BaseAddress = new Uri("https://localhost:7156/") });

            builder.Services.AddScoped<AuthService>();

            var host = builder.Build();

            var auth = host.Services.GetRequiredService<AuthService>();
            await auth.InitializeAsync();

            await host.RunAsync();
        }
    }
}
