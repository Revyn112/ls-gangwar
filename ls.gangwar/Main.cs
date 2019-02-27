using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;

namespace ls.gangwar
{
    public class Main : Resource
    {
        private static readonly Random Random = new Random();

        private static readonly List<Team> Teams = new List<Team>
        {
            new Team("ballas", new List<string> {"chino2", "buccaneer2", "buccaneer", "faction"},
                new List<Position>
                {
                    new Position(88.6285f, -1959.3890f, 20.7370f),
                    new Position(109.3054f, -1955.8022f, 20.7370f),
                    new Position(117.7318f, -1947.7583f, 20.7200f),
                    new Position(118.9186f, -1934.2681f, 20.7707f),
                    new Position(105.7318f, -1923.4154f, 20.7370f)
                },
                new Position(84.9890f, -1958.6241f, 21.1076f),
                new Position(105.7186f, -1941.5867f, 20.7875f),
                new Rgba(196, 0, 171, 150), "C400AB"),
            new Team("families", new List<string> {"faction2", "blade", "gauntlet", "impaler"},
                new List<Position>
                {
                    new Position(-196.4439f, -1607.0505f, 34.1494f),
                    new Position(-174.3560f, -1609.9780f, 33.7281f),
                    new Position(-175.0681f, -1623.1647f, 33.5596f),
                    new Position(-191.1692f, -1641.4813f, 33.4080f),
                    new Position(-183.5736f, -1587.5999f, 34.8234f)
                },
                new Position(-210.7648f, -1606.8132f, 34.8571f),
                new Position(-183.5736f, -1587.5999f, 34.8234f),
                new Rgba(0, 127, 0, 150), "008000"),
            new Team("vagos", new List<string> {"hermes", "ellie", "chino", "dukes"},
                new List<Position>
                {
                    new Position(334.6681f, -2052.6726f, 20.8212f),
                    new Position(341.7890f, -2051.3669f, 21.3267f),
                    new Position(345.7582f, -2044.6812f, 21.6300f),
                    new Position(342.3955f, -2040.3560f, 21.5626f),
                    new Position(351.2835f, -2043.2043f, 22.0007f)
                },
                new Position(84.9890f, -1958.6241f, 21.1076f),
                new Position(105.7186f, -1941.5867f, 20.7875f),
                new Rgba(255, 191, 0, 150), "FFBF00")
        };

        private static readonly List<Turf> Turfs = new List<Turf>();

        private static Turf _currentTurf;

        private Chat chat;

        static Main()
        {
            const float xStartTurf = -404.1889f;
            const float yStartTurf = -1221.2967f;

            for (var i = 0; i < 5; ++i)
            {
                for (var j = 0; j < 5; ++j)
                {
                    var x1 = xStartTurf + 200 * i;
                    var y1 = yStartTurf - 200 * j;
                    Turfs.Add(new Turf(x1, y1, x1 + 200, y1 - 200));
                }
            }
        }

