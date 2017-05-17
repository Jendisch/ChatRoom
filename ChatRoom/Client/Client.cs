using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Client
    {
        TcpClient clientSocket;
        NetworkStream stream;
        bool connected;

        public Client(string IP, int port)
        {
            connected = true;
            using (clientSocket = new TcpClient())
            {
                Console.WriteLine("Connecting.....");
                clientSocket.Connect(IPAddress.Parse(IP), port);
                Console.WriteLine("Connected");
                string userName = AskForUserName();
                stream = clientSocket.GetStream();
                try
                {
                    byte[] message = Encoding.ASCII.GetBytes(userName);
                    stream.Write(message, 0, message.Count());
                }
                catch
                {
                    Console.WriteLine("Something went wrong.");
                }
                //Place loops around here somewhere
                Task send = Task.Run(() => Send());
                Task receive = Task.Run(() => Receive());
                receive.Wait();
                clientSocket.Close();
                Console.WriteLine("Disconnected.");
                Console.ReadKey();
                connected = false;
            }
        }

        public void Send()
        {
            while (connected == true)
            {
                try
                {
                    string messageString = UI.GetInput();
                    byte[] message = Encoding.ASCII.GetBytes(messageString);
                    stream.Write(message, 0, message.Count());
                }
                catch
                {
                    Console.WriteLine("Something went wrong.");
                }
            }
        }

        public void Receive()
        {
            while (connected == true)
            {
                try
                {
                    byte[] receivedMessages = new byte[256];
                    stream.Read(receivedMessages, 0, receivedMessages.Length);
                    UI.DisplayMessage(Encoding.ASCII.GetString(receivedMessages));
                }
                catch
                {
                    Console.WriteLine("Something went wrong.");
                }
            }
        }
        
        private string AskForUserName()
        {
            Console.WriteLine("Please enter your chat room username.");
            string user = UI.GetInput();
            return user;
        }
    }
}

