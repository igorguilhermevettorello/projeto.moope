CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `Vendedor` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `PercentualComissao` decimal(7,4) NOT NULL,
    `ChavePix` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `CodigoCupom` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Created` datetime(6) NOT NULL,
    `Updated` datetime(6) NOT NULL,
    `VendedorId` char(36) COLLATE ascii_general_ci NULL,
    CONSTRAINT `PK_Vendedor` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Vendedor_Vendedor_VendedorId` FOREIGN KEY (`VendedorId`) REFERENCES `Vendedor` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_Vendedor_VendedorId` ON `Vendedor` (`VendedorId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260408234935_InitialVendedor', '8.0.0');

COMMIT;

