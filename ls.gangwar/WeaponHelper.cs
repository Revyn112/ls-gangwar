using System.Collections.Generic;
using AltV.Net;

namespace ls.gangwar
{
    public class WeaponHelper
    {
        private static readonly Dictionary<string, string> Weapons = new Dictionary<string, string>
        {
            {"WEAPON_KNIFE", "Knife"},
            {"WEAPON_BAT", "Bat"},
            {"WEAPON_BOTTLE", "Bottle"},
            {"WEAPON_WRENCH", "Wrench"},
            {"WEAPON_PISTOL", "Pistol"},
            {"WEAPON_HEAVYPISTOL", "Heavy pistol"},
            {"WEAPON_REVOLVER", "Revolver"},
            {"WEAPON_MICROSMG", "Micro-SMG"},
            {"WEAPON_SMG", "SMG"},
            {"WEAPON_COMBATPDW", "Combat PDW"},
            {"WEAPON_ASSAULTRIFLE", "Assault Rifle"},
            {"WEAPON_CARBINERIFLE", "Carbin Rifle"},
            {"WEAPON_PUMPSHOTGUN", "Pump Shotgun"},
            {"WEAPON_GRENADE", "Grenade"},
            {"WEAPON_RAMMED_BY_CAR", "Jumped out of car"},
            {"WEAPON_RUN_OVER_BY_CAR", "Run over by car"},
            {"WEAPON_FALL", "Fall"},
            {"WEAPON_DROWNING", "Drowning"},
            {"WEAPON_DROWNING_IN_VEHICLE", "Drowning"},
            {"WEAPON_EXPLOSION", "Explosion"},
            {"WEAPON_FIRE", "Fired"},
            {"WEAPON_BLEEDING", "Bleeding"},
            {"WEAPON_BARBED_WIRE", "Barbed wire"},
            {"WEAPON_EXHAUSTION", "Exhaustion"},
            {"WEAPON_ELECTRIC_FENCE", "Electric fence"}
        };

        private static readonly Dictionary<uint, string> WeaponHashes = new Dictionary<uint, string>();

        static WeaponHelper()
        {
            foreach (var (hash, translation) in Weapons)
            {
                WeaponHashes[Alt.Hash(hash)] = translation;
            }
        }

        public static string GetWeaponTranslation(uint weapon)
        {
            return WeaponHashes.TryGetValue(weapon, out var translation) ? translation : "Killed";
        }
    }
}