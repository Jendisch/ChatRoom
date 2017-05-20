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
        public static Dictionary<string, Client> connectedClientsInChat;
        public static Client client;
        TcpListener server;
        bool connected;
        private TxtLog chatLog;
        public static Object dictionaryLock;

        public Server(TxtLog chatLog)
        {
            this.chatLog = chatLog;
            connectedClientsInChat = new Dictionary<string, Client>();
            server = new TcpListener(IPAddress.Parse("127.0.0.1"), 9999);
            server.Start();
            Console.WriteLine("Attemping to make a connection...");
            CreateResponse();
            dictionaryLock = new Object();
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
                    break;
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

        private void CreateResponse()
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
                        lock (dictionaryLock)
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
                                        chatLog.Log($"[{DateTime.Now.ToString("h:mm:ss tt")}] {message.Sender.UserId} 'WHISPER' to {matchedUserId} >> {message.MessageBody}");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        lock (dictionaryLock)
                        {
                            foreach (KeyValuePair<string, Client> keyValue in connectedClientsInChat)
                            {
                                if (keyValue.Key != message.UserId)
                                {
                                    keyValue.Value.Send($"[{DateTime.Now.ToString("h:mm:ss tt")}] {message.Sender.UserId}: {message.MessageBody}");
                                    chatLog.Log($"[{DateTime.Now.ToString("h:mm:ss tt")}] {message.Sender.UserId} >> {message.MessageBody}");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CheckForDupilcateUserName(Client client, NetworkStream stream)
        {
            client.SetUserName();
            lock (dictionaryLock)
            {
                if (!connectedClientsInChat.ContainsKey(client.UserId))
                {
                    connectedClientsInChat.Add(client.UserId, client);
                    ShowOnlineUsers(client);
                    chatLog.Log($"[{DateTime.Now.ToString("h:mm:ss tt")}] >> {client.UserId} connected to the chatroom");       //WORKING ON GETTING A LOG METHOD BY THE MESSAGES SOMEWHERE TO BE ABLE TO LOG ALL MESSAGES
                    Thread receive = new Thread(() => client.Receive(chatLog, connectedClientsInChat));
                    receive.Start();
                }
                else
                {
                    client.Send("This username already in use. Try to use another.\n");
                }
            }
        }

        private bool SearchForUser(string matchedUserId)
        {
            lock (dictionaryLock)
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
        }

        private string GetMessage(NetworkStream stream)
        {
            byte[] recievedMessage = new byte[256];
            stream.Read(recievedMessage, 0, recievedMessage.Length);
            return Encoding.ASCII.GetString(recievedMessage).Trim(new char[] { '\0' }).Trim();
        }

        private void ShowOnlineUsers(Client client)
        {
            lock (dictionaryLock)
            {
                if (connectedClientsInChat.Count() > 1)
                {
                    foreach (KeyValuePair<string, Client> keyValue in connectedClientsInChat)
                    {
                        if (keyValue.Key != client.UserId)
                        {
                            keyValue.Value.Send($"\n{client.UserId} just joined the chat!");
                        }
                    }
                    client.Send("Online Users:");
                    foreach (KeyValuePair<string, Client> online in connectedClientsInChat)
                    {
                        if (online.Key != client.UserId)
                        {
                            client.Send($"\n>>{online.Key}");
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
}










