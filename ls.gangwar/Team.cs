using System;
using System.Collections.Generic;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;

namespace ls.gangwar
{
    public class Team
    {
        private static readonly Random Random = new Random();

        public readonly string Name;

        private readonly List<string> vehicles;

        private readonly List<Position> spawns;

        private readonly Position weaponSpawn;

        private readonly Position vehicleSpawn;

        public readonly Rgba Color;

        public readonly string Hex;

        public readonly ICheckpoint VehicleCheckpoint;

        public readonly ICheckpoint WeaponCheckpoint;

        public ulong TurfPoints = 0;

        public Team(string name, List<string> vehicles, List<Position> spawns, Position weaponSpawn,
            Position vehicleSpawn,
            Rgba color, string hex)
        {
            Name = name;
            this.vehicles = vehicles;
            this.spawns = spawns;
            this.weaponSpawn = weaponSpawn;
            this.vehicleSpawn = vehicleSpawn;
            Color = color;
            Hex = hex;
            VehicleCheckpoint = Alt.CreateCheckpoint(CheckpointType.Cyclinder,
                new Position(vehicleSpawn.X, vehicleSpawn.Y, vehicleSpawn.Z - 1.1f), 5f, 1f,
                new Rgba(color.r, color.g, color.b, 255));
            WeaponCheckpoint = Alt.CreateCheckpoint(CheckpointType.Cyclinder,
                new Position(weaponSpawn.X, weaponSpawn.Y, weaponSpawn.Z - 1.1f), 5f, 1f,
                new Rgba(color.r, color.g, color.b, 255));
        }

        public void ResetTurfPoints()
        {
            TurfPoints = 0;
        }

        public Position GetRandomSpawn()
        {
            return spawns[Random.Next(0, spawns.Count)];
        }
        
        public string GetRandomVehicle()
        {
            return vehicles[Random.Next(0, vehicles.Count)];
        }
    }
}