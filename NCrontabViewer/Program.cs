namespace NCrontabViewer
{
    using System;
    using Microsoft.AspNetCore.Blazor.Hosting;

    static class Program
    {
        public static IWebAssemblyHostBuilder CreateHostBuilder() =>
            BlazorWebAssemblyHost
                .CreateDefaultBuilder()
                .UseBlazorStartup<Startup>();

        public static int Main()
        {
            try
            {
                CreateHostBuilder().Build().Run();
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
