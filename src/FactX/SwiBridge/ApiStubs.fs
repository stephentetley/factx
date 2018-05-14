module FactX.SwiBridge.ApiStubs

open System
open System.Runtime.InteropServices

// The embedding API is defined in <SWI-Prolog.h>

// Target 5.0.1
[<Literal>]
let SwiDLL = @"C:\Program Files\swipl\bin\libswipl.dll"

type AtomT = IntPtr                     // Prolog atom
type FunctorT = IntPtr                  // Name/arity pair
type ModuleT = IntPtr                   // Prolog module
type PredicateT = IntPtr                // Prolog procedure
type RecordT = IntPtr                   // Prolog recorded term

type TermT = IntPtr

type QidT = IntPtr
type FidT = IntPtr

// Foreign context frames
// PL_EXPORT(fid_t)	PL_open_foreign_frame(void);
[<DllImport(SwiDLL, EntryPoint="PL_open_foreign_frame", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern FidT PL_open_foreign_frame();


// PL_EXPORT(void)		PL_close_foreign_frame(fid_t cid);
[<DllImport(SwiDLL, EntryPoint="PL_close_foreign_frame", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern void PL_close_foreign_frame(FidT cid);

// Finding predicates
// PL_EXPORT(predicate_t)	PL_pred(functor_t f, module_t m);
[<DllImport(SwiDLL, EntryPoint="PL_pred", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern PredicateT PL_pred(FunctorT t, ModuleT m);

// PL_EXPORT(predicate_t)	PL_predicate(const char *name, int arity, const char* module);
[<DllImport(SwiDLL, EntryPoint="PL_predicate", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern PredicateT PL_predicate(string name, int arity, string m);


// Call-back 
// PL_EXPORT(qid_t)  PL_open_query(module_t m, int flags, predicate_t pred, term_t t0);
[<DllImport(SwiDLL, EntryPoint="PL_open_query", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern QidT PL_open_query(ModuleT m, int flags, PredicateT pred, TermT t0);


// PL_EXPORT(int)		PL_next_solution(qid_t qid) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_next_solution", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_next_solution(QidT qid);

// PL_EXPORT(void)		PL_close_query(qid_t qid);
[<DllImport(SwiDLL, EntryPoint="PL_close_query", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern void PL_close_query(QidT qid);



// PL_EXPORT(int)	PL_chars_to_term(const char *chars, term_t term);
[<DllImport(SwiDLL, EntryPoint="PL_chars_to_term", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_chars_to_term(string chars, TermT term);





// *******************************
// * EMBEDDING

// PL_EXPORT(int)		PL_initialise(int argc, char **argv);
[<DllImport(SwiDLL, EntryPoint="PL_initialise", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_initialise(int argc, 
    [<MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStr, SizeParamIndex=1s)>] string[] argv);


// PL_EXPORT(int)		PL_cleanup(int status);
[<DllImport(SwiDLL, EntryPoint="PL_cleanup", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_cleanup(int status);

// PL_EXPORT(void)		PL_cleanup_fork();
[<DllImport(SwiDLL, EntryPoint="PL_cleanup_fork", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern void PL_cleanup_fork();

// PL_EXPORT(int)		PL_halt(int status);
[<DllImport(SwiDLL, EntryPoint="PL_halt", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_halt(int status);