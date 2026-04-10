CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `Cliente` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Created` datetime(6) NOT NULL,
    `Updated` datetime(6) NOT NULL,
    `Telefone` longtext CHARACTER SET utf8mb4 NULL,
    `TelefoneEmergencia` longtext CHARACTER SET utf8mb4 NULL,
    `VendedorId` char(36) COLLATE ascii_general_ci NULL,
    CONSTRAINT `PK_Cliente` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260410131646_InitialCliente', '8.0.0');

COMMIT;

