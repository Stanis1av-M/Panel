-- =====================================================================
-- СКРИПТ РУЧНЫХ ИЗМЕНЕНИЙ БД ДЛЯ ApexDB
-- =====================================================================
-- Этот файл НЕ выполняется программой автоматически. Чтобы изменения
-- попали в базу, нужно вручную:
--   1. Открыть SQL Server Management Studio (SSMS) или Azure Data Studio
--   2. Подключиться к своей базе (Server=(localdb)\mssqllocaldb или твой сервер)
--   3. Открыть этот файл (или скопировать текст) и нажать "Execute" / F5
--
-- ЕСЛИ ПОЛУЧАЕШЬ ОШИБКУ "Database 'ApexDB' does not exist":
-- это значит, что на сервере, к которому ты подключился в SSMS, базы с
-- таким именем ещё нет — либо проект её ещё не создал, либо ты подключён
-- к другому серверу/instance, чем тот, что указан в AppDbContext.cs
-- (Server=(localdb)\mssqllocaldb). Сначала запусти сам проект (Visual
-- Studio) хотя бы раз — если в нём есть код создания БД/миграций, она
-- появится. Либо в SSMS подключись именно к "(localdb)\mssqllocaldb" —
-- это и есть строка подключения из AppDbContext.cs.
--
-- Я проверил структуру по твоему свежему дампу (111.sql) — таблица
-- AiSettings там отсутствует, и новых способов доставки тоже нет.
-- Значит этот скрипт ещё не выполнялся — ниже всё актуально.
-- =====================================================================

IF DB_ID('ApexDB') IS NULL
BEGIN
    RAISERROR('База ApexDB не найдена на этом сервере. Проверь подключение в SSMS — должно быть к (localdb)\mssqllocaldb, как указано в Models/AppDbContext.cs. Если базы там тоже нет — запусти сначала сам проект в Visual Studio, чтобы она создалась.', 16, 1)
END
GO

USE [ApexDB]
GO


-- =====================================================================
-- ЧАСТЬ 1. Таблица AiSettings (для окна "Чат с нейросетью")
-- =====================================================================
-- Хранит один API-ключ (и опционально провайдера/URL), который вводится
-- в окне чата и сохраняется через EF Core. Без этой таблицы кнопка
-- "Сохранить" в окне чата выдаст ошибку (таблицы не существует).

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AiSettings')
BEGIN
    CREATE TABLE [dbo].[AiSettings](
        [AiSettingId] [int] IDENTITY(1,1) NOT NULL,
        [ProviderName] [nvarchar](100) NULL,
        [ApiKey] [nvarchar](500) NULL,
        [ApiUrl] [nvarchar](500) NULL,
        [UpdatedAt] [datetime2](7) NOT NULL,
     CONSTRAINT [PK_AiSettings] PRIMARY KEY CLUSTERED
    (
        [AiSettingId] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]

    PRINT 'Таблица AiSettings создана.'
END
ELSE
BEGIN
    PRINT 'Таблица AiSettings уже существует — пропускаю.'
END
GO

-- Пояснение по содержимому таблицы:
-- Строка в этой таблице появится САМА, когда ты первый раз нажмёшь
-- "Сохранить" в окне чата и введёшь любой ключ. Заранее ничего вставлять
-- не нужно. Если всё же хочешь создать пустую строку заранее — пример:
--
-- INSERT INTO [dbo].[AiSettings] ([ProviderName], [ApiKey], [ApiUrl], [UpdatedAt])
-- VALUES (NULL, NULL, NULL, GETDATE())


-- =====================================================================
-- ЧАСТЬ 2. Новые способы доставки в DeliveryMethods
-- =====================================================================
-- Сейчас в таблице (судя по дампу) есть только одна запись с битой
-- кодировкой: Id=1, Name='?????????' — похоже на "Самовывоз", но текст
-- испорчен при экспорте дампа (это не моя вставка, она была у тебя и до
-- наших изменений). Ниже добавляю два новых варианта для выбора в корзине.

-- Исправляем старую запись с битой кодировкой ('?????????') на нормальный текст.
-- Эта запись была у тебя ещё до наших изменений — судя по контексту (единственный
-- способ доставки с ценой 0), это был "Самовывоз".
IF EXISTS (SELECT * FROM [dbo].[DeliveryMethods] WHERE [DeliveryMethodId] = 1 AND [Name] = N'?????????')
BEGIN
    UPDATE [dbo].[DeliveryMethods] SET [Name] = N'Самовывоз' WHERE [DeliveryMethodId] = 1
    PRINT 'Запись Id=1 переименована в "Самовывоз".'
END
ELSE
BEGIN
    PRINT 'Запись с битой кодировкой не найдена (возможно уже переименована) — пропускаю.'
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[DeliveryMethods] WHERE [Name] = N'Доставка на дом')
BEGIN
    INSERT INTO [dbo].[DeliveryMethods] ([Name], [Price], [IsActive])
    VALUES (N'Доставка на дом', 500.00, 1)

    PRINT 'Добавлена доставка: Доставка на дом (500 руб.)'
END
ELSE
BEGIN
    PRINT 'Способ "Доставка на дом" уже есть — пропускаю.'
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[DeliveryMethods] WHERE [Name] = N'Пункт выдачи')
BEGIN
    INSERT INTO [dbo].[DeliveryMethods] ([Name], [Price], [IsActive])
    VALUES (N'Пункт выдачи', 250.00, 1)

    PRINT 'Добавлена доставка: Пункт выдачи (250 руб.)'
END
ELSE
BEGIN
    PRINT 'Способ "Пункт выдачи" уже есть — пропускаю.'
END
GO


-- =====================================================================
-- ПРОВЕРКА РЕЗУЛЬТАТА
-- =====================================================================
-- После выполнения всего скрипта эти две команды покажут, что получилось:

SELECT * FROM [dbo].[AiSettings]
SELECT * FROM [dbo].[DeliveryMethods]
GO
