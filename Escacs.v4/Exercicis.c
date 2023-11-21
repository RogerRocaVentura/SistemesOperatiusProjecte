#include <stdio.h>
#include <stdlib.h>
#include <string.h>

typedef struct{
    char titulo[20];
    char autor[20];
    int precio, año;
}Libro;


typedef struct{
    Libro Libros[20];
    int num;
}ListaLibros;

int PrimerLibro(ListaLibros *l, int año, int precio, char resultado[200]){
    for (int i = 0; i < l->num; i++) {
        if (l->Libros[i].precio < precio && l->Libros[i].año == año) {
            strcpy(resultado, l->Libros[i].titulo);
            return 0; // Book found
        }
    }
    return -1; // No book found
}
