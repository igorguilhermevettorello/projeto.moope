CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `IdempotenciaPagamento` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `IdempotencyKey` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Scope` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `RequestHash` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `ResponseStatusCode` int NULL,
    `ResponseBody` text CHARACTER SET utf8mb4 NULL,
    `ResourceId` varchar(100) CHARACTER SET utf8mb4 NULL,
    `ResourceType` varchar(50) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_IdempotenciaPagamento` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Pagamento` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `ClienteId` char(36) COLLATE ascii_general_ci NOT NULL,
    `GatewayCustomerId` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `GatewayPlanId` varchar(100) CHARACTER SET utf8mb4 NULL,
    `GatewaySubscriptionId` varchar(100) CHARACTER SET utf8mb4 NULL,
    `Created` datetime(6) NOT NULL,
    `Updated` datetime(6) NOT NULL,
    CONSTRAINT `PK_Pagamento` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `UQ_Idempotency_Pagamento` ON `IdempotenciaPagamento` (`IdempotencyKey`, `Scope`);

CREATE UNIQUE INDEX `IX_Pagamento_ClienteId` ON `Pagamento` (`ClienteId`);

CREATE UNIQUE INDEX `IX_Pagamento_GatewayCustomerId` ON `Pagamento` (`GatewayCustomerId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260423170915_InitialPagamento', '8.0.0');

COMMIT;

START TRANSACTION;

CREATE TABLE `IntencaoPagamento` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Valor` decimal(18,2) NOT NULL,
    `Moeda` varchar(3) CHARACTER SET utf8mb4 NOT NULL,
    `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `MetodoPagamento` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `ExpiresAt` datetime(6) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_IntencaoPagamento` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_IntencaoPagamento_ExpiresAt` ON `IntencaoPagamento` (`ExpiresAt`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260424232553_IntencaoPagamento', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `Pagamento` DROP INDEX `IX_Pagamento_ClienteId`;

ALTER TABLE `Pagamento` DROP INDEX `IX_Pagamento_GatewayCustomerId`;

CREATE INDEX `IX_Pagamento_ClienteId` ON `Pagamento` (`ClienteId`);

CREATE INDEX `IX_Pagamento_GatewayCustomerId` ON `Pagamento` (`GatewayCustomerId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260425203031_RemoveUniquePagamentoIndexes', '8.0.0');

COMMIT;
