#include <stdlib.h>
#include "test.h"

__attribute__((weak, export_name("canonical_abi_realloc"))) void *canonical_abi_realloc(
    void *ptr,
    size_t orig_size,
    size_t org_align,
    size_t new_size)
{
  void *ret = realloc(ptr, new_size);
  if (!ret)
    abort();
  return ret;
}

__attribute__((weak, export_name("canonical_abi_free"))) void canonical_abi_free(
    void *ptr,
    size_t size,
    size_t align)
{
  free(ptr);
}
__attribute__((import_module("test"), import_name("test"))) void __wasm_import_test_test(float);
void test_test(float a)
{
  __wasm_import_test_test(a);
}
