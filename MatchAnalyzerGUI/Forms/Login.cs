using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.CSGO.Internal;
using SteamKit2.GC.Internal;
using SteamKit2.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
using MatchAnalyzer.Analyzer;

namespace MatchAnalyzer
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread analyzeThread = new Thread(() => ReadFile());
            analyzeThread.Start();

            //SteamApp.SteamLoggedOnCallback = LoggedOnEvent;
            //SteamApp.Need2FactorCallback = TwoFactorEvent;
            //SteamApp.NeedAuthCodeCallback = AuthCodeEvent;
            //Thread myNewThread = new Thread(() => SteamApp.Login(textBox1.Text, textBox2.Text));
            //myNewThread.Start();
        }
        void ReadFile()
        {
            ReplayAnalyzer replayAnalyzer = new ReplayAnalyzer(File.OpenRead("demo.dem"));
            replayAnalyzer.Start();


            bool breakpoint = true;
            //Set Breakpoint to Read Replay Analyze Class
        }  



        void TwoFactorEvent()
        {
            MessageBox.Show("Please enter the two factor authentication code sent to your email in the 3rd box!");
            SteamApp.twoFactorAuth = textBox3.Text;
        }
        void AuthCodeEvent()
        {
            MessageBox.Show("Please enter the authentication code sent to your email in the 3rd box!");
            SteamApp.authCode = textBox3.Text;
        }
        void LoggedOnEvent()
        {
            CSGOApp.CSGOMatchHistoryCallback = OnCSGOMatchDetails;
            CSGOApp.CSGOClientWelcomeCallback = CSGOWelcomeEvent;

            Thread myNewThread2 = new Thread(() => CSGOApp.LaunchClient());
            myNewThread2.Start();
        }
        void CSGOWelcomeEvent()
        {
            Thread myNewThread2 = new Thread(() => CSGOApp.RequestMatchHistory());
            myNewThread2.Start();
        }
        void OnCSGOMatchDetails(IPacketGCMsg packetMsg)
        {
            var msg = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_MatchList>(packetMsg);
            
            List<ReplayAnalyzer> matchAnalyzes = new List<ReplayAnalyzer>();
            foreach(var match in msg.Body.matches)
            {
                matchAnalyzes.Add(new ReplayAnalyzer(match));
            }

            matchAnalyzes[0].Start();
        }    
    }

}
