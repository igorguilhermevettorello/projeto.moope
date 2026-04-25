-- Migration: IntencaoPagamento (incremental; aplica após InitialPagamento)

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
