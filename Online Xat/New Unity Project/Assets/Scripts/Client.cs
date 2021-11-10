using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
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
    public TMP_InputField inputNameField;
    public TMP_InputField inputMessageField;

    public GameObject loginPanel;
    public GameObject chatPanel;

    public string name = "";

    Thread thread;


    bool connected = false;


    void Start()
    {
        callbBacksList = new List<Action>();

        /*---TCP---*/
        TcpSetup();


        AddCallbackMessage("Connecting to server");

    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            connected = false;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            ConnectToServer();
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
        //ConnectToServer();

        if (!connected)
            return;

        ReceiveData();

    }
    private void TcpSetup()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1818);
        ipepRemote = (EndPoint)ipep;
    }

 
    public void ConnectToServer()
    {
        try
        {
            socket.Connect(ipepRemote);
            loginPanel.SetActive(false);
            chatPanel.SetActive(true);
            //AddCallbackMessage("You are connected! Write Your Name, please.");
            SendTCPData(inputNameField);
            connected = true;

            thread = new Thread(TCPLoop);

            thread.Start();
        }
        catch (SocketException e)
        {
            Debug.Log("Unable to connect to server");
            Debug.Log(e.ToString());
        }
    }

    public void SendTCPData(TMP_InputField message)
    {
        if (string.IsNullOrEmpty(name) && connected)
        {
            name = message.text;
            //conect to server
            message.text = "";
            return;
        }

        try
        {
            byte[] data = Encoding.ASCII.GetBytes(message.text);
            socket.Send(data, SocketFlags.None);
        }
        catch(System.Exception e)
        {
            Debug.Log("Sent disconnected unsafe from server");
            AddCallbackMessage("Unable to connect. Wait for connection.");
            connected = false;
        }
        message.text = "";
    }
    private void ReceiveTCPData(Socket socket)
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
