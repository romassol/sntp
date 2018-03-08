using System;
using System.Collections;

namespace Sntp
{
    class Program
    {
        public static void Main(string[] args)
        {
            var server = new Server();
            while (true)
            {
                server.Start();
            }
            //server.Start();
        }
    }
}
