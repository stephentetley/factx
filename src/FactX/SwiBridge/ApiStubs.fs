module FactX.SwiBridge.ApiStubs

open System
open System.Runtime.InteropServices

// The embedding API is defined in <SWI-Prolog.h>

// Target 5.0.1
[<Literal>]
let SwiDLL = @"C:\Program Files\swipl\bin\libswipl.dll"

// PL_EXPORT(int)		PL_initialise(int argc, char **argv);
[<DllImport(SwiDLL, EntryPoint="PL_initialise", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_initialise(int argc, string [] argv);


// PL_EXPORT(int)		PL_cleanup(int status);
[<DllImport(SwiDLL, EntryPoint="PL_cleanup", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_cleanup(int status);

// PL_EXPORT(void)		PL_cleanup_fork();
[<DllImport(SwiDLL, EntryPoint="PL_cleanup_fork", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern void PL_cleanup_fork();

// PL_EXPORT(int)		PL_halt(int status);
[<DllImport(SwiDLL, EntryPoint="PL_halt", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_halt(int status);