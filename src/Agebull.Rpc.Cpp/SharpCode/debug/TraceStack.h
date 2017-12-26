#pragma once
#include <windows.h>
#include <iostream> 
#include "StackWalker.h"
class SimpleSymbolEngine
{
	HANDLE hProcess;
	string addressToString(PVOID address) const;
public:
	SimpleSymbolEngine();
	~SimpleSymbolEngine();
	static SimpleSymbolEngine& instance();
	string StackTrace(PCONTEXT pContext) const;
	static void PrintStackTrace();
};

#define print_stack() SimpleSymbolEngine::PrintStackTrace()


inline std::string get_call_stack()
{
	CONTEXT context;
	GET_CURRENT_CONTEXT(context, CONTEXT_FULL);
	SimpleSymbolEngine engine;
	return engine.StackTrace(&context);
}
