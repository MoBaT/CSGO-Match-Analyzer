using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.CSGO.Internal;
using SteamKit2.GC.Internal;
using SteamKit2.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MatchAnalyzer
{
    public static class CSGOApp
    {
        static bool clientLaunched = false;
        static bool clientWelcome = false;
        static bool clientHello = false;

        public static Action<IPacketGCMsg> CSGOMatchHistoryCallback;
        public static Action CSGOClientLaunchedCallback;
        public static Action CSGOClientWelcomeCallback;
        public static Action CSGOClientHelloCallback;

        public static void LaunchClient()
        {
            new Callback<SteamGameCoordinator.MessageCallback>(OnGCMessage, SteamApp.manager);

            var playGame = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);

            playGame.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
            {
                game_id = new GameID(730), // or game_id = APPID,
            });

            // send it off
            // notice here we're sending this message directly using the SteamClient
            SteamApp.steamClient.Send(playGame);
            Thread.Sleep(3000);
            clientLaunched = true;
            CSGOClientLaunchedCallback.SafeInvoke();

            var clientHello = new ClientGCMsgProtobuf<CMsgClientHello>((uint)EGCBaseClientMsg.k_EMsgGCClientHello);
            SteamApp.steamGameCoordinator.Send(clientHello, 730);
        }

        static void OnGCMessage(SteamGameCoordinator.MessageCallback callback)
        {
            // setup our dispatch table for messages
            // this makes the code cleaner and easier to maintain
            var messageMap = new Dictionary<uint, Action<IPacketGCMsg>>
            {
                { ( uint )ECsgoGCMsg.k_EMsgGCCStrike15_v2_MatchList, OnCSGOMatchDetails },
                { ( uint )EGCBaseClientMsg.k_EMsgGCClientWelcome, OnClientWelcome }
            };

            Action<IPacketGCMsg> func;
            if (!messageMap.TryGetValue(callback.EMsg, out func))
            {
                // this will happen when we recieve some GC messages that we're not handling
                // this is okay because we're handling every essential message, and the rest can be ignored
                return;
            }

            func(callback.Message);
        }

        static void OnClientWelcome(IPacketGCMsg packetMsg)
        {
            var msg = new ClientGCMsgProtobuf<CMsgClientWelcome>(packetMsg);
            clientWelcome = true;
            CSGOClientWelcomeCallback.SafeInvoke();
        }

        static void OnCSGOMatchDetails(IPacketGCMsg packetMsg)
        {
            var msg = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_MatchList>(packetMsg);
            CSGOMatchHistoryCallback.SafeInvoke(packetMsg);
            //Stream stream = Helpers.GetStreamFromUrl(msg.Body.matches[0].roundstats.map);
        }

        public static void RequestMatchHistory()
        {
            var requestMatch = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_MatchListRequestRecentUserGames>((uint)ECsgoGCMsg.k_EMsgGCCStrike15_v2_MatchListRequestRecentUserGames);
            requestMatch.Body.accountid = 39487429;

            SteamApp.steamGameCoordinator.Send(requestMatch, 730);
        }

        public static void ClearEvents()
        {
            CSGOMatchHistoryCallback = null;
            CSGOClientLaunchedCallback = null;
            CSGOClientWelcomeCallback = null;
            CSGOClientHelloCallback = null;
        }
    }
}
