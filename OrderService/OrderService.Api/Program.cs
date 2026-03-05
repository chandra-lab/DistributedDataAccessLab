using OrderService.Api.Data;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlite("Data Source=Data/orders.db"));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<ICustomerClient, CustomerClient>(client =>
{
    client.BaseAddress = new Uri("http://customerservice:8080/");
});
// Typed HttpClient for ProductService
builder.Services.AddHttpClient<IProductClient, ProductClient>(client =>
{
    var url = "http://productservice:8080/";
    client.BaseAddress = new Uri(url);
});
builder.Services.AddSingleton<IEventPublisher, RabbitMqPublisher>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
