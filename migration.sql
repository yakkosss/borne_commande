-- ============================================================
-- Borne de Commande — Script de migration complet
-- Cible : MySQL 8.0+ / MariaDB 10.5+
-- Usage : mysql -u root -p < migration.sql
-- ============================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------------------------------------
-- Base de donnees
-- ----------------------------------------------------------
CREATE DATABASE IF NOT EXISTS borne_commande
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE borne_commande;

-- ----------------------------------------------------------
-- Table CLIENT
-- ----------------------------------------------------------
CREATE TABLE IF NOT EXISTS client (
    id_client      INT          AUTO_INCREMENT PRIMARY KEY,
    numero_client  VARCHAR(20)  NOT NULL UNIQUE,
    pseudo         VARCHAR(50)  NOT NULL,
    type_client    ENUM('invite','inscrit') NOT NULL DEFAULT 'invite',
    date_creation  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ----------------------------------------------------------
-- Table CLIENT_INSCRIT (specialisation de CLIENT)
-- ----------------------------------------------------------
CREATE TABLE IF NOT EXISTS client_inscrit (
    id_client    INT          PRIMARY KEY,
    nom          VARCHAR(50)  NOT NULL,
    prenom       VARCHAR(50)  NOT NULL,
    email        VARCHAR(100) NOT NULL UNIQUE,
    mot_de_passe VARCHAR(255) NOT NULL,
    telephone    VARCHAR(15),
    adresse      VARCHAR(255),
    role         ENUM('admin','cuisine') NOT NULL DEFAULT 'admin',
    CONSTRAINT fk_client_inscrit
        FOREIGN KEY (id_client) REFERENCES client (id_client)
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ----------------------------------------------------------
-- Table CATEGORIES
-- ----------------------------------------------------------
CREATE TABLE IF NOT EXISTS categories (
    id          INT           AUTO_INCREMENT PRIMARY KEY,
    nom         VARCHAR(100)  NOT NULL,
    description VARCHAR(255),
    image_path  VARCHAR(500)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ----------------------------------------------------------
-- Table PRODUITS
-- ----------------------------------------------------------
CREATE TABLE IF NOT EXISTS produits (
    id           INT            AUTO_INCREMENT PRIMARY KEY,
    nom          VARCHAR(200)   NOT NULL,
    description  TEXT,
    prix         DECIMAL(10,2)  NOT NULL,
    image_path   VARCHAR(500),
    categorie_id INT,
    CONSTRAINT fk_produit_categorie
        FOREIGN KEY (categorie_id) REFERENCES categories (id)
        ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ----------------------------------------------------------
-- Table MENUS
-- ----------------------------------------------------------
CREATE TABLE IF NOT EXISTS menus (
    id          INT            AUTO_INCREMENT PRIMARY KEY,
    nom         VARCHAR(200)   NOT NULL,
    description TEXT,
    prix        DECIMAL(10,2)  NOT NULL,
    image_path  VARCHAR(500)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table de composition des menus
CREATE TABLE IF NOT EXISTS menu_produits (
    menu_id    INT NOT NULL,
    produit_id INT NOT NULL,
    quantite   INT NOT NULL DEFAULT 1,
    PRIMARY KEY (menu_id, produit_id),
    CONSTRAINT fk_mp_menu
        FOREIGN KEY (menu_id)    REFERENCES menus    (id) ON DELETE CASCADE,
    CONSTRAINT fk_mp_produit
        FOREIGN KEY (produit_id) REFERENCES produits (id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ----------------------------------------------------------
-- Table COMMANDES
-- RG2 : statuts = en_attente | en_preparation | prete
-- ----------------------------------------------------------
CREATE TABLE IF NOT EXISTS commandes (
    id              INT            AUTO_INCREMENT PRIMARY KEY,
    numero_commande VARCHAR(20)    NOT NULL UNIQUE,
    client_id       INT,
    date_creation   DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    statut          ENUM('en_attente','en_preparation','prete') NOT NULL DEFAULT 'en_attente',
    mode_paiement   ENUM('Carte bancaire','Especes','Ticket restaurant'),
    total           DECIMAL(10,2)  NOT NULL DEFAULT 0.00,
    CONSTRAINT fk_commande_client
        FOREIGN KEY (client_id) REFERENCES client (id_client)
        ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ----------------------------------------------------------
-- Table LIGNES_COMMANDE
-- ----------------------------------------------------------
CREATE TABLE IF NOT EXISTS lignes_commande (
    id             INT            AUTO_INCREMENT PRIMARY KEY,
    commande_id    INT            NOT NULL,
    produit_id     INT,
    nom_produit    VARCHAR(200)   NOT NULL,
    prix_unitaire  DECIMAL(10,2)  NOT NULL,
    quantite       INT            NOT NULL,
    CONSTRAINT fk_ligne_commande
        FOREIGN KEY (commande_id) REFERENCES commandes (id)
        ON DELETE CASCADE,
    CONSTRAINT fk_ligne_produit
        FOREIGN KEY (produit_id) REFERENCES produits (id)
        ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ----------------------------------------------------------
-- Donnees : categories
-- ----------------------------------------------------------
INSERT INTO categories (id, nom, description) VALUES
    (1, 'Plats',           'Nos plats maison'),
    (2, 'Boissons',        'Froides et chaudes'),
    (3, 'Desserts',        'Pour finir en douceur'),
    (4, 'Accompagnements', 'Frites, salades et plus')
ON DUPLICATE KEY UPDATE nom = VALUES(nom);

-- ----------------------------------------------------------
-- Donnees : produits
-- ----------------------------------------------------------
INSERT INTO produits (id, nom, description, prix, categorie_id) VALUES
    (1,  'Classic Burger', 'Steak hache, salade, tomate, oignons',       8.50, 1),
    (2,  'Cheese Burger',  'Steak hache, cheddar, cornichons, ketchup',  9.00, 1),
    (3,  'Veggie Burger',  'Galette vegetale, avocat, tomate',           8.00, 1),
    (4,  'Coca-Cola',      '33cl',                                        2.50, 2),
    (5,  'Eau minerale',   '50cl',                                        1.50, 2),
    (6,  'Jus d\'orange',  'Presse, 25cl',                                3.00, 2),
    (7,  'Tiramisu',       'Recette maison',                              4.50, 3),
    (8,  'Brownie',        'Chocolat noir, noix',                         3.50, 3),
    (9,  'Frites',         'Portion individuelle',                        3.00, 4),
    (10, 'Salade Cesar',   'Laitue, parmesan, croutons',                  4.00, 4)
ON DUPLICATE KEY UPDATE nom = VALUES(nom), prix = VALUES(prix);

-- ----------------------------------------------------------
-- Donnees : menus
-- ----------------------------------------------------------
INSERT INTO menus (id, nom, description, prix) VALUES
    (1, 'Menu Classic', 'Classic Burger + Frites + Coca-Cola',    14.50),
    (2, 'Menu Veggie',  'Veggie Burger + Frites + Eau minerale',  13.50),
    (3, 'Menu Cheese',  'Cheese Burger + Frites + Jus d\'orange', 15.00)
ON DUPLICATE KEY UPDATE nom = VALUES(nom), prix = VALUES(prix);

INSERT INTO menu_produits (menu_id, produit_id, quantite) VALUES
    (1, 1, 1), (1, 9, 1), (1, 4, 1),
    (2, 3, 1), (2, 9, 1), (2, 5, 1),
    (3, 2, 1), (3, 9, 1), (3, 6, 1)
ON DUPLICATE KEY UPDATE quantite = VALUES(quantite);

-- ----------------------------------------------------------
-- Comptes du personnel
--
-- Mot de passe admin   : adminpwd
-- Mot de passe cuisine : cuisinepwd
-- Hash : SHA-256 (hex, minuscules)
-- ----------------------------------------------------------
INSERT INTO client (id_client, numero_client, pseudo, type_client) VALUES
    (1, 'USR-0001', 'admin',   'inscrit'),
    (2, 'USR-0002', 'cuisine', 'inscrit')
ON DUPLICATE KEY UPDATE pseudo = VALUES(pseudo);

INSERT INTO client_inscrit (id_client, nom, prenom, email, mot_de_passe, role) VALUES
    (1, 'Admin',   'Admin',   'admin@borne.local',
     '7b18601f5caaa6dbbc7ad058ac54a25d30e7a508ce814c41f44ea5cabf9b3181', 'admin'),
    (2, 'Cuisine', 'Cuisine', 'cuisine@borne.local',
     'bc4a38398372ab721b9fcd2860257d4a33984bfd450fb0e50cd66c3e8a7f98d7', 'cuisine')
ON DUPLICATE KEY UPDATE mot_de_passe = VALUES(mot_de_passe), role = VALUES(role);

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- Installation terminee.
-- Comptes crees :
--   admin@borne.local   / adminpwd   (role : admin)
--   cuisine@borne.local / cuisinepwd (role : cuisine)
-- ============================================================
