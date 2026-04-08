CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory_Business` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory_Business` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `PessoaFisica` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Nome` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Cpf` varchar(14) CHARACTER SET utf8mb4 NOT NULL,
    `Created` datetime(6) NOT NULL,
    `Updated` datetime(6) NOT NULL,
    CONSTRAINT `PK_PessoaFisica` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `PessoaJuridica` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Cnpj` varchar(18) CHARACTER SET utf8mb4 NOT NULL,
    `RazaoSocial` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `NomeFantasia` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `InscricaoEstadual` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `Created` datetime(6) NOT NULL,
    `Updated` datetime(6) NOT NULL,
    CONSTRAINT `PK_PessoaJuridica` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `RefreshToken` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `UsuarioId` char(36) COLLATE ascii_general_ci NOT NULL,
    `Token` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `ExpiresAt` datetime(6) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `RevokedAt` datetime(6) NULL,
    `ReplacedByToken` varchar(500) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_RefreshToken` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Usuario` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `Nome` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `TipoUsuario` int NOT NULL,
    `EnderecoId` char(36) COLLATE ascii_general_ci NULL,
    `Created` datetime(6) NOT NULL,
    `Updated` datetime(6) NOT NULL,
    CONSTRAINT `PK_Usuario` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Papel` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `UsuarioId` char(36) COLLATE ascii_general_ci NULL,
    `TipoUsuario` int NOT NULL,
    `Created` datetime(6) NOT NULL,
    `Updated` datetime(6) NOT NULL,
    CONSTRAINT `PK_Papel` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Papel_Usuario_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `Usuario` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_Papel_UsuarioId` ON `Papel` (`UsuarioId`);

CREATE INDEX `IX_RefreshToken_Token` ON `RefreshToken` (`Token`);

CREATE INDEX `IX_RefreshToken_UsuarioId` ON `RefreshToken` (`UsuarioId`);

INSERT INTO `__EFMigrationsHistory_Business` (`MigrationId`, `ProductVersion`)
VALUES ('20260407013245_InitialBusiness', '8.0.0');

COMMIT;

