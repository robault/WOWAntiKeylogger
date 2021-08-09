////////////////////////////////////////////////////////////////////////////////////
// +----------------------------------------------------------------------------+ //
// |                                                                            | //
// | ENUM WINDOWS HOOKS                                                         | //
// |                                                                            | //
// +----------------------------------------------------------------------------+ //
// |                                                                            | //
// | NT Internals - http://www.ntinternals.org/                                 | //
// | alex ntinternals org                                                       | //
// | 06 September 2010                                                          | //
// |                                                                            | //
// | References:                                                                | //
// |                                                                            | //
// | Any application-defined hook procedure on my machine?                      | //
// | Zairon - http://zairon.wordpress.com/2006/12/06/                           | //
// |          any-application-defined-hook-procedure-on-my-machine/             | //
// |                                                                            | //
// | Window hooks: Inside                                                       | //
// | Twister - http://twister.rootkits.su/articles/hooks_inside.php             | //
// |                                                                            | //
// | Inject: climb through the window                                           | //
// | Twister - http://twister.rootkits.su/articles/windowinject.php             | //
// |                                                                            | //
// +----------------------------------------------------------------------------+ //
////////////////////////////////////////////////////////////////////////////////////

#include "EnumWindowsHooks.h"


int __cdecl main(int argc, char **argv)
{
    NTSTATUS NtStatus = STATUS_UNSUCCESSFUL;

    PVOID ImageBase;
    ULONG DllCharacteristics = 0;
    PVOID User32InitializeImmEntryTable = NULL;

	UNICODE_STRING DllName;
    ANSI_STRING ProcedureName;
    
    ULONG i;
    ULONG UserDelta = 0;
    ULONG HandleEntries = 0;

    SHARED_INFO *SharedInfo = NULL;
    HANDLE_ENTRY *UserHandleTable = NULL;
    HOOK *HookInfo = NULL;
    
    
	__try
    {
        system("cls");


        RtlInitUnicodeString(
			                 &DllName,
			                 L"user32");

		NtStatus = LdrLoadDll(
							  NULL,                // DllPath
							  &DllCharacteristics, // DllCharacteristics
							  &DllName,            // DllName
							  &ImageBase);         // DllHandle

		if(NtStatus == STATUS_SUCCESS)
		{
			RtlInitAnsiString(
				              &ProcedureName,
				              "User32InitializeImmEntryTable");

			NtStatus = LdrGetProcedureAddress(
											  ImageBase,                               // DllHandle
											  &ProcedureName,                          // ProcedureName
											  0,                                       // ProcedureNumber OPTIONAL
											  (PVOID*)&User32InitializeImmEntryTable); // ProcedureAddress

			if(NtStatus == STATUS_SUCCESS)
			{
				__asm
				{
					mov esi, User32InitializeImmEntryTable
					test esi, esi
					jz __exit2
					mov ecx, 0x80
						
				__loop:
					dec ecx
					test ecx, ecx
					jz __exit1

					lodsb
					cmp al, 0x50
					jnz __loop

					lodsb
					cmp al, 0x68
					jnz __loop

					lodsd
					mov SharedInfo, eax

                    jmp __exit2

				__exit1:
					mov SharedInfo, ecx
					
				__exit2:
					sub eax, eax
					mov eax, fs:[eax+0x18]
					lea eax, [eax+0x06CC]
					mov eax, [eax+0x001C]
					mov UserDelta, eax
				}

                HandleEntries = *((ULONG *)((ULONG)SharedInfo->ServerInfo + 8));

                printf(
                       " +--------------------------------------------------------------------+\n"
                       " | SHARED_INFO - %.8X                                             |\n"
                       " +--------------------------------------------------------------------+\n"
                       " | ServerInfo - %.8X                                              |\n"
                       " | HandleEntryList - %.8X                                         |\n"
                       " | HandleEntries - %.8X                                           |\n"
                       " | DisplayInfo - %.8X                                             |\n"
                       " | SharedDelta - %.8X                                             |\n"
                       " | UserDelta - %.8X                                               |\n"
                       " +--------------------------------------------------------------------+\n\n",
                       SharedInfo,
                       SharedInfo->ServerInfo,
                       SharedInfo->HandleEntryList,
                       HandleEntries,
                       SharedInfo->DisplayInfo,
                       SharedInfo->SharedDelta,
                       UserDelta);

                UserHandleTable = (HANDLE_ENTRY *)SharedInfo->HandleEntryList;
                
                for(i=0; i<HandleEntries; i++)
                {
                    if(UserHandleTable[i].Type == TYPE_HOOK)
                    {
                        __try
                        {
                            HookInfo = (HOOK *)((ULONG)UserHandleTable[i].Head - UserDelta);

                            printf(
                                   " +--------------------------------------------------------------------+\n"
                                   " | HOOK - %.8X                                                    |\n"
                                   " +--------------------------------------------------------------------+\n"
                                   " | Handle - %.8X                                                  |\n"
                                   " | LockObj - %.8X                                                 |\n"
                                   " | ThreadInfo- %.8X                                               |\n"
                                   " | Desktop1 - %.8X                                                |\n"
                                   " | Self - %.8X                                                    |\n"
                                   " | NextHook - %.8X                                                |\n"
                                   " | HookType - %.8X                                                |\n"
                                   " | FunctionAddress - %.8X                                         |\n"
                                   " | Flags - %.8X                                                   |\n"
                                   " | ModuleHandle - %.8X                                            |\n"
                                   " | Hooked - %.8X                                                  |\n"
                                   " | Desktop2 - %.8X                                                |\n"
                                   " +--------------------------------------------------------------------+\n\n",
                                   (ULONG)UserHandleTable[i].Head - UserDelta,
                                   HookInfo->Handle,
                                   HookInfo->LockObj,
                                   HookInfo->ThreadInfo,
                                   HookInfo->Desktop1,
                                   HookInfo->Self,
                                   HookInfo->NextHook,
                                   HookInfo->HookType,
                                   HookInfo->FunctionAddress,
                                   HookInfo->Flags,
                                   HookInfo->ModuleHandle,
                                   HookInfo->Hooked,
                                   HookInfo->Desktop2);
                        }
                        __except(EXCEPTION_EXECUTE_HANDLER) {}
                    }
                }
            }
        }
    }
    __except(EXCEPTION_EXECUTE_HANDLER)
    {
        printf(">> main - %.8X\n", GetExceptionCode());

        return GetExceptionCode();
    }

    return FALSE;
}