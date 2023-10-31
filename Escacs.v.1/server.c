#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <arpa/inet.h>
#include <sys/socket.h>
#include <pthread.h>
#include <mysql/mysql.h>
#include <openssl/sha.h>

#define SERVER_IP "127.0.0.1"
#define SERVER_PORT 5555
#define BUFFER_SIZE 4096
#define MAX_USERS 100
#define DB_HOST "127.0.0.1"
#define DB_USER "RogerRoca"
#define DB_PASS "20031994"
#define DB_NAME "chessdb"

typedef struct {
    char* username;
    int socket;
} User;

User *online_users[MAX_USERS];
pthread_mutex_t users_mutex = PTHREAD_MUTEX_INITIALIZER;
MYSQL *conn;

#include <openssl/evp.h>

void hashPassword(const char* input, char* output) {
    EVP_MD_CTX* ctx = EVP_MD_CTX_new();
    if (!ctx) {
        perror("EVP_MD_CTX_new failed");
        exit(EXIT_FAILURE);
    }

    if (EVP_DigestInit_ex(ctx, EVP_sha256(), NULL) != 1) {
        perror("EVP_DigestInit_ex failed");
        exit(EXIT_FAILURE);
    }

    if (EVP_DigestUpdate(ctx, input, strlen(input)) != 1) {
        perror("EVP_DigestUpdate failed");
        exit(EXIT_FAILURE);
    }

    unsigned char hash[EVP_MAX_MD_SIZE];
    unsigned int length;
    if (EVP_DigestFinal_ex(ctx, hash, &length) != 1) {
        perror("EVP_DigestFinal_ex failed");
        exit(EXIT_FAILURE);
    }

    for(unsigned int i = 0; i < length; i++) {
        sprintf(output + (i * 2), "%02x", hash[i]);
    }
    output[length * 2] = '\0';

    EVP_MD_CTX_free(ctx);
}


int find_user_index(const char *username) {
    pthread_mutex_lock(&users_mutex);

    for (int i = 0; i < MAX_USERS; ++i) {
        if (online_users[i] && strcmp(online_users[i]->username, username) == 0) {
            pthread_mutex_unlock(&users_mutex);
            return i;
        }
    }

    pthread_mutex_unlock(&users_mutex);
    return -1;
}

void add_user(const char *username, int socket) {
    pthread_mutex_lock(&users_mutex);

    for (int i = 0; i < MAX_USERS; ++i) {
        if (!online_users[i]) {
            online_users[i] = (User*) malloc(sizeof(User));
            online_users[i]->username = strdup(username);
            online_users[i]->socket = socket;
            break;
        }
    }

    pthread_mutex_unlock(&users_mutex);
}

void remove_user(const char *username) {
    pthread_mutex_lock(&users_mutex);

    int index = find_user_index(username);
    if (index != -1) {
        free(online_users[index]->username);
        free(online_users[index]);
        online_users[index] = NULL;
    }

    pthread_mutex_unlock(&users_mutex);
}

char* get_online_users() {
    char *list = (char*) malloc(BUFFER_SIZE);
    strcpy(list, "list*");

    pthread_mutex_lock(&users_mutex);

    for (int i = 0; i < MAX_USERS; ++i) {
        if (online_users[i]) {
            strcat(list, online_users[i]->username);
            strcat(list, "!");
        }
    }

    pthread_mutex_unlock(&users_mutex);

    return list;
}

void broadcast_online_users() {
    char *list = get_online_users();

    pthread_mutex_lock(&users_mutex);

    for (int i = 0; i < MAX_USERS; ++i) {
        if (online_users[i]) {
            send(online_users[i]->socket, list, strlen(list), 0);
        }
    }

    pthread_mutex_unlock(&users_mutex);

    free(list);
}

int authenticate_user(const char *username, const char *password) {
    char query[512];
    snprintf(query, sizeof(query), "SELECT passwordHash FROM Users WHERE login = '%s'", username);

    if (mysql_query(conn, query)) {
        fprintf(stderr, "%s\n", mysql_error(conn));
        return 0;
    }

    MYSQL_RES *result = mysql_store_result(conn);
    if (!result) {
        fprintf(stderr, "%s\n", mysql_error(conn));
        return 0;
    }

    MYSQL_ROW row = mysql_fetch_row(result);
    if (!row) return 0;

    char hashed_password[65];
    hashPassword(password, hashed_password);

    int auth_result = strcmp(hashed_password, row[0]) == 0;

    mysql_free_result(result);
    return auth_result;
}

int create_user(const char *username, const char *password, const char *ip, const char *name) {
    char hashed_password[65];
    hashPassword(password, hashed_password);
    printf("Hashed Password: %s\n", hashed_password);

    char query[512];
    snprintf(query, sizeof(query), "INSERT INTO Users (login, passwordHash, userIP, name) VALUES ('%s', '%s', '%s', '%s')", username, hashed_password, ip, name);
    printf("Query: %s\n", query);

    if (mysql_query(conn, query)) {
        fprintf(stderr, "MySQL Error: %s\n", mysql_error(conn));
        return 0;
    }

    printf("User Created Successfully\n");
    return 1;
}


