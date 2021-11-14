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

    public UserInfo userPrefab;
    public TextFieldItem textItemPrefab;
    public Transform content;
    public TMP_InputField inputNameField;
    public TMP_InputField inputMessageField;
    public Transform userListContainer;

    public GameObject loginPanel;
    public GameObject chatPanel;

    public string name = "";

    Thread thread;


    bool connected = false;

    private List<UserInfo> userInfosList = new List<UserInfo>();

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

        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendTCPData(inputMessageField);
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


        if (socket.Poll(1000, SelectMode.SelectRead))
        {
            Debug.Log("Socket is readable");
            ReceiveTCPData(socket);
        }
        else if (socket.Poll(1000, SelectMode.SelectWrite))
        {
            Debug.Log("Socket is writable");

        }
        else if (socket.Poll(1000, SelectMode.SelectError))
        {
            Debug.Log("Socket has error");
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
            connected = true;
            SendTCPData(inputNameField);

        }
        catch (SocketException e)
        {
            Debug.Log("Unable to connect to server");
            Debug.Log(e.ToString());
        }
    }

    public void SendTCPData(TMP_InputField message)
    {
        if (string.IsNullOrEmpty(message.text) || !connected)
        {
            //name = message.text;
            //conect to server
            AddCallbackMessage("Disconnected");
            message.text = "";
            return;
        }

        try
        {
            Server.Message messageToSend = new Server.Message()
            {
                createProfile = -1,
                message = message.text,
                userName = "",
                userNamesList = null,
                
            };

            byte[] data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(messageToSend));
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
            Server.Message messageReceived = JsonUtility.FromJson<Server.Message>(message);
            if(messageReceived.createProfile == 1)
            {
                for (int i = 0; i < userInfosList.Count; i++)
                {
                    Destroy(userInfosList[i].gameObject);
                }

                userInfosList.Clear();

                for (int i = 0; i < messageReceived.userNamesList.Count; i++)
                {
                    NewUserConnected(messageReceived.userNamesList[i]);
                }
                AddCallbackMessage($"{messageReceived.message}");
                return;
            }
           
            AddCallbackMessage($"{messageReceived.userName}: {messageReceived.message}");
        }
        catch(System.Exception e)
        {
            Debug.Log("Disconnected unsafe server");
            connected = false;
        }



    }


    private void NewUserConnected(string name)
    {
        UserInfo userProfile = Instantiate(userPrefab, userListContainer);
        userProfile.Setup(name);
        userInfosList.Add(userProfile);
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
