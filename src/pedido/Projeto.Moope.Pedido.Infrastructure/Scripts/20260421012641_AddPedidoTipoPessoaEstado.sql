CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

START TRANSACTION;

ALTER TABLE `Pedido` ADD `Estado` varchar(50) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Pedido` ADD `TipoPessoa` int NOT NULL DEFAULT 1;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260421012641_AddPedidoTipoPessoaEstado', '8.0.0');

COMMIT;
