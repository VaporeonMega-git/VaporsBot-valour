using System.Collections;
using System.Net.Http;
using System.Text.Json;
using Microsoft.VisualBasic;
using Jint;

namespace Pkmn {

    class PokedexEntry
    {
        public string Name { get; set; }
        public int Num { get; set; }

        public List<string> Types { get; set; }

        public int Stat_HP { get; set; }
        public int Stat_ATK { get; set; }
        public int Stat_DEF { get; set; }
        public int Stat_SPA { get; set; }
        public int Stat_SPD { get; set; }
        public int Stat_SPE { get; set; }

        public double HeightM { get; set; }
        public double WeightKG { get; set; }

        public List<Ability> Abilities { get; set; }
        public List<Move> Learnset { get; set; }
        public int LearnsetGen { get; set; }

        public PokedexEntry(
            string name,
            int num,
            List<string> types,
            int stat_hp,
            int stat_atk,
            int stat_def,
            int stat_spa,
            int stat_spd,
            int stat_spe,
            double heightm,
            double weightkg,
            List<Ability> abilities,
            List<Move> learnset,
            int learnsetGen)
        {
            Name = name;
            Num = num;
            Types = types;

            Stat_HP = stat_hp;
            Stat_ATK = stat_atk;
            Stat_DEF = stat_def;
            Stat_SPA = stat_spa;
            Stat_SPD = stat_spd;
            Stat_SPE = stat_spe;

            HeightM = heightm;
            WeightKG = weightkg;

            Abilities = abilities;
            Learnset = learnset;
            LearnsetGen = learnsetGen;
        }

        public int BST()
        {
            return this.Stat_HP + this.Stat_ATK + this.Stat_DEF + this.Stat_SPA + this.Stat_SPD + this.Stat_SPE;
        }
    }

    class Ability
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public string ShortDesc { get; set; }

