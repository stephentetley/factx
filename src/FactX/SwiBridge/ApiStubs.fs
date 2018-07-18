// Copyright (c) Stephen Tetley 2018
// License: BSD 3 Clause

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

type Atom_T = IntPtr                    // Prolog atom
type Functor_T = IntPtr                 // Name/arity pair
type Module_T = IntPtr                  // Prolog module
type Predicate_T = IntPtr               // Prolog procedure
type Record_T = IntPtr                  // Prolog recorded term

type Term_T = IntPtr

type Qid_T = IntPtr
type Fid_T = IntPtr

type Atom_TPtr = IntPtr 


// *********************************
// *            MODULES            *
// *********************************

// 
// PL_EXPORT(module_t)	PL_context(void);
[<DllImport(SwiDLL, EntryPoint="PL_context", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern Module_T PL_context();

// PL_EXPORT(atom_t)	PL_module_name(module_t module);
[<DllImport(SwiDLL, EntryPoint="PL_module_name", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern Atom_T PL_module_name(Module_T modulet);

// PL_EXPORT(module_t)	PL_new_module(atom_t name);
[<DllImport(SwiDLL, EntryPoint="PL_new_module", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern Module_T PL_new_module(Atom_T name);

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
extern Predicate_T PL_pred(Functor_T t, Module_T m);

// PL_EXPORT(predicate_t)	PL_predicate(const char *name, int arity, const char* module);
[<DllImport(SwiDLL, EntryPoint="PL_predicate", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern Predicate_T PL_predicate([<MarshalAs(UnmanagedType.LPStr)>] string name, int arity, [<MarshalAs(UnmanagedType.LPStr)>] string m);


// Call-back 
// PL_EXPORT(qid_t)  PL_open_query(module_t m, int flags, predicate_t pred, term_t t0);
[<DllImport(SwiDLL, EntryPoint="PL_open_query", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern Qid_T PL_open_query(Module_T m, int flags, Predicate_T pred, Term_T t0);


// PL_EXPORT(int)		PL_next_solution(qid_t qid) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_next_solution", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_next_solution(Qid_T qid);

// PL_EXPORT(void)		PL_close_query(qid_t qid);
[<DllImport(SwiDLL, EntryPoint="PL_close_query", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern void PL_close_query(Qid_T qid);

// PL_EXPORT(void)		PL_cut_query(qid_t qid);
[<DllImport(SwiDLL, EntryPoint="PL_cut_query", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern void PL_cut_query(Qid_T qid);

// PL_EXPORT(qid_t)	PL_current_query(void);
[<DllImport(SwiDLL, EntryPoint="PL_current_query", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern Qid_T PL_current_query();


// Simplified (but less flexible) call-back
// PL_EXPORT(int)		PL_call(term_t t, module_t m);
[<DllImport(SwiDLL, EntryPoint="PL_call", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_call(Term_T t, Module_T m);


// PL_EXPORT(int)  PL_call_predicate(module_t m, int debug,
//                      predicate_t pred, term_t t0);
[<DllImport(SwiDLL, EntryPoint="PL_call_predicate", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_call_predicate(Module_T m, int debug, Predicate_T pred, Term_T t0);

// Handling exceptions 
// PL_EXPORT(term_t)	PL_exception(qid_t qid);
[<DllImport(SwiDLL, EntryPoint="PL_exception", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern Term_T PL_exception(Qid_T qid);

// PL_EXPORT(int)		PL_raise_exception(term_t exception);
[<DllImport(SwiDLL, EntryPoint="PL_raise_exception", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_raise_exception(Term_T exceptn);

// PL_EXPORT(int)		PL_throw(term_t exception);
[<DllImport(SwiDLL, EntryPoint="PL_throw", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_throw(Term_T exceptn);

// PL_EXPORT(void)		PL_clear_exception(void);
[<DllImport(SwiDLL, EntryPoint="PL_clear_exception", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern void PL_clear_exception();


// Engine-based coroutining
// PL_EXPORT(term_t)	PL_yielded(qid_t qid);
[<DllImport(SwiDLL, EntryPoint="PL_yielded", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern Term_T PL_yielded(Qid_T qid);

// *******************************
// *        TERM-REFERENCES      *
// *******************************

// Creating and destroying term-refs

// PL_EXPORT(term_t)	PL_new_term_refs(int n);
[<DllImport(SwiDLL, EntryPoint="PL_new_term_refs", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern Term_T PL_new_term_refs(int n);

// PL_EXPORT(term_t)	PL_new_term_ref(void);
[<DllImport(SwiDLL, EntryPoint="PL_new_term_ref", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern Term_T PL_new_term_ref();

// PL_EXPORT(term_t)	PL_copy_term_ref(term_t from);
[<DllImport(SwiDLL, EntryPoint="PL_copy_term_ref", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern Term_T PL_copy_term_ref(Term_T from);

// PL_EXPORT(void)		PL_reset_term_refs(term_t r);
[<DllImport(SwiDLL, EntryPoint="PL_reset_term_refs", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern void PL_reset_term_refs(Term_T r);

// Constants 
// PL_EXPORT(atom_t)	PL_new_atom(const char *s);
[<DllImport(SwiDLL, EntryPoint="PL_new_atom", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern Atom_T PL_new_atom([<MarshalAs(UnmanagedType.LPStr)>] string s);

// PL_EXPORT(atom_t)	PL_new_atom_nchars(size_t len, const char *s);
[<DllImport(SwiDLL, EntryPoint="PL_new_atom_nchars", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern Atom_T PL_new_atom_nchars(SizeT len, [<MarshalAs(UnmanagedType.LPStr)>] string s);

// PL_EXPORT(atom_t)	PL_new_atom_wchars(size_t len, const pl_wchar_t *s);
// PL_EXPORT(atom_t)	PL_new_atom_mbchars(int rep, size_t len, const char *s);

// PL_EXPORT(const char *)	PL_atom_chars(atom_t a);
[<DllImport(SwiDLL, EntryPoint="PL_atom_chars", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern IntPtr PL_atom_chars(Atom_T a);

// PL_EXPORT(const char *)	PL_atom_nchars(atom_t a, size_t *len);
[<DllImport(SwiDLL, EntryPoint="PL_atom_nchars", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern IntPtr PL_atom_nchars(Atom_T a, SizeTPtr len);

// PL_EXPORT(functor_t)	PL_new_functor_sz(atom_t f, size_t a);
[<DllImport(SwiDLL, EntryPoint="PL_new_functor_sz", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern Functor_T PL_new_functor_sz(Atom_T f, SizeT a);

// PL_EXPORT(functor_t)	PL_new_functor(atom_t f, int a);
[<DllImport(SwiDLL, EntryPoint="PL_new_functor", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern Functor_T PL_new_functor(Atom_T f, int a);

// PL_EXPORT(atom_t)	PL_functor_name(functor_t f);
[<DllImport(SwiDLL, EntryPoint="PL_functor_name", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern Atom_T PL_functor_name(Functor_T f);

// PL_EXPORT(int)		PL_functor_arity(functor_t f);
[<DllImport(SwiDLL, EntryPoint="PL_functor_arity", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_functor_arity(Functor_T f);

// PL_EXPORT(size_t)	PL_functor_arity_sz(functor_t f);
[<DllImport(SwiDLL, EntryPoint="PL_functor_arity_sz", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern SizeT PL_functor_arity_sz(Functor_T f);

// Get C-values from Prolog terms 
//PL_EXPORT(int)		PL_get_atom(term_t t, atom_t *a) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_get_atom", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_get_atom(Term_T t, Atom_TPtr a);

//PL_EXPORT(int)		PL_get_bool(term_t t, int *value) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_get_bool", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_get_bool(Term_T t, IntPtr value);

//PL_EXPORT(int)		PL_get_atom_chars(term_t t, char **a) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_get_atom_chars", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_get_atom_chars(Term_T t, StringPtr value);


// PL_EXPORT(int)		PL_get_chars(term_t t, char **s, unsigned int flags) WUNUSED;
// WARNING - unsigned int maps to what?
[<DllImport(SwiDLL, EntryPoint="PL_get_chars", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_get_chars(Term_T t, StringPtr value, uint32 flags);


// PL_EXPORT(int)		PL_get_integer(term_t t, int *i) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_get_integer", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_get_integer(Term_T t, [<Out>] IntPtr i);



// PL_EXPORT(int)		PL_get_integer(term_t t, int *i) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_get_integer", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_get_integerZ(Term_T t, int *i);


// PL_EXPORT(int)		PL_get_long(term_t t, long *i) WUNUSED;
// WARNING - long * TODO
[<DllImport(SwiDLL, EntryPoint="PL_get_long", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_get_long(Term_T t, IntPtr i);


// PL_EXPORT(int)		PL_get_float(term_t t, double *f) WUNUSED;
// WARNING - double * TODO
[<DllImport(SwiDLL, EntryPoint="PL_get_float", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_get_float(Term_T t, [<Out>] double *f);

// ...

// Verify types
// PL_EXPORT(int)		PL_term_type(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_term_type", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_term_type(Term_T t);

// PL_EXPORT(int)		PL_is_variable(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_variable", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_variable(Term_T t);

//PL_EXPORT(int)		PL_is_ground(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_ground", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_ground(Term_T t);

//PL_EXPORT(int)		PL_is_atom(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_atom", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_atom(Term_T t);

//PL_EXPORT(int)		PL_is_integer(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_integer", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_integer(Term_T t);

//PL_EXPORT(int)		PL_is_string(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_string", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_string(Term_T t);

//PL_EXPORT(int)		PL_is_float(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_float", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_float(Term_T t);

//PL_EXPORT(int)		PL_is_rational(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_rational", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_rational(Term_T t);

//PL_EXPORT(int)		PL_is_compound(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_compound", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_compound(Term_T t);

//PL_EXPORT(int)		PL_is_callable(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_callable", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_callable(Term_T t);

//PL_EXPORT(int)		PL_is_functor(term_t t, functor_t f);
[<DllImport(SwiDLL, EntryPoint="PL_is_functor", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_functor(Term_T t, Functor_T f);

//PL_EXPORT(int)		PL_is_list(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_list", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_list(Term_T t);

//PL_EXPORT(int)		PL_is_pair(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_pair", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_pair(Term_T t);

//PL_EXPORT(int)		PL_is_atomic(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_atomic", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_atomic(Term_T t);

//PL_EXPORT(int)		PL_is_number(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_number", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_number(Term_T t);

//PL_EXPORT(int)		PL_is_acyclic(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_is_acyclic", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_is_acyclic(Term_T t);


// Assign to term-references 
//PL_EXPORT(int)		PL_put_variable(term_t t);
[<DllImport(SwiDLL, EntryPoint="PL_put_variable", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_variable(Term_T t);

//PL_EXPORT(int)		PL_put_atom(term_t t, atom_t a);
[<DllImport(SwiDLL, EntryPoint="PL_put_atom", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_atom(Term_T t, Atom_T a);

//PL_EXPORT(int)		PL_put_bool(term_t t, int val);
[<DllImport(SwiDLL, EntryPoint="PL_put_bool", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_bool(Term_T t, int a);

// PL_EXPORT(int)		PL_put_atom_chars(term_t t, const char *chars);
[<DllImport(SwiDLL, EntryPoint="PL_put_atom_chars", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_atom_chars(Term_T t, [<MarshalAs(UnmanagedType.LPStr)>] string chars);



// PL_EXPORT(int)		PL_put_integer(term_t t, long i) WUNUSED;
// WARNING - long TODO
[<DllImport(SwiDLL, EntryPoint="PL_put_integer", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_integer(Term_T t, int i);


// PL_EXPORT(int)		PL_put_float(term_t t, double f) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_put_float", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_float(Term_T t, double f);


// PL_EXPORT(int)		PL_put_functor(term_t t, functor_t functor) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_put_functor", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_functor(Term_T t, Functor_T functor);

// PL_EXPORT(int)		PL_put_list(term_t l) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_put_list", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_list(Term_T l);

// PL_EXPORT(int)		PL_put_nil(term_t l);
[<DllImport(SwiDLL, EntryPoint="PL_put_nil", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_nil(Term_T l);

// PL_EXPORT(int)		PL_put_term(term_t t1, term_t t2);
[<DllImport(SwiDLL, EntryPoint="PL_put_term", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_term(Term_T t1, Term_T t2);

// ** construct a functor or list-cell **

// PL_EXPORT(int)		PL_cons_functor(term_t h, functor_t f, ...) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_cons_functor", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_cons_functor1(Term_T h, Functor_T fd, Term_T a0);

[<DllImport(SwiDLL, EntryPoint="PL_cons_functor", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_cons_functor2(Term_T h, Functor_T fd, Term_T a0, Term_T a1);

// PL_EXPORT(int)		PL_cons_functor_v(term_t h, functor_t fd, term_t a0) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_cons_functor_v", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_cons_functor_v(Term_T h, Functor_T fd, Term_T a0);

// PL_EXPORT(int)		PL_cons_list(term_t l, term_t h, term_t t) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_cons_list", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_cons_list(Term_T l, Term_T h, Term_T t);

// ** Unify term-references **
// PL_EXPORT(int)		PL_unify(term_t t1, term_t t2) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_unify", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_unify(Term_T t1, Term_T t2);

// PL_EXPORT(int)		PL_unify_atom(term_t t, atom_t a) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_unify_atom", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_unify_atom(Term_T t, Atom_T a);


// PL_EXPORT(int)		PL_unify_bool(term_t t, int n) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_unify_bool", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_unify_bool(Term_T t, int n);

// PL_EXPORT(int)		PL_unify_integer(term_t t, intptr_t n) WUNUSED;
[<DllImport(SwiDLL, EntryPoint="PL_unify_integer", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_unify_integer(Term_T t, int n);


// *******************************
// *	       LISTS		*
// *******************************

// PL_EXPORT(int)		PL_skip_list(term_t list, term_t tail, size_t *len);
[<DllImport(SwiDLL, EntryPoint="PL_skip_list", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_skip_list(Term_T list, Term_T tail, SizeTPtr len);


// ...

// PL_EXPORT(int)  PL_put_term_from_chars(term_t t, int flags, size_t len, const char *s);
[<DllImport(SwiDLL, EntryPoint="PL_put_term_from_chars", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_put_term_from_chars(Term_T t, int flags, SizeT len, string s);


// PL_EXPORT(int)	PL_chars_to_term(const char *chars, term_t term);
[<DllImport(SwiDLL, EntryPoint="PL_chars_to_term", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern int PL_chars_to_term(string chars, Term_T term);

// ********************************
// *      RECORDED DATABASE       *
// ********************************


// PL_EXPORT(record_t)	PL_record(term_t term);
[<DllImport(SwiDLL, EntryPoint="PL_record", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)>]
extern Record_T PL_record(Term_T term);

// ********************************
// *      EMBEDDING               *
// ********************************

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