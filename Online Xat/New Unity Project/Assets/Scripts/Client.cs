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

    public TextFieldItem textItemPrefab;
    public Transform content;


    Thread thread;

    Socket client;

    bool connected = false;
    // Start is called before the first frame update
    void Start()
    {
        callbBacksList = new List<Action>();

        //Comment and uncomment to use tcp or udp setups (just 1 setup)

        /*---UDP---*/
        //UdpSetup();
        //thread = new Thread(DataLoop);

        /*---TCP---*/
        TcpSetup();
        thread = new Thread(TCPLoop);

        thread.Start();

        //displayText.text = "";

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

        //SendTCPData("Hello");
        int countToDisconnect = 5;
        while (countToDisconnect > 0)
        {
            if (!connected)
                break;
            ReceiveTCPData();
            countToDisconnect--;

            Thread.Sleep(500);

          //  SendTCPData("Hello");
        }

        Debug.Log("Disconnecting from server...");
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }
    private void TcpSetup()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1818);
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

    public void SendTCPData(TMP_InputField message)
    {
        try
        {
            byte[] data = Encoding.ASCII.GetBytes(message.text);
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
            if(ReceiveData() == 0)
            {
                break;
            }

            Thread.Sleep(500);

            SendData("Ping");
        }
        socket.Close();
        Debug.Log("Closing socket");
    }

    void SendData(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        socket.SendTo(data, SocketFlags.None, ipepRemote);
    }

    int ReceiveData()
    {
        try
        {
            byte[] data = new byte[256];
            int size = socket.ReceiveFrom(data, ref ipepRemote);
            if (size == 0)
            {
                return size;
            }
            string message = Encoding.ASCII.GetString(data, 0, size);

            AddCallbackMessage(message);
        }
        catch (SystemException e)
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
            textItemPrefab = Instantiate(textItemPrefab,content);
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
