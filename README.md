# Крестики-нолики NxN

RESTful API для игры в крестики-нолики. Проект разработан на .NET 9 с использованием PostgreSQL через EF Core.

## Архитектура

Проект разделен на четыре слоя:

### Domain
- Модели: Game, Player, Cell
- Перечисления: GameStatus, CellValue

### DataAccess
- Сущности: GameEntity, PlayerEntity, CellEntity
- Конфигурации для EF Core
- GameDbContext
- Репозиторий IGameRepository
- Оптимистичная блокировка через RowVersion

### Application
- DTO: CreateGameRequest, MakeMoveRequest, GameResponse
- Сервисы: IGameService, GameService
- Реализация игровой логики
- Идемпотентность через ETag

### API
- GameController с REST endpoints
- Swagger документация
- Обработка ошибок через ProblemDetails

## API Endpoints

### POST /api/game
Создание новой игры.

Запрос:
```json
{
    "size": 3
}
```

### GET /api/game/{id}
Получение состояния игры.

### POST /api/game/{id}/moves
Выполнение хода.

Запрос:
```json
{
    "row": 0,
    "column": 0,
    "player": "X"
}
```

### GET /health
Проверка работоспособности сервиса.

## Требования для запуска

- .NET 9 SDK
- Docker Desktop
- Visual Studio 2022 (опционально)

## Запуск приложения

1. Клонируйте репозиторий:
```bash
git clone https://github.com/cofheim/ModulBank
cd ModulBank
```

2. Запустите Docker Desktop и дождитесь его полной загрузки

3. Запустите инфраструктуру через Docker Compose:
```bash
docker-compose up -d
```

4. Примените миграции базы данных:
```bash
dotnet ef database update -p ModulBank.DataAccess -s ModulBank
```

5. Запустите приложение:
```bash
cd ModulBank
dotnet run
```

Приложение будет доступно по адресу: http://localhost:8080

Swagger UI доступен по адресу: http://localhost:8080/swagger

## Конфигурация

Основные настройки находятся в `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=modulbank_db;Username=postgres;Password=postgres"
  },
  "Game": {
    "DefaultSize": 3,
    "WinCondition": 3
  }
}
```

## Запуск тестов

```bash
dotnet test
```

## Docker

Проект включает multi-stage Dockerfile для оптимальной сборки:

```bash
docker build -t modulbank-game .
docker run -p 8080:8080 modulbank-game
```

## Особенности реализации

1. **Concurrency & Идемпотентность**
   - Оптимистичная блокировка через RowVersion
   - ETag для идемпотентных операций
   - Обработка параллельных ходов

2. **Persistence & Crash-safe**
   - Сохранение сессии игрока
   - Восстановление состояния после перезапуска
   - Транзакционная целостность

3. **Безопасность**
   - Валидация входных данных
   - Защита от некорректных ходов
   - Обработка ошибок по RFC 7807

4. **Тестирование**
   - Unit тесты для бизнес-логики
   - Интеграционные тесты для API
   - Тесты для проверки специальных правил
   - In-memory база данных для тестов 