# Arrays and Threads
>From September 09, 2019

## Max and Min
To start working with arrays, we can do a simple task to find the indices of minimum and maximum elements in an array.

Given the size of the array, the program fills it with random numbers and creates **n** tasks according to the *argument* received by the function **startWithTasks**.
 
The actual search is multi-threaded and when done results are displayed with time execution.

```csharp
using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace M1
{
    public class Program2
    {    
        private static int n = 20000000;
        private static int[] a = new int[n];

        private static List < int > mins = new List < int > ();
        private static List < int > maxs = new List < int > ();

        private static List < Task > tasks = new List < Task > ();
        private static Stopwatch stopwatch = new Stopwatch();

        [MTAThread]
        public static void Main()
        {
            Random rand = new Random();

            /*
            do
            {
                Console.WriteLine("Enter the size of the array");

                if (!Int32.TryParse(Console.ReadLine(), out n))
                {
                    n = 0;
                }
            } while (n < 3);
            */

            Task t1 = new Task(() => {
                for (int i = 0; i < n; i++)
                {
                    a[i] = rand.Next();
                }
            });

            stopwatch.Reset();
            stopwatch.Start();

            t1.Start();
            t1.Wait();

            stopwatch.Stop();

            Console.WriteLine($"Array filled! in {stopwatch.ElapsedMilliseconds} ms");
 
            startWithTasks(2);
            startWithTasks(4);
            startWithTasks(8);
            startWithTasks(16);
 
            /*
            for (int i = 1; i <= 8; i++)
            {
                int temp = i;
                int val = i * n / 8;
                int fromVal = (i - 1) * n / 8;

                Console.WriteLine($"from: {fromVal} to: {val}");
                Task tMin = new Task(() =>
                {
                    int tmp = a[0];

                    for (int j = fromVal; j < val; j++)
                    {
                        if (tmp > a[j]) tmp = a[j];
                    }
                    mins.Add(tmp);
                });
 
                tMin.Start();
                Task tMax = new Task(() =>
                {
                    int tmp = a[0];

                    for (int k = fromVal; k < val; k++)
                    {
                        if (tmp < a[k]) tmp = a[k];
                    }

                    maxs.Add(tmp);
                });

                tMax.Start();
                tasks.Add(tMin);
                tasks.Add(tMax);
            }

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine($"Min: {mins.Min()} found!");
            Console.WriteLine($"Max: {maxs.Max()} found!");
            */
        }

        static void startWithTasks(int m)
        {
            stopwatch.Reset();
            stopwatch.Start();
 
            for (int i = 1; i <= m; i++)
            {
                int temp = i;
                int val = i * n / m;
                int fromVal = (i - 1) * n / m;

                Task tMin = new Task(() => {
                    int tmp = a[0];

                    for (int j = fromVal; j < val; j++)
                    {
                        if (tmp > a[j]) tmp = a[j];
                    }
                    mins.Add(tmp);
                });

                tMin.Start();

                Task tMax = new Task(() => {
                    int tmp = a[0];

                    for (int k = fromVal; k < val; k++)
                    {
                        if (tmp < a[k]) tmp = a[k];
                    }
                    maxs.Add(tmp);
                });

                tMax.Start();
                tasks.Add(tMin);
                tasks.Add(tMax);
            }

            Task.WaitAll(tasks.ToArray());
            stopwatch.Stop();

            Console.WriteLine($"Min: {mins.Min()} found!");
            Console.WriteLine($"Max: {maxs.Max()} found!");
            Console.WriteLine($"With {m} threads in {stopwatch.ElapsedMilliseconds} ms");

            mins.Clear();
            maxs.Clear();
            tasks.Clear();
        }
    }
}
```
