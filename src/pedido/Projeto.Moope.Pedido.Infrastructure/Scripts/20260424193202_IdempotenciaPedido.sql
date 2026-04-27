CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `Pedido` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `ClienteId` char(36) COLLATE ascii_general_ci NOT NULL,
    `VendedorId` char(36) COLLATE ascii_general_ci NULL,
    `PlanoId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Quantidade` int NOT NULL,
    `PlanoValor` decimal(15,2) NOT NULL,
    `PlanoDescricao` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `PlanoCodigo` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `PlanoTaxaAdesao` decimal(15,2) NOT NULL,
    `PlanoPercentualDesconto` decimal(7,4) NOT NULL,
    `PlanoValorComDesconto` decimal(15,2) NOT NULL,
    `Total` decimal(15,2) NOT NULL,
    `StatusAssinatura` int NOT NULL,
    `Status` varchar(50) CHARACTER SET utf8mb4 NULL,
    `StatusDescricao` varchar(255) CHARACTER SET utf8mb4 NULL,
    `GalaxPayId` int NULL,
    `Created` datetime(6) NOT NULL,
    `Updated` datetime(6) NOT NULL,
    CONSTRAINT `PK_Pedido` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Desconto` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `PedidoId` char(36) COLLATE ascii_general_ci NOT NULL,
    `ValorPercentual` decimal(7,4) NOT NULL,
    `Descricao` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `TipoPessoa` int NOT NULL,
    `CodigoDesconto` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `ValorDesconto` decimal(15,2) NULL,
    `Ativo` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Desconto` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Desconto_Pedido_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedido` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Transacao` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `PedidoId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Valor` decimal(15,2) NOT NULL,
    `DataPagamento` datetime(6) NOT NULL,
    `StatusPagamento` int NOT NULL,
    `Status` varchar(50) CHARACTER SET utf8mb4 NULL,
    `StatusDescricao` varchar(255) CHARACTER SET utf8mb4 NULL,
    `GalaxPayId` int NULL,
    `MetodoPagamento` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Created` datetime(6) NOT NULL,
    `Updated` datetime(6) NOT NULL,
    CONSTRAINT `PK_Transacao` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Transacao_Pedido_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedido` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_Desconto_PedidoId` ON `Desconto` (`PedidoId`);

CREATE INDEX `IX_Transacao_PedidoId` ON `Transacao` (`PedidoId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260420124106_InitialPedido', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `Pedido` ADD `Estado` varchar(50) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Pedido` ADD `TipoPessoa` int NOT NULL DEFAULT 1;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260421012641_AddPedidoTipoPessoaEstado', '8.0.0');

COMMIT;

START TRANSACTION;

CREATE TABLE `IdempotenciaPedido` (
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
    CONSTRAINT `PK_IdempotenciaPedido` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `UQ_Idempotency` ON `IdempotenciaPedido` (`IdempotencyKey`, `Scope`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260424193202_IdempotenciaPedido', '8.0.0');

COMMIT;
