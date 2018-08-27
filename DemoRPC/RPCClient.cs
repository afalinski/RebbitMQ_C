using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DemoRPC
{
   public class RpcClient
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _replyQueueName;
        private readonly EventingBasicConsumer _consumer;

        public RpcClient()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672,
                Password = "guest",
                UserName = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _replyQueueName = _channel.QueueDeclare().QueueName;
            _consumer = new EventingBasicConsumer(_channel);
        }

        public string Call(string message)
        {
            var tcs = new TaskCompletionSource<string>();
            var resultTask = tcs.Task;
            var correlationId = Guid.NewGuid().ToString();
            IBasicProperties props = _channel.CreateBasicProperties();
            props.CorrelationId = correlationId;
            props.ReplyTo = _replyQueueName;
            EventHandler<BasicDeliverEventArgs> handler = null;
            handler = (model, ea) =>
            {
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    _consumer.Received -= handler;
                    var body = ea.Body;
                    var response = Encoding.UTF8.GetString(body);
                    tcs.SetResult(response);
                }
            };

            _consumer.Received += handler;
            var messageBytes = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange:"", routingKey: "rpc_queue", basicProperties: props, body: messageBytes);
            _channel.BasicConsume(consumer: _consumer, queue: _replyQueueName, autoAck: true);

            return resultTask.Result;
        }

        public void Close()
        {
            _connection.Close();
        }
    }
}
