# BankAccounts

[![.NET](https://img.shields.io/badge/.NET-9.0-blue)]()
[![C#](https://img.shields.io/badge/C%23-13-purple)]()

Этот проект представляет собой микросервис "Bank Accounts" (Сервис банковских счетов) для розничного банка. Он предоставляет API для управления банковскими счетами, транзакциями и выписками.

Сервис реализует надежный обмен событиями с другими системами банка через **RabbitMQ**, используя паттерны **Transactional Outbox** (для публикации) и **Inbox** (для потребления) для обеспечения надежной доставки сообщений без потерь и дубликатов. Реализована **идемпотентность** обработки входящих сообщений.

## 📚 Основные функции

- Создание и управление счетами (текущие, депозитные).
- Проведение транзакций (пополнение, списание, переводы между счетами).
- Начисление процентов по депозитным счетам (ежедневно через Hangfire).
- Генерация выписок.
- Потребление событий блокировки/разблокировки клиентов (`ClientBlocked`/`ClientUnblocked`) от внешних систем (например, Antifraud) для ограничения операций.
- Публикация доменных событий (`AccountOpened`, `MoneyCredited`, `MoneyDebited`, `TransferCompleted`, `InterestAccrued`) в RabbitMQ.
- Интеграционные тесты с использованием Testcontainers.

## 📦 Архитектура обмена сообщениями (RabbitMQ)

Сервис использует RabbitMQ для обмена сообщениями.

### Топология

- **Exchange:** `account.events` (тип: `topic`)
- **Очереди и маршрутизация:**
  - `account.crm`: Привязана к `account.events` с routing key `account.*`. Предназначена для получения событий, связанных с открытием счетов (например, `AccountOpened`).
  - `account.notifications`: Привязана к `account.events` с routing key `money.*`. Предназначена для получения событий, связанных с деньгами (например, `MoneyCredited`, `MoneyDebited`, `TransferCompleted`).
  - `account.antifraud`: Привязана к `account.events` с routing key `client.*`. Используется сервисом "Счета" для получения событий блокировки/разблокировки клиентов (`ClientBlocked`, `ClientUnblocked`).
  - `account.audit`: Привязана к `account.events` с routing key `#` (получает все сообщения). Используется для аудита.

### Поток событий

1. **Публикация:** При выполнении бизнес-операций (открытие счета, транзакция и т.д.) соответствующие события публикуются в exchange `account.events` с соответствующими routing keys.
2. **Потребление:** Сервис потребляет события `ClientBlocked`/`ClientUnblocked` из очереди `account.antifraud`, чтобы блокировать/разблокировать возможность операций для клиентов.

## 🚀 Запуск сервиса
### Запуск с помощью Docker Compose (Рекомендуется)
>**Примечание:** Если страница сервиса не доступна после запуска - отключите VPN и перезапустите сервис.

1. Откройте терминал в корневой директории проекта (где находится `docker-compose.yml`).
2. Выполните команду:

```bash
   docker-compose up --build
```

   Это соберет Docker-образы и запустит контейнеры для сервиса банковских счетов, сервиса Identity, PostgreSQL и RabbitMQ.\
3. После запуска сервисы будут доступны:
   - **API сервиса "BankAccounts":** [http://localhost](http://localhost)
   - **Сервис Identity:** [http://localhost:7045](http://localhost:7045)
   - **RabbitMQ Management UI:** [http://localhost:15672](http://localhost:15672) (Логин: `admin`, Пароль: `admin` - см. `docker-compose.yml`)
   - **Hangfire Dashboard:** [http://localhost/hangfire](http://localhost/hangfire)
4. Для остановки используйте `Ctrl+C` и команду:
```bash
    docker-compose down
```


>**Если проект не запускается** попробуйте остановить docker-compose и пересобрать образы (в корневой папке проекта):
>
>```bash
>docker-compose down -v
>docker-compose up --build
>```
>**Другой вариант запуска** - выберите `docker-compose` в списке конфигураций Visual Studio и запустите его:
>
><img width="298" height="126" alt="image" src="https://github.com/user-attachments/assets/3d809f2a-7c94-47de-8404-353b9f8ca46c" />
>
> ### "Error response from daemon: Conflict."
>В Случае ошибки **`"Error response from daemon: Conflict. The container name "/bankaccounts_db" is already in use..."`** удалите все контейнеры в Docker Desktop и перезапустите сервис.


## 🧪 Тестирование

Проект включает unit-тесты и интеграционные тесты.

Для запуска тестов используйте команду в корневой директории проекта:
```bash
docker-compose pull
docker-compose build
dotnet test bankaccounts.tests --logger "console;verbosity=detailed"
```
Интеграционные тесты используют Testcontainers для автоматического запуска PostgreSQL и RabbitMQ.
>**Примечание:** Если интеграционные тесты завершаются с ошибкой (`502 BadGateway`):
> - Отключите VPN
> - Удалите запущенные контейнеры в Docker Desktop 
> - Соберите заново образы контейнеров с помощью выполненной в корне проекта команды:
>```
> docker-compose build
>```

## 📁 Структура проекта

- `BankAccounts.Api/`: Основной сервис банковских счетов.
  - `Features/`: Функциональные модули (Accounts, Transactions).
  - `Infrastructure/`: Конфигурация БД, репозитории, сервисы, Hangfire.
  - `Common/`: Общие классы (ошибки, валидация, MbResult).
  - `Middleware/`: Пользовательские middleware (CustomExceptionHandlerMiddleware).
  - `Migrations/`: Миграции Entity Framework Core.
- `BankAccounts.Identity/`: Сервис аутентификации и авторизации.
  - `Identity/`: Контроллеры, конфигурация IdentityServer.
- `BankAccounts.Tests/`: Проект с unit и интеграционными тестами.
  - `Unit/`: Тесты для контроллеров, обработчиков, валидаторов, репозиториев.
  - `Integration/`: Интеграционные тесты с использованием Testcontainers.

## ⚙️ Хранение и обработка данных, PostgreSQL

### Индексы и оптимизация

Для повышения производительности в базе данных используются индексы:

- Хэш-индекс по `OwnerId` в таблице `Accounts`.
- B-Tree индекс по `(AccountId, DateTime)` в таблице `Transactions`.
- GiST индекс по `DateTime` в таблице `Transactions` для оптимизации диапазонных запросов.

### Перевод денежных средств

Для реализации потокобезопасного поведения при проведении банковских переводов между счетами в базе данных осуществлена **оптимистичная блокировка** с использованием системного поля `xmin`. Такая блокировка исключает одновременное редактирование записей из разных потоков. В классе `PerformTransferHandler` при работе с базой данных используется встроенный в **Entity Framework** механизм транзакций, который в случае ошибки конкурентного доступа отменяет произведенные изменения.

## Технологии и особенности

- **Язык:** C# 13 (.NET 9)
- **Архитектура:** Vertical Slice Architecture, CQRS, MediatR
- **База данных:** PostgreSQL (EF Core, миграции, индексы, хранимая процедура) в контейнере Docker
- **Очереди сообщений:** RabbitMQ (AMQP 0.9.1)
- **Паттерны:** Transactional Outbox, Inbox, Idempotency, Optimistic Concurrency Control (xmin)
- **Аутентификация/Авторизация:** JWT (самописный сервис на базе IdentityServer4)
- **Контейнеризация:** Docker, Docker Compose
- **Документация API:** Swagger/OpenAPI (Swashbuckle)
- **Фоновые задачи:** Hangfire
- **Логирование:** Serilog
- **Тестирование:** xUnit, Moq, Testcontainers (интеграционные тесты с реальной БД)
- **Валидация:** FluentValidation
- **Обработка ошибок:** Custom Middleware

### Особенности проекта

- Использование единого типа `MbResult` для ответов сервиса с возможностью возврата сообщения об ошибке или результата запроса.
- Транзакции и "оптимистичная блокировка" при переводе денежных средств между счетами, в случае конфликта возвращается код `409 Conflict`.
- Хранимая процедура для начисления процентов, выполняемая ежедневно.
- Использование набора индексов для ускорения и оптимизации работы с данными

