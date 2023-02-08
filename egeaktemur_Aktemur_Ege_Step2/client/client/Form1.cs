using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace client
{
    public partial class Form1 : Form
    {
        //Define Global variables
        bool terminating = false;
        bool connected = false;
        Socket clientSocket;

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void button_connect_Click(object sender, EventArgs e)
        {
            string Name = NameBox.Text;
            string IP = textBox_ip.Text;
            string Port = PortBox.Text;

            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            int portNum;
            if (IP != "" && IP.Length <= 256)
            {
                if (Name != "" && Name.Length <= 256)
                {
                    if (Int32.TryParse(Port, out portNum)) // Check if given Port is integer
                    {
                        try
                        {
                            clientSocket.Connect(IP, portNum);
                            button_connect.Enabled = false;
                            connected = true;
                            logs.AppendText("Connected to the server!\n");

                            // Send name to server
                            Byte[] namebuffer = Encoding.Default.GetBytes("Name:"+Name);
                            clientSocket.Send(namebuffer);

                            Thread receiveThread = new Thread(Receive);
                            receiveThread.Start();

                        }
                        catch
                        {
                            logs.AppendText("Could not connect to the server!\n");
                        }
                    }
                    else
                    {
                        logs.AppendText("Check the port\n");
                    }
                }
                else
                {
                    logs.AppendText("Check the Name\n");
                }
            }
            else
            {
                logs.AppendText("Check the IP\n");
            }

        }

        private void Receive()
        {
            while (connected)
            {
                
                try
                {
                    disconnect_button.Enabled = true;
                    Byte[] buffer = new Byte[256];
                    clientSocket.Receive(buffer);

                    string incomingMessage = Encoding.Default.GetString(buffer);
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf("\0"));

                    logs.AppendText("Server: " + incomingMessage + "\n");

                    if (incomingMessage == "This name Exists") 
                    {
                        disconnect_button.Enabled = false;
                        terminating = true;
                        connected = false;
                        button_connect.Enabled = true;
                        AnswerBox.Enabled = false;
                        button_send.Enabled = false;
                        clientSocket.Close();
                    }
                    else if ( incomingMessage == "There is a game running.")
                    {
                        disconnect_button.Enabled = true;
                        terminating = false;
                        connected = true;
                        button_connect.Enabled = false;
                        AnswerBox.Enabled = false;
                        button_send.Enabled = false;
                    }
                    else if (incomingMessage.Contains("Server disconnecting"))
                    {
                        disconnect_button.Enabled = false;
                        terminating = true;
                        connected = false;
                        button_connect.Enabled = true;
                        AnswerBox.Enabled = false;
                        button_send.Enabled = false;
                        clientSocket.Close();
                    }
                    else if (incomingMessage.Length > 0 && !incomingMessage.Contains("Game ended")&& !incomingMessage.Contains("has answered the question. Server is waiting for your answer")) // If question received
                    {
                        Question.Text = incomingMessage;
                        button_send.Enabled = true;
                        AnswerBox.Enabled = true;
                    }
                    else if (incomingMessage.Contains("Game ended"))
                    {
                        button_send.Enabled = false;

                    }
                    
                }
                catch
                {
                    if (!terminating)
                    {
                        logs.AppendText("The server has disconnected\n");
                        button_connect.Enabled = true;
                        AnswerBox.Enabled = false;
                        button_send.Enabled = false;
                    }
                    disconnect_button.Enabled = false;
                    clientSocket.Close();
                    connected = false;
                }
                
            }
        }



        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Byte[] errorbuffer = Encoding.Default.GetBytes("Disconnect");
                clientSocket.Send(errorbuffer);
            }
            catch
            {

            }
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }

        private void button_send_Click(object sender, EventArgs e)
        {
            string message = AnswerBox.Text;
            int messageint;
            if(Int32.TryParse(message, out messageint))// Check if given answer is integer
            {
                if (message != "" && message.Length <= 256)
                {
                    Byte[] buffer = Encoding.Default.GetBytes(message);
                    clientSocket.Send(buffer);
                    AnswerBox.Enabled = false;
                    button_send.Enabled = false;
                }
            }
            else
            {
                logs.AppendText("Answer should be an integer");
            }
        }


        private void disconnect_button_Click(object sender, EventArgs e)
        {
            try
            {
                Byte[] errorbuffer = Encoding.Default.GetBytes("Disconnect");
                clientSocket.Send(errorbuffer);
            }
            catch
            {

            }
            terminating = true;
            connected = false;
            button_connect.Enabled = true;
            AnswerBox.Enabled = false;
            button_send.Enabled = false;
            clientSocket.Close();
            logs.AppendText("Disconnecting...\n");
        }
    }
}
