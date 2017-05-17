using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Server
    {

        public static ConcurrentQueue<Message> messageQueue = new ConcurrentQueue<Message>();
        private static Dictionary<string, Client> connectedClientsInChat;
        public static Client client;
        TcpListener server;
        bool connected;

        public Server()
        {
            connectedClientsInChat = new Dictionary<string, Client>();
            server = new TcpListener(IPAddress.Parse("127.0.0.1"), 9999);
            server.Start();
            Console.WriteLine("Attemping to make a connection...");
            CreateResponseReceive(connectedClientsInChat);
        }

        public void Run()
        {
            connected = true;
            while (connected == true)
            {
                try
                {
                    TcpClient clientSocket = default(TcpClient);
                    clientSocket = server.AcceptTcpClient();
                    Thread clientThread = new Thread(() => AcceptClient(clientSocket));
                    clientThread.Start();
                }
                catch
                {
                    Console.WriteLine("Something went wrong.");
                }
            }

            connected = false;
        }

        private void AcceptClient(TcpClient clientSocket)
        {
            Console.WriteLine("A new user is now connected");
            NetworkStream stream = clientSocket.GetStream();
            client = new Client(stream, clientSocket);
            Task userNameSet = Task.Run(() => client.SetUserName());
            userNameSet.Wait();
            connectedClientsInChat.Add(client.UserId, client);
            while (connected == true)
            {
                client.Recieve();
            }
        }



        public void CreateResponseReceive(Dictionary<string, Client> connectedClientsInChat)
        {
            Thread respond = new Thread(() => Respond(connectedClientsInChat));
            respond.Start();
        }


        private void Respond(Dictionary<string, Client> connectedClientsInChat)
        {
            while (connected == true)
            {
                Message message = default(Message);
                if (messageQueue.TryDequeue(out message))
                {
                    foreach (KeyValuePair<string, Client> keyValue in connectedClientsInChat)
                    {
                        if (keyValue.Key != message.UserId)
                        {
                            keyValue.Value.Send($"[{DateTime.Now.ToString("h:mm:ss tt")}] {message.sender.UserId}: {message.Body}");
                        }
                    }
                }
            }
        }

    }
}










