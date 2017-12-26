#include "stdinc.h"
#include "TraceStack.h"  
#include <windows.h>  
#include <psapi.h>  
#include <iostream>  
#include <sstream>  
#include <cstddef>  
#include <dbghelp.h>  
#include "StackWalker.h"
#pragma comment( lib, "dbghelp" )  
static char const szRCSID[] = "$Id: SimpleSymbolEngine.cpp,v 1.4 2005/05/04 21:52:05 Eleanor Exp $";
//////////////////////////////////////////////////////////////////////////////////////  
// Singleton for the engine (SymInitialize doesn't support multiple calls)  
SimpleSymbolEngine& SimpleSymbolEngine::instance()
{
	static SimpleSymbolEngine theEngine;
	return theEngine;
}

/////////////////////////////////////////////////////////////////////////////////////  
SimpleSymbolEngine::SimpleSymbolEngine()
{
	hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, GetProcessId(GetCurrentProcess()));
	DWORD dwOpts = SymGetOptions();
	dwOpts |= SYMOPT_LOAD_LINES | SYMOPT_DEFERRED_LOADS;
	SymSetOptions(dwOpts);
	SymInitialize(hProcess, nullptr, true);
}

/////////////////////////////////////////////////////////////////////////////////////  
SimpleSymbolEngine::~SimpleSymbolEngine()
{
	SymCleanup(hProcess);
}

/////////////////////////////////////////////////////////////////////////////////////  
std::string SimpleSymbolEngine::addressToString(PVOID address) const
{
	std::ostringstream oss;
	// First the raw address  
	oss << "0x" << address;
	// Then any name for the symbol  
	struct tagSymInfo
	{
		IMAGEHLP_SYMBOL symInfo;
		char nameBuffer[4 * 256];
	} SymInfo = { {sizeof(IMAGEHLP_SYMBOL)} };
	IMAGEHLP_SYMBOL* pSym = &SymInfo.symInfo;
	pSym->MaxNameLength = sizeof(SymInfo) - offsetof(tagSymInfo, symInfo.Name);
	DWORD dwDisplacement;
	if (SymGetSymFromAddr(hProcess, reinterpret_cast<DWORD>(address), &dwDisplacement, pSym))
	{
		oss << " " << pSym->Name;
		if (dwDisplacement != 0)
			oss << "+0x" << std::hex << dwDisplacement << std::dec;
	}

	// Finally any file/line number  
	IMAGEHLP_LINE lineInfo = { sizeof(IMAGEHLP_LINE) };
	if (SymGetLineFromAddr(hProcess, reinterpret_cast<DWORD>(address), &dwDisplacement, &lineInfo))
	{
		char const* pDelim = strrchr(lineInfo.FileName, '//');
		oss << " at " << (pDelim ? pDelim + 1 : lineInfo.FileName) << "(" << lineInfo.LineNumber << ")";
	}
	return oss.str();
}

/////////////////////////////////////////////////////////////////////////////////////  
// StackTrace: try to trace the stack to the given output  
string SimpleSymbolEngine::StackTrace(PCONTEXT pContext) const
{
	HANDLE t_handle = GetCurrentThread();
	std::ostringstream os;
	os << "  Frame       Code address\n";
	STACKFRAME stackFrame = { 0 };
	stackFrame.AddrPC.Offset = pContext->Eip;
	stackFrame.AddrPC.Mode = AddrModeFlat;
	stackFrame.AddrFrame.Offset = pContext->Ebp;
	stackFrame.AddrFrame.Mode = AddrModeFlat;
	stackFrame.AddrStack.Offset = pContext->Esp;
	stackFrame.AddrStack.Mode = AddrModeFlat;
	while (::StackWalk(
		IMAGE_FILE_MACHINE_I386,
		hProcess,
		t_handle, // this value doesn't matter much if previous one is a real handle  
		&stackFrame,
		pContext,
		nullptr,
		::SymFunctionTableAccess,
		::SymGetModuleBase,
		nullptr))
	{
		if (stackFrame.AddrPC.Offset == 0x0)
			break;
		os << "  0x" << reinterpret_cast<PVOID>(stackFrame.AddrFrame.Offset) << "  " << addressToString(reinterpret_cast<PVOID>(stackFrame.AddrPC.Offset)) << "\n";
	}
	os.flush();
	return os.str();
}

/////////////////////////////////////////////////////////////////////////////////////  
// StackTrace: try to trace the stack to the given output  
void SimpleSymbolEngine::PrintStackTrace()
{
	CONTEXT context;
	GET_CURRENT_CONTEXT(context, CONTEXT_FULL);
	SimpleSymbolEngine engine;
	cout << engine.StackTrace(&context) << endl;
}