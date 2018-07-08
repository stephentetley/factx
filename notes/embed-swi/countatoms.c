/* compile with:
 * swipl-ld -o countatoms countatoms.c 
 */

#include<stdio.h>
#include<SWI-Prolog.h>


int count_atoms()
{ 
    fid_t fid = PL_open_foreign_frame();
    term_t goal = PL_new_term_ref();
    term_t a1 = PL_new_term_ref();
    term_t a2 = PL_new_term_ref();
    functor_t s2 = PL_new_functor(PL_new_atom("statistics"), 2);
    int atoms;
    
    PL_put_atom_chars(a1, "atoms");
    PL_cons_functor(goal, s2, a1, a2);
    PL_call(goal, NULL); /* call it in current module */
    
    PL_get_integer(a2, &atoms);
    PL_discard_foreign_frame(fid);
    
    return atoms;
}

int main(int argc, char **argv)
{ 
    int i;
    if ( !PL_initialise(argc, argv) )
        PL_halt(1);
    i = count_atoms();
    printf("count_atoms:%i", i);
    return 0;
}