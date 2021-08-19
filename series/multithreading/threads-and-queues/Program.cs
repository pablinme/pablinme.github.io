using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace M1
{
    public class ConsumerThread
    {
        public int ID { get; set; }
        public bool IS_BUSY { get; set; }
    }

    public class ProducerThread
    {
        public int ID { get; set; }
        public int QUEUE_SIZE { get; set; }
    }

    public class Program
    {
        private static ConcurrentQueue<int> queue;
        private static List<Thread> threads;
        private static Thread producerThread;

        private static Random rand = new Random();

        [MTAThread]
        public static void Main()
        {
            int consumers;
            int queueSize;
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
 
            do
            {
                Console.WriteLine("Enter the number of consumers (n > 0)");

                if (Int32.TryParse(Console.ReadLine(), out consumers) && consumers > 0)
                {
                    threads = new List<Thread>(consumers);

                    do
                    {
                        Console.WriteLine("Enter the size of the queue (n > 4)");

                        if (Int32.TryParse(Console.ReadLine(), out queueSize) && queueSize > 4)
                        {
                            queue = new ConcurrentQueue<int>();

                            for (int i = 0; i < queueSize; i++)
                            {
                                queue.Enqueue(rand.Next());
                            }

                            ProducerThread producerData = new ProducerThread
                            {
                                ID = 1,
                                QUEUE_SIZE = queueSize
                            };

                            producerThread = new Thread(new ParameterizedThreadStart(ProducerThreadProc));
                            producerThread.Start(producerData);

                            for (int i = 1; i <= consumers; i++)
                            {
                                ConsumerThread threadData = new ConsumerThread
                                {
                                    ID = i,
                                    IS_BUSY = false
                                };

                                Thread t = new Thread(new ParameterizedThreadStart(ConsumerThreadProc));
                                t.Start(threadData);
                                threads.Add(t);
                            }

                            while (true)
                            {
                                Console.WriteLine("Current Queue size: {0}", queue.Count);

                                Console.WriteLine("State of Consumers");

                                for (int t = 1; t <= threads.Count; t++)
                                {
                                    Console.WriteLine("Consumer : {0} -> {1}", t, threads[t - 1].ThreadState);
                                }

                                Thread.Sleep(1000);
                            }
                        }
                        else
                        {
                            queueSize = 0;
                        }
                    } while (queueSize > 4);
                }
                else
                {
                    consumers = 0;
                }
            } while (consumers > 0);
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (e.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                producerThread.Interrupt();

                foreach (Thread current in threads)
                {
                    current.Interrupt();
                }
            }
        }

        public static void ProducerThreadProc(object data)
        {
            var currentData = (ProducerThread)data;
 
            try
            {
                while (true)
                {
                    if (queue.Count < currentData.QUEUE_SIZE)
                    {
                        queue.Enqueue(rand.Next());
                    }

                    Thread.Sleep(1000);
                    Thread.Yield();
                }
            }
            catch (ThreadInterruptedException) { return; }
        }

        public static void ConsumerThreadProc(object data) 
        {
            try
            {
                while (true)
                {
                    if (queue.Count > 0)
                    {
                        if (queue.TryDequeue(out int m))
                        {
                            Thread.Sleep(2000);
                        }
                    }
                    Thread.Yield();
                }
            }
            catch (ThreadInterruptedException) { return; }
        }
    }
}