void *handle_client(void *arg) {
    int client_socket = *(int*)arg;
    char buffer[BUFFER_SIZE];
    int bytes_read;
    char username_buffer[256] = {0};
    char client_ip[INET_ADDRSTRLEN];

    struct sockaddr_in addr;
    socklen_t addr_size = sizeof(struct sockaddr_in);
    int res = getpeername(client_socket, (struct sockaddr *)&addr, &addr_size);
    if (res < 0) {
        perror("getpeername failed");
        strcpy(client_ip, "0.0.0.0");
    } else {
        inet_ntop(AF_INET, &addr.sin_addr, client_ip, sizeof(client_ip));
    }

    while ((bytes_read = recv(client_socket, buffer, BUFFER_SIZE - 1, 0)) > 0) {
        buffer[bytes_read] = '\0';

        if (strstr(buffer, "login*") == buffer) {
            char *username = buffer + 6;
            char *password = strchr(username, '*');
            if (password) {
                *password = '\0';
                password++;
            } else {
                const char *message = "error*Invalid request format!";
                send(client_socket, message, strlen(message), 0);
                continue;
            }

            if (authenticate_user(username, password)) {
                strcpy(username_buffer, username);
                add_user(username, client_socket);
                const char *message = "success*Login successful!";
                send(client_socket, message, strlen(message), 0);
                broadcast_online_users();
            } else {
                const char *message = "error*Invalid username or password!";
                send(client_socket, message, strlen(message), 0);
            }
        } else if (strstr(buffer, "signup*") == buffer) {
            char *username = buffer + 7;
            char *password = strchr(username, '*');
            if (password) {
                *password = '\0';
                password++;
            } else {
                const char *message = "error*Invalid request format!";
                send(client_socket, message, strlen(message), 0);
                continue;
            }

            char *name = strchr(password, '*');
            if (name) {
                *name = '\0';
                name++;
            } else {
                const char *message = "error*Invalid request format!";
                send(client_socket, message, strlen(message), 0);
                continue;
            }

            if (create_user(username, password, client_ip, name)) {
                const char *message = "success*Account created successfully!";
                send(client_socket, message, strlen(message), 0);
            } else {
                const char *message = "error*Failed to create account!";
                send(client_socket, message, strlen(message), 0);
            }
        } else if (strcmp(buffer, "list") == 0) {
            char *list = get_online_users();
            send(client_socket, list, strlen(list), 0);
            free(list);
        } else if (strcmp(buffer, "logout") == 0) {
            break;
        } else {
            const char *message = "error*Invalid command!";
            send(client_socket, message, strlen(message), 0);
        }
    }

    if (bytes_read < 0) {
        perror("recv failed");
    }

    if (username_buffer[0] != '\0') {
        remove_user(username_buffer);
        broadcast_online_users();
    }

    close(client_socket);
    free(arg);
    return NULL;
}

int main() {
    int server_socket = socket(AF_INET, SOCK_STREAM, 0);
    if (server_socket == -1) {
        perror("Could not create socket");
        return 1;
    }

    struct sockaddr_in server_address;
    server_address.sin_family = AF_INET;
    server_address.sin_port = htons(SERVER_PORT);
    server_address.sin_addr.s_addr = inet_addr(SERVER_IP);

    if (bind(server_socket, (struct sockaddr*)&server_address, sizeof(server_address)) < 0) {
        perror("Bind failed");
        close(server_socket);
        return 1;
    }

    listen(server_socket, 5);

    conn = mysql_init(NULL);
    if (conn == NULL) {
        fprintf(stderr, "mysql_init() failed\n");
        close(server_socket);
        return 1;
    }

    if (mysql_real_connect(conn, DB_HOST, DB_USER, DB_PASS, DB_NAME, 0, NULL, 0) == NULL) {
        fprintf(stderr, "%s\n", mysql_error(conn));
        mysql_close(conn);
        close(server_socket);
        return 1;
    }

    printf("Server is running on %s:%d\n", SERVER_IP, SERVER_PORT);

    while (1) {
        struct sockaddr_in client_address;
        socklen_t client_address_len = sizeof(client_address);
        int client_socket = accept(server_socket, (struct sockaddr*)&client_address, &client_address_len);
        if (client_socket < 0) {
            perror("Accept failed");
            continue;
        }

        pthread_t thread_id;
        int *pclient_socket = malloc(sizeof(int));
        *pclient_socket = client_socket;
        if (pthread_create(&thread_id, NULL, handle_client, pclient_socket) < 0) {
            perror("Could not create thread");
            free(pclient_socket);
            continue;
        }

        pthread_detach(thread_id);
    }

    close(server_socket);
    mysql_close(conn);
    return 0;
}
