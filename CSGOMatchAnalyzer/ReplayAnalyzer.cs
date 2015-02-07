using DemoInfo;
using ICSharpCode.SharpZipLib.BZip2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;
using SteamKit2.GC.CSGO.Internal;
using MatchAnalyzer.Analyzer.Models;

namespace MatchAnalyzer.Analyzer
{
    public class ReplayAnalyzer
    {
        private DemoParser parser;
        private Match MatchData;
        private CDataGCCStrike15_v2_MatchInfo _matchInformation;
        private Stream replayStream;
        private bool replayCompressed;

        private Round CurrentRound { get { return MatchData.Rounds.Last(); } }
        private long lastBombPlanter = 0;

        public CDataGCCStrike15_v2_MatchInfo MatchInformation { get { return _matchInformation; } }

        public ReplayAnalyzer(CDataGCCStrike15_v2_MatchInfo matchInformation)
        {
            _matchInformation = matchInformation;
            this.replayCompressed = true;
        }

        public ReplayAnalyzer(Stream replayStream, bool isCompressed = false)
        {
            this.replayStream = replayStream;
            this.replayCompressed = isCompressed;
        }

        public void Start()
        {
            MatchData = new Match();

            using (var compressedStream = _matchInformation != null ? Helpers.GetStreamFromUrl(_matchInformation.roundstats.map) : replayStream)
            using (var outputStream = new MemoryStream())
            {
                if (replayCompressed)
                {
                    BZip2.Decompress(compressedStream, outputStream, false);
                    outputStream.Seek(0L, SeekOrigin.Begin);
                    parser = new DemoParser(outputStream);
                }
                else
                    parser = new DemoParser(compressedStream);

                parser.MatchStarted += MatchStarted_Event;
                parser.ParseHeader();
                parser.ParseToEnd();

                MatchEnded_Event();
            }
        }

        void MatchEnded_Event()
        {
            MatchData.CTClanName = parser.CTClanName;
            MatchData.TClanName = parser.TClanName;
            MatchData.CTScore = parser.CTScore;
            MatchData.TScore = parser.TScore;
        }
        void MatchStarted_Event(object sender, MatchStartedEventArgs e)
        {
            parser.RoundStart += RoundStarted_Event;

            if (_matchInformation != null)
                MatchData.MatchNumber = _matchInformation.matchid;

            parser.Participants.Where(i => i.Name != "GOTV").ToList().ForEach(i => MatchData.Players.Add(i.SteamID, new Models.Player { SteamId = i.SteamID, Name = i.Name, Team = (i.Team == DemoInfo.Team.Terrorist) ? 0 : 1 }));
        }
        void RoundStarted_Event(object sender, RoundStartedEventArgs e)
        {
            if(!MatchData.Rounds.Any())
            {
                parser.PlayerKilled += PlayerKilled_Event;
                parser.WeaponFired += WeaponFired_Event;
                parser.SmokeNadeStarted += SmokeNadeStarted_Event;
                parser.SmokeNadeEnded += SmokeNadeEnded_Event;
                parser.DecoyNadeStarted += DecoyNadeStarted_Event;
                parser.DecoyNadeEnded += DecoyNadeEnded_Event;
                parser.FireNadeStarted += FireNadeStarted_Event;
                parser.FireNadeEnded += FireNadeEnded_Event;
                parser.FlashNadeExploded += FlashNadeExploded_Event;
                parser.ExplosiveNadeExploded += ExplosiveNadeExploded_Event;
                parser.NadeReachedTarget += NadeReachedTarget_Event;
                parser.BombBeginPlant += BombBeginPlant_Event;
                parser.BombAbortPlant += BombAbortPlant_Event;
                parser.BombPlanted += BombPlanted_Event;
                parser.BombDefused += BombDefused_Event;
                parser.BombExploded += BombExploded_Event;
                parser.BombBeginDefuse += BombBeginDefuse_Event;
                parser.BombAbortDefuse += BombAbortDefuse_Event;
            }

            MatchData.Rounds.Add(new Round { RoundNumber = MatchData.Rounds.Count() + 1 });
            parser.Participants.Where(i => i.Name != "GOTV").ToList().ForEach(i => CurrentRound.Players.Add(i.SteamID, new Models.RoundPlayer { Player = MatchData.Players[i.SteamID] }));
        }
        void PlayerKilled_Event(object sender, PlayerKilledEventArgs e)
        {
            if (MatchData.Players[e.DeathPerson.SteamID].Team == 0)
            {
                CurrentRound.Team1.PlayersKilled += 1;
                CurrentRound.Team2.PlayersDead += 1;
            }
            else
            {
                CurrentRound.Team2.PlayersKilled += 1;
                CurrentRound.Team1.PlayersDead += 1;
            }


        }
        void WeaponFired_Event(object sender, WeaponFiredEventArgs e)
        {
            if (e.Shooter != null)
            {
                if (MatchData.Players[e.Shooter.SteamID].Team == 0)
                    CurrentRound.Team1.WeaponsFired += 1;
                else
                    CurrentRound.Team2.WeaponsFired += 1;
            }
        }

