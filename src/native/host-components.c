#include <stdlib.h>
#include <string.h>

#include <mono-wasi/driver.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/class.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/image.h>
#include <mono/metadata/metadata.h>
#include <mono/metadata/object.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/reflection.h>
#include <mono/utils/mono-publib.h>

#include "host-components.h"
#include "wasi-outbound-http.h"
#include "outbound-redis.h"
#include "outbound-pg.h"

typedef struct {
    char *ptr;
    size_t len;
} abi_string_t;

void* abi_alloc(size_t size) {
    return realloc(NULL, size);
}

void abi_string_dup(abi_string_t *ret, const char *s) {
    ret->len = strlen(s);
    ret->ptr = realloc(NULL, ret->len);
    memcpy(ret->ptr, s, ret->len);
}

void spin_attach_internal_calls()
{
    mono_add_internal_call("InteropString::abi_string_dup", abi_string_dup);
    mono_add_internal_call("Abi::abi_alloc", abi_alloc);

    mono_add_internal_call("Fermyon.Spin.Sdk.OutboundHttpInterop::wasi_outbound_http_request", wasi_outbound_http_request);

    mono_add_internal_call("Fermyon.Spin.Sdk.OutboundRedisInterop::outbound_redis_get", outbound_redis_get);
    mono_add_internal_call("Fermyon.Spin.Sdk.OutboundRedisInterop::outbound_redis_set", outbound_redis_set);
    mono_add_internal_call("Fermyon.Spin.Sdk.OutboundRedisInterop::outbound_redis_publish", outbound_redis_publish);

    mono_add_internal_call("Fermyon.Spin.Sdk.OutboundPgInterop::outbound_pg_query", outbound_pg_query);
    mono_add_internal_call("Fermyon.Spin.Sdk.OutboundPgInterop::outbound_pg_execute", outbound_pg_execute);
}
