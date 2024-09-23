using System;

class Program
{
    static void Main(string[] args)
    {
        var consumer = new Consumer(
            hostName: "localhost",
            exchangeName: "tourExchange",
            queueName: "backOfficeQueue",
            routingKey: "tour.*",
            deadLetterExchange: "deadLetterExchange",
            deadLetterQueue: "deadLetterQueue",
        );
        consumer.Consume();
    }
}
