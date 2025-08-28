
using System;

namespace FFRogue
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var game = new Game(60, 25);
            game.ShowTitle();
            string name = game.AskName();
            var job = game.AskJob();
            game.InitializePlayer(name, job);
            game.Run();
        }
    }
}
