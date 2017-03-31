using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ShareDLL;
namespace MyClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        byte[] buffer = new byte[1024];
        private Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            LoopConnect();
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), clientSocket);
        }

        private void ReceiveCallback(IAsyncResult iar)
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
                MessageBox.Show("Dis-connected to My Server.", "Error");
                return;       
            }
            if (receivedLenght != 0)
            {                
                byte[] machineByteArray = new byte[receivedLenght];
                Array.Copy(buffer, machineByteArray, receivedLenght);
                MachineModel machine = ConvertHelper.ByteArrayToObject(machineByteArray) as MachineModel;
                MessageBox.Show(string.Format("Machine ID: {0}\rMachine Name: {1}\rDate of Buy: {2:dd/MM/yyyy}\rActive: {3}", machine.MachineId, machine.MachineName, machine.DateOfBuy, machine.Status.ToString()), "Machine Info");
            }
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), clientSocket);
        }

        private void LoopConnect()
        {
            if (clientSocket.Connected == true)
            {
                MessageBox.Show("Already connected to My Server.", "Info");
                return;
            }
            while (clientSocket.Connected == false)
            {
                try
                {
                    clientSocket.Connect(IPAddress.Loopback, 9999);
                }
                catch (Exception)
                {
                    MessageBox.Show("Can't connect to My Server, please check My Server is running!", "Error");
                }
            }
            MessageBox.Show("Connected to My Server.", "Info");
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (clientSocket.Connected == false)
            {
                MessageBox.Show("Dis-connected to My Server.", "Error");
            }

            try
            {
                //Send message follow byte array.
                MachineModel machine = new MachineModel
                {
                    MachineId = txtMachineId.Text,
                    MachineName = txtMachineName.Text,
                    DateOfBuy = dpkDateOfBuy.SelectedDate.Value,
                    Status = chbStatus.IsChecked.Value,
                };
                clientSocket.Send(ConvertHelper.ObjectToByteArray(machine));
            }
            catch (Exception)
            {
                MessageBox.Show("Can't send Machine Info to My Server.", "Error");
            }
        }
    }
}
