CREATE DATABASE ChessDB;
USE ChessDB;

-- Player table
CREATE TABLE Jugador (
    jugadorID INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(255) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL
);

-- Match table
CREATE TABLE Partida (
    partidaID INT AUTO_INCREMENT PRIMARY KEY,
    fechaHoraFin DATETIME,
    duracion TIME, -- Storing duration as a TIME type.
    ganadorID INT,
    FOREIGN KEY (ganadorID) REFERENCES Jugador(jugadorID)
);

-- This table links players and matches, representing the many-to-many relationship
CREATE TABLE JugadorPartida (
    jugadorID INT,
    partidaID INT,
    PRIMARY KEY (jugadorID, partidaID),
    FOREIGN KEY (jugadorID) REFERENCES Jugador(jugadorID),
    FOREIGN KEY (partidaID) REFERENCES Partida(partidaID)
);
