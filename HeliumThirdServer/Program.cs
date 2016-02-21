using System.Diagnostics;

namespace HeliumThirdServer
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(System.Console.Out));

            HeliumThird.Connections.NetworkConnection conn = new HeliumThird.Connections.NetworkConnection(8945);
            HeliumThird.Game game = new HeliumThird.Game(conn);

            Stopwatch timer = new Stopwatch();
            long lastTime = timer.ElapsedMilliseconds;
            timer.Start();
            
            while (true)
            {
                long now = timer.ElapsedMilliseconds;
                game.Update((now - lastTime) / 1000.0);
                lastTime = now;

                System.Threading.Thread.Sleep(20);

                if (System.Console.KeyAvailable)
                    break;
            }

            game.Shutdown();
        }
    }
}
