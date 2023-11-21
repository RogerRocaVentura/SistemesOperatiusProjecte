CREATE DATABASE chessdb;
USE chessdb;

CREATE TABLE Users (
    userID INT AUTO_INCREMENT PRIMARY KEY,
    login VARCHAR(255) UNIQUE NOT NULL,
    passwordHash VARCHAR(255) NOT NULL,
    userIP VARCHAR(45) NOT NULL,
    name VARCHAR(255) NOT NULL,
    rating INT DEFAULT 1000, 
    lastOnline DATETIME DEFAULT CURRENT_TIMESTAMP,
    findingMatch BOOLEAN DEFAULT FALSE
);

-- Matches table
CREATE TABLE Matches (
    matchID INT AUTO_INCREMENT PRIMARY KEY,
    player1ID INT,
    player2ID INT,
    startTime DATETIME NOT NULL,
    endTime DATETIME,
    winnerID INT,
    status ENUM('ongoing', 'completed', 'draw') NOT NULL,
    FOREIGN KEY (player1ID) REFERENCES Users(userID),
    FOREIGN KEY (player2ID) REFERENCES Users(userID),
    FOREIGN KEY (winnerID) REFERENCES Users(userID)
);

-- Moves table
CREATE TABLE Moves (
    moveID INT AUTO_INCREMENT PRIMARY KEY,
    matchID INT,
    playerID INT,
    moveDetails VARCHAR(10) NOT NULL,
    timestamp DATETIME NOT NULL,
    FOREIGN KEY (matchID) REFERENCES Matches(matchID),
    FOREIGN KEY (playerID) REFERENCES Users(userID)
);

CREATE TABLE Chats (
    chatID INT AUTO_INCREMENT PRIMARY KEY,
    senderID INT,
    receiverID INT,
    message TEXT NOT NULL,
    timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (senderID) REFERENCES Users(userID),
    FOREIGN KEY (receiverID) REFERENCES Users(userID)
);
