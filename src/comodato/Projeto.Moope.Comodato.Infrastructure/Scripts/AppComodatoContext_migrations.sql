CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `Comodato` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `ClienteId` char(36) COLLATE ascii_general_ci NOT NULL,
    `ProdutoNome` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
    `Valor` decimal(15,2) NOT NULL,
    `CriadoEm` datetime(6) NOT NULL,
    `Status` int NOT NULL,
    CONSTRAINT `PK_Comodato` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `ComodatoConvite` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `TokenHash` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedByAdminId` char(36) COLLATE ascii_general_ci NOT NULL,
    `PlanoId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Quantidade` int NOT NULL,
    `Valor` decimal(15,2) NOT NULL,
    `VendedorId` char(36) COLLATE ascii_general_ci NULL,
    `CriadoEm` datetime(6) NOT NULL,
    `ExpiradoEm` datetime(6) NOT NULL,
    `Status` int NOT NULL,
    `AbertoEm` datetime(6) NULL,
    `ConsumidoEm` datetime(6) NULL,
    `ConsumidoPorClienteId` char(36) COLLATE ascii_general_ci NULL,
    `ClienteEmail` varchar(255) CHARACTER SET utf8mb4 NULL,
    `ClienteDocumento` varchar(20) CHARACTER SET utf8mb4 NULL,
    `ComodatoId` char(36) COLLATE ascii_general_ci NULL,
    `Estado` varchar(50) CHARACTER SET utf8mb4 NULL,
    `DataPagamento` datetime(6) NULL,
    CONSTRAINT `PK_ComodatoConvite` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ComodatoConvite_Comodato_ComodatoId` FOREIGN KEY (`ComodatoId`) REFERENCES `Comodato` (`Id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_ComodatoConvite_ComodatoId` ON `ComodatoConvite` (`ComodatoId`);

CREATE UNIQUE INDEX `IX_ComodatoConvite_TokenHash` ON `ComodatoConvite` (`TokenHash`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260606120000_InitialComodato', '8.0.0');

COMMIT;
