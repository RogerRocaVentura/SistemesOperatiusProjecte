#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <arpa/inet.h>
#include <sys/socket.h>
#include <pthread.h>
#include <mysql/mysql.h>
#include <openssl/sha.h>
#include <openssl/evp.h>

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

void broadcast_online_users();
void hashPassword(const char* input, char* output);
int find_user_index(const char *username);
void add_user_without_broadcast(const char *username, int socket);
void remove_user_without_broadcast(const char *username);
char* get_online_users();
void *handle_client(void *arg);
int authenticate_user(const char *username, const char *password);
int create_user(const char *username, const char *password, const char *ip, const char *name);


void* periodic_broadcast(void *arg) {
    while (1) {
        sleep(5); // Wait for 30 seconds
        broadcast_online_users(); // Broadcast the online users list
    }

    return NULL;
}





void add_user_without_broadcast(const char *username, int socket) {
    // Adds a user to the online_users array without broadcasting the list.
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

void remove_user_without_broadcast(const char *username) {
    // Removes a user from the online_users array without broadcasting the list.
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
    // Gathers the list of online users into a single string.
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
    printf("Attempting to add user: %s\n", username); // Log attempt to add a user

    for (int i = 0; i < MAX_USERS; ++i) {
        if (!online_users[i]) {
            online_users[i] = (User*) malloc(sizeof(User));
            online_users[i]->username = strdup(username);
            online_users[i]->socket = socket;
            printf("User %s added with socket %d\n", username, socket); // Log user addition
            break;
        }
    }

    pthread_mutex_unlock(&users_mutex);
    broadcast_online_users(); // Broadcast every time a user is added.
}

void remove_user(const char *username) {
    pthread_mutex_lock(&users_mutex);
    printf("Attempting to remove user: %s\n", username); // Log attempt to remove a user

    int index = find_user_index(username);
    if (index != -1) {
        free(online_users[index]->username);
        free(online_users[index]);
        online_users[index] = NULL;
        printf("User %s removed\n", username); // Log user removal
        pthread_mutex_unlock(&users_mutex);
        broadcast_online_users(); // Broadcast every time a user is removed.
    } else {
        printf("User %s not found for removal\n", username); // Log if user not found
        pthread_mutex_unlock(&users_mutex);
    }
}

void broadcast_online_users() {
    pthread_mutex_lock(&users_mutex);

    for (int i = 0; i < MAX_USERS; ++i) {
        if (online_users[i]) {
            // Allocate a buffer to build the user list
            char *list = (char*) malloc(BUFFER_SIZE);
            strcpy(list, "list*");

            // Build the list excluding the current user (i is the index of the current user)
            for (int j = 0; j < MAX_USERS; ++j) {
                if (online_users[j] && i != j) { // Skip the current user
                    strcat(list, online_users[j]->username);
                    strcat(list, "!");
                }
            }

            // Debug output: What are we sending and to whom?
            printf("Preparing to send user list to %s: %s\n", online_users[i]->username, list);

            // Send the list to the current user
            char broadcast_message[BUFFER_SIZE];
            snprintf(broadcast_message, BUFFER_SIZE, "onlineUsers*%s", list);
            ssize_t bytes_sent = send(online_users[i]->socket, broadcast_message, strlen(broadcast_message), 0);
            if (bytes_sent < 0) {
                perror("Send failed");
            } else {
                printf("Sent user list to %s, bytes: %zd\n", online_users[i]->username, bytes_sent);
            }

            free(list); // Free the allocated buffer
        }
    }

    pthread_mutex_unlock(&users_mutex);
}


int authenticate_user(const char *username, const char *password) {
    char query[512];
    snprintf(query, sizeof(query), "SELECT passwordHash FROM Users WHERE login = '%s'", username);
    printf("Authenticating user: %s\n", username); // Log the username being authenticated

    if (mysql_query(conn, query)) {
        fprintf(stderr, "MySQL query error: %s\n", mysql_error(conn));
        return 0;
    }

    MYSQL_RES *result = mysql_store_result(conn);
    if (!result) {
        fprintf(stderr, "MySQL store result error: %s\n", mysql_error(conn));
        return 0;
    }

    MYSQL_ROW row = mysql_fetch_row(result);
    if (!row) {
        printf("No such user found: %s\n", username); // Log if no user is found
        mysql_free_result(result);
        return 0;
    }

    char hashed_password[65];
    hashPassword(password, hashed_password);
    printf("Received password: %s\n", password); // Log the received plaintext password
    printf("Hashed received password: %s\n", hashed_password); // Log the hashed password
    printf("Stored password hash: %s\n", row[0]); // Log the stored password hash

    int auth_result = strcmp(hashed_password, row[0]) == 0;
    if(auth_result) {
        printf("User authenticated successfully: %s\n", username); // Log successful authentication
    } else {
        printf("Authentication failed for user: %s\n", username); // Log failed authentication
    }

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
    char username_buffer[256] = {0}; // Initialize username buffer to store the username for this session.
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

        if (strstr(buffer, "createAccount*") == buffer) {
            char *username = buffer + 14; // Move pointer past "createAccount*"
            char *password = strchr(username, '!');
            char *name = NULL;
            
            if (password) {
                *password = '\0';
                password++;
                name = strchr(password, '#');
                if (name) {
                    *name = '\0';
                    name++;
                }
            }

            if (username && password && name && create_user(username, password, client_ip, name)) {
                const char *message = "accountCreationROk*Account created successfully!";
                send(client_socket, message, strlen(message), 0);
            } else {
                const char *message = "accountCreationRFail*Account creation failed!";
                send(client_socket, message, strlen(message), 0);
            }
        } else if (strstr(buffer, "login*") == buffer) {
            char *username = buffer + 6;
            char *password = strchr(username, '!');
            if (password) {
                *password = '\0';  // Null-terminate the username
                password++;  // Move past the '!' to get the password
                if (authenticate_user(username, password)) {
                    strcpy(username_buffer, username);
                    add_user_without_broadcast(username, client_socket);  // Add the user without broadcasting
                    broadcast_online_users();  // Immediately broadcast the updated list
                    const char *message = "loginReplyROk*";
                    send(client_socket, message, strlen(message), 0);
                } else {
                    const char *message = "loginRFail*Invalid username or password!";
                    send(client_socket, message, strlen(message), 0);
                }
            } else {
                const char *message = "error*Invalid request format!";
                send(client_socket, message, strlen(message), 0);
            }
        } else if (strcmp(buffer, "list") == 0) {
            char *list = get_online_users();
            send(client_socket, list, strlen(list), 0);
            free(list);

        
        } 

        else if (strstr(buffer, "userQuit*") == buffer) {
            printf("User %s has quit.\n", username_buffer);
            if (username_buffer[0] != '\0') {
                remove_user_without_broadcast(username_buffer);
                broadcast_online_users(); // Update the user list for all clients
                username_buffer[0] = '\0';
            }
            close(client_socket); // Close the socket as the user has quit
            break; // Break out of the receive loop
        }
 
        
        else if (strcmp(buffer, "logout") == 0) {
            if (username_buffer[0] != '\0') {
                remove_user_without_broadcast(username_buffer);  // Remove the user without broadcasting
                broadcast_online_users();  // Immediately broadcast the updated list
                username_buffer[0] = '\0';
                const char *message = "logoutReply*";
                send(client_socket, message, strlen(message), 0);
            }
            close(client_socket);
            break;  // Exit the thread
            
        } else {
            const char *message = "error*Invalid command!";
            send(client_socket, message, strlen(message), 0);
        }
    }

    if (bytes_read == 0) {
        // Client disconnected gracefully
        if (username_buffer[0] != '\0') {
            remove_user_without_broadcast(username_buffer);
            broadcast_online_users();
        }
    } else if (bytes_read < 0) {
        perror("recv failed");
    }

    // Clean up before exiting the thread
    if (username_buffer[0] != '\0') {
        remove_user_without_broadcast(username_buffer);
        broadcast_online_users();
    }
    close(client_socket);  // Close the client socket
    free(arg);  // Free the memory allocated for the argument
    return NULL;  // Exit the thread
}

