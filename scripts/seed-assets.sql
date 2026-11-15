-- ============================================================================
-- Hi-Trade Platform — Asset Seed Script
-- Populates the [Assets] table with the current platform assets / stock names.
-- Idempotent: safe to run multiple times without duplicates or errors.
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'HiTrade')
BEGIN
    PRINT 'Creating database [HiTrade]...';
    CREATE DATABASE [HiTrade];
END;
GO

USE [HiTrade];
GO

-- Ensure Assets table exists before seeding
IF OBJECT_ID(N'[dbo].[Assets]', N'U') IS NULL
BEGIN
    PRINT 'Table [dbo].[Assets] does not exist. Creating table structure...';
    CREATE TABLE [dbo].[Assets] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Ticker] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [CurrentPrice] decimal(18,6) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [Enabled] bit NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Assets] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Assets_Ticker] ON [dbo].[Assets] ([Ticker]);
    PRINT 'Table [dbo].[Assets] created successfully.';
END;
GO

PRINT 'Seeding current assets into [dbo].[Assets]...';

MERGE INTO [dbo].[Assets] AS target
USING (VALUES
    (N'AAPL',  N'Apple Inc.',                180.000000),
    (N'MSFT',  N'Microsoft Corporation',     420.000000),
    (N'GOOGL', N'Alphabet Inc.',             175.000000),
    (N'AMZN',  N'Amazon.com Inc.',           185.000000),
    (N'TSLA',  N'Tesla Inc.',                210.000000),
    (N'NVDA',  N'NVIDIA Corporation',        120.000000),
    (N'META',  N'Meta Platforms Inc.',       500.000000),
    (N'NFLX',  N'Netflix Inc.',              620.000000),
    (N'BRK.A', N'Berkshire Hathaway Inc.', 650000.000000),
    (N'NIO',   N'NIO Inc.',                    5.500000)
) AS source ([Ticker], [Name], [CurrentPrice])
ON target.[Ticker] = source.[Ticker]
WHEN MATCHED AND (target.[IsDeleted] = 1 OR target.[Enabled] = 0) THEN
    UPDATE SET 
        target.[Enabled] = 1, 
        target.[IsDeleted] = 0, 
        target.[UpdatedAt] = SYSUTCDATETIME()
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Ticker], [Name], [CurrentPrice], [CreatedAt], [UpdatedAt], [Enabled], [IsDeleted])
    VALUES (source.[Ticker], source.[Name], source.[CurrentPrice], SYSUTCDATETIME(), SYSUTCDATETIME(), 1, 0);

PRINT 'Asset seeding completed successfully.';
PRINT 'Current assets in platform:';

SELECT 
    [Id], 
    [Ticker], 
    [Name], 
    [CurrentPrice], 
    [Enabled], 
    [IsDeleted]
FROM [dbo].[Assets]
ORDER BY [Ticker];
GO
