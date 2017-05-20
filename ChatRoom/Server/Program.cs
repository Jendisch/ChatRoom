using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            new Server(new TxtLog("chatRoomLog")).Run();
            Console.ReadLine();
        }
    }
}



//one loop thats always sending messages
//one loop thats always accepting new clients