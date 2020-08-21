namespace NCrontabViewer
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    static class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                var builder = WebAssemblyHostBuilder.CreateDefault(args);
                builder.RootComponents.Add<App>("app");
                builder.Services.AddTransient(sp => new HttpClient
                {
                    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
                });
                await builder.Build().RunAsync();
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0xbad;
            }
        }
    }
}
