#ifndef __BINDINGS_OUTBOUND_PG_H
#define __BINDINGS_OUTBOUND_PG_H
#ifdef __cplusplus
extern "C"
{
  #endif
  
  #include <stdint.h>
  #include <stdbool.h>
  
  typedef struct {
    char *ptr;
    size_t len;
  } outbound_pg_string_t;
  
  void outbound_pg_string_set(outbound_pg_string_t *ret, const char *s);
  void outbound_pg_string_dup(outbound_pg_string_t *ret, const char *s);
  void outbound_pg_string_free(outbound_pg_string_t *ret);
  typedef uint8_t outbound_pg_error_t;
  #define OUTBOUND_PG_ERROR_SUCCESS 0
  #define OUTBOUND_PG_ERROR_ERROR 1
  typedef struct {
    uint8_t *ptr;
    size_t len;
  } outbound_pg_payload_t;
  void outbound_pg_payload_free(outbound_pg_payload_t *ptr);
  typedef struct {
    outbound_pg_string_t *ptr;
    size_t len;
  } outbound_pg_list_string_t;
  void outbound_pg_list_string_free(outbound_pg_list_string_t *ptr);
  typedef struct {
    outbound_pg_payload_t *ptr;
    size_t len;
  } outbound_pg_list_payload_t;
  void outbound_pg_list_payload_free(outbound_pg_list_payload_t *ptr);
  typedef struct {
    outbound_pg_list_payload_t *ptr;
    size_t len;
  } outbound_pg_list_list_payload_t;
  void outbound_pg_list_list_payload_free(outbound_pg_list_list_payload_t *ptr);
  outbound_pg_error_t outbound_pg_query(outbound_pg_string_t *address, outbound_pg_string_t *statement, outbound_pg_list_string_t *params, outbound_pg_list_list_payload_t *ret0);
  outbound_pg_error_t outbound_pg_execute(outbound_pg_string_t *address, outbound_pg_string_t *statement, outbound_pg_list_string_t *params, uint64_t *ret0);
  #ifdef __cplusplus
}
#endif
#endif
