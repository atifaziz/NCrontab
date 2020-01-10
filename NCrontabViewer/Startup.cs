namespace NCrontabViewer
{
    using Microsoft.AspNetCore.Components.Builder;

    class Startup
    {
        public void Configure(IComponentsApplicationBuilder app) =>
            app.AddComponent<App>("app");
    }
}
