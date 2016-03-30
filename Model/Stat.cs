using System;
using System.Collections.Generic;
using System.Drawing;

namespace Statter
{
    public class Stat
    {
        public static readonly Color DEFAULT_COLOUR = Color.Black;

        private string _name = "";
        private string _clientAttribute = "";
        private List<StatReading> _readings = new List<StatReading>();

        public string Name { get { return _name; } }
        public string ClientAttribute { get { return _clientAttribute; } }
        public Color Colour { get; set; }
        public List<StatReading> Readings { get { return _readings; } }

        // Trackable stats are pulled from <EQ2_Dir>\UI\Default\eq2ui_gamedata.xml
        // Search for: <DataSource description="Stats" Name="Stats">
        // TODO: Present an option to slurp this in?
        private static Dictionary<string, string> ClientAttributeLookupTable = new Dictionary<string, string>()
        {
            { "Stamina", "Stamina" },
            { "Wisdom", "Wisdom" },
            { "Agility", "Agility" },
            { "Strength", "Strength" },
            { "Intelligence", "Intelligence" },
            { "Max Health", "HealthRange" },
            { "Max Mana", "PowerRange" },
            { "Crit Chance", "Crit_Chance" },
            { "Crit Bonus", "Crit_Bonus" },
            { "Potency", "Potency" },
            { "Ability Mod", "Ability_Mod" },
            { "Fervor", "Fervor" },
            { "Cast Speed", "Spell_Cast_Percent" },
            { "Recovery", "Spell_Recovery_Percent" },
            { "Ability Reuse", "Spell_Reuse_Percent" },
            { "Spell Reuse", "Spell_Reuse_Spell_Only" },
            { "Spell Double Cast", "SpellDoubleAttack" },
            { "DPS", "DPS" },
            { "Haste", "Haste" },
            { "Multi Attack", "Double_Atk_Percent" },
            { "Flurry", "Flurry" },
            { "AE Auto", "AE_AutoAtk_Percent" },
            { "Weapon Damage Bonus", "Weapon_Damage_Bonus" },
            { "Spell Weapon Damage Bonus", "Spell_Weapon_Damage_Bonus" },
            { "Accuracy", "Accuracy" },
            { "Strikethrough", "Strikethrough" },
            { "Hate Mod", "Hate_Mod" },
            { "Mitigation", "Defense_Mitigation" },
            { "Mitigation %", "Defense_MitigationPercent" },
            { "Avoidance", "Defense_Avoidance" },
            { "Avoidance %", "Defense_AvoidanceBase" },
            { "Shield Effectiveness", "Shield_Effectiveness" }
        };

        public Stat(string name)
        {
            if (!ClientAttributeLookupTable.ContainsKey(name))
                throw new Exception("Unknown stat: " + name);

            _name = name;
            _clientAttribute = ClientAttributeLookupTable[_name];

            Colour = DEFAULT_COLOUR;
        }

        public override bool Equals(object obj)
        {
            Stat other = obj as Stat;
            if (other == null) return false;

            return other.Name.Equals(Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        // Return a list of stat names we know how to track
        public static List<string> GetKnownStats()
        {
            List<string> knownStats = new List<string>();

            foreach (string key in ClientAttributeLookupTable.Keys)
                knownStats.Add(key);

            return knownStats;
        }

        // Return a list of stat names we know how to track, minus the ones specified
        public static List<string> GetAvailableStats(List<string> usedStats)
        {
            List<string> availableStats = new List<string>();

            foreach (string stat in GetKnownStats())
                if (!usedStats.Contains(stat))
                    availableStats.Add(stat);

            return availableStats;
        }

        // Extract a single value from the logline, and record it
        public void ParseReading(string reading, DateTime time)
        {
            double temp = 0;
            string[] parts;

            switch (_clientAttribute)
            {
                case "HealthRange":
                case "PowerRange":
                case "Primary_Damage_Range":
                case "Secondary_Damage_Range":
                case "Ranged_Damage_Range":
                    parts = reading.Split(new string[] { " ", "-", "/" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && double.TryParse(parts[1], out temp))
                        AddReading(temp, time);
                    break;
                default:
                    string cleaned = reading.Replace("%", "");
                    if (double.TryParse(cleaned, out temp))
                        AddReading(temp, time);
                    break;
            }
        }

        public void ClearReadings()
        {
            _readings.Clear();
        }

        // Get the reading at or immediately before the time specified
        public StatReading GetReading(DateTime time)
        {
            return _readings.FindLast(x => { return x.Time <= time; });
        }

        // Get the largest reading during time specified
        public StatReading GetMaxReading(DateTime start, DateTime end)
        {
            StatReading maxReading = null;

            foreach (StatReading reading in _readings)
                if (reading.Time >= start && reading.Time <= end && (maxReading == null || reading.Value > maxReading.Value))
                    maxReading = reading;

            return maxReading;
        }

        // Get the smallest reading during time specified
        public StatReading GetMinReading(DateTime start, DateTime end)
        {
            StatReading minReading = null;

            foreach (StatReading reading in _readings)
                if (reading.Time >= start && reading.Time <= end && (minReading == null || reading.Value < minReading.Value))
                    minReading = reading;

            return minReading;
        }

        public List<StatReading> GetReadings(DateTime start, DateTime end)
        {
            return _readings.FindAll(x => { return x.Time >= start && x.Time <= end; });
        }

        private void AddReading(double value, DateTime time)
        {
            StatReading reading = new StatReading() { Value = value, Time = time };
            _readings.Add(reading);
        }
    }
}
