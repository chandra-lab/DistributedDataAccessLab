using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot();
builder.Services.AddSwaggerForOcelot(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClients for the aggregation endpoint
builder.Services.AddHttpClient("orderservice",      c => c.BaseAddress = new Uri("http://orderservice:8080/"));
builder.Services.AddHttpClient("customerservice",   c => c.BaseAddress = new Uri("http://customerservice:8080/"));
builder.Services.AddHttpClient("productservice",    c => c.BaseAddress = new Uri("http://productservice:8080/"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerForOcelotUI(opt => {
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

// Aggregation endpoint
app.MapGet("/api/aggregate/orders/{id:int}", async (int id, IHttpClientFactory factory) =>
{
    var orderClient = factory.CreateClient("orderservice");
    var orderResp = await orderClient.GetAsync($"api/orders/{id}");
    if (!orderResp.IsSuccessStatusCode)
        return Results.NotFound(new { message = $"Order {id} not found." });

    var orderRaw = await orderResp.Content.ReadAsStringAsync();
    var orderJson = System.Text.Json.JsonDocument.Parse(orderRaw);

    int customerId = orderJson.RootElement.GetProperty("customerId").GetInt32();
    int productId  = orderJson.RootElement.GetProperty("productId").GetInt32();

    var customerClient = factory.CreateClient("customerservice");
    var productClient  = factory.CreateClient("productservice");

    var customerResp = await customerClient.GetAsync($"api/customers/{customerId}");
    var productResp  = await productClient.GetAsync($"api/products/{productId}");

    var customerJson = customerResp.IsSuccessStatusCode
        ? System.Text.Json.JsonDocument.Parse(await customerResp.Content.ReadAsStringAsync()).RootElement
        : (System.Text.Json.JsonElement?)null;

    var productJson = productResp.IsSuccessStatusCode
        ? System.Text.Json.JsonDocument.Parse(await productResp.Content.ReadAsStringAsync()).RootElement
        : (System.Text.Json.JsonElement?)null;

    return Results.Ok(new { order = orderJson.RootElement, customer = customerJson, product = productJson });
});

await app.UseOcelot();
app.Run();
