---
layout: page
title: Event Reset
permalink: /series/multithreading/event-reset/
---
> From September 02, 2019

## Signals
Given the number of threads -at least 3 of them- them have to start working until a stop signal is triggered. 

At first all threads start on a waiting state and work when signaled, this signal that indicates any thread to *work*  is triggered every second to a random thread.

{% highlight csharp linenos %}

using System;
using System.Threading;
using System.Collections.Generic;

namespace M1
{
    public class MyThread
    {
        public int ID { get; set; }
        public bool FLAG { get { return mRES.IsSet; } }
        public ManualResetEventSlim mRES { get; set; }
    }

    public class Program1
    {
        private static List<Thread> threads;
        private static List<ManualResetEventSlim> listMRES;

        [MTAThread]
        public static void Main()
        {
             int nThreads;
             Random rand = new Random();
             Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

             do
             {
                 Console.WriteLine("Enter the number of threads (n > 3)");

                 if (Int32.TryParse(Console.ReadLine(), out nThreads) && nThreads > 3)
                 {
                     threads = new List<Thread>(nThreads);
                     listMRES = new List<ManualResetEventSlim>(nThreads);

                     for (int i = 1; i <= nThreads; i++)
                     {
                         listMRES.Add(new ManualResetEventSlim(false)); //unsignaled initialized
                        
                         MyThread threadData = new MyThread
                         {
                             ID = i,
                             mRES = listMRES[i - 1]
                         };

                         Thread t = new Thread(new ParameterizedThreadStart(ThreadProc));
                         t.Start(threadData);
                         threads.Add(t);
                     }
                    
                     // release one thread each second until all threads have been released
                     while (true)
                     {
                         listMRES[rand.Next(0, nThreads)].Set();
                         Thread.Sleep(1000);
                     }
                 }
                 else
                 {
                     nThreads = 0;
                 }
            } while (nThreads > 3);
        }
     
        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (e.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                foreach (Thread current in threads)
                {
                    current.Interrupt();
                    current.Join();
                }
            }
        }

        public static void ThreadProc(object data)
        {
            var currentData = (MyThread) data;

            try
            {
                while (true)
                {
                    while (currentData.FLAG)
                    {
                        Console.WriteLine("Thread ID: {0}", currentData.ID);
                        Thread.Sleep(1000);
                    }
                    currentData.mRES.Wait();
                }
            }
            catch (ThreadInterruptedException) { return; }
        }
    }
}

{% endhighlight %}
