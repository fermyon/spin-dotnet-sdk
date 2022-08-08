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
  typedef struct {
    uint8_t tag;
    union {
      outbound_pg_string_t connection_failed;
      outbound_pg_string_t query_failed;
      outbound_pg_string_t other_error;
    } val;
  } outbound_pg_pg_error_t;
  #define OUTBOUND_PG_PG_ERROR_SUCCESS 0
  #define OUTBOUND_PG_PG_ERROR_CONNECTION_FAILED 1
  #define OUTBOUND_PG_PG_ERROR_QUERY_FAILED 2
  #define OUTBOUND_PG_PG_ERROR_OTHER_ERROR 3
  void outbound_pg_pg_error_free(outbound_pg_pg_error_t *ptr);
  typedef uint8_t outbound_pg_db_data_type_t;
  #define OUTBOUND_PG_DB_DATA_TYPE_BOOLEAN 0
  #define OUTBOUND_PG_DB_DATA_TYPE_INT32 1
  #define OUTBOUND_PG_DB_DATA_TYPE_INT64 2
  #define OUTBOUND_PG_DB_DATA_TYPE_DB_STRING 3
  #define OUTBOUND_PG_DB_DATA_TYPE_OTHER 4
  typedef struct {
    outbound_pg_string_t name;
    outbound_pg_db_data_type_t data_type;
  } outbound_pg_column_t;
  void outbound_pg_column_free(outbound_pg_column_t *ptr);
  typedef struct {
    uint8_t tag;
    union {
      bool boolean;
      int32_t int32;
      int64_t int64;
      outbound_pg_string_t db_string;
    } val;
  } outbound_pg_db_value_t;
  #define OUTBOUND_PG_DB_VALUE_BOOLEAN 0
  #define OUTBOUND_PG_DB_VALUE_INT32 1
  #define OUTBOUND_PG_DB_VALUE_INT64 2
  #define OUTBOUND_PG_DB_VALUE_DB_STRING 3
  #define OUTBOUND_PG_DB_VALUE_DB_NULL 4
  #define OUTBOUND_PG_DB_VALUE_UNSUPPORTED 5
  void outbound_pg_db_value_free(outbound_pg_db_value_t *ptr);
  typedef struct {
    outbound_pg_db_value_t *ptr;
    size_t len;
  } outbound_pg_row_t;
  void outbound_pg_row_free(outbound_pg_row_t *ptr);
  typedef struct {
    outbound_pg_column_t *ptr;
    size_t len;
  } outbound_pg_list_column_t;
  void outbound_pg_list_column_free(outbound_pg_list_column_t *ptr);
  typedef struct {
    outbound_pg_row_t *ptr;
    size_t len;
  } outbound_pg_list_row_t;
  void outbound_pg_list_row_free(outbound_pg_list_row_t *ptr);
  typedef struct {
    outbound_pg_list_column_t columns;
    outbound_pg_list_row_t rows;
  } outbound_pg_row_set_t;
  void outbound_pg_row_set_free(outbound_pg_row_set_t *ptr);
  typedef struct {
    outbound_pg_string_t *ptr;
    size_t len;
  } outbound_pg_list_string_t;
  void outbound_pg_list_string_free(outbound_pg_list_string_t *ptr);
  typedef struct {
    bool is_err;
    union {
      outbound_pg_row_set_t ok;
      outbound_pg_pg_error_t err;
    } val;
  } outbound_pg_expected_row_set_pg_error_t;
  void outbound_pg_expected_row_set_pg_error_free(outbound_pg_expected_row_set_pg_error_t *ptr);
  typedef struct {
    bool is_err;
    union {
      uint64_t ok;
      outbound_pg_pg_error_t err;
    } val;
  } outbound_pg_expected_u64_pg_error_t;
  void outbound_pg_expected_u64_pg_error_free(outbound_pg_expected_u64_pg_error_t *ptr);
  void outbound_pg_query(outbound_pg_string_t *address, outbound_pg_string_t *statement, outbound_pg_list_string_t *params, outbound_pg_expected_row_set_pg_error_t *ret0);
  void outbound_pg_execute(outbound_pg_string_t *address, outbound_pg_string_t *statement, outbound_pg_list_string_t *params, outbound_pg_expected_u64_pg_error_t *ret0);
  #ifdef __cplusplus
}
#endif
#endif
