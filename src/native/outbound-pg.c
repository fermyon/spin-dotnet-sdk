#include <stdlib.h>
#include "outbound-pg.h"

__attribute__((weak, export_name("canonical_abi_realloc")))
void *canonical_abi_realloc(
void *ptr,
size_t orig_size,
size_t org_align,
size_t new_size
) {
  void *ret = realloc(ptr, new_size);
  if (!ret)
  abort();
  return ret;
}

__attribute__((weak, export_name("canonical_abi_free")))
void canonical_abi_free(
void *ptr,
size_t size,
size_t align
) {
  free(ptr);
}
#include <string.h>

void outbound_pg_string_set(outbound_pg_string_t *ret, const char *s) {
  ret->ptr = (char*) s;
  ret->len = strlen(s);
}

void outbound_pg_string_dup(outbound_pg_string_t *ret, const char *s) {
  ret->len = strlen(s);
  ret->ptr = canonical_abi_realloc(NULL, 0, 1, ret->len);
  memcpy(ret->ptr, s, ret->len);
}

void outbound_pg_string_free(outbound_pg_string_t *ret) {
  canonical_abi_free(ret->ptr, ret->len, 1);
  ret->ptr = NULL;
  ret->len = 0;
}
void outbound_pg_payload_free(outbound_pg_payload_t *ptr) {
  canonical_abi_free(ptr->ptr, ptr->len * 1, 1);
}
void outbound_pg_list_string_free(outbound_pg_list_string_t *ptr) {
  for (size_t i = 0; i < ptr->len; i++) {
    outbound_pg_string_free(&ptr->ptr[i]);
  }
  canonical_abi_free(ptr->ptr, ptr->len * 8, 4);
}
void outbound_pg_list_payload_free(outbound_pg_list_payload_t *ptr) {
  for (size_t i = 0; i < ptr->len; i++) {
    outbound_pg_payload_free(&ptr->ptr[i]);
  }
  canonical_abi_free(ptr->ptr, ptr->len * 8, 4);
}
void outbound_pg_list_list_payload_free(outbound_pg_list_list_payload_t *ptr) {
  for (size_t i = 0; i < ptr->len; i++) {
    outbound_pg_list_payload_free(&ptr->ptr[i]);
  }
  canonical_abi_free(ptr->ptr, ptr->len * 8, 4);
}
typedef struct {
  bool is_err;
  union {
    outbound_pg_list_list_payload_t ok;
    outbound_pg_error_t err;
  } val;
} outbound_pg_expected_list_list_payload_error_t;
typedef struct {
  bool is_err;
  union {
    uint64_t ok;
    outbound_pg_error_t err;
  } val;
} outbound_pg_expected_u64_error_t;

__attribute__((aligned(8)))
static uint8_t RET_AREA[16];
__attribute__((import_module("outbound-pg"), import_name("query")))
void __wasm_import_outbound_pg_query(int32_t, int32_t, int32_t, int32_t, int32_t, int32_t, int32_t);
outbound_pg_error_t outbound_pg_query(outbound_pg_string_t *address, outbound_pg_string_t *statement, outbound_pg_list_string_t *params, outbound_pg_list_list_payload_t *ret0) {
  int32_t ptr = (int32_t) &RET_AREA;
  __wasm_import_outbound_pg_query((int32_t) (*address).ptr, (int32_t) (*address).len, (int32_t) (*statement).ptr, (int32_t) (*statement).len, (int32_t) (*params).ptr, (int32_t) (*params).len, ptr);
  outbound_pg_expected_list_list_payload_error_t expected;
  switch ((int32_t) (*((uint8_t*) (ptr + 0)))) {
    case 0: {
      expected.is_err = false;
      
      expected.val.ok = (outbound_pg_list_list_payload_t) { (outbound_pg_list_payload_t*)(*((int32_t*) (ptr + 4))), (size_t)(*((int32_t*) (ptr + 8))) };
      break;
    }
    case 1: {
      expected.is_err = true;
      
      expected.val.err = (int32_t) (*((uint8_t*) (ptr + 4)));
      break;
    }
  }*ret0 = expected.val.ok;
  return expected.is_err ? expected.val.err : -1;
}
__attribute__((import_module("outbound-pg"), import_name("execute")))
void __wasm_import_outbound_pg_execute(int32_t, int32_t, int32_t, int32_t, int32_t, int32_t, int32_t);
outbound_pg_error_t outbound_pg_execute(outbound_pg_string_t *address, outbound_pg_string_t *statement, outbound_pg_list_string_t *params, uint64_t *ret0) {
  int32_t ptr = (int32_t) &RET_AREA;
  __wasm_import_outbound_pg_execute((int32_t) (*address).ptr, (int32_t) (*address).len, (int32_t) (*statement).ptr, (int32_t) (*statement).len, (int32_t) (*params).ptr, (int32_t) (*params).len, ptr);
  outbound_pg_expected_u64_error_t expected;
  switch ((int32_t) (*((uint8_t*) (ptr + 0)))) {
    case 0: {
      expected.is_err = false;
      
      expected.val.ok = (uint64_t) (*((int64_t*) (ptr + 8)));
      break;
    }
    case 1: {
      expected.is_err = true;
      
      expected.val.err = (int32_t) (*((uint8_t*) (ptr + 8)));
      break;
    }
  }*ret0 = expected.val.ok;
  return expected.is_err ? expected.val.err : -1;
}