        #region Grenade Start/End
        void SmokeNadeStarted_Event(object sender, SmokeEventArgs e)
        {
            if (e.ThrownBy != null)
            {
                CurrentRound.Players[e.ThrownBy.SteamID].PlayerRoundGrenade.Smokes.Add(new Grenade { StartTick = parser.CurrentTick, StartTime = parser.CurrentTime, Type = (int)e.NadeType });
               
                if(MatchData.Players[e.ThrownBy.SteamID].Team == 0)
                    CurrentRound.Team1.SmokesThrown += 1;
                else
                    CurrentRound.Team2.SmokesThrown += 1;
            }
        }
        void SmokeNadeEnded_Event(object sender, SmokeEventArgs e)
        {
            if (e.ThrownBy != null)
            {
                Grenade grenade = CurrentRound.Players[e.ThrownBy.SteamID].PlayerRoundGrenade.Smokes.Last();
                grenade.EndTick = parser.CurrentTick;
                grenade.EndTime = parser.CurrentTime;
            }
        }
        void DecoyNadeStarted_Event(object sender, DecoyEventArgs e)
        {
            if (e.ThrownBy != null)
            {
                CurrentRound.Players[e.ThrownBy.SteamID].PlayerRoundGrenade.Decoys.Add(new Grenade { StartTick = parser.CurrentTick, StartTime = parser.CurrentTime, Type = (int)e.NadeType });

                if (MatchData.Players[e.ThrownBy.SteamID].Team == 0)
                    CurrentRound.Team1.DecoysThrown += 1;
                else
                    CurrentRound.Team2.DecoysThrown += 1;
            }
        }
        void DecoyNadeEnded_Event(object sender, DecoyEventArgs e)
        {
            if (e.ThrownBy != null)
            {
                Grenade grenade = CurrentRound.Players[e.ThrownBy.SteamID].PlayerRoundGrenade.Decoys.Last();
                grenade.EndTick = parser.CurrentTick;
                grenade.EndTime = parser.CurrentTime;
            }
        }
        void FireNadeStarted_Event(object sender, FireEventArgs e)
        {
            if (e.ThrownBy != null)
            {
                CurrentRound.Players[e.ThrownBy.SteamID].PlayerRoundGrenade.Fires.Add(new Grenade { StartTick = parser.CurrentTick, StartTime = parser.CurrentTime, Type = (int)e.NadeType });

                if (MatchData.Players[e.ThrownBy.SteamID].Team == 0)
                    CurrentRound.Team1.FiresThrown += 1;
                else
                    CurrentRound.Team2.FiresThrown += 1;
            }
        }
        void FireNadeEnded_Event(object sender, FireEventArgs e)
        {
            if (e.ThrownBy != null)
            {
                Grenade grenade = CurrentRound.Players[e.ThrownBy.SteamID].PlayerRoundGrenade.Decoys.Last();
                grenade.EndTick = parser.CurrentTick;
                grenade.EndTime = parser.CurrentTime;
            }
        }

