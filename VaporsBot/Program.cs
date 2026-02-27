using Valour.Sdk.Client;
using Valour.Sdk.Models;
using DotNetEnv;
using VaporsBot;
using Pkmn;

Env.Load();

var token = Environment.GetEnvironmentVariable("BOT_TOKEN");

if (string.IsNullOrWhiteSpace(token))
{
    Console.WriteLine("TOKEN environment variable not set.");
    return;
}

var client = new ValourClient("https://api.valour.gg/");
client.SetupHttpClient();

var loginResult = await client.InitializeUser(token);
if (!loginResult.Success)
{
    Console.WriteLine($"Login Failed: {loginResult.Message}");
    return;
}

// await client.PlanetService.JoinPlanetAsync(42061742971289601, "");
// await client.PlanetService.JoinPlanetAsync(12215159187308544);
// await client.PlanetService.LeavePlanetAsync(await client.PlanetService.FetchPlanetAsync(12215159187308544));

var channelCache = new Dictionary<long, Channel>();

await client.BotService.JoinAllChannelsAsync();

foreach (var planet in client.PlanetService.JoinedPlanets)
{
    foreach (var channel in planet.Channels)
    {
        channelCache[channel.Id] = channel;
        Console.WriteLine($"Cached: {channel.Id}");
    }
}

Console.WriteLine($"Logged in as {client.Me.Name} (ID: {client.Me.Id})");

var bannedUserIDs = new List<long> {};

client.MessageService.MessageReceived += async (message) =>
{
    string content = message.Content ?? "";
    long channelId = message.ChannelId;
    var member = await message.FetchAuthorMemberAsync();
    var planetId = message.PlanetId;

    if (content is null) {
        return;
    }

    if (message.AuthorUserId == client.Me.Id) {
        return;
    }

    if (bannedUserIDs.Contains(message.AuthorUserId)) {
        return;
    }

    string[] split = content.Split(" ");;

    if (planetId != null) {
        var planet = await client.PlanetService.FetchPlanetAsync(planetId.Value);
        var selfMember = await client.PlanetService.FetchMemberByUserAsync(client.Me.Id, planet.Id);

        // if (Utils.StartsWithAny(content, "«@m-" + selfMember.Id.ToString() + "» github", "«@m-" + selfMember.Id.ToString() + "»  github"))
        // {
        //     await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}» You can see my source code here: https://github.com/VaporeonMega-git/VaporsBot-valour");
        // };
    }

    if (Utils.StartsWithAny(content, "v/source", "v/src"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}» You can see my source code here: https://github.com/VaporeonMega-git/VaporsBot-valour");
    }

    if (Utils.StartsWithAny(content, "v/pokemon pokedex", "v/pkmn pokedex"))
    {
        string pkmnName = string.Join(" ", split[2..]);
        string pokedexOutput = await Pkmn.Pkmn.pokedexEntry(pkmnName);
        await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}»\n{pokedexOutput}");
    }

    // if (Utils.StartsWithAny(content, "v/pokemon move", "v/pkmn move"))
    // {
    //     string moveName = string.Join(" ", split[2..]);
    //     string pokemoveOutput = await Pkmn.Pkmn.movedexEntry(abilityName);
    //     await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}»\n{pokemoveyOutput}");
    // }

    // if (Utils.StartsWithAny(content, "v/sayhitovictor"))
    // {
    //     await Utils.SendReplyAsync(channelCache, channelId, $"clanker/meow/bot/native/roadmap");
    // }
};

Console.WriteLine("Listening for messages...");
await Task.Delay(Timeout.Infinite);