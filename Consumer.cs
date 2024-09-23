using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using Newtonsoft.Json;

public class Consumer
{
    //Opretter properties, for de nødvenige parametre for RabbitMQ
    public string HostName { get; set; }
    public string ExchangeName { get; set; }
    public string QueueName { get; set; }
    public string RoutingKey { get; set; }
    public string DeadLetterExchange { get; set; }
    public string DeadLetterQueue { get; set; }

    // Constructor til at initialisere properties   
    public Consumer(string hostName, string exchangeName, string queueName, string routingKey, string deadLetterExchange, string deadLetterQueue)
    {
        HostName = hostName;
        ExchangeName = exchangeName;
        QueueName = queueName;
        RoutingKey = routingKey;
        DeadLetterExchange = deadLetterExchange;
        DeadLetterQueue = deadLetterQueue;
    }

    //Consume metode til at modtage beskeder fra RabbitMQ, samt at validere beskederne
    //Her ved jeg godt, at det kunne splittes op yderligere, beklager jeg ikke helt følger SRP her Morten :(
   public void Consume()
   {
    var factory = new ConnectionFactory() { HostName = this.HostName };
    using (var connection = factory.CreateConnection())
    using (var channel = connection.CreateModel())
    {
        //Opretter dead letter exchange
        channel.ExchangeDeclare(exchange: DeadLetterExchange, type: "direct");

        //Opretter dead letter queue
        channel.QueueDeclare(queue: DeadLetterQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);

        //Binder dead letter queue sammen med dead letter exchange
        channel.QueueBind(queue: DeadLetterQueue, exchange: DeadLetterExchange, routingKey: "");

        //Opretter 'main' exchange og queue, og angiver dead letter exchange og queue som argumenter main queue
        channel.ExchangeDeclare(exchange: this.ExchangeName, type: "topic");
        channel.QueueDeclare(queue: this.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", DeadLetterExchange }
        });

        //Binder main queue og main exchange
        channel.QueueBind(queue: this.QueueName, exchange: this.ExchangeName, routingKey: this.RoutingKey);

        Console.WriteLine(" [*] Waiting for all tour messages...");

        //Opretter en consumer, som lytter efter beskeder på main queue, i dette tilfælde, backOfficeQueue
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var messageObject = JsonConvert.DeserializeObject<Message>(message);

            //Kalder isMessageInvalid metoden, som returnerer et MessageValidation objekt
            var validation = IsMessageInvalid(messageObject);

            //Hvis den ikke er valid, skriver vi det til konsollen, og sender beskeden til dead letter exchange.
            if (!validation.Valid)
            {
                var invalidMessage = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: DeadLetterExchange, routingKey: "", body: invalidMessage);
                Console.WriteLine($" [!] Invalid message: {message} sent to dead letter queue.");
                return;
            }

            //Hvis beskeden er valid, skriver vi den til konsollen.
            Console.WriteLine($" [x] Received: {message}");
        };

        channel.BasicConsume(queue: this.QueueName, autoAck: true, consumer: consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}


    //Metode, som returnerer et MessageValidation objekt, som indeholder en bool og en string
    private MessageValidation IsMessageInvalid(Message message)
    {
        Console.WriteLine("Validating message...");

        //Hvis navn er email er tomme, returnerer vi valid = false, og en besked.
        if (string.IsNullOrEmpty(message.Name) || string.IsNullOrEmpty(message.Email))
        {
            {
                return new MessageValidation(false, "Name and Email are required.");
            }
        }

        //Evt. kan der tilføjes yderligere validering her

        //Hvis message er valid, returnerer vi true
        return new MessageValidation(true, "Success");
    }
}
