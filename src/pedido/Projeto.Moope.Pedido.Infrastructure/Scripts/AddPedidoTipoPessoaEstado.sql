START TRANSACTION;

ALTER TABLE `Pedido` ADD `Estado` varchar(50) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Pedido` ADD `TipoPessoa` int NOT NULL DEFAULT 1;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260421012641_AddPedidoTipoPessoaEstado', '8.0.0');

COMMIT;

