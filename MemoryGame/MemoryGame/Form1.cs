using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace MemoryGame
{
    public partial class MemoryGame : Form
    {
        List<string> godDescriptions;
        List<string> cardPicsReferences;
        List<Button> buttonsList = new List<Button>();
        List<Label> labelsList = new List<Label>();
        List<PictureBox> cardsPictureboxList = new List<PictureBox>();
        List<PictureBox> alreadyChosenCards = new List<PictureBox>();
        static PictureBox firstCardClicked;
        static PictureBox secondCardClicked;
        Random generator = new Random();
        int totalRoundCount = 0;
        int cardPairs = 16;
        int timerUserCount;
        double roundPerSecond;
        Button startButton;
        Button exitButton;
        Button newGameButton;
        Button backButton;
        Label instructionLabel;
        Label wonLabel;
        Label averageLabel;
        Label resultLabel;
        Label resultsLabel;
        Timer timerCards = new Timer();
        Timer timerUser;

        public MemoryGame()
        {
            InitializeComponent();
            
            cardPicsReferences = Directory.GetFiles("../../Pictures/Gods").ToList();
            godDescriptions = Directory.GetFiles("../../Pictures/Descriptions").ToList();

            foreach (Button button in this.Controls.OfType<Button>())
            {
                buttonsList.Add(button);
            }
            foreach (Label label in this.Controls.OfType<Label>())
            {
                labelsList.Add(label);
            }

            foreach (PictureBox card in this.Controls.OfType<PictureBox>())
            {
                cardsPictureboxList.Add(card);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.BackgroundImage = Image.FromFile("../../Pictures/getgreek.PNG");
            BackgroundImageLayout = ImageLayout.Stretch;
            
            foreach (Button b in buttonsList)
            {
                b.Visible = false;
            }
            foreach (Label l in labelsList)
            {
                l.Visible = false;
            }
            foreach (PictureBox pb in cardsPictureboxList)
            {
                pb.Visible = false;
            }

            Controls.Add(new Button()
            {
                Name = "StartButton",
                Left = 500,
                Top = 550,
                Width = 180,
                Height = 50,
                Font = new Font("Papyrus", 20, FontStyle.Bold),
                ForeColor = Color.Black,
                Enabled = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Get Greek!",
            });
            startButton = (Button)Controls.Find("StartButton", false)[0];
            startButton.Click += startButton_Click;
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            StartGame();
        }
        
        List<string> RandomizeList(List<string> list)
        {
            return list.OrderBy(item => generator.Next()).ToList();
        }

        void StartGame()
        {
            cardPicsReferences = RandomizeList(cardPicsReferences);

            this.BackgroundImage = Image.FromFile("../../Pictures/greekbackground.jpg");
            BackgroundImageLayout = ImageLayout.Stretch;

            startButton.Dispose();

            foreach (Label l in labelsList)
            {
                l.Visible = true;
            }
            foreach (PictureBox pb in cardsPictureboxList)
            {
                pb.Visible = true;
            }
            foreach (Button b in buttonsList)
            {
                b.Visible = false;
            }

            countPairsLabel.Text = cardPairs.ToString();

            alreadyChosenCards.Clear();
            cardPairs = 16;
            totalRoundCount = 0;
            roundPerSecond = 0;

            for (int i = 0; i < cardsPictureboxList.Count; i++)
            {
                cardsPictureboxList[i].Image = Image.FromFile(cardPicsReferences[i]);
                cardsPictureboxList[i].ImageLocation = @cardPicsReferences[i];

                Size imageSize = cardsPictureboxList[i].Image.Size;
                Size fitSize = cardsPictureboxList[i].ClientSize;
                cardsPictureboxList[i].SizeMode = imageSize.Width > fitSize.Width || imageSize.Height > fitSize.Height ?
                                                  PictureBoxSizeMode.Zoom : PictureBoxSizeMode.CenterImage;
            }
            CoverCards();

            timerUser = new Timer();
            timerUserCount = 0;
            timerUser.Interval = 1000;
            timerUser.Start();
            timerUser.Enabled = true;
            timerUser.Tick += timerUser_Tick;
        }

        void timerUser_Tick(object sender, EventArgs e)
        {
            timerUserCount += 1;
            timerLabel.Text = timerUserCount.ToString();
        }

        void CoverCards()
        {
            foreach(PictureBox pb in cardsPictureboxList.Except(alreadyChosenCards))
            {
                pb.Image = Image.FromFile("../../Pictures/qmark.png");
                pb.ImageLocation = @"../../Pictures/qmark.png";
            }

        }

        private static byte[] ImageToByteArray(Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }

        private static bool cardsAreSame(Image pb1, Image pb2)
        {
            byte[] pb1Array = ImageToByteArray(pb1);
            byte[] pb2Array = ImageToByteArray(pb2);

            if(pb1Array.SequenceEqual(pb2Array) == true)
            {
                return true;
            }
            return false;
        }
        
        private void card_Click(object sender, EventArgs e)
        {
            if (sender.GetType().Name == "PictureBox")
            {
                if (timerCards.Enabled == true)
                {
                    return;
                }

                PictureBox pb = (PictureBox)sender;

                if (pb.ImageLocation == @"../../Pictures/qmark.png")
                { 
                    int indexPictureBox = cardsPictureboxList.IndexOf(pb);
                    pb.Image = Image.FromFile(cardPicsReferences[indexPictureBox]);
                    pb.ImageLocation = @cardPicsReferences[indexPictureBox];

                    if (firstCardClicked == null)
                    {
                        firstCardClicked = pb;
                        return;
                    }
                    secondCardClicked = pb;

                    if (cardsAreSame(firstCardClicked.Image, secondCardClicked.Image))
                    {
                        cardPairs--;
                        countPairsLabel.Text = cardPairs.ToString();

                        totalRoundCount++;
                        countRoundsLabel.Text = totalRoundCount.ToString();

                        alreadyChosenCards.Add(firstCardClicked);
                        alreadyChosenCards.Add(secondCardClicked);

                        firstCardClicked = null;
                        secondCardClicked = null;

                        if (cardPairs == 0)
                        {
                            timerUser.Stop();
                            timerUser.Enabled = false;
                            timerUser.Dispose();

                            WinGame();
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        totalRoundCount++;
                        countRoundsLabel.Text = totalRoundCount.ToString();

                        timerCards.Start();
                        timerCards.Enabled = true;
                        timerCards.Interval = 1000;
                        timerCards.Tick += coverCardBack;
                    }

                }
                else
                {
                    MessageBox.Show("Card already revealed, choose another card!");
                }
            }
        }
        
        private void coverCardBack(object sender, EventArgs e)
        {
            timerCards.Stop();
            timerCards.Enabled = false;

            CoverCards();

            firstCardClicked = null;
            secondCardClicked = null;
        }
        
        void WinGame()
        {
            timerCards.Dispose();

            this.BackgroundImage = Image.FromFile("../../Pictures/olympusbackground.jpg");
            BackgroundImageLayout = ImageLayout.Stretch;

            foreach (Label l in labelsList)
            {
                l.Visible = false;
            }
            foreach (PictureBox pb in cardsPictureboxList)
            {
                pb.Visible = false;
            }
            foreach (Button b in buttonsList)
            {
                b.Visible = true;
            }

            Controls.Add(new Label()
            {
                Name = "YouWonLabel",
                Left = 220,
                Top = 10,
                Width = 700,
                Height = 50,
                Font = new Font("Papyrus", 22, FontStyle.Bold),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Enabled = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Congratulations, you finished the Memory Game!",
            });
            wonLabel = (Label)Controls.Find("YouWonLabel", false)[0];

            Controls.Add(new Label()
            {
                Name = "AverageLabel",
                Left = 280,
                Top = 60,
                Width = 530,
                Height = 50,
                Font = new Font("Papyrus", 18, FontStyle.Bold),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Enabled = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Your average time in seconds to find a pair is : ",
            });
            averageLabel = (Label)Controls.Find("AverageLabel", false)[0];

            roundPerSecond = timerUserCount / totalRoundCount;

            Controls.Add(new Label()
            {
                Name = "ResultLabel",
                Left = 801,
                Top = 60,
                Width = 200,
                Height = 50,
                Font = new Font("Papyrus", 18, FontStyle.Bold),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Enabled = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = roundPerSecond.ToString(),
            });
            resultLabel = (Label)Controls.Find("ResultLabel", false)[0];

            string results = "(Total Rounds : " + totalRoundCount + " / Total Seconds : " + timerUserCount + " / Total Minutes : " + timerUserCount / 60 + ")";
            Controls.Add(new Label()
            {
                Name = "ResultsLabel",
                Left = 205,
                Top = 105,
                Width = 800,
                Height = 50,
                Font = new Font("Papyrus", 18, FontStyle.Bold),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Enabled = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = results,
            });
            resultsLabel = (Label)Controls.Find("ResultsLabel", false)[0];

            Controls.Add(new Label()
            {
                Name = "InstructionLabel",
                Left = 170,
                Top = 145,
                Width = 800,
                Height = 100,
                Font = new Font("Papyrus", 18, FontStyle.Bold),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Enabled = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Now you can have a look at each god and goddess a little bit closer,\njust click on the button of their names!",
            });
            instructionLabel = (Label)Controls.Find("InstructionLabel", false)[0];

            Controls.Add(new Button()
            {
                Name = "ExitButton",
                Left = 410,
                Top = 580,
                Width = 150,
                Height = 50,
                Font = new Font("Papyrus", 14, FontStyle.Bold),
                ForeColor = Color.Black,
                Enabled = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Exit",
            });
            exitButton = (Button)Controls.Find("ExitButton", false)[0];
            buttonsList.Add(exitButton);
            exitButton.Click += exitButton_Click;

            Controls.Add(new Button()
            {
                Name = "NewGameButton",
                Left = 570,
                Top = 580,
                Width = 150,
                Height = 50,
                Font = new Font("Papyrus", 14, FontStyle.Bold),
                ForeColor = Color.Black,
                Enabled = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "New Game",
            }); 
            newGameButton = (Button)Controls.Find("NewGameButton", false)[0];
            buttonsList.Add(newGameButton);
            newGameButton.Click += newGameButton_Click;
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void newGameButton_Click(object sender, EventArgs e)
        {
            wonLabel.Dispose();
            instructionLabel.Dispose();
            exitButton.Dispose();
            newGameButton.Dispose();
            averageLabel.Dispose();
            resultLabel.Dispose();
            resultsLabel.Dispose();

            StartGame();
            totalRoundCount = 0;
        }

        private void godButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            foreach (string description in godDescriptions)
            {
                if(description.Contains(button.Name))
                {
                    this.BackgroundImage = Image.FromFile(description);
                    BackgroundImageLayout = ImageLayout.Stretch;
                }
            }

            foreach (Label l in labelsList)
            {
                l.Visible = false;
            }
            foreach (PictureBox pb in cardsPictureboxList)
            {
                pb.Visible = false;
            }
            foreach (Button b in buttonsList)
            {
                b.Visible = false;
            }

            wonLabel.Dispose();
            instructionLabel.Dispose();
            exitButton.Dispose();
            newGameButton.Dispose();
            averageLabel.Dispose();
            resultLabel.Dispose();
            resultsLabel.Dispose();

            Controls.Add(new Button()
            {
                Name = "BackButton",
                Left = 1085,
                Top = 5,
                Width = 65,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Papyrus", 12, FontStyle.Bold),
                ForeColor = Color.Black,
                Enabled = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Back",
            });
            backButton = (Button)Controls.Find("BackButton", false)[0];
            backButton.Click += backButton_Click;
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            backButton.Dispose();
            WinGame();
        }
    }
}
