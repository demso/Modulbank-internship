## Примеры сообщений для тестирования работы RabbitMQ

Все операции при тестировании выполнялись с использованием пользователя с username "string", его идентификатор прописан в сообщениях.

##### Идентификатор (username "string")

`0452b2ec-5e4b-f650-b9ee-78ec1a129047`

##### Токен аутентификации (истекает через несколько лет)

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiI0ZDcyYTE3NS0xNDczLTQ2OTUtOWNjZC01NjNlOWFhMmM1MGIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic3RyaW5nIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIwNDUyYjJlYy01ZTRiLWY2NTAtYjllZS03OGVjMWExMjkwNDciLCJleHAiOjE3NTU2MjMyMDcsImlzcyI6IkJhbmtBY2NvdW50QXV0aG9yaXphdGlvbiIsImF1ZCI6IkJhbmtBY2NvdW50c1dlYkFQSSJ9.bUaOzAkN8IS_-el7EkaPgAz3eKyZxlsmeGuW7SDb2wI
```

### ClientBlocked
```
Routing key: client.blocked
HEADERS:
x-correlation-id  0452b2ec-5e4b-f650-b9ee-78ec1a128888
x-causation-id	  0452b2ec-5e4b-f650-b9ee-78ec1a130247
type			  ClientBlocked
PROPERTIES:
message_id		  0472b2vc-0e4b-f650-b9ee-78ec1a130247

{
  "EventId": "0452b2ec-5e4b-f650-b9ee-78ec1a129043",
  "OccuredAt": "2024-01-15T14:30:45.1234567Z",
  "ClientId": "0452b2ec-5e4b-f650-b9ee-78ec1a129047",
  "Metadata": {
    "CausationId": "0452b2ec-5e4b-f650-b9ee-78ec1a130247",
    "CorrelationId": "0452b2ec-5e4b-f650-b9ee-78ec1a128888",
    "Version": "v1",
    "Source": "account-service"
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
message_id		  0472b2vc-0e9b-f650-b9ee-78ec1a130247

{
  "EventId": "0452b2ec-5e4b-f650-b9ee-78ec1a129043",
  "OccuredAt": "2024-01-15T14:30:45.1234567Z",
  "ClientId": "0452b2ec-5e4b-f650-b9ee-78ec1a129047",
  "Metadata": {
    "CausationId": "0452b2ec-5e4b-f650-b9ee-78ec1a130247",
    "CorrelationId": "0452b2ec-5e4b-f650-b9ee-78ec1a128888",
    "Version": "v1",
    "Source": "account-service"
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
message_id		  0472b2ec-0e4b-f650-b9ee-78ec1a130247

{
  "AccountId": 1,
  "OwnerId": "0452b2ec-5e4b-f650-b9ee-78ec1a129047",
  "Currency": "Rub",
  "AccountType": "Checking",
  "EventId": "0452b2ec-5e4b-f650-b9ee-78ec1a129043",
  "OccuredAt": "2024-01-15T14:30:45.1234567Z",
  "Metadata": {
    "CausationId": "0452b2ec-5e4b-f650-b9ee-78ec1a130247",
    "CorrelationId": "0452b2ec-5e4b-f650-b9ee-78ec1a128888",
    "Version": "v1",
    "Source": "account-service"
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
message_id		  0479b2ec-0e4b-f650-b9ee-78ec1a130247

{
  "AccountId": 1,
  "PeriodFrom": "2024-01-15",
  "PeriodTo": "2025-10-15",
  "Amount": 300.43,
  "EventId": "0452b2ec-5e4b-f650-b9ee-78ec1a129043",
  "OccuredAt": "2024-01-15T14:30:45.1234567Z",
  "Metadata": {
    "CausationId": "0452b2ec-5e4b-f650-b9ee-78ec1a130247",
    "CorrelationId": "0452b2ec-5e4b-f650-b9ee-78ec1a128888",
    "Version": "v1",
    "Source": "account-service"
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
message_id		  0472b2ec-1e4b-f680-b9ee-78yc1a130247

{
  "AccountId": 1,
  "OwnerId": "0452b2ec-5e4b-f650-b9ee-78ec1a129047",
  "Currency": "Rub",
  "EventId": "0452b2ec-5e4b-f650-b9ee-78ec1a129043",
  "OccuredAt": "2024-01-15T14:30:45.1234567Z",
  "Amount": 200.56,
  "OperationId": 0452b2ec-5e4b-f350-b9ee-78ec1a129043,
  "Metadata": {
    "CausationId": "0452b2ec-5e4b-f650-b9ee-78ec1a130247",
    "CorrelationId": "0452b2ec-5e4b-f650-b9ee-78ec1a128888",
    "Version": "v1",
    "Source": "account-service"
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

{
  "AccountId": 1,
  "OwnerId": "0452b2ec-5e4b-f650-b9ee-78ec1a129047",
  "Currency": "Rub",
  "Reason": "reason",
  "EventId": "0452b2ec-5e4b-f650-b9ee-78ec1a129043",
  "OccuredAt": "2024-01-15T14:30:45.1234567Z",
  "Amount": 200.56,
  "OperationId": 0452b2ec-5e4b-f350-b9ee-78ec1a129043,
  "Metadata": {
    "CausationId": "0452b2ec-5e4b-f650-b9ee-78ec1a130247",
    "CorrelationId": "0452b2ec-5e4b-f650-b9ee-78ec1a128888",
    "Version": "v1",
    "Source": "account-service"
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
message_id		  0470b2ec-1e4b-m680-b5ee-78ec2a130247

{
  "SourceAccountId": 1,
  "DestinationAccountId": 2,
  "OwnerId": "0452b2ec-5e4b-f650-b9ee-78ec1a129047",
  "Currency": "Rub",
  "Reason": "reason",
  "EventId": "0452b2ec-5e4b-f650-b9ee-78ec1a129043",
  "OccuredAt": "2024-01-15T14:30:45.1234567Z",
  "Amount": 200.56,
  "OperationId": 0452b2ec-5e4b-f350-b9ee-78ec1a129043,
  "Metadata": {
    "CausationId": "0452b2ec-5e4b-f650-b9ee-78ec1a130247",
    "CorrelationId": "0452b2ec-5e4b-f650-b9ee-78ec1a128888",
    "Version": "v1",
    "Source": "account-service"
  } 
}
```
