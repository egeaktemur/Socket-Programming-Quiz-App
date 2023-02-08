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
        bool game_running = false; // indicates whether the game is currently running or not
        bool terminating = false; // true when it is decided terminate the game
        bool listening = false; // indicates whether the server is listening for buffers or not
        int playerCount = 0;      // Keeps the current player amount
        int QuestionNumber = 0; // Keeps the track of the current question
        int Question_Amount = 0;// Keeps the Question limit in the GUI
        int AnswerAmount = 0;   // Keeps how many answers arrived
        int playingUserCount = 0;

        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        List<Socket> clientSockets = new List<Socket>();        // Keeps the Client Sockets
        List<bool> playlist = new List<bool>(); //boolean array to store which player is currently playing
        List<bool> answered = new List<bool>(); //boolean array to keep track of which player answered (true if answered else false)
        List<String> clientNames = new List<String>();          // Keeps the Client Names
        List<int> clientIDs = new List<int>();                  // Keeps the Client IDs
        List<double> clientPoints = new List<double>();   // Keeps the Client Points
        List<int> clientAnswers = new List<int>();       // Keeps the Client Answers
        List<String> Questions = new List<String>() { };        // Keeps the Questions
        List<int> Answers = new List<int>() { };                // Keeps the Answers


        private String CorrectAnswer(int Answer) // creates a string for stating correct answer and the answers of the players
        {
            String CorrectAnswer = "Correct answer was: " + Answer + " ";
            for (int i = 0; i < playingUserCount; i++)
            {
                CorrectAnswer += clientNames[i] + "'s Answer was: " + clientAnswers[i] + " ";
            }
            return CorrectAnswer;
        }
        private String Scoreboard() // creates a scoreboard
        {
            Sort();
            String Scoreboard = "";
            for (int i = 0; i < playingUserCount; i++)
            {
                Scoreboard += clientNames[i] + "'s Point: " + clientPoints[i] + " ";
            }
            return Scoreboard;
        }

        private void SendResults() //sends the scoreboard and results to the players and prints them
        {
            String Scores = Scoreboard(); 
            bool draw = false; //if the game ended with a draw
            double maxPoint = clientPoints[0]; //points are sorted so 0th index is the maximum
            if (playerCount > 1 && maxPoint == clientPoints[1])
            {
                draw = true;
                logs.AppendText("Game ended with a draw !! \n"); 
            }


            for (int i = 0; i < playingUserCount; i++) 
            {
                if (clientIDs[i] != -1 && playlist[i])
                {
                    if (draw && clientPoints[i] == maxPoint)
                    {
                        logs.AppendText(clientNames[i] + " had max points \n");
                        Byte[] resultbuffer = Encoding.Default.GetBytes(" Game ended with a draw !!");
                        clientSockets[i].Send(resultbuffer);
                    }
                    else if (i == 0)
                    {
                        logs.AppendText("Game ended, " + clientNames[i] + " won the game with " + clientPoints[i] + " points \n");
                        Byte[] resultbuffer = Encoding.Default.GetBytes(" Game ended You Won !!");
                        clientSockets[i].Send(resultbuffer);
                    }
                    else
                    {
                        Byte[] resultbuffer = Encoding.Default.GetBytes(" Game ended You Lost !!");
                        clientSockets[i].Send(resultbuffer);
                    }
                }

            }

        }

        private void Sort() { //sorts all of the lists with respect to clientPoints
            for (int i = 1; i < clientPoints.Count(); i++) {
                double point = clientPoints[i];

                int id = clientIDs[i];
                Socket socket = clientSockets[i];
                String name = clientNames[i];
                int answer = clientAnswers[i];
                bool playing = playlist[i];
                bool answeq = answered[i];

                int j = i - 1;
                while (j >= 0 && clientPoints[j] < point) {
                    clientPoints[j + 1] = clientPoints[j];
                    playlist[j + 1] = playlist[j];
                    answered[j + 1] = answered[i];

                    clientIDs[j + 1] = clientIDs[j];
                    clientSockets[j + 1] = clientSockets[j];
                    clientNames[j + 1] = clientNames[j];
                    clientAnswers[j + 1] = clientAnswers[j];

                    j = j - 1;
                }
                clientPoints[j + 1] = point;
                playlist[j + 1] = playing;
                answered[j + 1] = answeq;
                clientSockets[j + 1] = socket;
                clientNames[j + 1] = name;
                clientIDs[j + 1] = id;
                clientAnswers[j + 1] = answer;
            }
        }

        private void SortwithID() //sorts all of the lists with respect to clientIDs
        {
            for (int i = 1; i < clientIDs.Count(); i++)
            {
                double point = clientPoints[i];

                int id = clientIDs[i];
                Socket socket = clientSockets[i];
                String name = clientNames[i];
                int answer = clientAnswers[i];
                bool playing = playlist[i];
                bool answeq = answered[i];

                int j = i - 1;
                while (j >= 0 && clientIDs[j] < id)
                {
                    clientPoints[j + 1] = clientPoints[j];
                    playlist[j + 1] = playlist[j];
                    answered[j + 1] = answered[i];

                    clientIDs[j + 1] = clientIDs[j];
                    clientSockets[j + 1] = clientSockets[j];
                    clientNames[j + 1] = clientNames[j];
                    clientAnswers[j + 1] = clientAnswers[j];

                    j = j - 1;
                }
                clientPoints[j + 1] = point;
                playlist[j + 1] = playing;
                answered[j + 1] = answeq;
                clientSockets[j + 1] = socket;
                clientNames[j + 1] = name;
                clientIDs[j + 1] = id;
                clientAnswers[j + 1] = answer;
            }
        }

        private int getindex(int idofclient) { //gets the index of a given client
            for (int i = 0; i < clientIDs.Count; i++)
            {
                if (clientIDs[i] == idofclient)
                {
                    return i;
                }
            }
            return -1;
        }
        private void GetQuestions() 
        { // Function to read Questions and Answers from file
            String[] lines = System.IO.File.ReadAllLines(@"C:\Users\kaanb\OneDrive\Desktop\408step2\questions.txt");
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

            if (Int32.TryParse(PortBox.Text, out serverPort)) // Check if given Port is integer
            {
                // Reset the global variables
                playerCount = 0;
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSockets = new List<Socket>();
                clientNames = new List<String>();
                clientIDs = new List<int>();
            
                playlist = new List<bool>();
                answered = new List<bool>();

                clientPoints = new List<double>();   // Keeps the Client Points
                clientAnswers = new List<int>();       // Keeps the Client Answers
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
                QuestionAmountBox.Enabled = true;
                startgame.Enabled = true;
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
        private void Accept()
        {
            while (listening)
            {
                try
                {
                    Socket newClient = serverSocket.Accept();
                    logs.AppendText("A client is trying to connect.\n");
                    if (!game_running) // Check if player can be added
                    {
                        Thread receiveThread = new Thread(() => Receive(newClient)); // updated
                        receiveThread.Start();
                    }
                    else
                    {
                        logs.AppendText("New client will be added after game ends! \n");
                        Byte[] errorbuffer = Encoding.Default.GetBytes("There is a game running.");
                        newClient.Send(errorbuffer);
                        Thread receiveThread = new Thread(() => Receive(newClient)); // receive olmaz yeni function lazım
                        receiveThread.Start();
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
            serverSocket.Close(); //closing the server and ending the game
            ServerButton.Enabled = true;
            button1.Enabled = false;
            PortBox.Enabled = true;
            QuestionAmountBox.Enabled = true;
            startgame.Enabled = false;

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
                        logs.AppendText("New Client: " + incomingMessage + "\n");
                    }
                    else if (incomingMessage == "Disconnect") // Disconnect player
                    {
                        logs.AppendText("Client " + idofclient + ": " + incomingMessage + "\n");
                    }
                    else if(incomingMessage.Length == 0 && !listening)
                    {
                        connected = false;
                    }
                    else
                    {
                        logs.AppendText("Client " + idofclient + " \"" + clientNames[getindex(idofclient)] + "\": " + incomingMessage + "\n");
                    }

                    if (namereceived == false && incomingMessage.Contains("Name:")) // Set the name
                    {
                        incomingMessage = incomingMessage.Substring(5); // Get after Name:
                        namereceived = true;
                        bool name_exists = false;
                        for (int i = 0; i < clientIDs.Count; i++)
                        {
                            if (clientIDs[i] != -1 && clientNames[i] == incomingMessage)
                            {
                                name_exists = true;
                            }
                        }
                        if (name_exists) //if the name exists, disconnect the player
                        {
                            logs.AppendText("This Name Exists, Disconnecting player \n");
                            Byte[] errorbuffer = Encoding.Default.GetBytes("This name Exists");
                            thisClient.Send(errorbuffer);
                            connected = false;
                            break;
                        }
                        else
                        {
                            // Assign player id and save the name
                            int new_id = clientIDs.Count+1;
                            List < bool > exists = new List<bool>(new bool[clientIDs.Count]);

                            for (int i = 0; i < clientIDs.Count; i++)
                            {
                                if (clientIDs[i] > -1 && clientIDs[i] <= clientIDs.Count)
                                {
                                    exists[clientIDs[i]] = true;
                                }
                            }
                            for (int i = 0; i < clientIDs.Count; i++) { //finds id for new client
                                if (!exists[i]) {
                                    new_id = i;
                                    break;
                                }
                            }
                            if (new_id == clientIDs.Count + 1 || game_running)
                            {
                                idofclient = clientIDs.Count;
                                clientIDs.Add(idofclient);
                                clientNames.Add(incomingMessage);
                                clientSockets.Add(thisClient);
                                playlist.Add(false);
                            }
                            else
                            {
                                for (int i = 0; i < clientIDs.Count; i++)
                                {
                                    if (clientIDs[i] == -1)
                                    {
                                        clientIDs[i] = new_id;
                                        idofclient = new_id;
                                        clientNames[i] = incomingMessage;
                                        clientSockets[i] = thisClient;
                                        playlist[i] = false;
                                        break;
                                    }
                                }
                                
                            }
                            
                            logs.AppendText("Client with name " + incomingMessage + " joined with ID " + idofclient + " \n");

                            
                        }
                    }
                    else if (incomingMessage == "Disconnect") // Disconnect player
                    {
                        connected = false;
                        if (!terminating)
                        {
                            logs.AppendText("A client is started disconnecting \n");
                        }
                        int index = getindex(idofclient);
                        if (index != -1)
                        {
                            logs.AppendText("Disconnecting Client " + idofclient + "\n");
                            if (game_running && playlist[index] && answered[index]) //if the player has already answered
                            {
                                AnswerAmount--;
                            }
                            clientIDs[index] = -1;   // Set client id as -1 which means exited
                            if (index < clientPoints.Count && clientPoints[index] > 0)
                            {
                                clientPoints[index] = 0; // Set client points as 0
                            }
                           
                            if (playlist[index]) //if player is actually playing the game (not waiting for the next game to start)
                            {
                               
                                playerCount--;
                                playlist[index] = false;
                                if (game_running && !answered[index] && AnswerAmount == playerCount)
                                {
                                    round_end();
                                }
                                
                            }
                            
                        }
                       
                        if (game_running && playerCount == 1) //if there is only one player remaining
                        {
                            Game_Ended();
                        }
                        thisClient.Close();
                    }

                    else if (incomingMessage.Length > 0) // If answer received
                    {
                        AnswerAmount++;
                        answered[getindex(idofclient)] = true;
                        clientAnswers[getindex(idofclient)] = Int32.Parse(incomingMessage); // Get answer as int
                        if (AnswerAmount < playerCount)
                        {
                            // Inform the other player that other one answered
                            for (int i = 0; i < clientIDs.Count; i++)
                            {
                                if (clientIDs[i] != idofclient && clientIDs[i] != -1 && playlist[i] && !answered[i])
                                {
                                    logs.AppendText(clientNames[i] + " has received the information that " + clientNames[getindex(idofclient)] + " answered the question \n");
                                    Byte[] announcebuffer = Encoding.Default.GetBytes(clientNames[getindex(idofclient)] + " has answered the question. Server is waiting for your answer");
                                    clientSockets[i].Send(announcebuffer);
                                }
                            }
                        }
                        else if (AnswerAmount == playerCount) //round ends
                        {
                            round_end();
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e.GetType().Name == "SocketException")
                    {
                        connected = false;
                    }
                    else
                    {
                        connected = false;
                        if (!terminating)
                        {
                            logs.AppendText("A client is started disconnecting \n");
                        }
                        int index = getindex(idofclient);
                        if (index != -1)
                        {
                            logs.AppendText("Disconnecting Client " + idofclient + "\n");
                            if (game_running && playlist[index] && answered[index])
                            {
                                AnswerAmount--;
                            }
                            clientIDs[index] = -1;   // Set client id as -1 which means exited
                            if (index < clientPoints.Count && clientPoints[index] > 0)
                            {
                                clientPoints[index] = 0; // Set client points as 0
                            }

                            if (playlist[index])
                            {

                                playerCount--;
                                if (game_running && !answered[index] && AnswerAmount == playerCount)
                                {
                                    round_end();
                                }
                                playlist[index] = false;
                            }

                        }

                        if (game_running && playerCount == 1)
                        {
                            Game_Ended();
                        }
                        thisClient.Close();
                    }
                }
            }
        }


        private void SendQuestion()
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
                for (int i = 0; i < clientIDs.Count; i++)
                {
                    if (clientIDs[i] != -1 && playlist[i])
                    {
                        logs.AppendText("Question \"" + Question + "\" to " + clientNames[i] + "\n");
                        Byte[] questionbuffer = Encoding.Default.GetBytes(Question);
                        clientSockets[i].Send(questionbuffer);
                    }
                }
            }
            else // If we reached the total questions amount
            {
                Game_Ended();
            }
        }
        private void Game_Ended() //a function to end the game
        {
            if (game_running)
            {
                if (listening)
                {
                    if (playerCount == 1) // If one user disconnected while game was running inform the other user
                    {
                        for (int i = 0; i < clientIDs.Count; i++)
                        {
                            if (clientIDs[i] != -1 && playlist[i])
                            {
                                logs.AppendText("Client " + clientIDs[i] + " won!! \n");
                                String Scores = Scoreboard();
                                logs.AppendText(Scores + "\n");

                                Byte[] resultbuffer = Encoding.Default.GetBytes("Other players Disconnected " + Scores + " Game ended You Won !!");

                                clientSockets[i].Send(resultbuffer);

                                break;
                            }
                        }
                    }
                    else
                    {
                        SendResults();
                    }
                }
               
            }
            for (int i = 0; i < clientIDs.Count; i++)
            {
                playlist[i] = false;

            }
            game_running = false;
            QuestionAmountBox.Enabled = true;
            startgame.Enabled = true;
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e) //closing the application
        {
            listening = false;
            terminating = true;
            logs.AppendText("Disconnecting server.\n");
            for (int i = 0; i < clientIDs.Count; i++)
            {
                if (clientIDs[i] != -1)
                {
                    Byte[] server = Encoding.Default.GetBytes("Server disconnecting");
                    clientSockets[i].Send(server);
                }
            }
            for (int i = 0; i < clientIDs.Count; i++)
            {
                if (clientIDs[i] != -1)
                {

                    clientSockets[i].Close();
                }
            }
            serverSocket.Close();
            game_running = false;
            Environment.Exit(0);
        }

        private void button1_Click(object sender, EventArgs e) //disconnect button
        {
            listening = false;
            terminating = true;
            logs.AppendText("Disconnecting server.\n");
            for (int i = 0; i < clientIDs.Count; i++)
            {
                if (clientIDs[i] != -1)
                {
                    Byte[] server = Encoding.Default.GetBytes("Server disconnecting");
                    clientSockets[i].Send(server);
                }
            }
            for (int i = 0; i < clientIDs.Count; i++)
            {
                if (clientIDs[i] != -1)
                {

                    clientSockets[i].Close();
                }
            }
            serverSocket.Close();
            game_running = false;
            ServerButton.Enabled = true;
            PortBox.Enabled = true;
            QuestionAmountBox.Enabled = true;
            button1.Enabled = false;
            startgame.Enabled = false;
        }

        private void startgame_Click(object sender, EventArgs e) //startgame button
        {
            if (Int32.TryParse(QuestionAmountBox.Text, out Question_Amount)) // Check if given Question Amount is integer
            {
                playingUserCount = 0;
                playerCount = 0;
                playlist.Clear();
                for (int i = 0; i < clientIDs.Count; i++)
                {
                    if (clientIDs[i] != -1)
                    {
                        playerCount++;
                        playingUserCount++;
                        playlist.Add(true);

                    }
                    else
                    {
                        playlist.Add(false);

                    }

                }
                if (playerCount >= 2)
                {
                    game_running = true;
                    
                    startgame.Enabled = false;
                    QuestionNumber = 0;
                    AnswerAmount = 0;
                    
                    clientPoints = new List<double>(new double[clientIDs.Count]);   // Keeps the Client Points
                    clientAnswers = new List<int>(new int[clientIDs.Count]);       // Keeps the Client Answers
                    answered = new List<bool>(new bool[clientIDs.Count]);
                    SortwithID();

                    SendQuestion();
                }
                else
                {
                    logs.AppendText("There is not enough players to start the game \n");
                }
            }

            else
            {
                logs.AppendText("Please check Question Amount \n");
            }
        }
        private void round_end() //a function to end the round
        {
            
            int Answer = Answers.ElementAt(QuestionNumber);
            int closest_difference = Int32.MaxValue;
            List<int> closest_answers = new List<int>();
            bool draw = false;
            //detect if there is a draw
            for (int i = 0; i < clientPoints.Count; i++)
            {
                if (clientIDs[i] != -1 && playlist[i])
                {
                    answered[i] = false;
                    if (Math.Abs(clientAnswers[i] - Answer) < closest_difference)
                    {
                        draw = false;
                        closest_difference = Math.Abs(clientAnswers[i] - Answer);
                        closest_answers.Clear();
                        closest_answers.Add(clientAnswers[i]);
                    }
                    else if (Math.Abs(clientAnswers[i] - Answer) == closest_difference)
                    {
                        draw = true;
                        closest_answers.Add(clientAnswers[i]);
                    }
                }
            }
            //calculate the points to give to the players
            for (int i = 0; i < clientPoints.Count; i++)
            {
                if (clientIDs[i] != -1 && playlist[i])
                {
                    if (draw && closest_answers.Contains(clientAnswers[i]))
                    {
                        clientPoints[i] = clientPoints[i] + Math.Round((double)1 / closest_answers.Count, 2);
                    }
                    else if (closest_answers.Contains(clientAnswers[i]))
                    {
                        clientPoints[i] = clientPoints[i] + 1;
                    }
                }
            }

            String Scores = Scoreboard();
            logs.AppendText(Scores + "\n");

            for (int i = 0; i < clientAnswers.Count; i++) // Inform the players
            {
                if (clientIDs[i] != -1 && playlist[i])
                {
                    Byte[] answersbuffer = Encoding.Default.GetBytes(CorrectAnswer(Answer));
                    clientSockets[i].Send(answersbuffer);
                }
            }
            for (int i = 0; i < clientAnswers.Count; i++) // Inform the players
            {
                if (clientIDs[i] != -1 && playlist[i])
                {
                    Byte[] resultbuffer = Encoding.Default.GetBytes(Scores);
                    clientSockets[i].Send(resultbuffer);
                }
            }
            AnswerAmount = 0;
            QuestionNumber++;
            // If game not ended Send new Question
            SendQuestion();

            
        }
    }
}
