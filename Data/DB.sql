-- Active: 1772983474410@@localhost@3306@sa_individual
CREATE DATABASE IF NOT EXISTS SA_Individual;

USE SA_Individual;

CREATE TABLE IF NOT EXISTS Produtos
(
	Id INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(255) NOT NULL,
    codigo VARCHAR(255) NOT NULL,
    quantidade INT NOT NULL,
    preco DECIMAL(10,2) NOT NULL,
    estoqueMinimo INT NOT NULL
);

CREATE OR REPLACE VIEW vw_relatorio_estoque AS
SELECT
    COUNT(*)                        AS TotalProdutos,
    SUM(quantidade)                 AS TotalItens,
    SUM(quantidade * preco)         AS ValorTotalEstoque
FROM Produtos;

INSERT INTO Produtos (nome, codigo, quantidade, preco, estoqueMinimo) VALUES
('Parafuso M6',      'PAR-M6',   500, 0.25, 100),
('Porca M6',         'POC-M6',   500, 0.15, 100),
('Chapa de Aço 3mm', 'CHP-003',   50, 45.90,  10),
('Lubrificante 1L',  'LUB-001',   30, 28.50,   5),
('Rebite 4mm',       'REB-004', 1000,  0.08, 200);