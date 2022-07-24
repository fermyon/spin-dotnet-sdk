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

void spin_attach_internal_calls()
{
    mono_add_internal_call("Fermyon.Spin.Sdk.OutboundHttpInterop::wasi_outbound_http_request", wasi_outbound_http_request);
}
