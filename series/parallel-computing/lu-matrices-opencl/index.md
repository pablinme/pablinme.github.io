# LU Matrix Decomposition, OpenCL
The program decompose the original matrix into LU matrices, using **OpenCL** and validating the results in the main function.

### Kernel
The kernel dispatches values for matrix decomposition given the indices *i*,  *j* and the position by using the following macro:

`INDX_POS(i,j) ((WIDTH * i) + (j))`

```cpp
#define WIDTH 2
#define NUM_VALUES 4
#define INDX_POS(i,j) ((WIDTH * i) + (j))
 
kernel void ludecomposition(global float* input, global float* output)
{
    size_t i = get_global_id(0);
    size_t j = get_global_id(1);
}
```

### Main
The program begins by making all matrices' values *zero* and so to discard any **garbage values** that may end up in the matrix as the whole contents of memory goes to the kernel as a single block.

```cpp
#include <chrono>
#include <stdio.h>
#include <stdlib.h>
#include <OpenCL/opencl.h>
#include "mykernel.cl.h"

#define WIDTH 2
#define NUM_VALUES 4
#define INDX_POS(i,j) ((WIDTH * i) + (j))

void print(char* n, float* m)
{
    fprintf(stdout, "\n%s\n", n);

    for(int i = 0; i < NUM_VALUES; i++)
    {
        fprintf(stdout, "%.3f ", m[i]);
        if (i % (WIDTH) == WIDTH - 1) { fprintf(stdout, "\n"); }
    }
}
 
int validate(float* input, float* output)
{
    for (int i = 0; i < WIDTH; i++) {
        if ( input[i] != output[i] ) {
            fprintf(stdout, "Error: Element %d did not match expected output.\n", i);
            fflush(stdout);
            return 0;
        }
    }
    return 1;
}
 
void createL(float* output, float* l)
{
    for(int i = 0; i < WIDTH; i++)
    {
        for(int j = 0; j < WIDTH; j++)
        {
            if(i == j)
            {
                l[INDX_POS(i,j)] = 1;
            }
            else if (i < j)
            {
                l[INDX_POS(i,j)] = 0;
            }
            else if(i > j)
            {
                l[INDX_POS(i,j)] = output[INDX_POS(i,j)];
            }
        }
    }
}
 
void createU(float* output, float* u)
{
    for(int i = 0; i < WIDTH; i++)
    {
        for(int j = 0; j < WIDTH; j++)
        {
            if(j >= i)
            {
                u[INDX_POS(i,j)] = output[INDX_POS(i,j)];
            }
        }
    }
}

int main (int argc, const char * argv[])
{
    char name[128];
 
    dispatch_queue_t queue = gcl_create_dispatch_queue(CL_DEVICE_TYPE_GPU, NULL);

    if (queue == NULL)
    {
        queue = gcl_create_dispatch_queue(CL_DEVICE_TYPE_CPU, NULL);
    }

    cl_device_id gpu = gcl_get_device_id_with_dispatch_queue(queue);
    clGetDeviceInfo(gpu, CL_DEVICE_NAME, 128, name, NULL);
    fprintf(stdout, "Created a dispatch queue using the %s\n", name);
 
    float* l = (float*) malloc(sizeof(float) * NUM_VALUES);
    float* u = (float*) malloc(sizeof(float) * NUM_VALUES);
    float* test_in = (float*) malloc(sizeof(cl_float) * NUM_VALUES);
    float* test_out = (float*) malloc(sizeof(cl_float) * NUM_VALUES);
    
    for (int i = 0; i < NUM_VALUES; i++)
    {
        l[i] = 0;
        u[i] = 0;
        test_out[i] = 0;
        test_in[i] = i;
    }
    
    // i: [index / NUM_VALUES/2] -- j: [index % NUM_VALUES/2]
    //   A         L       U
    // 4   3    1     0   4   3
    // 6   3    1.5   1   0   -1.5
    
    //   A         L       U
    // 3   1    1     0   3   1
    // 4   2    1.3   1   0   0.6
    
    test_in[0] = 4; // 0 0
    test_in[1] = 3; // 0 1
    test_in[2] = 6; // 1 0
    test_in[3] = 3; // 1 1
    
    //     A            L            U
    // 1   2   3    1   0   0    1   2   3
    // 4   5   6    4   1   0    0  -3  -6
    // 7   8   9    7   2   1    0   0   0
    
    /*
    test_in[0] = 80.428;
    test_in[1] = -12.818;
    test_in[2] = -81.284;
    test_in[3] = -95.437;
    test_in[4] = -79.099;
    test_in[5] = 14.754;
    test_in[6] = 80.749;
    test_in[7] = 46.408;
    test_in[8] = -28.549;
    test_in[9] = 76.515;
    test_in[10] = -14.687;
    test_in[11] = -75.622;
    test_in[12] = -88.490;
    test_in[13] = -82.585;
    test_in[14] = 85.373;
    test_in[15] = -61.574;
    */
    
    print("INPUT", test_in);
 
    // Kernel space
    void* mem_in  = gcl_malloc(sizeof(cl_float) * NUM_VALUES, test_in, CL_MEM_READ_ONLY | CL_MEM_COPY_HOST_PTR);
    void* mem_out = gcl_malloc(sizeof(cl_float) * NUM_VALUES, NULL, CL_MEM_WRITE_ONLY);
 
    dispatch_sync(queue, ^{
        size_t wgs;
        gcl_get_kernel_block_workgroup_info(ludecomposition_kernel, CL_KERNEL_WORK_GROUP_SIZE, sizeof(wgs), &wgs, NULL);
    
        cl_ndrange range = {
            1,                     // The number of dimensions to use.
            {0, 0, 0},             // The offset in each dimension.
            {NUM_VALUES, 0, 0},    // The global range —how many items in each dimension
            {wgs, 0, 0}            // The local size of each workgroup.
        };
        
        auto t1 = std::chrono::high_resolution_clock::now();

        ludecomposition_kernel(&range,(cl_float*)mem_in, (cl_float*)mem_out);

        auto t2 = std::chrono::high_resolution_clock::now();
           
        auto duration = std::chrono::duration_cast<std::chrono::milliseconds>( t2 - t1 ).count();

        printf("%lld milliseconds\n", duration);
           
        gcl_memcpy(test_out, mem_out, sizeof(cl_float) * NUM_VALUES);
    });
   
    createL(test_in, l);
    createU(test_in, u);
    
    print("L", l);
    print("U", u);
    
    double sum = 0;
    for(int i = 0; i < WIDTH; i++)
    {
        for(int j = 0; j < WIDTH; j++)
        {
            for(int k = 0; k < WIDTH; k++)
            {
                //test_out[i][j] += l[i][k] * u[k][j];
                sum = sum + l[INDX_POS(i,k)] * u[INDX_POS(k,j)];
            }
            test_out[INDX_POS(i,j)] = sum;
            sum = 0;
        }
    }
    
    print("OUTPUT", test_out);
    
    if ( validate(test_in, test_out))
    {
        fprintf(stdout, "\nAll values were properly calculated.\n");
    }
    
    // Don't forget to free up the CL device's memory when you're done.
    gcl_free(mem_in);
 
    // And the same goes for system memory, as usual.
    free(test_in);
    free(l);
    free(u);
    free(test_out);
 
    // Finally, release your queue just as you would any GCD queue.
    dispatch_release(queue);
    
    return 0;
}
```
