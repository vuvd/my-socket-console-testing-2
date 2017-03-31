using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

using ShareDLL;
namespace MyServer
{
    class Program
    {
        private static byte[] buffer = new byte[1024];
        private static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static List<Socket> clientSocketList = new List<Socket>();
        static void Main(string[] args)
        {
            SetupServer();
            while (true)
                Console.ReadLine();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up My Server...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 9999));
            serverSocket.Listen(1);
            serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
            Console.WriteLine("Waiting for a connection...");
        }

        private static void AppceptCallback(IAsyncResult iar)
        {
            Socket clientSocket = serverSocket.EndAccept(iar);
            clientSocketList.Add(clientSocket);
            Console.WriteLine(string.Format("{0} connected to Server(Total: {1}).", clientSocket.RemoteEndPoint, clientSocketList.Count));
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), clientSocket);
            Console.WriteLine("Waiting for a Machine Info...");
            serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }

        private static void ReceiveCallback(IAsyncResult iar)
        {
            Socket clientSocket = iar.AsyncState as Socket;
            if (clientSocket.Connected == false)
                return;
            //Check connection from Client.
            int receivedLenght = 0;
            try
            {
                receivedLenght = clientSocket.EndReceive(iar);
            }
            catch (Exception)
            {
                clientSocketList.RemoveAll(c => c.RemoteEndPoint.ToString().Equals(clientSocket.RemoteEndPoint.ToString()));
                Console.WriteLine(string.Format("{0} dis-connect from Server(Total: {1}).", clientSocket.RemoteEndPoint, clientSocketList.Count));
                return;
            }

            //Get message from Client.
            if (receivedLenght != 0)
            {
                //Receive message follow byte array.               
                byte[] machineByteArray = new byte[receivedLenght];
                Array.Copy(buffer, machineByteArray, receivedLenght);
                MachineModel machine = ConvertHelper.ByteArrayToObject(machineByteArray) as MachineModel;                
                Console.WriteLine(string.Format("Machine received from {0} is:", clientSocket.RemoteEndPoint));
                Console.WriteLine("---");
                Console.WriteLine(string.Format("Machine ID: {0}", machine.MachineId));
                Console.WriteLine(string.Format("Machine Name: {0}", machine.MachineName));
                Console.WriteLine(string.Format("Date of Buy: {0:dd/MM/yyyy}", machine.DateOfBuy));
                Console.WriteLine(string.Format("Active Status: {0}", machine.Status.ToString()));
                Console.WriteLine("---");
                Console.WriteLine("Sending to other My Client...");

                foreach (Socket socket in clientSocketList)
                {
                    if (socket.RemoteEndPoint.ToString().Equals(clientSocket.RemoteEndPoint.ToString()) == false)
                    {
                        SendToOtherSocket(socket, machineByteArray);
                    }
                }
            }
            //Loop receive message.
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), clientSocket);
        }

        private static void SendToOtherSocket(Socket clientSocket, byte[] machineByteArray)
        {
            clientSocket.BeginSend(machineByteArray, 0, machineByteArray.Length, SocketFlags.None, new AsyncCallback(SendCallback), clientSocket);
            serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }
        private static void SendCallback(IAsyncResult iar)
        {
            Socket clientSocket = (Socket)iar.AsyncState;
            clientSocket.EndSend(iar);
        }
    }
}
