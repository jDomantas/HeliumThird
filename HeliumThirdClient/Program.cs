using System;

namespace HeliumThirdClient
{
#if WINDOWS || LINUX
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new GameHelium())
                game.Run();
        }
    }
#endif
}
