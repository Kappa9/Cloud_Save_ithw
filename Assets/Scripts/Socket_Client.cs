using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Socket_Client : MonoBehaviour
{

    //因为此项目可以不用Socket通信，这个类现已被废弃，不过还是留着吧

    public string ipAddress = "106.13.88.104";      //ip地址
    public int portNumber = 10001;                  //端口号
    public bool connected = false;                  //在线状态
    
    Socket clientSocket;                            //客户端Socket
    int connectCount;                           //连接次数

    void OnEnable()
    {
        connectCount = 0;
        ConnectedToServer();
    }

    //连接到服务器
    public void ConnectedToServer()
    {
        connectCount++;
        Debug.Log("这是第" + connectCount + "次连接");

        //创建新的连接
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //设置端口号和ip地址
        EndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), portNumber);

        //发起连接
        clientSocket.BeginConnect(endPoint, OnConnectCallBack, "");
    }

    //连接的回调
    public void OnConnectCallBack(IAsyncResult ar)
    {
        if (clientSocket.Connected)
        {
            Debug.Log("连接成功");
            connected = true;
            connectCount = 0;
        }
        else
        {
            Debug.Log("连接失败");
            clientSocket = null;
            ConnectedToServer();
        }
        //结束连接
        clientSocket.EndConnect(ar);
    }

    void Update()
    {
        if (connected && !clientSocket.Connected)
        {
            Debug.Log("与服务器断开连接");
            clientSocket = null;
            connected = false;
        }
    }

    void OnApplicationQuit()
    {
        if (connected && clientSocket != null)
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();                       //关闭连接
        }
    }
}