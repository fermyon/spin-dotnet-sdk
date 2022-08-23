#include <stdlib.h>

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
#include "spin-config.h"

void spin_attach_internal_calls()
{
    mono_add_internal_call("Fermyon.Spin.Sdk.OutboundHttpInterop::wasi_outbound_http_request", wasi_outbound_http_request);

    mono_add_internal_call("Fermyon.Spin.Sdk.OutboundRedisInterop::outbound_redis_get", outbound_redis_get);
    mono_add_internal_call("Fermyon.Spin.Sdk.OutboundRedisInterop::outbound_redis_set", outbound_redis_set);
    mono_add_internal_call("Fermyon.Spin.Sdk.OutboundRedisInterop::outbound_redis_publish", outbound_redis_publish);

    mono_add_internal_call("Fermyon.Spin.Sdk.OutboundPgInterop::outbound_pg_query", outbound_pg_query);
    mono_add_internal_call("Fermyon.Spin.Sdk.OutboundPgInterop::outbound_pg_execute", outbound_pg_execute);

    mono_add_internal_call("Fermyon.Spin.Sdk.SpinConfigNative::spin_config_get_config", spin_config_get_config);
}
