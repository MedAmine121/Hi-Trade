IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'HiTrade')
BEGIN
    PRINT 'Creating database [HiTrade]...';
    CREATE DATABASE [HiTrade];
END;
GO

USE [HiTrade];
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428234655_Initial'
)
BEGIN
    CREATE TABLE [Assets] (
        [Id] int NOT NULL IDENTITY,
        [Ticker] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [CurrentPrice] decimal(18,6) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [Enabled] bit NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Assets] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428234655_Initial'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [Email] nvarchar(max) NOT NULL,
        [Password] nvarchar(max) NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [ProfilePictureUrl] nvarchar(max) NOT NULL,
        [Balance] decimal(18,6) NOT NULL,
        [Role] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [Enabled] bit NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428234655_Initial'
)
BEGIN
    CREATE TABLE [Portfolios] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [UserId] int NOT NULL,
        [TotalRealizedGainLoss] decimal(18,6) NOT NULL,
        [PositionId] int NOT NULL,
        [NetInvested] decimal(18,6) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [Enabled] bit NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Portfolios] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Portfolios_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428234655_Initial'
)
BEGIN
    CREATE TABLE [Position] (
        [Id] int NOT NULL IDENTITY,
        [AssetId] int NOT NULL,
        [PortfolioId] int NOT NULL,
        [Quantity] int NOT NULL,
        [AveragePrice] decimal(18,6) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [Enabled] bit NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Position] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Position_Assets_AssetId] FOREIGN KEY ([AssetId]) REFERENCES [Assets] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Position_Portfolios_PortfolioId] FOREIGN KEY ([PortfolioId]) REFERENCES [Portfolios] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428234655_Initial'
)
BEGIN
    CREATE TABLE [Transactions] (
        [Id] int NOT NULL IDENTITY,
        [PortfolioId] int NOT NULL,
        [AssetId] int NOT NULL,
        [Quantity] decimal(18,6) NOT NULL,
        [PriceAtPurchase] decimal(18,6) NOT NULL,
        [Type] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [Enabled] bit NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Transactions_Assets_AssetId] FOREIGN KEY ([AssetId]) REFERENCES [Assets] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Transactions_Portfolios_PortfolioId] FOREIGN KEY ([PortfolioId]) REFERENCES [Portfolios] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428234655_Initial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Assets_Ticker] ON [Assets] ([Ticker]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428234655_Initial'
)
BEGIN
    CREATE INDEX [IX_Portfolios_UserId] ON [Portfolios] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428234655_Initial'
)
BEGIN
    CREATE INDEX [IX_Position_AssetId] ON [Position] ([AssetId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428234655_Initial'
)
BEGIN
    CREATE INDEX [IX_Position_PortfolioId] ON [Position] ([PortfolioId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428234655_Initial'
)
BEGIN
    CREATE INDEX [IX_Transactions_AssetId] ON [Transactions] ([AssetId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428234655_Initial'
)
BEGIN
    CREATE INDEX [IX_Transactions_PortfolioId] ON [Transactions] ([PortfolioId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428234655_Initial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260428234655_Initial', N'8.0.26');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428234858_Position'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260428234858_Position', N'8.0.26');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428235537_Initial2'
)
BEGIN
    ALTER TABLE [Position] DROP CONSTRAINT [FK_Position_Assets_AssetId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428235537_Initial2'
)
BEGIN
    ALTER TABLE [Position] DROP CONSTRAINT [FK_Position_Portfolios_PortfolioId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428235537_Initial2'
)
BEGIN
    ALTER TABLE [Position] DROP CONSTRAINT [PK_Position];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428235537_Initial2'
)
BEGIN
    EXEC sp_rename N'[Position]', N'Positions';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428235537_Initial2'
)
BEGIN
    EXEC sp_rename N'[Positions].[IX_Position_PortfolioId]', N'IX_Positions_PortfolioId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428235537_Initial2'
)
BEGIN
    EXEC sp_rename N'[Positions].[IX_Position_AssetId]', N'IX_Positions_AssetId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428235537_Initial2'
)
BEGIN
    ALTER TABLE [Positions] ADD CONSTRAINT [PK_Positions] PRIMARY KEY ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428235537_Initial2'
)
BEGIN
    ALTER TABLE [Positions] ADD CONSTRAINT [FK_Positions_Assets_AssetId] FOREIGN KEY ([AssetId]) REFERENCES [Assets] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428235537_Initial2'
)
BEGIN
    ALTER TABLE [Positions] ADD CONSTRAINT [FK_Positions_Portfolios_PortfolioId] FOREIGN KEY ([PortfolioId]) REFERENCES [Portfolios] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260428235537_Initial2'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260428235537_Initial2', N'8.0.26');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502234532_BuyOptimization'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Portfolios]') AND [c].[name] = N'PositionId');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Portfolios] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Portfolios] DROP COLUMN [PositionId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502234532_BuyOptimization'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Positions]') AND [c].[name] = N'Quantity');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Positions] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Positions] ALTER COLUMN [Quantity] decimal(18,6) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502234532_BuyOptimization'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260502234532_BuyOptimization', N'8.0.26');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506193941_PurchaseTrace'
)
BEGIN
    CREATE TABLE [PaymentTraces] (
        [Id] int NOT NULL IDENTITY,
        [PaymentId] nvarchar(max) NOT NULL,
        [UserId] int NOT NULL,
        [Amount] bigint NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        [Enabled] bit NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PaymentTraces] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PaymentTraces_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506193941_PurchaseTrace'
)
BEGIN
    CREATE INDEX [IX_PaymentTraces_UserId] ON [PaymentTraces] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506193941_PurchaseTrace'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260506193941_PurchaseTrace', N'8.0.26');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111152127_SeedAdminUser'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Role] = 1 OR [Email] = N'admin@hi-trade.com')
    BEGIN
        INSERT INTO [Users] ([Address], [Balance], [CreatedAt], [Email], [Enabled], [FullName], [IsDeleted], [Password], [ProfilePictureUrl], [Role], [UpdatedAt])
        VALUES (N'Hi-Trade HQ', 0, SYSUTCDATETIME(), N'admin@hi-trade.com', 1, N'System Admin', 0, N'$2b$10$oV7uJ4NU/..Rif4HXzqmdOZkAC/BYU24Q07BkxmSOCoLKBsZi9oa6', N'', 1, SYSUTCDATETIME());
    END

END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111152127_SeedAdminUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261111152127_SeedAdminUser', N'8.0.26');
END;
GO

COMMIT;
GO


BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261116145634_AddEmailVerificationAndResetPassword'
)
BEGIN
    ALTER TABLE [Users] ADD [EmailVerificationCode] nvarchar(max) NULL;
    ALTER TABLE [Users] ADD [EmailVerificationExpiresAt] datetime2 NULL;
    ALTER TABLE [Users] ADD [IsEmailVerified] bit NOT NULL DEFAULT 0;
    ALTER TABLE [Users] ADD [PasswordResetExpiresAt] datetime2 NULL;
    ALTER TABLE [Users] ADD [PasswordResetToken] nvarchar(max) NULL;
    UPDATE [Users] SET [IsEmailVerified] = 1 WHERE [Role] = 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261116145634_AddEmailVerificationAndResetPassword'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261116145634_AddEmailVerificationAndResetPassword', N'8.0.26');
END;
GO

COMMIT;
GO
