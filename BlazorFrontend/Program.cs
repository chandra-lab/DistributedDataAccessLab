using BlazorFrontend;
using BlazorFrontend.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// All API calls go through the API Gateway
var gatewayUrl = builder.Configuration["ApiGatewayUrl"] ?? "http://localhost:5000";

builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(gatewayUrl) });

builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<OrderService>();

await builder.Build().RunAsync();
