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
type Fid_T = IntPtr

type AtomTPtr = IntPtr 

// Foreign context frames
// PL_EXPORT(fid_t)	PL_open_foreign_frame(void);
[<DllImport(SwiDLL, EntryPoint="PL_open_foreign_frame", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern Fid_T PL_open_foreign_frame();

// PL_EXPORT(void)		PL_rewind_foreign_frame(fid_t cid);
[<DllImport(SwiDLL, EntryPoint="PL_rewind_foreign_frame", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern void PL_rewind_foreign_frame(Fid_T cid);

// PL_EXPORT(void)		PL_close_foreign_frame(fid_t cid);
[<DllImport(SwiDLL, EntryPoint="PL_close_foreign_frame", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern void PL_close_foreign_frame(Fid_T cid);

// PL_EXPORT(void)		PL_discard_foreign_frame(fid_t cid);
[<DllImport(SwiDLL, EntryPoint="PL_discard_foreign_frame", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern void PL_discard_foreign_frame(Fid_T cid);

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
// WARNING - long * TODO
[<DllImport(SwiDLL, EntryPoint="PL_get_long", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_get_long(TermT t, IntPtr i);


// PL_EXPORT(int)		PL_get_float(term_t t, double *f) WUNUSED;
// WARNING - double * TODO
[<DllImport(SwiDLL, EntryPoint="PL_get_float", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_get_float(TermT t, IntPtr i);

// ...

// Verify types
// PL_EXPORT(int)		PL_term_type(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_term_type", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_term_type(TermT t);

// PL_EXPORT(int)		PL_is_variable(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_variable", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_variable(TermT t);

//PL_EXPORT(int)		PL_is_ground(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_ground", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_ground(TermT t);

//PL_EXPORT(int)		PL_is_atom(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_atom", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_atom(TermT t);

//PL_EXPORT(int)		PL_is_integer(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_integer", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_integer(TermT t);

//PL_EXPORT(int)		PL_is_string(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_string", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_string(TermT t);

//PL_EXPORT(int)		PL_is_float(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_float", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_float(TermT t);

//PL_EXPORT(int)		PL_is_rational(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_rational", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_rational(TermT t);

//PL_EXPORT(int)		PL_is_compound(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_compound", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_compound(TermT t);

//PL_EXPORT(int)		PL_is_callable(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_callable", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_callable(TermT t);

//PL_EXPORT(int)		PL_is_functor(term_t t, functor_t f);
[<DllImport(SwiDLL, EntryPoint="PL_is_functor", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_functor(TermT t, FunctorT f);

//PL_EXPORT(int)		PL_is_list(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_list", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_list(TermT t);

//PL_EXPORT(int)		PL_is_pair(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_pair", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_pair(TermT t);

//PL_EXPORT(int)		PL_is_atomic(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_atomic", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_atomic(TermT t);

//PL_EXPORT(int)		PL_is_number(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_number", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_number(TermT t);

//PL_EXPORT(int)		PL_is_acyclic(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_acyclic", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_acyclic(TermT t);


// Assign to term-references 
//PL_EXPORT(int)		PL_put_variable(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_put_variable", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_variable(TermT t);

//PL_EXPORT(int)		PL_put_atom(term_t t, atom_t a);
[<DllImport(SwiDLL, EntryPoint="PL_put_atom", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_atom(TermT t, AtomT a);


// PL_EXPORT(int)		PL_put_integer(term_t t, long i) WUNUSED;
// WARNING - long TODO
[<DllImport(SwiDLL, EntryPoint="PL_put_integer", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_integer(TermT t, int i);

// PL_EXPORT(int)		PL_put_functor(term_t t, functor_t functor) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_put_functor", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_functor(TermT t, FunctorT functor);

// PL_EXPORT(int)		PL_put_list(term_t l) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_put_list", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_list(TermT l);

// PL_EXPORT(int)		PL_put_nil(term_t l);
[<DllImport(SwiDLL, EntryPoint="PL_put_nil", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_nil(TermT l);

// PL_EXPORT(int)		PL_put_term(term_t t1, term_t t2);
[<DllImport(SwiDLL, EntryPoint="PL_put_term", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_term(TermT t1, TermT t2);

// ** construct a functor or list-cell **

// PL_EXPORT(int)		PL_cons_functor_v(term_t h, functor_t fd, term_t a0) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_cons_functor_v", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_cons_functor_v(TermT h, FunctorT fd, TermT a0);

// PL_EXPORT(int)		PL_cons_list(term_t l, term_t h, term_t t) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_cons_list", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_cons_list(TermT l, TermT h, TermT t);

// ** Unify term-references **
// PL_EXPORT(int)		PL_unify(term_t t1, term_t t2) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_unify", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_unify(TermT t1, TermT t2);

// PL_EXPORT(int)		PL_unify_atom(term_t t, atom_t a) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_unify_atom", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_unify_atom(TermT t, AtomT a);


// PL_EXPORT(int)		PL_unify_bool(term_t t, int n) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_unify_bool", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_unify_bool(TermT t, int n);



// *******************************
// *	       LISTS		*
// *******************************

// PL_EXPORT(int)		PL_skip_list(term_t list, term_t tail, size_t *len);
[<DllImport(SwiDLL, EntryPoint="PL_skip_list", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_skip_list(TermT list, TermT tail, SizeTPtr len);


// ...

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

// PL_EXPORT(int)		PL_is_initialised(int *argc, char ***argv);
[<DllImport(SwiDLL, EntryPoint="PL_is_initialised", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_initialise(IntPtr argc, IntPtr argv);

// PL_EXPORT(int)		PL_toplevel(void);
[<DllImport(SwiDLL, EntryPoint="PL_toplevel", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_toplevel();


// PL_EXPORT(int)		PL_cleanup(int status);
[<DllImport(SwiDLL, EntryPoint="PL_cleanup", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_cleanup(int status);

// PL_EXPORT(void)		PL_cleanup_fork();
[<DllImport(SwiDLL, EntryPoint="PL_cleanup_fork", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern void PL_cleanup_fork();

// PL_EXPORT(int)		PL_halt(int status);
[<DllImport(SwiDLL, EntryPoint="PL_halt", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_halt(int status);