        void FlashNadeExploded_Event(object sender, FlashEventArgs e)
        {
            if (e.ThrownBy != null)
            {
                CurrentRound.Players[e.ThrownBy.SteamID].PlayerRoundGrenade.Flashes.Add(new FlashGrenade { StartTick = parser.CurrentTick, StartTime = parser.CurrentTime, Type = (int)e.NadeType, PlayersFlashed = e.FlashedPlayers.Where(i => i.SteamID != 0).ToList().Select(i => CurrentRound.Players[i.SteamID]).ToList() });

                if (MatchData.Players[e.ThrownBy.SteamID].Team == 0)
                    CurrentRound.Team1.FlashesThrown += 1;
                else
                    CurrentRound.Team2.FlashesThrown += 1;
            }
        }
        void ExplosiveNadeExploded_Event(object sender, GrenadeEventArgs e)
        {
            if (e.ThrownBy != null)
            {
                CurrentRound.Players[e.ThrownBy.SteamID].PlayerRoundGrenade.Grenades.Add(new Grenade { StartTick = parser.CurrentTick, StartTime = parser.CurrentTime, Type = (int)e.NadeType });

                if (MatchData.Players[e.ThrownBy.SteamID].Team == 0)
                    CurrentRound.Team1.GrenadesThrown += 1;
                else
                    CurrentRound.Team2.GrenadesThrown += 1;
            }
        }
        void NadeReachedTarget_Event(object sender, NadeEventArgs e)
        {

        }
        #endregion

        #region Bomb Events
        void BombBeginPlant_Event(object sender, BombEventArgs e)
        {
            CurrentRound.Players[e.Player.SteamID].BombPlantAttempts.Add(new RoundBombPlantAttempt { StartTick = parser.CurrentTick, StartTime = parser.CurrentTime, Site = e.Site, Player = CurrentRound.Players[e.Player.SteamID] });
        }
        void BombAbortPlant_Event(object sender, BombEventArgs e)
        {
            RoundBombPlantAttempt RoundBombAbortAttempt = CurrentRound.Players[e.Player.SteamID].BombPlantAttempts.LastOrDefault();
            RoundBombAbortAttempt.EndTick = parser.CurrentTick;
            RoundBombAbortAttempt.EndTime = parser.CurrentTime;
        }
        void BombPlanted_Event(object sender, BombEventArgs e)
        {
            RoundBombPlantAttempt RoundBombPlantAttempt = CurrentRound.Players[e.Player.SteamID].BombPlantAttempts.LastOrDefault();
            RoundBombPlantAttempt.EndTick = parser.CurrentTick;
            RoundBombPlantAttempt.EndTime = parser.CurrentTime;
            RoundBombPlantAttempt.BombPlant = new RoundBombPlant { StartTick = parser.CurrentTick, StartTime = parser.CurrentTime, Player = CurrentRound.Players[e.Player.SteamID] };
            lastBombPlanter = e.Player.SteamID;
        }

        void BombExploded_Event(object sender, BombEventArgs e)
        {
            RoundBombPlant RoundBombPlant = CurrentRound.Players[e.Player.SteamID].BombPlantAttempts.Last().BombPlant;
            RoundBombPlant.EndTick = parser.CurrentTick;
            RoundBombPlant.EndTime = parser.CurrentTime;
            RoundBombPlant.Exploded = true;
        }
        void BombBeginDefuse_Event(object sender, BombDefuseEventArgs e)
        {
            CurrentRound.Players[e.Player.SteamID].BombDefuseAttempts.Add(new RoundBombDefuseAttempt { StartTick = parser.CurrentTick, StartTime = parser.CurrentTime, Site = CurrentRound.Players[lastBombPlanter].BombPlantAttempts.Last().Site, HasKit = e.HasKit, Player = CurrentRound.Players[e.Player.SteamID] });
        }
        void BombAbortDefuse_Event(object sender, BombDefuseEventArgs e)
        {
            RoundBombDefuseAttempt RoundBombDefuseAttempt = CurrentRound.Players[e.Player.SteamID].BombDefuseAttempts.LastOrDefault();
            RoundBombDefuseAttempt.EndTick = parser.CurrentTick;
            RoundBombDefuseAttempt.EndTime = parser.CurrentTime;
        }

        void BombDefused_Event(object sender, BombEventArgs e)
        {
            RoundBombDefuseAttempt RoundBombDefused = CurrentRound.Players[e.Player.SteamID].BombDefuseAttempts.LastOrDefault();
            RoundBombDefused.EndTick = parser.CurrentTick;
            RoundBombDefused.EndTime = parser.CurrentTime;

            RoundBombPlant RoundBombPlant = CurrentRound.Players[lastBombPlanter].BombPlantAttempts.LastOrDefault().BombPlant;
            RoundBombPlant.EndTick = parser.CurrentTick;
            RoundBombPlant.EndTime = parser.CurrentTime;
            RoundBombPlant.Defused = true;

            CurrentRound.BombDefused = true;
        }
        #endregion
    }
}
