---
layout: page
title: Matrices
permalink: /series/multithreading/matrices/
---
>From September 16, 2019

## Matrix Multiplication
Given the size of the matrices, the program generates two matrices that are filled with random integers using a couple of threads for each matrix, after that the matrices are multiplied in parallel by using multiple threads.

```csharp
using System;
using System.Threading;
using System.Collections.Generic;

namespace M1
{
    public class Program3
    {
         private static int n = 50;
         private static Random rand = new Random();
         private static int[,] a;
         private static int[,] b;
         private static int[,] result;

        [MTAThread]
        public static void Main()
        {
            do
            {
                Console.WriteLine("Enter the size of the matrix (>= 50)");

                if (!Int32.TryParse(Console.ReadLine(), out n))
                {
                    n = 0;
                }
            } while (n < 49);

            a = new int[n, n];

            Thread t1 = new Thread(() =>
            {
                for (int i = 0; i < n / 2; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        a[i, j] = rand.Next();
                        Thread.Yield();
                    }
                }
            });

            t1.Start();

            Thread t2 = new Thread(() =>
            {
                for (int i = n / 2; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        a[i, j] = rand.Next();
                        Thread.Yield();
                    }
                }
            });

            t2.Start();

            b = new int[n, n];

            Thread t3 = new Thread(() =>
            {
                for (int i = 0; i < n / 2; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        b[i, j] = rand.Next();
                        Thread.Yield();
                    }
                }
            });

            t3.Start();

            Thread t4 = new Thread(() =>
            {
                for (int i = n / 2; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        b[i, j] = rand.Next();
                        Thread.Yield();
                    }
                }
            });

            t4.Start();

            t1.Join();
            Console.WriteLine($"T1 finished!");
            t2.Join();
            Console.WriteLine($"T2 finished!");
            t3.Join();
            Console.WriteLine($"T3 finished!");
            t4.Join();

            Console.WriteLine($"T4 finished!");
            Console.WriteLine($"A {a.Length} elements");
            Console.WriteLine($"B {b.Length} elements");

            /*
            a[0, 0] = 10;
            a[0, 1] = 20;
            a[0, 2] = 10;

            a[1, 0] = 4;
            a[1, 1] = 5;
            a[1, 2] = 6;

            a[2, 0] = 2;
            a[2, 1] = 3;
            a[2, 2] = 5;

            b[0, 0] = 3;
            b[0, 1] = 2;
            b[0, 2] = 4;

            b[1, 0] = 3;
            b[1, 1] = 3;
            b[1, 2] = 9;

            b[2, 0] = 4;
            b[2, 1] = 4;
            b[2, 2] = 2;
            */


            // 130 120 240
            //  51  47  73
            //  35  33  45

            result = new int[n, n];
            List<Thread> threads = new List<Thread>();

            for (int i = 0; i < n * n; i++)
            {
                int temp = i;

                Thread thread = new Thread(() =>
                {
                    int i = temp / n;
                    int j = temp % n;

                    int[] x = GetRow(a, i);
                    int[] y = GetColumn(b, j);

                    for (int k = 0; k < x.Length; k++)
                    {
                        result[i, j] += x[k] * y[k];
                    }

                    //Console.WriteLine("Element [{0}, {1}]", i, j);
                });

                thread.Start();
                threads.Add(thread);
            }

            foreach (Thread t in threads)
            {
                t.Join();
            }

            Console.WriteLine("Matrix multiplication complete!");
        }

        static int[] GetColumn(int[,] arr, int i)
        {
            int[] res = new int[n];
            
            for (int j = 0; j < n; j++)
            {
                res[j] = arr[j, i];
            }
            return res;
        }

        static int[] GetRow(int[,] arr, int i)
        {
            int[] res = new int[n];

            for (int j = 0; j < n; j++)
            {
                res[j] = arr[i, j];
            }
            return res;
        }
    }
}
```
