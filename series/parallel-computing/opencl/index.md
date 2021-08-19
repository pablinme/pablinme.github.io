# OpenCL
A simple example of parallel execution using OpenCL, which is an API that allows the execution of *tasks* on the **CPU** and **GPU**.

In this example we cover the *square* function in the **kernel** while validating the results in the main function as well.

### Kernel
```cpp
// Kernel block
kernel void square(global float* input, global float* output)
{
    size_t i = get_global_id(0);
    output[i] = input[i] * input[i];
}
```

### Main
```cpp
#include <chrono>
#include <stdio.h>
#include <stdlib.h>
#include <OpenCL/opencl.h>
#include "mykernel.cl.h"

#define NUM_VALUES 2048000

static int validate(cl_float* input, cl_float* output)
{
    for (int i = 0; i < NUM_VALUES; i++)
    {
        if ( output[i] != (input[i] * input[i]) )
        {
            fprintf(stdout, "Error: Element %d did not match expected output.\n", i);
            
            fprintf(stdout, "       Got %1.4f, EXPECTED %1.4f\n", output[i], input[i] * input[i]);

            fflush(stdout);
            return 0;
        }
    }
    return 1;
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
 
    float* test_in = (float*) malloc(sizeof(cl_float) * NUM_VALUES);

    for (int i = 0; i < NUM_VALUES; i++)
    {
        test_in[i] = (cl_float) i;
    }

    float* test_out = (float*)malloc(sizeof(cl_float) * NUM_VALUES);
    
    // Kernel space
    void* mem_in  = gcl_malloc(sizeof(cl_float) * NUM_VALUES, test_in, CL_MEM_READ_ONLY | CL_MEM_COPY_HOST_PTR);
    void* mem_out = gcl_malloc(sizeof(cl_float) * NUM_VALUES, NULL, CL_MEM_WRITE_ONLY);

    dispatch_sync(queue, ^{
        size_t wgs;
        gcl_get_kernel_block_workgroup_info(square_kernel, CL_KERNEL_WORK_GROUP_SIZE, sizeof(wgs), &wgs, NULL);

        cl_ndrange range = {
            1,                     // The number of dimensions to use.
            {0, 0, 0},             // The offset in each dimension.
            {NUM_VALUES, 0, 0},    // The global range —how many items in each dimension
            {wgs, 0, 0}            // The local size of each workgroup.
        };

        auto t1 = std::chrono::high_resolution_clock::now();
        square_kernel(&range,(cl_float*)mem_in, (cl_float*)mem_out);
        auto t2 = std::chrono::high_resolution_clock::now();

        auto duration = std::chrono::duration_cast<std::chrono::milliseconds>( t2 - t1 ).count();
        printf("%lld milliseconds\n", duration);
        
        gcl_memcpy(test_out, mem_out, sizeof(cl_float) * NUM_VALUES);
    });
   
    // Check to see if the kernel did what it was supposed to:
    if ( validate(test_in, test_out))
    {
        fprintf(stdout, "All values were properly squared.\n");
    }
    
    // Don't forget to free up the CL device's memory when you're done.
    gcl_free(mem_in);
    gcl_free(mem_out);

    // And the same goes for system memory, as usual.
    free(test_in);
    free(test_out);
 
    // Finally, release your queue just as you would any GCD queue.
    dispatch_release(queue);
}
``` 
