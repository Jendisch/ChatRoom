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
            CreateResponse();
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
            Task userNameSet = Task.Run(() => CheckForDupilcateUserName(client, stream));
        }



        public void CreateResponse()
        {
            Thread respond = new Thread(() => Respond());
            respond.Start();
        }


        private void Respond()
        {
            while (connected == true)
            {
                Message message = default(Message);
                if (messageQueue.TryDequeue(out message))
                {
                    if (message.MessageBody.StartsWith("WHISPER")) //use whisper and then username of other client to send a personal message
                    {
                        string matchedUserId = message.MessageBody.Split()[1];
                        bool dictionaryContainsUserId = SearchForUser(matchedUserId);
                        if (dictionaryContainsUserId == true)
                        {
                            foreach (KeyValuePair<string, Client> privateSender in connectedClientsInChat)
                            {
                                if (privateSender.Key == matchedUserId)
                                {
                                    privateSender.Value.Send($"[{DateTime.Now.ToString("h:mm:ss tt")}] {message.Sender.UserId}: {message.MessageBody}");
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<string, Client> keyValue in connectedClientsInChat)
                        {
                            if (keyValue.Key != message.UserId)
                            {
                                keyValue.Value.Send($"[{DateTime.Now.ToString("h:mm:ss tt")}] {message.Sender.UserId}: {message.MessageBody}");
                            }
                        }
                    }
                }
            }
        }

        private async Task CheckForDupilcateUserName(Client client, NetworkStream stream)
        {
            client.SetUserName();
            if (!connectedClientsInChat.ContainsKey(client.UserId))
            {
                connectedClientsInChat.Add(client.UserId, client);
                ShowOnlineUsers(client);
                Thread receive = new Thread(() => client.Receive());
                receive.Start();
            }
            else
            {
                client.Send("This username already in use. Try to use another.\n");
                await CheckForDupilcateUserName(client, stream);
            }
        }

        private bool SearchForUser(string matchedUserId)
        {
            if (connectedClientsInChat.ContainsKey(matchedUserId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string GetMessage(NetworkStream stream)
        {
            byte[] recievedMessage = new byte[256];
            stream.Read(recievedMessage, 0, recievedMessage.Length);
            return Encoding.ASCII.GetString(recievedMessage).Trim(new char[] { '\0' }).Trim();
        }

        private void ShowOnlineUsers(Client client)
        {
            if (connectedClientsInChat.Count() > 1)
            {
                client.Send("Online Users:");
                foreach (KeyValuePair<string, Client> entry in connectedClientsInChat)
                {
                    if (entry.Key != client.UserId)
                    {
                        client.Send($"\n>>{entry.Key}");
                    }
                }
            }
            else
            {
                client.Send($"You're the only one here {client.UserId}, sorry!");
            }
        }



    }
}










