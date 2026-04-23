using BlazorFrontend;
using BlazorFrontend.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// If ApiGatewayUrl is set in appsettings.json, use it.
// Otherwise fall back to the app's own origin — nginx will proxy /api/ to the gateway.
// This makes the app work in GitHub Codespaces, local Docker, or anywhere else
// without needing to know the dynamic host URL at build time.
var gatewayUrl = builder.Configuration["ApiGatewayUrl"];
if (string.IsNullOrWhiteSpace(gatewayUrl))
    gatewayUrl = builder.HostEnvironment.BaseAddress;

builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(gatewayUrl) });

builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<OrderService>();

await builder.Build().RunAsync();
