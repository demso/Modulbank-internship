## Примеры сообщений для тестирования работы RabbitMQ

Все операции при тестировании выполнялись с использованием пользователя с username "string", его идентификатор прописан в сообщениях.

##### Идентификатор (username "string")

`0452b2ec-5e4b-f650-b9ee-78ec1a129047`

##### Токен

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJlZmY1MmU2NS1hNTA3LTQzY2UtYWQxNS03OGRkOTJjNDJiMTkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic3RyaW5nIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIwNDUyYjJlYy01ZTRiLWY2NTAtYjllZS03OGVjMWExMjkwNDciLCJleHAiOjI1MTMwNjU1ODEsImlzcyI6IkJhbmtBY2NvdW50QXV0aG9yaXphdGlvbiIsImF1ZCI6IkJhbmtBY2NvdW50c1dlYkFQSSJ9.TTQN6qtqZi7UY1N_qoSUIEva_zID9_VHSKE4uUMTV9w
```

#### Пример заполнения сообщения

![ClientBlocked message](https://github.com/user-attachments/assets/ff56ebc0-3cb2-45ee-a588-c4a65e8f0868)

### ClientBlocked
```
Routing key: client.blocked
HEADERS:
x-correlation-id  0452b2ec-5e4b-f650-b9ee-78ec1a128888
x-causation-id	  0452b2ec-5e4b-f650-b9ee-78ec1a130247
type			  ClientBlocked
PROPERTIES:
message_id		  a8bc1dca-b39e-426f-83b7-bdb3d9a7e16e
```
```json
{
  "eventId": "0452b2ec-5e4b-f650-b9ee-78ec1a129043",
  "occuredAt": "2024-01-15T14:30:45.1234567Z",
  "clientId": "0452b2ec-5e4b-f650-b9ee-78ec1a129047",
  "meta": {
    "causationId": "0452b2ec-5e4b-f650-b9ee-78ec1a130247",
    "correlationId": "0452b2ec-5e4b-f650-b9ee-78ec1a128888",
    "version": "v1",
    "source": "account-service"
  } 
}
```
### ClientUnblocked

```
Routing key: client.unblocked
HEADERS:
x-correlation-id  0452b2ec-5e4b-f650-b9ee-78ec1a128888
x-causation-id	  0452b2ec-5e4b-f650-b9ee-78ec1a130247
type			  ClientUnblocked
PROPERTIES:
message_id		  0452b2ec-5e4b-f650-b9ee-78ec1a129047
```
```json
{
  "eventId": "0452b2ec-5e4b-f650-b9ee-78ec1a129043",
  "occuredAt": "2024-01-15T14:30:45.1234567Z",
  "clientId": "0452b2ec-5e4b-f650-b9ee-78ec1a129047",
  "meta": {
    "causationId": "0452b2ec-5e4b-f650-b9ee-78ec1a130247",
    "correlationId": "0452b2ec-5e4b-f650-b9ee-78ec1a128888",
    "version": "v1",
    "source": "account-service"
  } 
}
```
### AccountOpened
```
Routing key: account.opened
HEADERS:
x-correlation-id  0452b2ec-5e4b-f650-b9ee-78ec1a128888
x-causation-id	  0452b2ec-5e4b-f650-b9ee-78ec1a130247
type			  AccountOpened
PROPERTIES:
message_id		  0452b2ec-5e4b-f650-b9ee-08ec1a130247
```
```json
{
  "accountId": 1,
  "ownerId": "0452b2ec-5e4b-f650-b9ee-78ec1a129047",
  "currency": "Rub",
  "accountType": "Checking",
  "eventId": "0452b2ec-5e4b-f650-b9ee-78ec1a129043",
  "occuredAt": "2024-01-15T14:30:45.1234567Z",
  "meta": {
    "causationId": "0452b2ec-5e4b-f650-b9ee-78ec1a130247",
    "correlationId": "0452b2ec-5e4b-f650-b9ee-78ec1a128888",
    "version": "v1",
    "source": "account-service"
  } 
}
```
### InterestAccrued
```
Routing key: money.interest.accrued
HEADERS:
x-correlation-id  0452b2ec-5e4b-f650-b9ee-78ec1a128888
x-causation-id	  0452b2ec-5e4b-f650-b9ee-78ec1a130247
type			  InterestAccrued
PROPERTIES:
message_id		  0479b2ec-0e4b-f650-b9ee-78ec1a133347
```
```json
{
  "accountId": 1,
  "PeriodFrom": "2024-01-15",
  "PeriodTo": "2025-10-15",
  "amount": 300.43,
  "eventId": "0452b2ec-5e4b-f650-b9ee-78ec1a129043",
  "occuredAt": "2024-01-15T14:30:45.1234567Z",
  "meta": {
    "causationId": "0452b2ec-5e4b-f650-b9ee-78ec1a130247",
    "correlationId": "0452b2ec-5e4b-f650-b9ee-78ec1a128888",
    "version": "v1",
    "source": "account-service"
  } 
}
```
### MoneyCredited
```
Routing key: money.credited
HEADERS:
x-correlation-id  0452b2ec-5e4b-f650-b9ee-78ec1a128888
x-causation-id	  0452b2ec-5e4b-f650-b9ee-78ec1a130247
type			  MoneyCredited
PROPERTIES:
message_id		  0452b2ec-5e4b-f650-b9ee-78ec1a538247
timestamp         5250597465388560401
```
```json
{
  "accountId": 1,
  "ownerId": "0452b2ec-5e4b-f650-b9ee-78ec1a129047",
  "currency": "Rub",
  "eventId": "0452b2ec-5e4b-f650-b9ee-78ec1a129043",
  "occuredAt": "2024-01-15T14:30:45.1234567Z",
  "amount": 200.56,
  "operationId": "0452b2ec-5e4b-f350-b9ee-78ec1a129043",
  "meta": {
    "causationId": "0452b2ec-5e4b-f650-b9ee-78ec1a130247",
    "correlationId": "0452b2ec-5e4b-f650-b9ee-78ec1a128888",
    "version": "v1",
    "source": "account-service"
  } 
}
```

### MoneyDebited
```
Routing key: money.debited
HEADERS:
x-correlation-id  0452b2ec-5e4b-f650-b9ee-78ec1a128888
x-causation-id	  0452b2ec-5e4b-f650-b9ee-78ec1a130247
type			  MoneyDebited
PROPERTIES:
message_id		  0472b2ec-1e4b-f680-b5ee-78ec2a130247
```
```json
{
  "accountId": 1,
  "ownerId": "0452b2ec-5e4b-f650-b9ee-78ec1a129047",
  "currency": "Rub",
  "reason": "reason",
  "eventId": "0452b2ec-5e4b-f650-b9ee-78ec1a129043",
  "occuredAt": "2024-01-15T14:30:45.1234567Z",
  "amount": 200.56,
  "operationId": "0452b2ec-5e4b-f350-b9ee-78ec1a129043",
  "meta": {
    "causationId": "0452b2ec-5e4b-f650-b9ee-78ec1a130247",
    "correlationId": "0452b2ec-5e4b-f650-b9ee-78ec1a128888",
    "version": "v1",
    "source": "account-service"
  } 
}
```

### TransferCompleted

```
Routing key: transfer.completed
HEADERS:
x-correlation-id  0452b2ec-5e4b-f650-b9ee-78ec1a128888
x-causation-id	  0452b2ec-5e4b-f650-b9ee-78ec1a130247
type			  TransferCompleted
PROPERTIES:
message_id		  0452b2ec-5e4b-f650-b9ff-78ec1a924888
```
```json
{
  "sourceaccountId": 1,
  "destinationaccountId": 2,
  "ownerId": "0452b2ec-5e4b-f650-b9ee-78ec1a129047",
  "currency": "Rub",
  "eventId": "0452b2ec-5e4b-f650-b9ee-78ec1a129043",
  "occuredAt": "2024-01-15T14:30:45.1234567Z",
  "amount": 200.56,
  "transferId": "0452b2ec-5e4b-f350-b9ee-78ec1a129043",
  "meta": {
    "causationId": "0452b2ec-5e4b-f650-b9ee-78ec1a130247",
    "correlationId": "0452b2ec-5e4b-f650-b9ee-78ec1a128888",
    "version": "v1",
    "source": "account-service"
  } 
}
```
