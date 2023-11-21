#include <stdio.h>
#include <mysql/mysql.h>

MYSQL *create_connection() {
    MYSQL *conn = mysql_init(NULL);
    if (conn == NULL) {
        fprintf(stderr, "mysql_init() failed\n");
        return NULL;
    }
    if (mysql_real_connect(conn, "localhost", "your_username", "your_password", "ChessDB", 0, NULL, 0) == NULL) {
        fprintf(stderr, "mysql_real_connect() failed\n");
        mysql_close(conn);
        return NULL;
    }
    return conn;
}

void get_all_players_and_ratings(MYSQL *conn) {
    if (mysql_query(conn, "SELECT login, rating FROM Jugador")) {
        fprintf(stderr, "%s\n", mysql_error(conn));
        return;
    }
    MYSQL_RES *result = mysql_store_result(conn);
    MYSQL_ROW row;
    printf("Players and their ratings:\n");
    while ((row = mysql_fetch_row(result))) {
        printf("%s: %s\n", row[0], row[1]);
    }
    mysql_free_result(result);
}

void get_all_completed_matches(MYSQL *conn) {
    const char *query = 
        "SELECT a.login AS player1, b.login AS player2, c.login AS winner "
        "FROM Partida "
        "JOIN Jugador a ON Partida.player1ID = a.userID "
        "JOIN Jugador b ON Partida.player2ID = b.userID "
        "JOIN Jugador c ON Partida.winnerID = c.userID "
        "WHERE Partida.status = 'completed'";

    if (mysql_query(conn, query)) {
        fprintf(stderr, "%s\n", mysql_error(conn));
        return;
    }
    MYSQL_RES *result = mysql_store_result(conn);
    MYSQL_ROW row;
    printf("Completed matches:\n");
    while ((row = mysql_fetch_row(result))) {
        printf("Player1: %s, Player2: %s, Winner: %s\n", row[0], row[1], row[2]);
    }
    mysql_free_result(result);
}

void get_all_moves_for_match(MYSQL *conn, int matchID) {
    char query[512];
    sprintf(query, "SELECT moveDetails FROM Moves WHERE matchID=%d", matchID);
    if (mysql_query(conn, query)) {
        fprintf(stderr, "%s\n", mysql_error(conn));
        return;
    }
    MYSQL_RES *result = mysql_store_result(conn);
    MYSQL_ROW row;
    printf("Moves for match %d:\n", matchID);
    while ((row = mysql_fetch_row(result))) {
        printf("%s\n", row[0]);
    }
    mysql_free_result(result);
}

int main() {
    MYSQL *conn = create_connection();
    if (!conn) return 1;

    get_all_players_and_ratings(conn);
    printf("\n");
    get_all_completed_matches(conn);
    printf("\n");
    get_all_moves_for_match(conn, 1);  

    mysql_close(conn);
    return 0;
}
