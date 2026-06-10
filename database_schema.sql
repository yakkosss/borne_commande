-- ============================================================
-- Borne de Commande - Script MPD (Modele Physique de Donnees)
-- ============================================================
-- MCD :
--   Client (0,n) ──[passer]── (1,1) Commande
--   Commande (1,n) ──[contenir]── (0,n) Produit
--   Produit (0,n) ──[appartenir]── (1,1) Categorie
--   Client_Inscrit (0,1) ──[est]── (1,1) Client   (specialisation)
-- ============================================================

CREATE DATABASE IF NOT EXISTS borne_commande
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE borne_commande;

-- ----------------------------------------------------------
-- RG1 – Table CLIENT (client invite = anonyme)
--   Un client invite a un numero aleatoire + un pseudo genere.
--   Un client inscrit est lie a cette table ET a client_inscrit.
-- ----------------------------------------------------------
CREATE TABLE IF NOT EXISTS client (
    id_client      INT          AUTO_INCREMENT PRIMARY KEY,
    numero_client  VARCHAR(20)  NOT NULL UNIQUE,   -- ex: INV-7829 ou USR-0034
    pseudo         VARCHAR(50)  NOT NULL,
    type_client    ENUM('invite','inscrit') NOT NULL DEFAULT 'invite',
    date_creation  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- ----------------------------------------------------------
-- Table CLIENT_INSCRIT (specialisation de CLIENT)
--   1,1 avec client : chaque ligne correspond exactement a un client.
-- ----------------------------------------------------------
CREATE TABLE IF NOT EXISTS client_inscrit (
    id_client    INT          PRIMARY KEY,        -- PK et FK vers client
    nom          VARCHAR(50)  NOT NULL,
    prenom       VARCHAR(50)  NOT NULL,
    email        VARCHAR(100) NOT NULL UNIQUE,
    mot_de_passe VARCHAR(255) NOT NULL,           -- stocke hashe (bcrypt/SHA256)
    telephone    VARCHAR(15),
    adresse      VARCHAR(255),
    role         ENUM('admin','cuisine') NOT NULL DEFAULT 'admin',
    CONSTRAINT fk_client_inscrit
        FOREIGN KEY (id_client) REFERENCES client (id_client)
        ON DELETE CASCADE
) ENGINE=InnoDB;

-- ----------------------------------------------------------
-- Table CATEGORIES
-- ----------------------------------------------------------
CREATE TABLE IF NOT EXISTS categories (
    id          INT           AUTO_INCREMENT PRIMARY KEY,
    nom         VARCHAR(100)  NOT NULL,
    description VARCHAR(255),
    image_path  VARCHAR(500)                       -- chemin image optionnel
) ENGINE=InnoDB;

-- ----------------------------------------------------------
-- Table PRODUITS
-- ----------------------------------------------------------
CREATE TABLE IF NOT EXISTS produits (
    id           INT            AUTO_INCREMENT PRIMARY KEY,
    nom          VARCHAR(200)   NOT NULL,
    description  TEXT,
    prix         DECIMAL(10, 2) NOT NULL,
    image_path   VARCHAR(500),                    -- chemin ou URL de l'image
    categorie_id INT,
    CONSTRAINT fk_produit_categorie
        FOREIGN KEY (categorie_id) REFERENCES categories (id)
        ON DELETE SET NULL
) ENGINE=InnoDB;

-- ----------------------------------------------------------
-- Table COMMANDES
-- RG2 : statuts possibles = en_attente, en_preparation, prete
-- ----------------------------------------------------------
CREATE TABLE IF NOT EXISTS commandes (
    id             INT            AUTO_INCREMENT PRIMARY KEY,
    numero_commande VARCHAR(20)   NOT NULL UNIQUE, -- ex: CMD-20260608-001
    client_id      INT,                            -- FK vers client (NULL si invite non trace)
    date_creation  DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    statut         ENUM('en_attente','en_preparation','prete')
                                  NOT NULL DEFAULT 'en_attente',
    mode_paiement  ENUM('Carte bancaire','Especes','Ticket restaurant'),
    total          DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    CONSTRAINT fk_commande_client
        FOREIGN KEY (client_id) REFERENCES client (id_client)
        ON DELETE SET NULL
) ENGINE=InnoDB;

-- ----------------------------------------------------------
-- Table MENUS (formules / combos a prix fixe)
-- ----------------------------------------------------------
CREATE TABLE IF NOT EXISTS menus (
    id          INT            AUTO_INCREMENT PRIMARY KEY,
    nom         VARCHAR(200)   NOT NULL,
    description TEXT,
    prix        DECIMAL(10, 2) NOT NULL,
    image_path  VARCHAR(500)
) ENGINE=InnoDB;

-- Composition d'un menu (quels produits et en quelle quantite)
CREATE TABLE IF NOT EXISTS menu_produits (
    menu_id    INT NOT NULL,
    produit_id INT NOT NULL,
    quantite   INT NOT NULL DEFAULT 1,
    PRIMARY KEY (menu_id, produit_id),
    CONSTRAINT fk_mp_menu
        FOREIGN KEY (menu_id)    REFERENCES menus    (id) ON DELETE CASCADE,
    CONSTRAINT fk_mp_produit
        FOREIGN KEY (produit_id) REFERENCES produits (id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- ----------------------------------------------------------
-- Table LIGNES_COMMANDE (relation many-to-many Commande ↔ Produit)
-- ----------------------------------------------------------
CREATE TABLE IF NOT EXISTS lignes_commande (
    id             INT            AUTO_INCREMENT PRIMARY KEY,
    commande_id    INT            NOT NULL,
    produit_id     INT,
    nom_produit    VARCHAR(200)   NOT NULL,         -- copie snapshot au moment de la commande
    prix_unitaire  DECIMAL(10, 2) NOT NULL,
    quantite       INT            NOT NULL,
    CONSTRAINT fk_ligne_commande
        FOREIGN KEY (commande_id) REFERENCES commandes (id)
        ON DELETE CASCADE,
    CONSTRAINT fk_ligne_produit
        FOREIGN KEY (produit_id) REFERENCES produits (id)
        ON DELETE SET NULL
) ENGINE=InnoDB;

-- ----------------------------------------------------------
-- Donnees de demonstration
-- ----------------------------------------------------------
INSERT INTO categories (nom, description) VALUES
    ('Plats',            'Nos plats maison'),
    ('Boissons',         'Froides et chaudes'),
    ('Desserts',         'Pour finir en douceur'),
    ('Accompagnements',  'Frites, salades et plus');

INSERT INTO produits (nom, description, prix, categorie_id) VALUES
    ('Classic Burger',  'Steak hache, salade, tomate, oignons',       8.50,  1),
    ('Cheese Burger',   'Steak hache, cheddar, cornichons, ketchup',  9.00,  1),
    ('Veggie Burger',   'Galette vegetale, avocat, tomate',           8.00,  1),
    ('Coca-Cola',       '33cl',                                        2.50,  2),
    ('Eau minerale',    '50cl',                                        1.50,  2),
    ('Jus d\'orange',   'Presse, 25cl',                                3.00,  2),
    ('Tiramisu',        'Recette maison',                              4.50,  3),
    ('Brownie',         'Chocolat noir, noix',                         3.50,  3),
    ('Frites',          'Portion individuelle',                        3.00,  4),
    ('Salade Cesar',    'Laitue, parmesan, croutons',                  4.00,  4);

-- Menus de demonstration (les produit_id correspondent aux produits inseres ci-dessus)
INSERT INTO menus (nom, description, prix) VALUES
    ('Menu Classic',  'Classic Burger + Frites + Coca-Cola',    14.50),
    ('Menu Veggie',   'Veggie Burger + Frites + Eau minerale',  13.50),
    ('Menu Cheese',   'Cheese Burger + Frites + Jus d\'orange', 15.00);

-- Composition des menus (burger=1/2/3, coca=4, eau=5, jus=6, frites=9)
INSERT INTO menu_produits (menu_id, produit_id, quantite) VALUES
    (1, 1, 1), (1, 9, 1), (1, 4, 1),   -- Menu Classic
    (2, 3, 1), (2, 9, 1), (2, 5, 1),   -- Menu Veggie
    (3, 2, 1), (3, 9, 1), (3, 6, 1);   -- Menu Cheese

-- Client de demonstration
INSERT INTO client (numero_client, pseudo, type_client) VALUES
    ('USR-0001', 'admin', 'inscrit');

-- Compte admin  — mot de passe : "adminpwd"  (SHA-256)
INSERT INTO client_inscrit (id_client, nom, prenom, email, mot_de_passe, role) VALUES
    (1, 'Admin', 'Admin', 'admin@borne.local',
     '7b18601f5caaa6dbbc7ad058ac54a25d30e7a508ce814c41f44ea5cabf9b3181', 'admin');

-- Compte cuisine — mot de passe : "cuisinepwd" (SHA-256)
INSERT INTO client (numero_client, pseudo, type_client) VALUES
    ('USR-0002', 'cuisine', 'inscrit');
INSERT INTO client_inscrit (id_client, nom, prenom, email, mot_de_passe, role) VALUES
    (2, 'Cuisine', 'Cuisine', 'cuisine@borne.local',
     'bc4a38398372ab721b9fcd2860257d4a33984bfd450fb0e50cd66c3e8a7f98d7', 'cuisine');
