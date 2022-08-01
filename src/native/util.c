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

#include "./util.h"

// These are generated by the WASI SDK during build
const char* dotnet_wasi_getentrypointassemblyname();
const char* dotnet_wasi_getbundledfile(const char* name, int* out_length);
void dotnet_wasi_registerbundledassemblies();

resolve_err_t find_decorated_method(MonoAssembly* assembly, const char* attr_name, MonoMethod** decorated_method) {
    MonoImage* image = mono_assembly_get_image(assembly);
    if (!image) {
        return RESOLVE_ERR_IMAGE_NOT_RESOLVED;
    }

    const MonoTableInfo* table_info = mono_image_get_table_info(image, MONO_TABLE_TYPEDEF);
    if (!table_info) {
        return RESOLVE_ERR_TYPEDEF_TABLE_NOT_RESOLVED;
    }

    int rows = mono_table_info_get_rows(table_info);

    for (int i = 0; i < rows; i++) 
    {
        uint32_t cols[MONO_TYPEDEF_SIZE];
        mono_metadata_decode_row(table_info, i, cols, MONO_TYPEDEF_SIZE);

        const char* name = mono_metadata_string_heap(image, cols[MONO_TYPEDEF_NAME]);
        const char* name_space = mono_metadata_string_heap(image, cols[MONO_TYPEDEF_NAMESPACE]);
        if (!name || !name_space) {
            continue; 
        }

        MonoClass* klass = mono_class_from_name(image, name_space, name);
        if (!klass) {
            continue;
        }

        void* iter = NULL;
        MonoMethod* method;
        while ((method = mono_class_get_methods(klass, &iter)) != NULL)
        {
            MonoCustomAttrInfo* attr_info = mono_custom_attrs_from_method(method);
            if (attr_info) {
                for (int i = 0; i < attr_info->num_attrs; ++i) {
                    char* attr_ctor_name = mono_method_full_name(attr_info->attrs[i].ctor, 1);
                    if (strstr(attr_ctor_name, attr_name) != NULL) {
                        *decorated_method = method;
                        return RESOLVE_ERR_OK;
                    }
                    mono_free(attr_ctor_name);
                }
                mono_free(attr_info);
            }
            mono_free(method);
        }

        mono_free(klass);
    }

    return RESOLVE_ERR_NO_MATCH;
}

entry_points_err_t find_entry_points(const char* attr_name, MonoMethod** handler) {
    MonoAssembly* assembly = mono_assembly_open(dotnet_wasi_getentrypointassemblyname(), NULL);
    if (!assembly) {
        return EP_ERR_NO_ENTRY_ASSEMBLY;
    }

    MonoMethod* method;
    resolve_err_t find_err = find_decorated_method(assembly, attr_name, &method);
    if (find_err) {
        return EP_ERR_NO_HANDLER_METHOD;
    }

    *handler = method;
    return EP_ERR_OK;
}
