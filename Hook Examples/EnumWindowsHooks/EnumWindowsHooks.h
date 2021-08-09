#ifndef __ENUM_WINDOWS_HOOKS__
#define __ENUM_WINDOWS_HOOKS__

#include <stdio.h>
#include <stdlib.h>
#include <windows.h>

#define IMP_VOID __declspec(dllimport) VOID __stdcall
#define IMP_SYSCALL __declspec(dllimport) NTSTATUS __stdcall

#define STATUS_SUCCESS 0x00000000
#define STATUS_UNSUCCESSFUL 0xC0000001
#define STATUS_INVALID_PARAMETER_1 0xC00000EF
#define STATUS_INVALID_PARAMETER_2 0xC00000F0

typedef ULONG NTSTATUS;

typedef struct ANSI_STRING
{
    /* 0x00 */ USHORT Length;
    /* 0x02 */ USHORT MaximumLength;
    /* 0x04 */ PCHAR Buffer;
    /* 0x08 */
}
    ANSI_STRING,
  *PANSI_STRING,
**PPANSI_STRING;

typedef struct _UNICODE_STRING 
{
    /* 0x00 */ USHORT Length;
    /* 0x02 */ USHORT MaximumLength;
    /* 0x04 */ PWSTR Buffer;
    /* 0x08 */
}
    UNICODE_STRING,
  *PUNICODE_STRING,
**PPUNICODE_STRING;

#define TYPE_FREE           0
#define TYPE_WINDOW         1
#define TYPE_MENU           2
#define TYPE_CURSOR         3
#define TYPE_SETWINDOWPOS   4
#define TYPE_HOOK           5
#define TYPE_CLIPDATA       6
#define TYPE_CALLPROC       7
#define TYPE_ACCELTABLE     8
#define TYPE_DDEACCESS      9
#define TYPE_DDECONV        10
#define TYPE_DDEXACT        11
#define TYPE_MONITOR        12
#define TYPE_KBDLAYOUT      13
#define TYPE_KBDFILE        14
#define TYPE_WINEVENTHOOK   15
#define TYPE_TIMER          16
#define TYPE_INPUTCONTEXT   17
#define TYPE_CTYPES         18
#define TYPE_GENERIC        255

#define HMINDEXBITS 0x0000FFFF
#define HMUNIQSHIFT 16
#define HMUNIQBITS 0xFFFF0000

#define MAX_HANDLE_COUNT 0x8000

typedef struct _HEAD
{
    /* 0x00 */ HANDLE Handle;
    /* 0x04 */ ULONG LockObj;
    /* 0x08 */
}
    HEAD,
  *PHEAD,
**PPHEAD;

typedef struct _HANDLE_ENTRY
{
    /* 0x00 */ HEAD *Head;
    /* 0x08 */ PVOID Owner;
    /* 0x0C */ UCHAR Type;
    /* 0x0D */ UCHAR Flags;
    /* 0x0E */ USHORT Unique;
    /* 0x10 */
}
    HANDLE_ENTRY,
  *PHANDLE_ENTRY,
**PPHANDLE_ENTRY;

typedef struct _SHARED_INFO
{
    /* 0x00 */ PVOID ServerInfo;
    /* 0x04 */ HANDLE_ENTRY *HandleEntryList;
    /* 0x08 */ PVOID DisplayInfo;
    /* 0x0C */ ULONG SharedDelta;
}
    SHARED_INFO,
  *PSHARED_INFO,
**PPSHARED_INFO;

typedef struct _HOOK
{
    /* 0x00 */ ULONG Handle;
    /* 0x04 */ ULONG LockObj;
	/* 0x08 */ PVOID ThreadInfo;
	/* 0x0C */ PVOID Desktop1;
	/* 0x10 */ PVOID Self;
    /* 0x14 */ PVOID NextHook;
    /* 0x18 */ LONG HookType;
    /* 0x1C */ PVOID FunctionAddress;
    /* 0x20 */ ULONG Flags;
    /* 0x24 */ ULONG ModuleHandle;
    /* 0x28 */ PVOID Hooked;
    /* 0x2C */ PVOID Desktop2;
    /* 0x30 */
}
    HOOK,
  *PHOOK,
**PPHOOK;

IMP_SYSCALL LdrLoadDll
(
    IN PWSTR DllPath OPTIONAL,
    IN PULONG DllCharacteristics OPTIONAL,
    IN PUNICODE_STRING DllName,
    OUT PVOID *DllHandle
);

IMP_SYSCALL LdrGetProcedureAddress
(
    IN PVOID DllHandle,
    IN PANSI_STRING ProcedureName OPTIONAL,
    IN ULONG ProcedureNumber OPTIONAL,
    OUT PVOID *ProcedureAddress
);

IMP_VOID RtlInitAnsiString
(
    IN OUT PANSI_STRING DestinationString,
    IN PCSTR SourceString
);

IMP_VOID RtlInitUnicodeString
(
    IN OUT PUNICODE_STRING DestinationString,
    IN PCWSTR SourceString
);

#endif