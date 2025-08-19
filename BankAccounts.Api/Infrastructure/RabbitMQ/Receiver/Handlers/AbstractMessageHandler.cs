using BankAccounts.Api.Infrastructure.Database.Context;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Consumed.Entity;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared;
using BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared.DeadLetter;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BankAccounts.Api.Infrastructure.RabbitMQ.Receiver.Handlers;

/// <summary>
/// Абстрактный класс со вспомогательными методами для обработчиков
/// </summary>
/// <param name="logger"></param>
/// <typeparam name="T"></typeparam>
/// <param name="handlerName">Имя обработчика</param>
public class AbstractMessageHandler<T>(ILogger<T> logger, string handlerName)
{
    private const int MajorMessageVersion = 1;
    private string HandlerName { get; } = handlerName;

    // ReSharper disable once ReturnTypeCanBeNotNullable Может быть null
    private protected async Task<(EventType, Guid, JsonDocument)?> CheckAndSaveIfDeadLetterAck(IChannel channel, IBankAccountsDbContext dbContext, BasicDeliverEventArgs ea, string handler)
    {
        byte[] body = ea.Body.ToArray();
        string message = BytesToString(body);
        JsonDocument document = JsonDocument.Parse(message);

        string? reason = ValidateMessage(ea.BasicProperties, document,
            out EventType? eventType,
            out Guid? messageId);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Может быть null в исключительных случаях
        // ReSharper disable once InvertIf Предлагает непонятный код
        if (reason != null)
        {
            LogDeadLetter(document, eventType, reason, handler);
            await AddToDeadLetters(dbContext, messageId, document.RootElement.GetRawText(), eventType,
                DateTime.UtcNow, handler,
                reason);
            await Ack(channel, ea.DeliveryTag);
            return null;
        }
        return (eventType!.Value, messageId!.Value, document);
    }

    private protected static DateTime? GetTimestamp(BasicDeliverEventArgs ea)
    {
        long propTimestamp = ea.BasicProperties.Timestamp.UnixTime;
        DateTime? timestamp = propTimestamp == 0 ? null : DateTime.FromBinary(propTimestamp);
        return timestamp;
    }

    private protected static async Task Ack(IChannel channel, ulong deliveryTag)
    {
        await channel.BasicAckAsync(deliveryTag, multiple: false);
    }

    // ReSharper disable once MemberCanBePrivate.Global Так нужно
    private protected static async Task Nack(IChannel channel, ulong deliveryTag, bool requeue)
    {
        await channel.BasicNackAsync(deliveryTag: deliveryTag, multiple: false, requeue: requeue);
    }

    private protected static int CheckAlreadyAdded(InboxConsumedEntity? entity)
    {
        int result = 0;

        bool notAdded = entity == null;

        if (notAdded)
        {
            return result;
        }

        result = 1;
        bool alreadyProcessed = entity!.Handler != "None";
        if (alreadyProcessed)
        {
            result = 2;
        }

        return result;
    }

    private static string ValidateMessage(IReadOnlyBasicProperties properties, JsonDocument document,
        out EventType? eventType, out Guid? messageId)
    {
        eventType = null;
        messageId = null;

        bool hasHeaders = false;

        string headerCausationId = null!;
        string headerCorrelationId = null!;

        string version = null!;
        string bodyCausationId = null!;
        string bodyCorrelationId = null!;

        string? reason = null!;

        try
        {
            IDictionary<string, object?> headers = properties.Headers!;
            hasHeaders = headers.Count != 0;

            eventType = Enum.Parse<EventType>(BytesToString(headers["type"]!));
            messageId = Guid.Parse((ReadOnlySpan<char>)properties.MessageId);

            headerCausationId = BytesToString(headers["x-causation-id"]!);
            headerCorrelationId = BytesToString(headers["x-correlation-id"]!);

            version = document.RootElement.GetProperty("meta").GetProperty("version").GetString()!;
            bodyCausationId =
                document.RootElement.GetProperty("meta").GetProperty("causationId").GetString()!;
            bodyCorrelationId = document.RootElement.GetProperty("meta").GetProperty("correlationId")
                .GetString()!;

            if (headerCausationId != bodyCausationId)
            {
                reason = "Causation id mismatch";
            }
            else if (headerCorrelationId != bodyCorrelationId)
            {
                reason = "Correlation id mismatch";
            }
            else if (int.Parse(version.Substring(1, 1)) > MajorMessageVersion) // проверка на соответствие версии
            {
                reason = "Message version not supported";
            }
            else if (eventType is EventType.ClientBlocked or EventType.ClientUnblocked)
            {
                try
                {
                    document.RootElement.GetProperty("clientId").GetGuid();
                }
                catch (Exception)
                {
                    reason = "No client id found";
                }
            }
        }
        catch (Exception)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Может быть null в исключительных случаях
            if (reason is null)
            {
                if (!hasHeaders)
                    reason = "No headers";
                else if (eventType is null)
                    reason = "No event type";
                else if (messageId is null)
                    reason = "No message id";
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Может быть null в исключительных случаях
                else if (headerCausationId is null)
                    reason = "No header causation";
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Может быть null в исключительных случаях
                else if (headerCorrelationId is null)
                    reason = "No header correlation";
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Может быть null в исключительных случаях
                else if (version is null)
                    reason = "No version";
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Может быть null в исключительных случаях
                else if (bodyCausationId is null)
                    reason = "No body causation";
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract Может быть null в исключительных случаях
                else if (bodyCorrelationId is null)
                    reason = "No body correlation";
                else
                    reason = "Unknown reason";
            }
        }

