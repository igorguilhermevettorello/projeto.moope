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
    `Telefone` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `TelefoneEmergencia` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `VendedorId` char(36) COLLATE ascii_general_ci NOT NULL,
    CONSTRAINT `PK_Cliente` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260413211607_InitialCliente', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `Cliente` MODIFY COLUMN `VendedorId` char(36) COLLATE ascii_general_ci NULL;

ALTER TABLE `Cliente` MODIFY COLUMN `TelefoneEmergencia` varchar(255) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Cliente` MODIFY COLUMN `Telefone` varchar(255) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Cliente` ADD `EnderecoId` char(36) COLLATE ascii_general_ci NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260415004710_AjusteColunas', '8.0.0');

COMMIT;

START TRANSACTION;

ALTER TABLE `Cliente` ADD `GalaxPayId` int NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260421133910_AjusteColunaGalaxPay', '8.0.0');

COMMIT;