        public override void OnStart()
        {
            Alt.OnPlayerConnect += OnPlayerConnect;
            Alt.OnPlayerDisconnect += OnPlayerDisconnect;
            Alt.OnCheckpoint += OnCheckpoint;
            Alt.OnPlayerDead += OnPlayerDead;
            Alt.On<IPlayer, ulong>("teamSelected", (player, teamId) =>
            {
                teamId--;
                player.SetMetaData("team", teamId);
                player.SetMetaData("selectingTeam", false);
                BroadcastTeamsPopulation();

                var team = Teams[(int) teamId];

                chat.Broadcast("{5555AA}" + player.Name + "{FFFFFF}joined " + team.Hex +
                               team.Name);

                player.Position = team.GetRandomSpawn();
                player.Emit("applyAppearance", team.Name);
                player.Emit("updateTeam", team.Name);

                if (_currentTurf != null)
                {
                    player.Emit("captureStateChanged", true);
                    Alt.EmitAllClients("startCapture", new Dictionary<string, float>
                    {
                        {"x1", Math.Min(_currentTurf.X1, _currentTurf.X2)},
                        {"y1", Math.Min(_currentTurf.Y1, _currentTurf.Y2)},
                        {"x2", Math.Min(_currentTurf.X1, _currentTurf.X2)},
                        {"y2", Math.Min(_currentTurf.Y1, _currentTurf.Y2)}
                    });
                    player.Emit("updateTeamPoints", new Dictionary<string, ulong>
                    {
                        {"ballas", Teams[0].TurfPoints},
                        {"families", Teams[1].TurfPoints},
                        {"vagos", Teams[2].TurfPoints}
                    });
                }
            });
            Alt.On("action", (player, args) =>
            {
                if (!player.GetMetaData("checkpoint", out ulong cp))
                {
                    return;
                }

                if (cp == 1)
                {
                    if (!player.GetMetaData("team", out ulong pTeam))
                    {
                        return;
                    }

                    var pos = player.Position;
                    if (player.GetMetaData("vehicle", out IVehicle vehicle))
                    {
                        vehicle.Remove();
                    }

                    var team = Teams[(int) pTeam];

                    var nextModel = team.GetRandomVehicle();
                    var vehColor = team.Color;
                    var curVeh = Alt.CreateVehicle(Alt.Hash(nextModel), pos, 0);
                    curVeh.PrimaryColorRgb = vehColor;
                    curVeh.SecondaryColorRgb = vehColor;

                    Task.Delay(200).ContinueWith(t => { player.Emit("setintoveh", curVeh); });

                    player.SetMetaData("vehicle", curVeh);
                }
                else if (cp == 2)
                {
                    player.Emit("giveAllWeapons");
                }
            });
            Alt.On("viewLoaded", (player, args) => { player.Emit("showTeamSelect", GetTeamsPopulation()); });

            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(1);

            var timer = new System.Threading.Timer((e) =>
            {
                var players = Alt.GetAllPlayers();
                if (players.Count > 0)
                {
                    if (_currentTurf == null)
                    {
                        StartCapture();
                    }
                    else
                    {
                        foreach (var p in players)
                        {
                            if (!p.GetMetaData("team", out ulong pTeamIndex))
                            {
                                continue;
                            }

                            var pTeam = Teams[(int) pTeamIndex];

                            if (_currentTurf.Contains(p.Position.X, p.Position.Y))
                            {
                                pTeam.TurfPoints++;
                                if (pTeam.TurfPoints >= 1000)
                                {
                                    chat.Broadcast("{" + pTeam.Hex + "}" + pTeam.Name +
                                                   " {FFFFFF}got this turf. Next capture started");
                                    StopCapture();
                                    return;
                                }
                            }
                        }

                        Alt.EmitAllClients("updateTeamPoints", new Dictionary<string, ulong>
                        {
                            {"ballas", Teams[0].TurfPoints},
                            {"families", Teams[1].TurfPoints},
                            {"vagos", Teams[2].TurfPoints}
                        });
                    }
                }
                else if (_currentTurf != null)
                {
                    StopCapture();
                }
            }, null, startTimeSpan, periodTimeSpan);

            chat = new Chat();
        }

        public override void OnStop()
        {
        }

        public void OnPlayerConnect(IPlayer player, string reason)
        {
            player.SetMetaData("selectingTeam", true);
            player.SetMetaData("checkpoint", 0);
            player.SetMetaData("vehicle", null);

            chat.Broadcast("{5555AA}" + player.Name + "{FFFFFF}connected");
        }

        public void OnPlayerDisconnect(IPlayer player, string reason)
        {
            if (player.GetMetaData("vehicle", out IVehicle vehicle))
            {
                vehicle.Remove();
            }

            player.SetMetaData("selectingTeam", false);

            BroadcastTeamsPopulation();

            chat.Broadcast("{5555AA}" + player.Name + "{FFFFFF}disconnected");
        }

        public void OnCheckpoint(ICheckpoint checkpoint, IEntity entity, bool state)
        {
            if (state)
            {
                if (entity is IPlayer player)
                {
                    if (entity.GetMetaData("team", out ulong teamIndex))
                    {
                        var team = Teams[(int) teamIndex];
                        if (checkpoint == team.VehicleCheckpoint)
                        {
                            entity.SetMetaData("checkpoint", 1);
                            player.Emit("showInfo", "~INPUT_PICKUP~ to get car");
                        }
                        else if (checkpoint == team.WeaponCheckpoint)
                        {
                            entity.SetMetaData("checkpoint", 2);
                            player.Emit("showInfo", "INPUT_PICKUP~ to get weapons and ammo");
                        }
                    }
                }
            }
            else
            {
                if (entity is IPlayer)
                {
                    entity.SetMetaData("checkpoint", 0);
                }
            }
        }