        return reason;
    }

    private static string BytesToString(object bytes)
    {
        return Encoding.UTF8.GetString((byte[])bytes);
    }

    private protected void LogSuccess(JsonDocument document, EventType type, Guid? messageId, DateTime? timestamp, string handler)
    {
        string? id = null;
        string? correlationId = null;
        string? ownerId = null;

        try
        {
            JsonElement root = document.RootElement;

            id = root.GetProperty("eventId").GetString();
            correlationId = root.GetProperty("meta").GetProperty("correlationId").GetString();
            ownerId = root.GetProperty("ownerId").GetString();
        }
        catch (Exception) { /* ignored */ }

        TimeSpan? latency = timestamp == null ? null : DateTime.UtcNow - timestamp;

        logger.LogInformation("[MESSAGE_CONSUMED] Successfully consumed event: id = {id}, ownerId = {owner}, type = {type}, " +
                              "correlationId = {correlationId}, messageId = {MessageId},latency = {latency}, handler = {handler}",
            id, ownerId, type.ToString(), correlationId, messageId, latency, handler);
    }

    private void LogDeadLetter(JsonDocument document, EventType? type, string reason, string handler)
    {
        string? id = null;
        string? correlationId = null;

        try
        {
            JsonElement root = document.RootElement;

            id = root.GetProperty("eventId").GetString();
            correlationId = root.GetProperty("meta").GetProperty("correlationId").GetString();
        }
        catch (Exception) {/* ignored */ }

        logger.LogWarning("Consumed \"dead letter\" event: id = {id}, type = {type}, " +
                          "correlationId = {correlationId}, reason = {reason}, handler = {handler}", id,
            type.ToString(), correlationId, reason, handler);
    }

    // ReSharper disable once MemberCanBePrivate.Global Так нужно
    private protected void LogError(bool redelivered, string error, string handler)
    {
        logger.LogWarning("Error while processing message ({handler}), is requeued: {Requeue}. {Message}"
            , handler, redelivered, error);
    }

    private protected void LogMessageProcessed(Guid messageId, string handler)
    {
        logger.LogInformation("Message with MessageId {MessageId} already processed ({Handler}).", messageId, handler);
    }

    private protected static async Task AddHandledToInbox(IBankAccountsDbContext dbContext, Guid messageId, EventType eventType,
        DateTime processedAt, string handler)
    {
        InboxConsumedEntity inboxConsumed = new()
        {
            MessageId = messageId,
            EventType = eventType,
            ProcessedAt = processedAt,
            Handler = handler
        };

        await dbContext.InboxConsumed.AddAsync(inboxConsumed);
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }

    private protected async Task ErrorNack(IChannel channel, BasicDeliverEventArgs ea, Exception ex)
    {
        await Nack(channel, deliveryTag: ea.DeliveryTag, requeue: !ea.Redelivered);
        LogError(!ea.Redelivered, ex.Message, HandlerName);
        await Task.Delay(1000);
    }

    private async Task AddToDeadLetters(IBankAccountsDbContext dbContext, Guid? messageId, string message,
        EventType? eventType, DateTime receivedAt, string handler, string error)
    {
        if (dbContext.DeadLetters.Where(e => e.MessageId == messageId).ToList().Count != 0)
        {
            logger.LogWarning("Dead letter with messageId = {messageId} is already added to inbox_dead_letters",
                messageId);
            return;
        }
        DeadLetterEntity deadLetter = new()
        {
            MessageId = messageId ?? Guid.NewGuid(),
            ReceivedAt = receivedAt,
            Handler = handler,
            Payload = message,
            EventType = eventType,
            Error = error
        };

        _ = (await dbContext.DeadLetters.AddAsync(deadLetter)).Entity;
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }

}