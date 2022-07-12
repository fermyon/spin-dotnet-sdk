#include <stdlib.h>
#include "./test.h"
#include "./test.c"

#include <mono-wasi/driver.h>

void internal_test(float a)
{
    test_test(a);
}

void spin_attach_internal_calls()
{
    mono_add_internal_call("Fermyon.Spin.Sdk.Interop::Test", __wasm_import_test_test);
}