        public void OnPlayerDead(IPlayer player, IEntity killer, uint weapon)
        {
            var weaponName = WeaponHelper.GetWeaponTranslation(weapon);

            if (player == killer && weaponName == "Killed")
                weaponName = "Suicided";
            else if (weaponName == "Killed")
                Alt.Log("Unknown death reason: " + weapon);

            if (!player.GetMetaData("team", out ulong teamIndex)) return;
            if (killer != null)
            {
                if (!killer.GetMetaData("team", out ulong killerTeamIndex)) return;
                var killerTeam = Teams[(int) killerTeamIndex];
                if (!(killer is IPlayer killerPlayer)) return;
                Alt.EmitAllClients("playerKill", new Dictionary<string, string>
                {
                    {"killerName", killerPlayer.Name},
                    {"killerGang", killerTeam.Name},
                    {"victimName", player.Name},
                    {"victimGang", Teams[(int) teamIndex].Name},
                    {"weapon", weaponName}
                });

                if (_currentTurf != null && killer != player && teamIndex != killerTeamIndex)
                {
                    if (_currentTurf.Contains(player.Position.X, player.Position.Y))
                    {
                        killerTeam.TurfPoints += 50;
                        if (killerTeam.TurfPoints >= 1000)
                        {
                            chat.Broadcast("{" + killerTeam.Hex + "} " + killerTeam.Name +
                                           "{FFFFFF}got this turf. Next capture started");
                            StopCapture();
                        }
                    }
                }
                else if (_currentTurf != null && teamIndex == killerTeamIndex)
                {
                    if (_currentTurf.Contains(player.Position.X, player.Position.Y))
                    {
                        if (killerTeam.TurfPoints > 50)
                            killerTeam.TurfPoints -= 50;
                        else
                            killerTeam.TurfPoints = 0;
                    }
                }
            }

            Task.Delay(5000).ContinueWith(t =>
            {
                if (!player.GetMetaData("team", out ulong deadTeam)) return;
                var nextSpawn = Teams[(int) deadTeam].GetRandomSpawn();
                Alt.Log("Trying to respawn " + player.Name);
                player.Position = nextSpawn;
            });
        }

        public Dictionary<string, ulong> GetTeamsPopulation()
        {
            var population = new List<ulong>
            {
                0,
                0,
                0
            };
            foreach (var player in Alt.GetAllPlayers())
            {
                if (player.GetMetaData("team", out ulong team))
                {
                    population[(int) team]++;
                }
            }

            return new Dictionary<string, ulong>
            {
                {"ballas", population[0]},
                {"families", population[1]},
                {"vagos", population[2]}
            };
        }

        public void BroadcastTeamsPopulation()
        {
            foreach (var player in Alt.GetAllPlayers())
            {
                if (player.GetMetaData("selectingTeam", out bool selectingTeam) && selectingTeam)
                {
                    player.Emit("showTeamSelect", GetTeamsPopulation());
                }
            }
        }

        public void StartCapture()
        {
            foreach (var team in Teams)
            {
                team.ResetTurfPoints();
            }

            _currentTurf = GetRandomTurf();
            Alt.EmitAllClients("captureStateChanged", true);
            Alt.EmitAllClients("startCapture", new Dictionary<string, float>
            {
                {"x1", _currentTurf.X1},
                {"y1", _currentTurf.Y1},
                {"x2", _currentTurf.X2},
                {"y2", _currentTurf.Y2}
            });
            Alt.EmitAllClients("updateTeamPoints", new Dictionary<string, ulong>
            {
                {"ballas", Teams[0].TurfPoints},
                {"families", Teams[1].TurfPoints},
                {"vagos", Teams[2].TurfPoints}
            });
        }

        public void StopCapture()
        {
            foreach (var team in Teams)
            {
                team.ResetTurfPoints();
            }

            _currentTurf = null;
            Alt.EmitAllClients("captureStateChanged", false);
            Alt.EmitAllClients("stopCapture");
        }

        public Turf GetRandomTurf()
        {
            return Turfs[Random.Next(0, Turfs.Count)];
        }
    }
}