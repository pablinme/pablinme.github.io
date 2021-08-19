# OpenMP
> From October 12, 2019

OpenMP is an API that allows to add concurrency to our programs wether they are written using C or C++, is composed by a set of compiler's directives for memory distributed systems.

### Fork-Join
This model divides a heavy task in **k** threads by making subtasks and collecting results at the end.

In order to mark a *section of the code* to be executed in parallel the following directive is used:
```cpp
#pragma omp parallel
    {
    }
```
---

Now with the example of getting the *square* off given values.

```cpp
#include <chrono>
#include <stdio.h>
#include <stdlib.h>
#include "omp.h"
 
#define NUM_VALUES 2048000
static int validate(float* input, float* output)
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
 
void square(float* input, float* output)
{
    for (int i = 0; i < NUM_VALUES; i++)
    {
        output[i] = input[i] * input[i];
    }
}
 
int main(int argc, const char * argv[])
{
    float* test_in = (float*) malloc(sizeof(float) * NUM_VALUES);
    float* test_out = (float*) malloc(sizeof(float) * NUM_VALUES);
    
    for (int i = 0; i < NUM_VALUES; i++)
    {
        test_in[i] = i;
    }
    auto t1 = std::chrono::high_resolution_clock::now();

    #pragma omp parallel
    {
        square(test_in, test_out);
    }

    auto t2 = std::chrono::high_resolution_clock::now();
    auto duration = std::chrono::duration_cast<std::chrono::milliseconds>( t2 - t1 ).count();

    std::cout << duration << " milliseconds\n";
    //std::cout << "Greetings from thread " << omp_get_thread_num() << std::endl;
    
    if ( validate(test_in, test_out)) {
        fprintf(stdout, "All values were properly squared.\n");
    }
    
    return 0;
}
```
