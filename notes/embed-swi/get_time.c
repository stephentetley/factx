/* compile with:
 * swipl-ld -o get_time get_time.c 
 */

#include<stdio.h>
#include<SWI-Prolog.h>


double get_time()
{ 
    fid_t fid = PL_open_foreign_frame();
    term_t goal = PL_new_term_ref();
    term_t a1 = PL_new_term_ref();
    functor_t s2 = PL_new_functor(PL_new_atom("get_time"), 1);
    double ptime;
    
    PL_cons_functor(goal, s2, a1);
    PL_call(goal, NULL); /* call it in current module */
    
    PL_get_float(a1, &ptime);
    PL_discard_foreign_frame(fid);
    
    return ptime;
}

int main(int argc, char **argv)
{ 
    double d;
    if ( !PL_initialise(argc, argv) )
        PL_halt(1);
    d = get_time();
    printf("get_time:%f", d);
    return 0;
}