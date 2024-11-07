using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine;

public class Network : MonoBehaviour
{
    bool bServer = false;
    bool bConnect = false;

    Socket socketListen = null;
    Socket socket = null;

    Thread thread = null;//각 유저 사이를 연결해서 데이터를 주고 받게함.
    bool bThreadBegin = false;

    Buffer bufferSend;
    Buffer bufferReceive;

    public string name;

    void Start()
    {
        bufferSend = new Buffer();
        bufferReceive = new Buffer();
    }

    public void ServerStart(int port, int backlog=10)
    {
        socketListen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
        socketListen.Bind(ep); // ip주소와 포트번호 연결하는거
        socketListen.Listen(backlog);
        bServer = true;
        Debug.Log("Server Start");
        StartThread();
    }

    public bool IsServer()
    {
        return bServer;
    }

    bool StartThread()
    {//기본적으로 쓰는 함수, 함수를 시작하면 계속 돌아감.
        ThreadStart threadDelegate = new ThreadStart(ThreadProc);//코루틴이랑 비슷함.
        thread = new Thread(threadDelegate);
        thread.Start();

        bThreadBegin = true;

        return true;
    }

    public void ThreadProc()
    {
        while (bThreadBegin)
        {
            AcceptClient();//새로운 클라이언트가 계속 접속을 시도하고 있는지 확인

            if (socket != null && bConnect == true)
            {
                SendUpdate(); //실시간으로 데이터를 주고 받음.
                ReceiveUpdate();
            }

            Thread.Sleep(10);//휴식 시간을 줘야지 잘 돌아감. 
        }
    }

    public void ClientStart(string address, int port)
    {
        socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(address, port);
        bConnect = true;
        Debug.Log("Client Start");
        StartThread();
    }

    void AcceptClient()
    {
        if (socketListen != null && socketListen.Poll(0, SelectMode.SelectRead))
        {//Poll은 소켓리스너에 뭐가 들어있는지 확인함.
            socket = socketListen.Accept();//클라이언트가 커넥트를 통해 맞이하는 것과 같음(호텔 로비같은 존재)
            bConnect = true;

            Debug.Log("Client Connect");
        }
    }

    public bool IsConnect()
    {
        return bConnect;
    }

    public int Send(byte[] bytes, int length)
    {
        return bufferSend.Write(bytes, length);
    }

    public int Receive(ref byte[] bytes, int length)
    {
        return bufferReceive.Read(ref bytes, length);
    }

    void SendUpdate()
    {
        if (socket.Poll(0, SelectMode.SelectWrite))
        {
            byte[] bytes = new byte[1024];

            int length = bufferSend.Read(ref bytes, bytes.Length);
            //버터에 있는 데이터를 가지고 와서 읽음.
            while (length > 0)
            {
                socket.Send(bytes, length, SocketFlags.None);
                //이후 소켓에다가 데이터를 보냄.
                length = bufferSend.Read(ref bytes, bytes.Length);
            }
        } 
    }

    void ReceiveUpdate()
    {//택배상자가 쌓이는 일
        while (socket.Poll(0, SelectMode.SelectRead))
        {
            byte[] bytes = new byte[1024];

            int length = socket.Receive(bytes, bytes.Length, SocketFlags.None);
            //보내기 위해서 리시브로 받아와서 버터에다가 저장한다.
            if (length > 0)
            {
                bufferReceive.Write(bytes, length);
            }
        }
    }
}
