#define WIDTH 2
#define NUM_VALUES 4
#define INDX_POS(i,j) ((WIDTH * i) + (j))
 
kernel void ludecomposition(global float* input, global float* output)
{
    size_t i = get_global_id(0);
    size_t j = get_global_id(1);
}
