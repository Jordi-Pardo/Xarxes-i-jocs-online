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
    List<Action> callbBacksList;

    public TextFieldItem textItemPrefab;
    public Transform content;

    Socket socket;
    IPEndPoint ipep;
    EndPoint ipepRemote;

    Thread thread;

    public List<Socket> clients;

    bool acceptingListenedConnections = true;

    string recievedmessage;

    // Start is called before the first frame update
    void Start()
    {
        callbBacksList = new List<Action>();

        //Comment and uncomment to use tcp or udp setups (just 1 setup)



        /*---TCP---*/
        TcpSetup();
        //thread = new Thread(TCPLoop);

        //thread.Start();

        AddCallbackMessage("Waiting for message...");

        //Obra el server
        //Reep connexio de client i envia mensaje de vuelta
        //per rebre més d'una connexió en el loop (update) el socket ha de ssaber en quin estat està per fer non blocking

    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            acceptingListenedConnections = false;
        }

        if(callbBacksList.Count > 0)
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

        if (socket.Poll(-1, SelectMode.SelectRead))
        {
            Debug.Log("Socket is readable");
            WaitToTCPConnection();

        }
        else if (socket.Poll(-1, SelectMode.SelectWrite))
        {
            Debug.Log("Socket is writable");
        }
        else if (socket.Poll(-1, SelectMode.SelectError))
        {
            Debug.Log("Socket has an error");
        }


        for (int i = 0; i < clients.Count; i++)
        {

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

    private void TCPLoop()
    {
        //Keep listening connection attempts
        socket.Listen(10);
        Debug.Log("Listening...");
        while (acceptingListenedConnections)
        {
            WaitToTCPConnection();

            while (true)
            {

                if (ReceiveTCPData() == 0)
                {
                    AddCallbackMessage("Client Disconnected");
                    break;
                }

                Thread.Sleep(500);
                if (!string.IsNullOrEmpty(recievedmessage))
                {

                    SendTCPData(recievedmessage);
                }
                else
                {
                    SendTCPData("Non recieved message");

                }
            }
        }
        Debug.Log("Shutting down");
        //client.Close();
        socket.Close();

    }
    private void WaitToTCPConnection()
    {
        try
        {
            Socket client = socket.Accept();
            clients.Add(client);
            Debug.Log("Client Connected");
            //SendTCPData("Welcome to the server");

            try
            {
                byte[] data = Encoding.ASCII.GetBytes("Welcome To the Server");
                client.Send(data, SocketFlags.None);
            }
            catch (SystemException e)
            {
                Debug.Log("Error sending data");
            }

        }
        catch (System.Exception e)
        {
            Debug.Log("Connection failed...trying again");
        }

    }

    private void SendTCPData(string message)
    {
        try
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Send(data, SocketFlags.None);
            }
        }
        catch(SystemException e) 
        {
            Debug.Log("Error sending data");   
        }

    }
    private int ReceiveTCPData()
    {
        if (!client.Connected)
            return 0;
        try
        {
            byte[] data = new byte[256];
            int size = client.Receive(data);
            if(size == 0)
            {
                return size;
            }
            string message = Encoding.ASCII.GetString(data, 0, size);
            recievedmessage = message;
            AddCallbackMessage(message);
        }
        catch(SystemException e)
        {
            Debug.Log("Error receiving data..Desconnecting");
            return 0;
        }
        return 1;
    }

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
        thread.Abort();
        socket.Close();
    }
}
