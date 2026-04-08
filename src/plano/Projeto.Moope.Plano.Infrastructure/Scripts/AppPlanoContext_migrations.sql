CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `Plano` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Codigo` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Descricao` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Valor` decimal(15,2) NOT NULL,
    `TaxaAdesao` decimal(15,2) NULL,
    `Status` tinyint(1) NOT NULL,
    `Plataforma` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Plano` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260408003555_Plano', '8.0.0');

COMMIT;

