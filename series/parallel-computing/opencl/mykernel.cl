// Kernel block
kernel void square(global float* input, global float* output)
{
    size_t i = get_global_id(0);
    output[i] = input[i] * input[i];
}
