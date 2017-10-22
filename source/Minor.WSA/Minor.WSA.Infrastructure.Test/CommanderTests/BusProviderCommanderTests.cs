using Minor.WSA.Infrastructure;
using Minor.WSA.Infrastructure.Test;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;

public class BusProviderCommanderTests
{
    [Fact]
    public void BusProviderSendsCommand()
    {
        var busOptions = new BusOptions(exchangeName: "BusProviderSendsCommand_Ex");
        var queueName = "BusProviderSendsCommand_Q";

        var factory = RabbitTestHelp.CreateFactoryFrom(busOptions);
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            BasicDeliverEventArgs receivedCommand = null;
            var handle = new AutoResetEvent(false);

            #region Receive Command
            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                receivedCommand = e;
                handle.Set();
            };
            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            #endregion Receive Command

            var command = new CommandRequestMessage(
                serviceQueueName: queueName,
                commandType: "MyCommandType",
                jsonMessage: "{\"testJson\":true}"
            );

            using (var target = new BusProvider(busOptions))
            {
                target.CreateConnection();
                // Act
                target.SendCommandAsync(command);

                // Assert
                bool signalReceived = handle.WaitOne(1000);
                Assert.True(signalReceived);
                Assert.NotNull(receivedCommand);
                Assert.Equal("MyCommandType", receivedCommand.BasicProperties.Type);
                Assert.Equal("{\"testJson\":true}", Encoding.UTF8.GetString(receivedCommand.Body));
            }
        }

        RabbitTestHelp.DeleteQueueAndExchange(busOptions, queueName);
    }

    [Fact]
    public void BusProviderReceivesResponseAfterSendingCommand()
    {
        var busOptions = new BusOptions(exchangeName: "BusProviderReceivesResponseAfterSendingCommand_Ex");
        var serverQueueName = "BusProviderReceivesResponseAfterSendingCommand_Q";

        var factory = RabbitTestHelp.CreateFactoryFrom(busOptions);
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            var responseJsonMessage = "{\"answerJson\":true}";
            #region Receive Command and Send 'responseJsonMessage'
            channel.QueueDeclare(queue: serverQueueName, durable: false, exclusive: false,
                                 autoDelete: false, arguments: null);
            channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var requestProps = ea.BasicProperties;

                // set metadata
                IBasicProperties responseProps = channel.CreateBasicProperties();
                responseProps.CorrelationId = requestProps.CorrelationId;
                // set payload
                var buffer = Encoding.UTF8.GetBytes(responseJsonMessage);
                // send command
                channel.BasicPublish(exchange: "",
                                     routingKey: requestProps.ReplyTo,
                                     basicProperties: responseProps,
                                     body: buffer);
                channel.BasicAck(ea.DeliveryTag, false);     // send acknowledgement
            };
            channel.BasicConsume(queue: serverQueueName, autoAck: false, consumer: consumer);
            #endregion Receive Command

            var commandRequest = new CommandRequestMessage(
                serviceQueueName: serverQueueName,
                commandType: "MyCommandType",
                jsonMessage: "{\"requestJson\":true}"
            );

            using (var target = new BusProvider(busOptions))
            {
                target.CreateConnection();

                // Act
                var commandResponse
                    = target.SendCommandAsync(commandRequest).Result;

                // Assert
                Assert.NotNull(commandResponse);
                Assert.NotNull(commandResponse.CallbackQueueName);
                Assert.NotNull(commandResponse.CorrelationId);
                Assert.Equal("{\"answerJson\":true}", commandResponse.JsonMessage);
            }
        }

        RabbitTestHelp.DeleteQueueAndExchange(busOptions, serverQueueName);
    }

    [Fact]
    public void BusProviderReceivesNoResponseIfCorrelationIsDoesNotMatch()
    {
        var busOptions = new BusOptions(exchangeName: "BusProviderReceivesResponseAfterSendingCommand_Ex");
        var serverQueueName = "BusProviderReceivesResponseAfterSendingCommand_Q";

        var factory = RabbitTestHelp.CreateFactoryFrom(busOptions);
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            var responseJsonMessage = "{\"answerJson\":true}";
            var wrongCorrelationId = Guid.NewGuid().ToString();
            #region Receive Command and Send 'responseJsonMessage'
            channel.QueueDeclare(queue: serverQueueName, durable: false, exclusive: false,
                                 autoDelete: false, arguments: null);
            channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var requestProps = ea.BasicProperties;

                // set metadata
                IBasicProperties responseProps = channel.CreateBasicProperties();
                responseProps.CorrelationId = wrongCorrelationId;   // this is the key line!!
                // set payload
                var buffer = Encoding.UTF8.GetBytes(responseJsonMessage);
                // send command
                channel.BasicPublish(exchange: "",
                                     routingKey: requestProps.ReplyTo,
                                     basicProperties: responseProps,
                                     body: buffer);
                channel.BasicAck(ea.DeliveryTag, false);     // send acknowledgement
            };
            channel.BasicConsume(queue: serverQueueName, autoAck: false, consumer: consumer);
            #endregion Receive Command

            var commandRequest = new CommandRequestMessage(
                serviceQueueName: serverQueueName,
                commandType: "MyCommandType",
                jsonMessage: "{\"requestJson\":true}"
            );

            using (var target = new BusProvider(busOptions))
            {
                target.CreateConnection();

                // Act
                var commandResponseTask
                    = target.SendCommandAsync(commandRequest);
                bool completed = commandResponseTask.Wait(1000);

                // Assert
                Assert.False(completed);
            }
        }

        RabbitTestHelp.DeleteQueueAndExchange(busOptions, serverQueueName);
    }

    [Fact]
    public void BusProviderStartReceivingCommands()
    {
        var busOptions = new BusOptions(exchangeName: "BusProviderStartReceivingCommands_Ex");
        var serverQueueName = "BusProviderStartReceivingCommands_Q";

        var factory = RabbitTestHelp.CreateFactoryFrom(busOptions);
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            using (var target = new BusProvider(busOptions))
            {
                // Arrange
                target.CreateConnection();
                CommandReceivedMessage commandReceivedMessage = null;
                var handle = new AutoResetEvent(false);

                // Act
                CommandReceivedCallback receivingCommandCallback = (CommandReceivedMessage crm) =>
                {
                    commandReceivedMessage = crm;
                    handle.Set();
                    return null;
                };
                target.StartReceivingCommands(serverQueueName, receivingCommandCallback);

                // Send Command
                var command = new CommandRequestMessage(serverQueueName, "MyCommandType", "{\"testJson\":true}");
                target.SendCommandAsync(command);

                // Assert
                bool signalReceived = handle.WaitOne(1000);
                Assert.True(signalReceived);
                Assert.NotNull(commandReceivedMessage);
                Assert.NotNull(commandReceivedMessage.CallbackQueueName);
                Assert.NotNull(commandReceivedMessage.CorrelationId);
                Assert.Equal("MyCommandType", commandReceivedMessage.CommandType);
                Assert.Equal("{\"testJson\":true}", commandReceivedMessage.JsonMessage);
            }
        }
        RabbitTestHelp.DeleteQueueAndExchange(busOptions, serverQueueName);
    }

    [Fact]
    public void BusProviderSendsResponseAfterReceivingCommand()
    {
        var busOptions = new BusOptions(exchangeName: "BusProviderSendsResponseAfterReceivingCommand_Ex");
        var serverQueueName = "BusProviderSendsResponseAfterReceivingCommand_Q";

        var factory = RabbitTestHelp.CreateFactoryFrom(busOptions);
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            using (var target = new BusProvider(busOptions))
            {
                // Arrange
                target.CreateConnection();

                CommandReceivedCallback receivingCommandCallback = (CommandReceivedMessage crm) =>
                {
                    var commandResultMessage = new CommandResultMessage("{\"answerJson\":true}");
                    return commandResultMessage;
                };
                target.StartReceivingCommands(serverQueueName, receivingCommandCallback);

                // Act
                var commandRequest = new CommandRequestMessage(serverQueueName, "MyCommandType", "{\"testJson\":true}");
                var commandResponse = target.SendCommandAsync(commandRequest).Result;

                // Assert
                Assert.NotNull(commandResponse);
                Assert.Equal("{\"answerJson\":true}", commandResponse.JsonMessage);
            }
        }
        RabbitTestHelp.DeleteQueueAndExchange(busOptions, serverQueueName);
    }
}