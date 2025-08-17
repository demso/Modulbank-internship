namespace BankAccounts.Api.Infrastructure.RabbitMQ.Events.Shared
{
    /// <summary>
    /// Тип события <see cref="Event"/>
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// Счет открыт
        /// </summary>
        AccountOpened,
        /// <summary>
        /// Начислены проценты
        /// </summary>
        InterestAccrued,
        /// <summary>
        /// Счет пополнен
        /// </summary>
        MoneyCredited,
        /// <summary>
        /// Снятие средств со счета
        /// </summary>
        MoneyDebited,
        /// <summary>
        /// Произведен трансфер средств
        /// </summary>
        TransferCompleted,
        /// <summary>
        /// Клиент заблокирован
        /// </summary>
        ClientBlocked,
        /// <summary>
        /// Клиент разблокирован
        /// </summary>
        ClientUnblocked
    }
 }