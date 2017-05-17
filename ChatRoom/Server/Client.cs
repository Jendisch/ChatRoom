using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Client
    {
        NetworkStream stream;
        TcpClient client;
        private string userId;
        private bool connected;

        public string UserId
        {
            get
            {
                return userId;
            }
            set
            {
                userId = value;
            }
        }

        public bool Connected
        {
            get
            {
                return connected;
            }
            set
            {
                connected = value;
            }
        }

        public Client(NetworkStream Stream, TcpClient Client)
        {
            stream = Stream;
            client = Client;
            UserId = "Default User";
            connected = true;
        }

        public void Send(string Message)
        {
            byte[] message = Encoding.ASCII.GetBytes(Message);
            stream.Write(message, 0, message.Count());
        }

        public void Recieve()
        {
            byte[] recievedMessage = new byte[256];
            stream.Read(recievedMessage, 0, recievedMessage.Length);
            string recievedMessageString = Encoding.ASCII.GetString(recievedMessage);
            Message message = new Message(null, recievedMessageString);
            Server.messageQueue.Enqueue(message);
            Console.WriteLine(recievedMessageString);
        }

        public void SetUserName()
        {
            byte[] recievedID = new byte[256];
            stream.Read(recievedID, 0, recievedID.Length);
            string recievedMessageString = Encoding.ASCII.GetString(recievedID).Trim('\0');
            userId = recievedMessageString;
            Console.WriteLine($"{userId} has entered the chat!");
        }


    }
}