int main() {
    int server_socket = socket(AF_INET, SOCK_STREAM, 0);
    if (server_socket == -1) {
        perror("Could not create socket");
        return 1;
    }

    struct sockaddr_in server_address;
    memset(&server_address, 0, sizeof(server_address));
    server_address.sin_family = AF_INET;
    server_address.sin_port = htons(SERVER_PORT);
    server_address.sin_addr.s_addr = inet_addr(SERVER_IP);

    if (bind(server_socket, (struct sockaddr*)&server_address, sizeof(server_address)) < 0) {
        perror("Bind failed");
        close(server_socket);
        return 1;
    }

    if (listen(server_socket, 5) < 0) {
        perror("Listen failed");
        close(server_socket);
        return 1;
    }

    // Initialize MySQL connection
    conn = mysql_init(NULL);
    if (conn == NULL) {
        fprintf(stderr, "mysql_init() failed\n");
        close(server_socket);
        return 1;
    }

    if (mysql_real_connect(conn, DB_HOST, DB_USER, DB_PASS, DB_NAME, 0, NULL, 0) == NULL) {
        fprintf(stderr, "mysql_real_connect() failed\n");
        mysql_close(conn);
        close(server_socket);
        return 1;
    }


    printf("Server is running on %s:%d\n", SERVER_IP, SERVER_PORT);

    pthread_t broadcast_thread;
    if (pthread_create(&broadcast_thread, NULL, periodic_broadcast, NULL) != 0) {
        perror("Could not create the periodic broadcast thread");
        // Handle the error as needed
    }


    // Server main loop to accept incoming connections
    while (1) {
        struct sockaddr_in client_address;
        socklen_t client_address_len = sizeof(client_address);
        int client_socket = accept(server_socket, (struct sockaddr*)&client_address, &client_address_len);
        if (client_socket < 0) {
            perror("Accept failed");
            continue;
        }

        printf("Connection accepted from %s:%d\n",
               inet_ntoa(client_address.sin_addr), ntohs(client_address.sin_port));

        pthread_t thread_id;
        int *pclient_socket = malloc(sizeof(int));
        *pclient_socket = client_socket;
        if (pthread_create(&thread_id, NULL, handle_client, (void*)pclient_socket) < 0) {
            perror("Could not create thread");
            free(pclient_socket);
        }

        pthread_detach(thread_id);
    }

    // Cleanup and close up shop
    mysql_close(conn);
    close(server_socket);
    return 0;
}
