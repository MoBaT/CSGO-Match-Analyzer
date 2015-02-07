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
    public static class SteamApp
    {
        public static SteamClient steamClient;
        public static CallbackManager manager;
        public static SteamUser steamUser;
        public static SteamGameCoordinator steamGameCoordinator;
        public static SteamFriends steamFriends;

        public static string authCode, twoFactorAuth;

        public static bool isRunning = false;
        public static bool welcomeMessage = false;
        static string user, pass;

        public static Action SteamLoggedOnCallback;
        public static Action SteamFailedCallback;
        public static Action SteamDisconnectCallback;
        public static Action SteamLoggedOffCallback;
        public static Action Need2FactorCallback;
        public static Action NeedAuthCodeCallback;

        public static void Login(string username, string password)
        {
            // save our logon details
            user = username;
            pass = password;
            isRunning = true;

            // create our steamclient instance
            steamClient = new SteamClient();
            // create the callback manager which will route callbacks to function calls
            manager = new CallbackManager(steamClient);
            // get the steamuser handler, which is used for logging on after successfully connecting
            steamUser = steamClient.GetHandler<SteamUser>();
            steamGameCoordinator = steamClient.GetHandler<SteamGameCoordinator>();
            steamFriends = steamClient.GetHandler<SteamFriends>();

            // register a few callbacks we're interested in
            // these are registered upon creation to a callback manager, which will then route the callbacks
            // to the functions specified
            new Callback<SteamClient.ConnectedCallback>(OnConnected, manager);
            new Callback<SteamClient.DisconnectedCallback>(OnDisconnected, manager);

            new Callback<SteamUser.LoggedOnCallback>(OnLoggedOn, manager);
            new Callback<SteamUser.LoggedOffCallback>(OnLoggedOff, manager);

            // this callback is triggered when the steam servers wish for the client to store the sentry file
            new Callback<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth, manager);
            new Callback<SteamUser.AccountInfoCallback>(OnAccountInfo, manager);

            Console.WriteLine("Connecting to Steam...");

            steamClient.Connect();

            while (isRunning)
            {
                // in order for the callbacks to get routed, they need to be handled by the manager
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }

        static void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                Console.WriteLine("Unable to connect to Steam: {0}", callback.Result);

                isRunning = false;
                SteamFailedCallback.SafeInvoke();
                return;
            }

            Console.WriteLine("Connected to Steam! Logging in '{0}'...", user);

            byte[] sentryHash = null;
            if (File.Exists("sentry.bin"))
            {
                // if we have a saved sentry file, read and sha-1 hash it
                byte[] sentryFile = File.ReadAllBytes("sentry.bin");
                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = user,
                Password = pass,

                // in this sample, we pass in an additional authcode
                // this value will be null (which is the default) for our first logon attempt
                AuthCode = authCode,

                // if the account is using 2-factor auth, we'll provide the two factor code instead
                // this will also be null on our first logon attempt
                TwoFactorCode = twoFactorAuth,

                // our subsequent logons use the hash of the sentry file as proof of ownership of the file
                // this will also be null for our first (no authcode) and second (authcode only) logon attempts
                SentryFileHash = sentryHash,
            });
        }

        static void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            // after recieving an AccountLogonDenied, we'll be disconnected from steam
            // so after we read an authcode from the user, we need to reconnect to begin the logon flow again

            Console.WriteLine("Disconnected from Steam, reconnecting in 5...");

            SteamDisconnectCallback.SafeInvoke();

            Thread.Sleep(TimeSpan.FromSeconds(5));

            steamClient.Connect();
        }

        static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            bool isSteamGuard = callback.Result == EResult.AccountLogonDenied;
            bool is2FA = callback.Result == EResult.AccountLogonDeniedNeedTwoFactorCode;

            if (isSteamGuard || is2FA)
            {
                Console.WriteLine("This account is SteamGuard protected!");

                if (is2FA)
                {
                    Console.Write("Please enter your 2 factor auth code from your authenticator app: ");
                    Need2FactorCallback.SafeInvoke();
                }
                else
                {
                    Console.Write("Please enter the auth code sent to the email at {0}: ", callback.EmailDomain);
                    NeedAuthCodeCallback.SafeInvoke();
                }

                return;
            }

            if (callback.Result != EResult.OK)
            {
                Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);

                isRunning = false;
                SteamFailedCallback.SafeInvoke();
                return;
            }


            SteamLoggedOnCallback.SafeInvoke();
            Console.WriteLine("Successfully logged on!");
        }

        static void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Console.WriteLine("Logged off of Steam: {0}", callback.Result);
            SteamLoggedOffCallback.SafeInvoke();
        }

        static void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            Console.WriteLine("Updating sentryfile...");

            byte[] sentryHash = CryptoHelper.SHAHash(callback.Data);

            // write out our sentry file
            // ideally we'd want to write to the filename specified in the callback
            // but then this sample would require more code to find the correct sentry file to read during logon
            // for the sake of simplicity, we'll just use "sentry.bin"
            File.WriteAllBytes("sentry.bin", callback.Data);

            // inform the steam servers that we're accepting this sentry file
            steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,

                FileName = callback.FileName,

                BytesWritten = callback.BytesToWrite,
                FileSize = callback.Data.Length,
                Offset = callback.Offset,

                Result = EResult.OK,
                LastError = 0,

                OneTimePassword = callback.OneTimePassword,

                SentryFileHash = sentryHash,
            });

            Console.WriteLine("Done!");
        }

        static void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            steamFriends.SetPersonaState(EPersonaState.Offline);
        }

        static void ClearEvents()
        {
            SteamLoggedOnCallback = null;
            SteamFailedCallback = null;
            SteamDisconnectCallback = null;
            SteamLoggedOffCallback = null;
            Need2FactorCallback = null;
            NeedAuthCodeCallback = null;
        }
    }
}
