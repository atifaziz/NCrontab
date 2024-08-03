using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NCrontabViewer;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
await builder.Build().RunAsync();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
