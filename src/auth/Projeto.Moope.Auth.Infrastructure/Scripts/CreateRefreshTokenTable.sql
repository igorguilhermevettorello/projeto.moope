-- Script para criar a tabela RefreshToken
-- Execute este script no banco de dados utilizado pelo AppAuthContext

CREATE TABLE IF NOT EXISTS `RefreshToken` (
    `Id` char(36) NOT NULL,
    `UsuarioId` char(36) NOT NULL,
    `Token` varchar(500) NOT NULL,
    `ExpiresAt` datetime(6) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `RevokedAt` datetime(6) NULL,
    `ReplacedByToken` varchar(500) NULL,
    PRIMARY KEY (`Id`),
    INDEX `IX_RefreshToken_Token` (`Token`(255)),
    INDEX `IX_RefreshToken_UsuarioId` (`UsuarioId`)
);
