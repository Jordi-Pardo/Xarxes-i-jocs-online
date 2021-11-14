using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using System;

public class Server : MonoBehaviour
{
    public class User
    {
        public bool firstConnection;
        public Socket socket;
        public string name;

    }

    [System.Serializable]
    public class Message
    {
        public int createProfile;
        public string message;
        public string userName;
        public List<string> userNamesList = new List<string>();
    }

    List<Action> callbBacksList;

    public TextFieldItem textItemPrefab;
    public Transform content;

    Socket socket;
    IPEndPoint ipep;
    EndPoint ipepRemote;

    Thread thread;

    public List<User> clients = new List<User>();
    public List<string> clientsNames = new List<string>();

    bool acceptingListenedConnections = true;

    Message recievedmessage;

    private List<User> disconnectedUsers = new List<User>();

    // Start is called before the first frame update
    void Start()
    {
        callbBacksList = new List<Action>();

        TcpSetup();


        AddCallbackMessage("Waiting for message...");

    }



    // Update is called once per frame
    void Update()
    {

        if (callbBacksList.Count > 0)
        {
            for (int i = 0; i < callbBacksList.Count; i++)
            {
                Action callback = callbBacksList[i];
                callbBacksList.RemoveAt(i);
                callback();
            }
        }


        socket.Listen(10);
        Debug.Log("Listening sockets...");

        UpdateServerSocket();

        UpdateClientsSockets();

    }

    private void UpdateClientsSockets()
    {
        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i].socket.Poll(1000, SelectMode.SelectRead))
            {
                Debug.Log("Client is readable");
                if( ReceiveTCPData(clients[i]) == 0)
                {
                    disconnectedUsers.Add(clients[i]);
                    clients.Remove(clients[i]);
                    return;
                }

            }
            else if (clients[i].socket.Poll(1000, SelectMode.SelectWrite))
            {
                Debug.Log("Client is writable");
            }
            else if (clients[i].socket.Poll(1000, SelectMode.SelectError))
            {
                Debug.Log("Client has an error");
            }
        }


        for (int i = 0; i < disconnectedUsers.Count; i++)
        {
            SendTCPData(new Message() { createProfile = -1, message = " Has desconnected.", userName = disconnectedUsers[i].name });
            AddCallbackMessage($"Client: {disconnectedUsers[i].name} has desconnected.");
        }
        if (disconnectedUsers.Count > 0)
        {   
           
            disconnectedUsers.Clear();
        }
    }

    private void UpdateServerSocket()
    {
        //Waiting for connect new user
        if (socket.Poll(1000, SelectMode.SelectRead))
        {
            //New user to connect
            Debug.Log("Socket is readable");
            NewUserConnected();

        }
        else if (socket.Poll(1000, SelectMode.SelectWrite))
        {
            Debug.Log("Socket is writable");
        }
        else if (socket.Poll(1000, SelectMode.SelectError))
        {
            Debug.Log("Socket has an error");
        }
    }

    private void TcpSetup()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ipep = new IPEndPoint(IPAddress.Any, 1818);
        ipepRemote = (EndPoint)ipep;
        try
        {
            socket.Bind(ipep);
            Debug.Log("Socket binded");
        }
        catch (System.Exception e)
        {
            Debug.Log("Error to bind");
        }
    }

    private void NewUserConnected()
    {
        try
        {
            Socket client = socket.Accept();
            clients.Add(new User() { name = "", socket = client, firstConnection = true});
            Debug.Log("Client Connected");

        }
        catch (System.Exception e)
        {
            Debug.Log("Connection failed...trying again");
        }
    }


    private void SendTCPData(Message message, Socket socket = null)
    {
        if (socket == null)
        {
            //broadcast sockets
            try
            {
                byte[] data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(message));
                for (int i = 0; i < clients.Count; i++)
                {
                    clients[i].socket.Send(data, SocketFlags.None);
                }
            }
            catch (SystemException e)
            {
                Debug.Log("Error sending data");
            }


        }
        else //Only to one socket
        {
            try
            {
                byte[] data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(message));
                for (int i = 0; i < clients.Count; i++)
                {
                    clients[i].socket.Send(data, SocketFlags.None);
                }

            }
            catch (SystemException e)
            {
                Debug.Log("Error sending data");
            }
        }

    }

    private int ReceiveTCPData(User client)
    {
        if (!client.socket.Connected)
            return 0;

        string message = string.Empty;

        try
        {
            byte[] data = new byte[256];
            int size = client.socket.Receive(data);
            if (size == 0)
            {
                return size;
            }


            message = Encoding.ASCII.GetString(data, 0, size);

            Message messageReceived = new Message();
            JsonUtility.FromJsonOverwrite(message, messageReceived);

            if (client.firstConnection == true)
            {
                client.firstConnection = false;
                
                
                client.name = messageReceived.message;
                clientsNames.Add(client.name);
                Message messageToSend = new Message()
                {
                    createProfile = 1,
                    message = "Welcome to the server: " + client.name,
                    userName = client.name,
                    userNamesList = clientsNames
                };
                SendTCPData(messageToSend,client.socket);
                
                AddCallbackMessage("User: " + client.name + " has connected");
                return 1;
            }



            recievedmessage = new Message()
            {
                createProfile = -1,
                message = messageReceived.message,
                userName = client.name,
                userNamesList = clientsNames
            };

   
           SendTCPData(recievedmessage);
         

            AddCallbackMessage(messageReceived.message);

        }
        catch (SystemException e)
        {
            Debug.Log("Error receiving data..Desconnecting");
            return 0;
        }
        return 1;
    }

    //private void SentBroadcastMessage()
    //{
    //    for (int i = 0; i < clients.Count; i++)
    //    {
       
    //        SendTCPData(recievedmessage);
    //    }
    //}

    void AddCallbackMessage(string message)
    {
        Action callback = () =>
        {
            textItemPrefab = Instantiate(textItemPrefab, content);
            textItemPrefab.SetupText(message);
        };

        callbBacksList.Add(callback);

    }

    private void OnApplicationQuit()
    {
        socket.Close();
    }
}