        public Ability(string name, string desc, string shortDesc)
        {
            Name = name;
            Desc = desc;
            ShortDesc = shortDesc;
        }
    }

    class Move
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public string ShortDesc { get; set; }

        public int BasePower { get; set; }
        public string Accuracy { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }

        public int CritRatio { get; set; }
        public int Priority { get; set; } = 0;
        public int PP { get; set; }
        public string Target { get; set; }

        public Move(string name, string desc, string shortDesc, int basePower, string accuracy, string type, string category, int critRatio, int pp, string target, int priority = 0)
        {
            Name = name;
            Desc = desc;
            ShortDesc = shortDesc;
            BasePower = basePower;
            Accuracy = accuracy;
            Type = type;
            Category = category;
            CritRatio = critRatio;
            PP = pp;
            Target = target;
            Priority = priority;
        }
    }

    public static class Pkmn
    {

        private static JsonElement? pokedex = null;
        private static JsonElement? learnsets = null;
        private static JsonElement? movedex = null;
        private static JsonElement? abilitydex = null;

        private static async Task<JsonElement> requestPokedex()
        {
            if (pokedex == null) {
                using HttpClient client = new HttpClient();

                string json = await client.GetStringAsync("https://play.pokemonshowdown.com/data/pokedex.json");

                JsonElement data = JsonSerializer.Deserialize<JsonElement>(json);
                pokedex = data;
                
                return data;
            } else {
                return (JsonElement) pokedex;
            }
        }
        
        private static async Task<JsonElement> requestLearnsets()
        {
            if (learnsets == null) {
                using HttpClient client = new HttpClient();

                string json = await client.GetStringAsync("https://play.pokemonshowdown.com/data/learnsets.json");

                JsonElement data = JsonSerializer.Deserialize<JsonElement>(json);
                learnsets = data;
                
                return data;
            } else {
                return (JsonElement) learnsets;
            }
        }

        private static async Task<JsonElement> requestMovedex()
        {
            if (movedex == null) {
                using HttpClient client = new HttpClient();

                string json = await client.GetStringAsync("https://play.pokemonshowdown.com/data/moves.json");

                JsonElement data = JsonSerializer.Deserialize<JsonElement>(json);
                movedex = data;
                
                return data;
            } else {
                return (JsonElement) movedex;
            }
        }

        private static async Task<JsonElement> requestAbilitydex()
        {
             if (abilitydex == null)
            {
                using HttpClient client = new HttpClient();
                string js = await client.GetStringAsync("https://play.pokemonshowdown.com/data/abilities.js");

                Engine engine = new();
                engine.SetValue("exports", new { });
                engine.Execute(js);
                var export = engine.GetValue("exports").AsObject().Get("BattleAbilities").ToObject();
                string json = JsonSerializer.Serialize(export);

                JsonElement data = JsonSerializer.Deserialize<JsonElement>(json);
                abilitydex = data;

                return data;
            } else
            {
                return (JsonElement) abilitydex;
            }
        }

        private static async Task<JsonElement?> GetPokedexEntryJson(string pokemonName)
        {
            JsonElement data = await requestPokedex();
            // JsonElement learnsets = await requestLearnsets();
            pokemonName = pokemonName.ToLower();

            // by name
            foreach (JsonProperty property in data.EnumerateObject())
            {
                string key = property.Name;
                JsonElement value = property.Value;

                if (key == pokemonName)
                {
                    return value;
                }
            }

            // by natdex
            foreach (JsonProperty property in data.EnumerateObject()) {
                string key = property.Name;
                JsonElement value = property.Value;

                if (value.GetProperty("num").GetInt32().ToString() == pokemonName)
                {
                    return value;
                }
            }

            return null;
        }

        private static async Task<JsonElement?> GetLearnsetJson(string pokemonName)
        {
            JsonElement data = await requestPokedex();
            JsonElement learnsets = await requestLearnsets();
            // JsonElement learnsets = await requestLearnsets();
            pokemonName = pokemonName.ToLower();

            // by name
            foreach (JsonProperty property in data.EnumerateObject())
            {
                string key = property.Name;
                JsonElement value = property.Value;

                if (key == pokemonName)
                {
                    if (learnsets.TryGetProperty(key, out var learnset)){
                        return learnsets.GetProperty(key).GetProperty("learnset");
                    }

                    return null;
                }
            }

            // by natdex
            foreach (JsonProperty property in data.EnumerateObject()) {
                string key = property.Name;
                JsonElement value = property.Value;

                if (value.GetProperty("num").GetInt32().ToString() == pokemonName)
                {
                    if (learnsets.TryGetProperty(key, out var learnset)){
                        return learnsets.GetProperty(key).GetProperty("learnset");
                    }

                    return null;
                }
            }

            return null;
        }

        private static async Task<PokedexEntry?> GetPokedexEntry(string pokemonName)
        {
            JsonElement? pokemonN = await GetPokedexEntryJson(pokemonName);
            if (pokemonN == null) return null;
            JsonElement pokemon = (JsonElement) pokemonN;

            JsonElement? learnset = await GetLearnsetJson(pokemonName);

            int gen = 9;

            int natdex = pokemon.GetProperty("num").GetInt32();
            string name = pokemon.GetProperty("name").GetString();
            List<string> types = GetStringList(pokemon.GetProperty("types"));
            
            int hp = pokemon.GetProperty("baseStats").GetProperty("hp").GetInt32();
            int atk = pokemon.GetProperty("baseStats").GetProperty("atk").GetInt32();
            int def = pokemon.GetProperty("baseStats").GetProperty("def").GetInt32();
            int spa = pokemon.GetProperty("baseStats").GetProperty("spa").GetInt32();
            int spd = pokemon.GetProperty("baseStats").GetProperty("spd").GetInt32();
            int spe = pokemon.GetProperty("baseStats").GetProperty("spe").GetInt32();
            // int bst = hp + atk + def + spa + spd + spe;

            List<Ability> abilities = [];
            if (pokemon.GetProperty("abilities").TryGetProperty("0", out JsonElement ability0)) abilities.Add(await GetAbility(ability0.GetString())); // these could technically be null but that shouldn't happen
            if (pokemon.GetProperty("abilities").TryGetProperty("1", out JsonElement ability1)) abilities.Add(await GetAbility(ability1.GetString())); // famous last words
            if (pokemon.GetProperty("abilities").TryGetProperty("H", out JsonElement abilityH)) abilities.Add(await GetAbility(abilityH.GetString()));

            double heightm = pokemon.GetProperty("heightm").GetDouble();
            double weightkg = pokemon.GetProperty("weightkg").GetDouble();

            List<Move> moves = [];
            if (learnset != null) {
                while (moves.Count == 0 && gen > 0) {
                    foreach (JsonProperty property in ((JsonElement) learnset).EnumerateObject())
                    {
                        string moveName = property.Name;
                        JsonElement moveList = property.Value;

                        bool currentGen = false;
                        foreach (string learnMethod in GetStringList(moveList))
                        {
                            if (learnMethod.StartsWith(gen.ToString())) currentGen = true;
                        }

                        if (currentGen)
                        {
                            Move? move = await GetMove(moveName);
                            if (move != null)
                            {
                                moves.Add(move);
                            } else
                            {
                                Console.WriteLine($"[GetPokedexEntry({pokemonName})]: Failed to add move {moveName}");
                            }
                        }
                    }

                    gen -= 1;
                }

                gen += 1;
            }

            return new PokedexEntry(name, natdex, types, hp, atk, def, spa, spd, spe, heightm, weightkg, abilities, moves, gen);

        }

        private static async Task<JsonElement?> GetMoveJson(string moveName)
        {
            JsonElement data = await requestMovedex();
            moveName = moveName.ToLower();

            // by name
            foreach (JsonProperty property in data.EnumerateObject())
            {
                string key = property.Name;
                JsonElement value = property.Value;

                if (key == moveName)
                {
                    return value;
                }
            }

            // by natdex
            foreach (JsonProperty property in data.EnumerateObject()) {
                string key = property.Name;
                JsonElement value = property.Value;

                if (value.GetProperty("num").GetInt32().ToString() == moveName)
                {
                    return value;
                }
            }

            return null;
        }

        private static async Task<Move?> GetMove(string moveName)
        {
            JsonElement? moveN = await GetMoveJson(moveName);
            if (moveN == null) return null;
            JsonElement move = (JsonElement) moveN;

            string name = move.GetProperty("name").GetString();
            string accuracy = move.GetProperty("accuracy").GetRawText();
            int basePower = move.GetProperty("basePower").GetInt16();
            string category = move.GetProperty("category").GetString();
            int critRatio = 1;
            if (move.TryGetProperty("critRatio", out var critRatioTry))
            {
                critRatio = critRatioTry.GetInt16();
            }
            int priority = 0;
            if (move.TryGetProperty("priority", out var priorityTry))
            {
                priority = priorityTry.GetInt16();
            }
            int pp = move.GetProperty("pp").GetInt16();
            string type = move.GetProperty("type").GetString();
            string target = move.GetProperty("target").GetString();
            string desc = move.GetProperty("desc").GetString();
            string shortDesc = move.GetProperty("shortDesc").GetString();

            return new Move(name, desc, shortDesc, basePower, accuracy, type, category, critRatio, pp, target, priority);
        }

        private static async Task<JsonElement?> GetAbilityJson(string abilityName)
        {
            JsonElement data = await requestAbilitydex();
            abilityName = abilityName.ToLower();

            // by key
            foreach (JsonProperty property in data.EnumerateObject())
            {
                string key = property.Name;
                JsonElement value = property.Value;

                if (key == abilityName)
                {
                    return value;
                }
            }

            // by name
            foreach (JsonProperty property in data.EnumerateObject()) {
                string key = property.Name;
                JsonElement value = property.Value;

                if (value.GetProperty("name").GetString().ToLower() == abilityName)
                {
                    return value;
                }
            }

            return null;
        }

        private static async Task<Ability?> GetAbility(string abilityName)
        {
            JsonElement? abilityN = await GetAbilityJson(abilityName);
            if (abilityN == null) return null;
            JsonElement ability = (JsonElement) abilityN;

            string name = ability.GetProperty("name").GetString();
            string desc = ability.GetProperty("desc").GetString();
            string shortDesc = ability.GetProperty("shortDesc").GetString();

            return new Ability(name, desc, shortDesc);
        }

        public static async Task<string> GetPokedexEntrySummary(string pokemonName)
        {
            PokedexEntry? pokemonN = await GetPokedexEntry(pokemonName);
            if (pokemonN == null) return "Pokemon not found!";
            PokedexEntry pokemon = (PokedexEntry) pokemonN;

            string output = $"{pokemon.Name} #{pokemon.Num}";
            if (pokemon.Types.Count == 1) {
                output += $"\nType: {pokemon.Types[0]}";
            } else {
                output += $"\nTypes: {pokemon.Types[0]}, {pokemon.Types[1]}";
            }
            output += $"\nHeight: {pokemon.HeightM}m";
            output += $"\nWeight: {pokemon.WeightKG}kg";
            output += $"\nAbilities: \n```text\n";
            foreach (Ability ability in pokemon.Abilities)
            {
                output += ability.Name + ": " + ability.ShortDesc + "\n";
            }
            output += "```";
            // output = output.Substring(0, output.Length - 2);
            output += $"\nStats:\n```text";
            output += $"\nHP: {pokemon.Stat_HP}";
            output += $"\nATK: {pokemon.Stat_ATK}";
            output += $"\nDEF: {pokemon.Stat_DEF}";
            output += $"\nSPA: {pokemon.Stat_SPA}";
            output += $"\nSPD: {pokemon.Stat_SPD}";
            output += $"\nSPE: {pokemon.Stat_SPE}";
            output += $"\nBST: {pokemon.BST()}";
            output += $"\n```";
            if (pokemon.Learnset.Count > 0) {
                output += $"\nGen {pokemon.LearnsetGen} Learnset: ";
                foreach (Move move in pokemon.Learnset)
                {
                    output += $"`{move.Name}`, ";
                }
                output = output.Substring(0, output.Length - 2);
            }
            
            return output;
        }

        public static async Task<string> GetMoveSummary(string moveName)
        {
            Move? moveN = await GetMove(moveName);
            if (moveN == null) return "Move not found!";
            Move move = (Move) moveN;

            string output = $"{move.Name}";
            output += $"\nType: `{move.Type}`";
            output += $"\nPower: `{move.BasePower}`, Accuracy: `{move.Accuracy}`";
            output += $"\nStat: `{move.Category}`, PP: `{move.PP}`";
            output += $"\nTargets: `{move.Target}`, Crit Ratio: `{move.CritRatio}`, Priority: `{(move.Priority > 0 ? "+" : "")}{move.Priority}`";
            output += $"\n```text\n{move.Desc}\n```";

            return output;
        }

        public static async Task<string> GetAbilitySummary(string abilityName)
        {
            Ability? abilityN = await GetAbility(abilityName);
            if (abilityN == null) return "Ability not found!";
            Ability ability = (Ability) abilityN;

            string output = $"{ability.Name}";
            output += $"\n```text\n{ability.Desc}\n```";
            
            return output;
        }

        public static List<string> GetStringList(JsonElement element)
        {
            var result = new List<string>();

            foreach (var item in element.EnumerateArray())
            {
                result.Add(item.GetString()!);
            }

            return result;
        }
    }
}