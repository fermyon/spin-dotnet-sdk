#ifndef __UTIL_H
#define __UTIL_H

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

typedef uint8_t set_member_err_t;

#define SET_MEMBER_ERR_OK 0
#define SET_MEMBER_ERR_NOT_FOUND 1
#define SET_MEMBER_ERR_EXCEPTION 2
#define SET_MEMBER_ERR_READONLY 3

set_member_err_t set_property(MonoClass* klass, MonoObject* instance, const char* name, void* value);
set_member_err_t set_field(MonoClass* klass, MonoObject* instance, const char* name, void* value);

typedef uint8_t get_member_err_t;

#define GET_MEMBER_ERR_OK 0
#define GET_MEMBER_ERR_NOT_FOUND 1

get_member_err_t get_field(MonoClass* klass, MonoObject* instance, const char* name, void* receiver);

typedef uint8_t resolve_err_t;

#define RESOLVE_ERR_OK 0
#define RESOLVE_ERR_NO_MATCH 1
#define RESOLVE_ERR_IMAGE_NOT_RESOLVED 2
#define RESOLVE_ERR_TYPEDEF_TABLE_NOT_RESOLVED 3

resolve_err_t find_decorated_method(MonoAssembly* assembly, const char* attr_name, MonoMethod** decorated_method);

typedef uint8_t entry_points_err_t;

#define EP_ERR_OK 0
#define EP_ERR_NO_ENTRY_ASSEMBLY 1
#define EP_ERR_NO_HANDLER_METHOD 2
#define EP_ERR_NO_SIGNATURE 3
#define EP_ERR_NO_REQUEST_CLASS 4
#define EP_ERR_NO_INTEROP_HELPER_CLASS 5
#define EP_ERR_NO_PARAM_TYPE 6
#define EP_ERR_NO_SDK_IMAGE 7

entry_points_err_t find_entry_points(const char* attr_name, const char* interop_helper_name, MonoMethod** handler, MonoClass** interop_helper_class, MonoImage** sdk_dll_image);

#endif
