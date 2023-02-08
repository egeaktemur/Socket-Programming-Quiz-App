using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace server
{
    public partial class Form1 : Form
    {


        //Define Global variables
        bool terminating = false;
        bool listening = false;
        int playerCount = 0;      // Keeps the current player amount
        int QuestionNumber = 0; // Keeps the track of the current question
        int Question_Amount = 0;// Keeps the Question limit in the GUI
        int AnswerAmount = 0;   // Keeps how many answers arrived
        int playerLimit = 2;      // Keeps how many players can join

        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        List<Socket> clientSockets = new List<Socket>();        // Keeps the Client Sockets
        List<String> clientNames = new List<String>();          // Keeps the Client Names
        List<int> clientIDs = new List<int>();                  // Keeps the Client IDs
        List<double> clientPoints = new List<double>() { 0, 0 };   // Keeps the Client Points
        List<int> clientAnswers = new List<int>() { 0, 0 };       // Keeps the Client Answers
        List<String> Questions = new List<String>() { };        // Keeps the Questions
        List<int> Answers = new List<int>() { };                // Keeps the Answers
        private void GetQuestions()
        { // Function to read Questions and Answers from file
            String[] lines = System.IO.File.ReadAllLines(@"C:\users\egeaktemur\Desktop\302\CS 408\Project Step 1\Project 1.0\questions.txt");
            for (int i = 0; i < lines.Length; i++)
            {
                if (i % 2 == 0)
                {
                    Questions.Add(lines[i]);
                }
                else if (i % 2 == 1)
                {
                    Answers.Add(Int32.Parse(lines[i]));
                }
            }
        }
        public Form1()
        {
            GetQuestions();
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }
        private void button_listen_Click(object sender, EventArgs e)
        {
            int serverPort;
            if (Int32.TryParse(QuestionAmountBox.Text, out Question_Amount)) // Check if given Question Amount is integer
            {
                if (Int32.TryParse(PortBox.Text, out serverPort)) // Check if given Port is integer
                {
                    // Reset the global variables
                    playerCount = 0;
                    serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    clientSockets = new List<Socket>();
                    clientNames = new List<String>();
                    clientIDs = new List<int>();
                    clientPoints = new List<double>() { 0, 0 };
                    clientAnswers = new List<int>() { 0, 0 };
                    QuestionNumber = 0;
                    AnswerAmount = 0;
                    terminating = false;
                    listening = false;

                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, serverPort);

                    // Start listening
                    serverSocket.Bind(endPoint);
                    serverSocket.Listen(3);
                    listening = true;
                    ServerButton.Enabled = false;
                    button1.Enabled = true;
                    PortBox.Enabled = false;
                    QuestionAmountBox.Enabled = false;
                    // Start accepting
                    Thread acceptThread = new Thread(Accept);
                    acceptThread.Start();
                    logs.AppendText("Started listening on port: " + serverPort + "\n");
                }
                else
                {
                    logs.AppendText("Please check port number \n");
                }
            }
            else
            {
                logs.AppendText("Please check Question Amount \n");
            }
        }
        private void Accept()
        {
            while (listening)
            {
                try
                {
                    Socket newClient = serverSocket.Accept();
                    logs.AppendText("A client is connected.\n");
                    if (playerCount < playerLimit) // Check if player can be added
                    {
                        clientSockets.Add(newClient);
                        Thread receiveThread = new Thread(() => Receive(newClient)); // updated
                        receiveThread.Start();
                    }
                    else
                    {
                        logs.AppendText("player Limit Reached and Disconnecting player \n");
                        newClient.Close();
                    }
                }
                catch
                {
                    if (terminating)
                    {
                        listening = false;
                    }
                    else
                    {
                        logs.AppendText("The socket stopped working.\n");
                    }
                }
            }
            logs.AppendText("Disconnecting server.\n");
            serverSocket.Close();
            ServerButton.Enabled = true;
            PortBox.Enabled = true;
            QuestionAmountBox.Enabled = true;
        }

        private void Receive(Socket thisClient)
        {
            // Definition of player specific variables 
            bool connected = true;      // Checks if the client is still connected
            bool namereceived = false;  // Checks if the clients name is received
            int idofclient = 0;         // Holds the id of client

            while (connected && !terminating)
            {
                try
                {
                    // Receive the message
                    Byte[] buffer = new Byte[256];
                    thisClient.Receive(buffer);
                    string incomingMessage = Encoding.Default.GetString(buffer);
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf("\0"));

                    if (incomingMessage.Contains("Name:")) // If name is not received yet
                    {
                        logs.AppendText("Client " + idofclient + ": " + incomingMessage + "\n");
                    }
                    else if (incomingMessage == "Disconnect") // Disconnect player
                    {
                        logs.AppendText("Client " + idofclient + ": " + incomingMessage + "\n");
                    }
                    else
                    {
                        logs.AppendText("Client " + idofclient + " \"" + clientNames[idofclient] + "\": " + incomingMessage + "\n");
                    }

                    if (namereceived == false && incomingMessage.Contains("Name:")) // Set the name
                    {
                        incomingMessage = incomingMessage.Substring(5); // Get after Name:
                        namereceived = true;
                        if (clientNames.Contains(incomingMessage)) // Check if the playername exists
                        {
                            logs.AppendText("This Name Exists, Disconnecting player \n");
                            Byte[] errorbuffer = Encoding.Default.GetBytes("This name Exists");
                            thisClient.Send(errorbuffer);
                            clientSockets.RemoveAt(clientSockets.Count-1);
                            connected = false;
                        }
                        else
                        {
                            // Assign player id and save the name
                            bool foundEmpty = false;
                            for (int i = 0; i < clientIDs.Count; i++)
                            {
                                if (clientIDs[i] == -1)
                                {
                                    foundEmpty = true;
                                    clientIDs[i] = i;
                                    idofclient = i;
                                    break;
                                }
                            }
                            if (!foundEmpty)
                            {
                                idofclient = clientIDs.Count;
                                clientIDs.Add(clientIDs.Count);
                            }
                            clientNames.Add(incomingMessage);
                            playerCount++;
                            logs.AppendText("Client with name " + incomingMessage + " joined with ID " + idofclient + " \n");
                            if (playerCount == playerLimit)
                            {
                                SendQuestion();
                            }
                        }
                    }
                    else if (incomingMessage == "Disconnect") // Disconnet player
                    {
                        connected = false;
                        logs.AppendText("Client " + idofclient + ": is disconnecting \n");
                        if (!terminating)
                        {
                            logs.AppendText("A client has disconnected\n");
                        }
                        playerCount--;
                        clientIDs[idofclient] = -1;   // Set client id as -1 which means exited
                        clientPoints[idofclient] = 0; // Set client points as 0
                        if (playerCount >0)
                        {
                            Game_Ended();
                        }
                        clientNames.Clear();
                        clientSockets.Clear();


                        thisClient.Close();
                    }

                    else if (incomingMessage.Length > 0) // If answer received
                    {
                        AnswerAmount++;
                        clientAnswers[idofclient] = Int32.Parse(incomingMessage); // Get answer as int
                        if (AnswerAmount == 1)
                        {
                            clientAnswers[idofclient] = Int32.Parse(incomingMessage);
                            // Inform the other player that other one answered
                            for (int i = 0; i < clientIDs.Count; i++)
                            {
                                if (clientIDs[i] != idofclient)
                                {
                                    logs.AppendText(clientNames[i] + " has received the information that " + clientNames[idofclient] + " answered the question \n");
                                    Byte[] announcebuffer = Encoding.Default.GetBytes(clientNames[idofclient] + " has answered the question. Server is waiting for your answer");
                                    clientSockets[i].Send(announcebuffer);
                                    break;
                                }
                            }
                        }
                        else if (AnswerAmount == 2)
                        {
                            int Answer = Answers.ElementAt(QuestionNumber);
                            if (Math.Abs(clientAnswers[1] - Answer) == Math.Abs(clientAnswers[0] - Answer))
                            {
                                clientPoints[0] = clientPoints[0] + 0.5;
                                clientPoints[1] = clientPoints[1] + 0.5;
                            }
                            else if (Math.Abs(clientAnswers[0] - Answer) < Math.Abs(clientAnswers[1] - Answer))
                            {
                                clientPoints[0] = clientPoints[0] + 1;
                            }

                            else if (Math.Abs(clientAnswers[1] - Answer) < Math.Abs(clientAnswers[0] - Answer))
                            {
                                clientPoints[1] = clientPoints[1] + 1;
                            }

                            if (clientPoints[0] < clientPoints[1])
                            {
                                logs.AppendText(clientNames[1] + "'s Point: " + clientPoints[1] + " " + clientNames[0] + "'s Point: " + clientPoints[0] + "\n");
                            }
                            else
                            {
                                logs.AppendText(clientNames[0] + "'s Point: " + clientPoints[0] + " " + clientNames[1] + "'s Point: " + clientPoints[1] + "\n");
                            }
                            for (int i = 0; i < clientAnswers.Count; i++) // Inform the players
                            {
                                Byte[] answersbuffer = Encoding.Default.GetBytes("Correct answer was: " + Answer + " " + clientNames[0] + "'s Answer was: " + clientAnswers[0] + " " + clientNames[1] + "'s Answer was: " + clientAnswers[1] + "\n");
                                clientSockets[i].Send(answersbuffer);

                                Byte[] resultbuffer = Encoding.Default.GetBytes(clientNames[0] + "'s Point: " + clientPoints[0] + " " + clientNames[1] + "'s Point: " + clientPoints[1] + "\n");
                                if (clientPoints[0] < clientPoints[1])
                                {
                                   resultbuffer = Encoding.Default.GetBytes(clientNames[1] + "'s Point: " + clientPoints[1] + " " + clientNames[0] + "'s Point: " + clientPoints[0] + "\n");
                                }
                                clientSockets[i].Send(resultbuffer);
                            }
                            AnswerAmount = 0;
                            QuestionNumber++;
                            // If not game ended Send new Question
                            bool game_ended = SendQuestion();
                            if (game_ended)
                            {
                                connected = false;
                            }
                        }
                    }
                }
                catch
                {
                    if (!terminating)
                    {
                        logs.AppendText("A client has disconnected\n");
                    }
                    playerCount--;
                    clientIDs[idofclient] = -1;   // Set client id as -1 which means exited
                    clientPoints[idofclient] = 0; // Set client points as 0

                    Game_Ended();
                    thisClient.Close();
                }
            }
        }


        private bool SendQuestion()
        {
            // If game should keep going but we read all of the questions start from begining
            if (QuestionNumber < Question_Amount && QuestionNumber == Questions.Count)
            {
                QuestionNumber = 0;
                Question_Amount = Question_Amount - Questions.Count;
            }
            if (QuestionNumber < Question_Amount)
            {
                String Question = Questions.ElementAt(QuestionNumber);
                foreach (int clientID in clientIDs) // Send Questions to clients
                {
                    logs.AppendText("Question \"" + Question + "\" to "+clientNames[clientID]+"\n");
                    Byte[] questionbuffer = Encoding.Default.GetBytes(Question);
                    clientSockets[clientID].Send(questionbuffer);
                }
            }
            else // If we reached the total questions amount
            {
                Game_Ended();
                return true;
            }
            return false;
        }
        private void Game_Ended()
        {
            if (listening)
            {
                if (playerCount == 1) // If one user disconnected inform the other user
                {
                    for (int i = 0; i < clientIDs.Count; i++)
                    {
                        if (clientIDs[i] != -1)
                        {
                            logs.AppendText("Client " + clientIDs[i] + " won!! \n");
                            if (clientPoints[0] < clientPoints[1])
                            {
                                logs.AppendText(clientNames[1] + "'s Point: " + clientPoints[1] + " " + clientNames[0] + "'s Point: " + clientPoints[0] + "\n");
                            }
                            else
                            {
                                logs.AppendText(clientNames[0] + "'s Point: " + clientPoints[0] + " " + clientNames[1] + "'s Point: " + clientPoints[1] + "\n");
                            }

                            Byte[] resultbuffer = Encoding.Default.GetBytes("Other player Disconnected "+clientNames[0] + "'s Point: " + clientPoints[0] + " " + clientNames[1] + "'s Point: " + clientPoints[1]+ " Game ended You Won !!");
                            if (clientPoints[0] < clientPoints[1])
                            {
                                resultbuffer = Encoding.Default.GetBytes("Other player Disconnected " + clientNames[1] + "'s Point: " + clientPoints[1] + " " + clientNames[0] + "'s Point: " + clientPoints[0] + " Game ended You Won !!");
                            }

                            clientSockets[i].Send(resultbuffer);
                            break;
                        }
                    }
                }
                else if (clientPoints[0] == clientPoints[1]) // If game ended with a draw
                {
                    logs.AppendText("Game Ended with a Draw \n");
                    logs.AppendText(clientNames[0] + "'s Point: " + clientPoints[0] + " " + clientNames[1] + "'s Point: " + clientPoints[1] + "\n");

                    Byte[] resultbuffer = Encoding.Default.GetBytes("Game Ended with a Draw "+clientNames[0] + "'s Point: " + clientPoints[0] + " " + clientNames[1] + "'s Point: " + clientPoints[1]);
                    foreach (Socket clientSocket in clientSockets)
                    {
                        clientSocket.Send(resultbuffer);
                    }
                }

                else if (clientPoints[0] > clientPoints[1]) 
                {
                    logs.AppendText("Client 0 won!! \n");
                    if (clientPoints[0] < clientPoints[1])
                    {
                        logs.AppendText(clientNames[1] + "'s Point: " + clientPoints[1] + " " + clientNames[0] + "'s Point: " + clientPoints[0] + "\n");
                    }
                    else
                    {
                        logs.AppendText(clientNames[0] + "'s Point: " + clientPoints[0] + " " + clientNames[1] + "'s Point: " + clientPoints[1] + "\n");
                    }

                    Byte[] results1buffer = Encoding.Default.GetBytes(clientNames[0] + "'s Point: " + clientPoints[0] + " " + clientNames[1] + "'s Point: " + clientPoints[1] + " Game ended You Won !!");
                    clientSockets[0].Send(results1buffer);
                    Byte[] results2buffer = Encoding.Default.GetBytes(clientNames[0] + "'s Point: " + clientPoints[0] + " " + clientNames[1] + "'s Point: " + clientPoints[1] + " Game ended You Lost !!");
                    clientSockets[1].Send(results2buffer);
                }

                else if (clientPoints[0] < clientPoints[1])
                {
                    logs.AppendText("Client 1 won!! \n");
                    if (clientPoints[0] < clientPoints[1])
                    {
                        logs.AppendText(clientNames[1] + "'s Point: " + clientPoints[1] + " " + clientNames[0] + "'s Point: " + clientPoints[0] + "\n");
                    }
                    else
                    {
                        logs.AppendText(clientNames[0] + "'s Point: " + clientPoints[0] + " " + clientNames[1] + "'s Point: " + clientPoints[1] + "\n");
                    }

                    
                    Byte[] results2buffer = Encoding.Default.GetBytes(clientNames[1] + "'s Point: " + clientPoints[1] + " " + clientNames[0] + "'s Point: " + clientPoints[0]+ " Game ended You Lost !!");
                    clientSockets[0].Send(results2buffer);
                    Byte[] results1buffer = Encoding.Default.GetBytes(clientNames[1] + "'s Point: " + clientPoints[1] + " " + clientNames[0] + "'s Point: " + clientPoints[0] + " Game ended You Won !!");
                    clientSockets[1].Send(results1buffer);
                }
                // Stop listening and terminate
                listening = false;
                terminating = true;
            }
            serverSocket.Close();
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;
            Environment.Exit(0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listening = false;
            terminating = true;
            logs.AppendText("Disconnecting server.\n");
            foreach (Socket clientSocket in clientSockets)
            {
                Byte[] server = Encoding.Default.GetBytes("Server disconnecting");
                clientSocket.Send(server);
            }
            serverSocket.Close();
            ServerButton.Enabled = true;
            PortBox.Enabled = true;
            QuestionAmountBox.Enabled = true;
            button1.Enabled = false;
        }
    }
}
