//Журнал строковых сообщений, асинхронно буферизирует добавляемые сообщения по истечении времени, либо заполнение буфера до 5 сообщений

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SPP_LR4_Zad6
{
    class Program
    {
        static void Main(string[] args)
        {
            LogBuffer logBuffer = new LogBuffer(@"C:\Users\Lenovo\Desktop\qwe.txt");
            for (int i = 0; i < 4; i++)
                logBuffer.Add(i.ToString());
            Console.ReadKey();
        }
    }

    class LogBuffer
    {
        private string path;
        private Thread thread;
        private ManualResetEvent addEvent;
        private Queue<string> buffer = new Queue<string>();
        private int ms = 20000;

        public LogBuffer(string path)
        {
            this.path = path;
            addEvent = new ManualResetEvent(false);
            thread = new Thread(WriteBufferAsync) { IsBackground=true };
            thread.Start();
            TimerAdd(ms);
        }

        public LogBuffer(string path, int loadTime)
        {
            this.path = path;
            ms = loadTime;
            addEvent = new ManualResetEvent(false);
            thread = new Thread(WriteBufferAsync) { IsBackground = true };
            thread.Start();
            TimerAdd(ms);
        }

        public void Add(string item)
        {
            buffer.Enqueue(item);
            if(buffer.Count>=5)
            addEvent.Set();
        }

        private void TimerAdd(int ms)
        {
            TimerCallback tm = new TimerCallback(SetEvent);
            Timer timer = new Timer(tm, new object(), ms, ms);
        }

        private void SetEvent(object state)
        {
            addEvent.Set();
            Console.WriteLine("SetEvent");
        }

        private async void WriteBufferAsync()
        {
            string s;

            while (true)
            {
                addEvent.WaitOne();
                Console.WriteLine("Write work");
                while (buffer.TryDequeue(out s))
                {
                    using (StreamWriter writer = new StreamWriter(path, true))
                    {
                        await writer.WriteLineAsync(s);
                    }
                }
                addEvent.Reset();
            }
        }
    }
}
