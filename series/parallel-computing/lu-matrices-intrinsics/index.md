# LU  Matrix Decomposition, Intrinsics
In this case we go from the original matrix to its LU form using  [intrinsics](https://software.intel.com/sites/landingpage/IntrinsicsGuide/#!=undefined&techs=MMX).

In the code you will find *commented* the equivalent line to the intrinsics one. For example

```cpp
//l[INDX_POS(i,j)] = 1;
__m128 result = _mm_set1_ps(1.0);
_mm_storeu_ps(&l[INDX_POS(i,j)], result);
``` 
---

`__m128 _mm_set1_ps(float a)` propagate the value of **a** on *assignment* in the following way:

```cpp
FOR j := 0 to 3
        i := j * 32
        dst[i + 31: i] := a[31:0]
ENDFOR
```
---

finally **result = 1.0**

`_mm_storeu_ps(&l[INDX_POS(i,j)], result)`

in the final step the contents of the memory address `&l[INDX_POS(i,j)` are passed and stored.

The program decompose the original matrix into LU matrices and validates the results.

The final result

```cpp
#include <chrono>
#include <stdio.h>
#include <stdlib.h>
#include <xmmintrin.h>
#include "omp.h"
 
#define WIDTH 4
#define NUM_VALUES 16
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
    for (int i = 0; i < WIDTH; i++)
    {
        if ( input[i] != output[i] )
        {
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
                //l[INDX_POS(i,j)] = 1;
                __m128 result = _mm_set1_ps(1.0);
                _mm_storeu_ps(&l[INDX_POS(i,j)], result);
            }
            else if (i < j)
            {
                //l[INDX_POS(i,j)] = 0;
                __m128 result = _mm_setzero_ps();
                _mm_storeu_ps(&l[INDX_POS(i,j)], result);
            }
            else if(i > j)
            {
                //l[INDX_POS(i,j)] = output[INDX_POS(i,j)];
                __m128 result = _mm_loadu_ps(&output[INDX_POS(i,j)]);
                _mm_storeu_ps(&l[INDX_POS(i,j)], result);
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
                //u[INDX_POS(i,j)] = output[INDX_POS(i,j)];
                __m128 result = _mm_loadu_ps(&output[INDX_POS(i,j)]);
                _mm_storeu_ps(&u[INDX_POS(i,j)], result);
            }
        }
    }
}
 
void LUdecomposition(float* input, float* l, float* u)
{
    for(int i = 0; i < NUM_VALUES - 1; i++)
    {
        int row = 0;
        
//        #pragma omp parallel for private(row) shared(input)
        //{
            for(row = i + 1; row < NUM_VALUES; row++)
            {
                __m128 in_rowi_value = _mm_loadu_ps(&input[INDX_POS(row,i)]);
                __m128 in_ii_value = _mm_loadu_ps(&input[INDX_POS(i,i)]);
                __m128 factor = _mm_div_ps(in_rowi_value, in_ii_value);
                                
                //float factor = input[INDX_POS(row,i)] / input[INDX_POS(i,i)];
                
                for(int col = i + 1; col < NUM_VALUES; col++)
                {
                    __m128 in_rowcol_value = _mm_loadu_ps(&input[INDX_POS(row,col)]);
                    __m128 in_icol_value = _mm_loadu_ps(&input[INDX_POS(i,col)]);
                    __m128 mult_value = _mm_mul_ps(factor, in_icol_value);
                    __m128 result = _mm_sub_ps(in_rowcol_value, mult_value);

                    _mm_storeu_ps(&input[INDX_POS(row,col)], result);
                    //input[INDX_POS(row,col)] = input[INDX_POS(row,col)] - factor * input[INDX_POS(i,col)];
                }
                
                //input[INDX_POS(row,i)] = factor;
                _mm_storeu_ps(&input[INDX_POS(row,i)], factor);
            }
        //}
    }
    
    //#pragma omp parallel
    //{
        createL(input, l);
        createU(input, u);
    //}
}
 
int main(int argc, const char * argv[])
{
    float* test_in = (float*) malloc(sizeof(float) * NUM_VALUES);
    float* test_out = (float*) malloc(sizeof(float) * NUM_VALUES);
    float* l = (float*) malloc(sizeof(float) * NUM_VALUES);

    float* u = (float*) malloc(sizeof(float) * NUM_VALUES);
    
    for (int i = 0; i < NUM_VALUES; i++)
    {
        l[i] = 0;
        u[i] = 0;
        test_out[i] = 0;
        test_in[i] = i + 1;
    }
    
    // i: [index / NUM_VALUES/2] -- j: [index % NUM_VALUES/2]
    //   A         L       U
    // 4   3    1     0   4   3
    // 6   3    1.5   1   0   -1.5
    
    //   A         L       U
    // 3   1    1     0   3   1
    // 4   2    1.3   1   0   0.6
    
    //test_in[0] = 4; // 0 0
    //test_in[1] = 3; // 0 1
    //test_in[2] = 6; // 1 0
    //test_in[3] = 3; // 1 1
    
    //     A            L            U
    // 1   2   3    1   0   0    1   2   3
    // 4   5   6    4   1   0    0  -3  -6
    // 7   8   9    7   2   1    0   0   0
    
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
    
    print("INPUT", test_in);
    
    auto t1 = std::chrono::high_resolution_clock::now();

    LUdecomposition(test_in, l, u);

    auto t2 = std::chrono::high_resolution_clock::now();

    auto duration = std::chrono::duration_cast<std::chrono::milliseconds>( t2 - t1 ).count();
    
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

    fprintf(stdout, "\n%llu milliseconds\n", duration);
    
    return 0;
}
```
