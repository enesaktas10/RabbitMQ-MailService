using System.Net.Mail;
using System.Text.Json;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.OrderConsumer;
using ProductAPI.Services;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;

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

Console.WriteLine("Waiting for messages...");

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (sender, args) =>
{
    var body = args.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    try
    {
        var emailMessage = JsonSerializer.Deserialize<EmailMessage>(message);

        Console.WriteLine($"Sending email to: {emailMessage.Email}");
        Console.WriteLine($"Subject: {emailMessage.Subject}");
        Console.WriteLine($"Body: {emailMessage.Body}");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        EmailService emailService = new EmailService(configuration);

        await emailService.SendEmailAsync(emailMessage.Email, emailMessage.Subject, emailMessage.Body);

        await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(args.DeliveryTag, false);
    }
    catch (JsonException jsonEx)
    {
        Console.WriteLine($"JSON Parsing Error: {jsonEx.Message}");
    }
    catch (SmtpException smtpEx)
    {
        Console.WriteLine($"SMTP Error: {smtpEx.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"General Error: {ex.Message}");
    }
};

await channel.BasicConsumeAsync("order_queue", autoAck: false, consumer);

Console.WriteLine("Press [enter] to exit.");
Console.ReadLine();
