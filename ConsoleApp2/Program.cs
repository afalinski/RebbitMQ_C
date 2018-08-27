using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672,
                Password = "guest",
                UserName = "guest"
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
               // channel.QueueDeclare(queue: "task_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

                channel.ExchangeDeclare(exchange: "topic_logs", type:"topic");

                var queueName = channel.QueueDeclare().QueueName;

                if (args.Length < 1)
                {
                    Console.Error.WriteLine("Usage: {0} [binding_key...]",
                        Environment.GetCommandLineArgs()[0]);
                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                    Environment.ExitCode = 1;
                    return;
                }

                foreach (var bindingKey in args)
                {
                    channel.QueueBind(queue: queueName, exchange: "topic_logs", routingKey: bindingKey);
                }
               
                Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {   
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);

                    Console.WriteLine(" [x] {0}", message);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine($" [x] Received '{routingKey}':'{message}'");

                };

                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}