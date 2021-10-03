using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;
using TMPro;
using System;
using System.Threading;

public class Client : MonoBehaviour
{
    Socket socket;
    IPEndPoint ipep;
    EndPoint ipepRemote;

    List<Action> callbBacksList;

    public TextMeshProUGUI displayText;

    Thread thread;

    Socket client;

    bool connected = false;
    // Start is called before the first frame update
    void Start()
    {

        callbBacksList = new List<Action>();
        TcpSetup();

        thread = new Thread(TCPLoop);
        thread.Start();

        displayText.text = "";

    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            connected = false;
        }

        if (callbBacksList.Count > 0)
        {
            for (int i = 0; i < callbBacksList.Count; i++)
            {
                Action callback = callbBacksList[i];
                callbBacksList.RemoveAt(i);
                callback();
            }
        }
    }
    private void TCPLoop()
    {
        ConnectToServer();

        if (!connected)
            return;

        SendTCPData("Ping");
        int countToDisconnect = 5;
        while (countToDisconnect > 0)
        {
            if (!connected)
                break;
            ReceiveTCPData();
            countToDisconnect--;

            Thread.Sleep(500);

            SendTCPData("Ping");
        }

        Debug.Log("Disconnecting from server...");
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }
    private void TcpSetup()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ipep = new IPEndPoint(IPAddress.Parse("192.168.0.29"), 1818);
        ipepRemote = (EndPoint)ipep;
    }

    private void UdpSetup()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        ipep = new IPEndPoint(IPAddress.Parse("192.168.0.29"), 1818);
        ipepRemote = (EndPoint)ipep;
    }

 
    private void ConnectToServer()
    {
        try
        {
            socket.Connect(ipepRemote);
            connected = true;
            AddCallbackMessage("Connecting to server");
        }
        catch (SocketException e)
        {
            Debug.Log("Unable to connect to server");
            Debug.Log(e.ToString());
        }
    }

    private void SendTCPData(string message)
    {
        try
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            socket.Send(data, SocketFlags.None);
        }
        catch(System.Exception e)
        {
            Debug.Log("Sent disconnected unsafe from server");
            connected = false;
        }

    }
    private void ReceiveTCPData()
    {
        try
        {
            byte[] data = new byte[256];
            int size = socket.Receive(data);
            if (size == 0)
            {
                Debug.Log("Client is disconnected");
                socket.Disconnect(false);
            }

            string message = Encoding.ASCII.GetString(data, 0, size);
            AddCallbackMessage(message);
        }
        catch(System.Exception e)
        {
            Debug.Log("Disconnected unsafe server");
            connected = false;
        }



    }
    void DataLoop()
    {
        SendData("Ping");

        while (true)
        {
            ReceiveData();

            Thread.Sleep(500);

            SendData("Ping");
        }

    }

    void SendData(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        socket.SendTo(data, SocketFlags.None, ipepRemote);
    }

    void ReceiveData()
    {
        byte[] data = new byte[256];
        int size = socket.ReceiveFrom(data, ref ipepRemote);

        string message = Encoding.ASCII.GetString(data, 0, size);

        AddCallbackMessage(message);

    }

    void AddCallbackMessage(string message)
    {
        Action callback = () =>
        {
            displayText.text += message + "\n";
        };

        callbBacksList.Add(callback);

    }
    private void OnApplicationQuit()
    {
        thread.Abort();
        socket.Close();
    }
}
