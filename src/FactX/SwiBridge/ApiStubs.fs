module FactX.SwiBridge.ApiStubs

open System
open System.Runtime.InteropServices

// The embedding API is defined in <SWI-Prolog.h>

// Target 5.0.1
[<Literal>]
let SwiDLL = @"C:\Program Files\swipl\bin\libswipl.dll"

type SizeT = int
type SizeTPtr = IntPtr
type StringPtr = IntPtr

type AtomT = IntPtr                     // Prolog atom
type FunctorT = IntPtr                  // Name/arity pair
type ModuleT = IntPtr                   // Prolog module
type PredicateT = IntPtr                // Prolog procedure
type RecordT = IntPtr                   // Prolog recorded term

type TermT = IntPtr

type QidT = IntPtr
type FidT = IntPtr

type AtomTPtr = IntPtr 

// Foreign context frames
// PL_EXPORT(fid_t)	PL_open_foreign_frame(void);
[<DllImport(SwiDLL, EntryPoint="PL_open_foreign_frame", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern FidT PL_open_foreign_frame();

// PL_EXPORT(void)		PL_rewind_foreign_frame(fid_t cid);
[<DllImport(SwiDLL, EntryPoint="PL_rewind_foreign_frame", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern void PL_rewind_foreign_frame(FidT cid);

// PL_EXPORT(void)		PL_close_foreign_frame(fid_t cid);
[<DllImport(SwiDLL, EntryPoint="PL_close_foreign_frame", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern void PL_close_foreign_frame(FidT cid);

// PL_EXPORT(void)		PL_discard_foreign_frame(fid_t cid);
[<DllImport(SwiDLL, EntryPoint="PL_discard_foreign_frame", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern void PL_discard_foreign_frame(FidT cid);

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

// PL_EXPORT(void)		PL_cut_query(qid_t qid);
[<DllImport(SwiDLL, EntryPoint="PL_cut_query", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern void PL_cut_query(QidT qid);

// PL_EXPORT(qid_t)	PL_current_query(void);
[<DllImport(SwiDLL, EntryPoint="PL_current_query", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern QidT PL_current_query();


// Simplified (but less flexible) call-back
// PL_EXPORT(int)		PL_call(term_t t, module_t m);
[<DllImport(SwiDLL, EntryPoint="PL_call", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_call(TermT t, ModuleT m);


// PL_EXPORT(int)  PL_call_predicate(module_t m, int debug,
//                      predicate_t pred, term_t t0);
[<DllImport(SwiDLL, EntryPoint="PL_call_predicate", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_call_predicate(ModuleT m, int debug, PredicateT pred, TermT t0);

// Handling exceptions 
// PL_EXPORT(term_t)	PL_exception(qid_t qid);
[<DllImport(SwiDLL, EntryPoint="PL_exception", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern TermT PL_exception(QidT qid);

// PL_EXPORT(int)		PL_raise_exception(term_t exception);
[<DllImport(SwiDLL, EntryPoint="PL_raise_exception", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_raise_exception(TermT exceptn);

// PL_EXPORT(int)		PL_throw(term_t exception);
[<DllImport(SwiDLL, EntryPoint="PL_throw", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_throw(TermT exceptn);

// PL_EXPORT(void)		PL_clear_exception(void);
[<DllImport(SwiDLL, EntryPoint="PL_clear_exception", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern void PL_clear_exception();


// Engine-based coroutining
// PL_EXPORT(term_t)	PL_yielded(qid_t qid);
[<DllImport(SwiDLL, EntryPoint="PL_yielded", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern TermT PL_yielded(QidT qid);

// *******************************
// *        TERM-REFERENCES	*
// *******************************

// Creating and destroying term-refs

// PL_EXPORT(term_t)	PL_new_term_refs(int n);
[<DllImport(SwiDLL, EntryPoint="PL_new_term_refs", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern TermT PL_new_term_refs(int n);

// PL_EXPORT(term_t)	PL_new_term_ref(void);
[<DllImport(SwiDLL, EntryPoint="PL_new_term_ref", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern TermT PL_new_term_ref();

// PL_EXPORT(term_t)	PL_copy_term_ref(term_t from);
[<DllImport(SwiDLL, EntryPoint="PL_copy_term_ref", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern TermT PL_copy_term_ref(TermT from);

// PL_EXPORT(void)		PL_reset_term_refs(term_t r);
[<DllImport(SwiDLL, EntryPoint="PL_reset_term_refs", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern void PL_reset_term_refs(TermT r);

// Constants 
// PL_EXPORT(atom_t)	PL_new_atom(const char *s);
[<DllImport(SwiDLL, EntryPoint="PL_new_atom", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern AtomT PL_new_atom(string s);

// PL_EXPORT(atom_t)	PL_new_atom_nchars(size_t len, const char *s);
[<DllImport(SwiDLL, EntryPoint="PL_new_atom_nchars", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern AtomT PL_new_atom_nchars(SizeT len, string s);

// PL_EXPORT(atom_t)	PL_new_atom_wchars(size_t len, const pl_wchar_t *s);
// PL_EXPORT(atom_t)	PL_new_atom_mbchars(int rep, size_t len, const char *s);

// PL_EXPORT(const char *)	PL_atom_chars(atom_t a);
[<DllImport(SwiDLL, EntryPoint="PL_atom_chars", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern string PL_atom_chars(AtomT a);

// PL_EXPORT(const char *)	PL_atom_nchars(atom_t a, size_t *len);
[<DllImport(SwiDLL, EntryPoint="PL_atom_nchars", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern string PL_atom_nchars(AtomT a, SizeTPtr len);

// PL_EXPORT(functor_t)	PL_new_functor_sz(atom_t f, size_t a);
[<DllImport(SwiDLL, EntryPoint="PL_new_functor_sz", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern FunctorT PL_new_functor_sz(AtomT f, SizeT a);

// PL_EXPORT(functor_t)	PL_new_functor(atom_t f, int a);
[<DllImport(SwiDLL, EntryPoint="PL_new_functor", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern FunctorT PL_new_functor(AtomT f, int a);

// PL_EXPORT(atom_t)	PL_functor_name(functor_t f);
[<DllImport(SwiDLL, EntryPoint="PL_functor_name", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern AtomT PL_functor_name(FunctorT f);

// PL_EXPORT(int)		PL_functor_arity(functor_t f);
[<DllImport(SwiDLL, EntryPoint="PL_functor_arity", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_functor_arity(FunctorT f);

// PL_EXPORT(size_t)	PL_functor_arity_sz(functor_t f);
[<DllImport(SwiDLL, EntryPoint="PL_functor_arity_sz", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern SizeT PL_functor_arity_sz(FunctorT f);

// Get C-values from Prolog terms 
//PL_EXPORT(int)		PL_get_atom(term_t t, atom_t *a) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_get_atom", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_get_atom(TermT t, AtomTPtr a);

//PL_EXPORT(int)		PL_get_bool(term_t t, int *value) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_get_bool", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_get_bool(TermT t, IntPtr value);

//PL_EXPORT(int)		PL_get_atom_chars(term_t t, char **a) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_get_atom_chars", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_get_atom_chars(TermT t, StringPtr value);


// PL_EXPORT(int)		PL_get_chars(term_t t, char **s, unsigned int flags) WUNUSED;
// WARNING - unsigned int maps to what?
[<DllImport(SwiDLL, EntryPoint="PL_get_chars", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_get_chars(TermT t, StringPtr value, uint32 flags);


// PL_EXPORT(int)		PL_get_integer(term_t t, int *i) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_get_integer", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_get_integer(TermT t, IntPtr i);

// PL_EXPORT(int)		PL_get_long(term_t t, long *i) WUNUSED;
// WARNING - long *
[<DllImport(SwiDLL, EntryPoint="PL_get_long", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_get_long(TermT t, IntPtr i);

// PL_EXPORT(int)  PL_put_term_from_chars(term_t t, int flags, size_t len, const char *s);
[<DllImport(SwiDLL, EntryPoint="PL_put_term_from_chars", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_term_from_chars(TermT t, int flags, SizeT len, string s);


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