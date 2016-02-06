namespace HeliumThirdServer
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(System.Console.Out));

            HeliumThird.Connections.NetworkConnection conn = new HeliumThird.Connections.NetworkConnection(8945);
            HeliumThird.Game game = new HeliumThird.Game(conn);
            
            while (true)
            {
                game.Update(123);
                System.Threading.Thread.Sleep(20);
                if (System.Console.KeyAvailable)
                    break;
            }

            game.Shutdown();
        }
    }
}
