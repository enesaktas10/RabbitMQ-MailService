using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductAPI.Models;
using RabbitMQ.Client;



var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

string connectionString = configuration.GetConnectionString("Sql");
Console.WriteLine($"Connection String: {connectionString}");

var serviceProvider = new ServiceCollection()
    .AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("Sql")))
    .BuildServiceProvider();


var context = serviceProvider.GetService<AppDbContext>();

if (context == null)
{
    throw new Exception("context nesnesi null!");
}


var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "order_queue",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null
);

Console.WriteLine("OrderPublisher started...");

while (true)
{
    var unsentOrders =await context.Products
                                .Where(p => p.IsPublished == false)
                                .ToListAsync();

    foreach (var order in unsentOrders)
    {
        var message = new
        {
            Email = order.Mail,
            Subject = $"Sayın {order.FirstName}, siparişiniz oluşturuldu",
            Body = $"Siparişiniz başarılı bir şekilde oluşturuldu. Ürün: {order.ProductName}"
        };

        var messageBody = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(messageBody);

        await channel.BasicPublishAsync(
                   exchange: string.Empty,
                   routingKey: "order_queue",
                   mandatory: true,
                   basicProperties: new BasicProperties { Persistent = true },
                   body: body
               );

        Console.WriteLine($"Message sent: {messageBody}");

        order.IsPublished = true;
        context.Products.Update(order);
    }

    await context.SaveChangesAsync();

    await Task.Delay(5000);
}
