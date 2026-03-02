using Valour.Sdk.Client;
using Valour.Sdk.Models;
using DotNetEnv;
using VaporsBot;
using Pkmn;
using Valour.Shared;
using System.Text.Json;
using Valour.Shared.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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

async Task joinChannels() { 
    await client.BotService.JoinAllChannelsAsync();

    // channelCache = new Dictionary<long, Channel>();

    foreach (var planet in client.PlanetService.JoinedPlanets)
    {
        foreach (var channel in planet.Channels)
        {
            channelCache[channel.Id] = channel;
            Console.WriteLine($"Cached: {channel.Id}");
        }
    }
}

await joinChannels();

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

    if (Utils.StartsWithAny(content, "v/help", "v/cmd", "v/list"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}» My commands:\n- `v/source|src` - links to my source code\n- `v/pokemon|pkmn - fetch information about pokemon`\n- `v/cat` - show a random image of a cat");
    }

    if (Utils.StartsWithAny(content, "v/source", "v/src"))
    {
        await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}» You can see my source code here: https://github.com/VaporeonMega-git/VaporsBot-valour");
    }

    if (Utils.StartsWithAny(content, "v/pokemon", "v/pkmn"))
    {

        if (Utils.StartsWithAny(content, "v/pokemon pokedex", "v/pkmn pokedex"))
        {
            if (split.Length <= 2)
            {
                await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}» Usage: `v/pokemon pokedex <species|natdex#>`");
                return;
            }

            string pkmnName = string.Join(" ", split[2..]);
            string pokedexOutput = await Pkmn.Pkmn.pokedexEntry(pkmnName);
            await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}»\n{pokedexOutput}");
        }
        
        else if (Utils.StartsWithAny(content, "v/pokemon move", "v/pkmn move"))
        {
            if (split.Length <= 2)
            {
                await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}» Usage: `v/pokemon move <move>`");
                return;
            }

            string moveName = string.Join(" ", split[2..]);
            string pokemoveOutput = await Pkmn.Pkmn.movedexEntry(moveName);
            await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}»\n{pokemoveOutput}");
        }

        else
        {
            await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}» Unknown sub-command. Valid sub-commands are:\n- `v/pokemon pokedex <species|natdex#>`\n- `v/pokemon move <name>`");
        }

    }

    if (Utils.StartsWithAny(content, "v/cat"))
    {
        using HttpClient httpClient = new();

        // string randomCat = await httpClient.GetStringAsync("https://aleatori.cat/random");
        string randomCatJson = await httpClient.GetStringAsync("https://aleatori.cat/random.json");
        JsonElement randomCatData = JsonSerializer.Deserialize<JsonElement>(randomCatJson);
        string randomCatUrl = randomCatData.GetProperty("url").GetString();

        // await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}»^^^^^^^^^^^^^^{randomCatUrl}^^^^^^^^^^^^^^");
        await Utils.SendReplyAsync(channelCache, channelId, $"[{randomCatUrl}]()«@m-{member.Id}»");

        // byte[] imageBytes = await httpClient.GetByteArrayAsync(randomCatUrl);

        // using Image image = Image.Load(imageBytes);
        // int width = image.Width;
        // int height = image.Height;

        // MessageAttachment ma = new(MessageAttachmentType.Image)
        // {
        //     Local = false,
        //     Location = randomCatUrl,
        //     MimeType = "image/jpeg",
        //     FileName = "cat",
        //     Height = height,
        //     Width = width,
        // };

        // await Utils.SendReplyFileAsync(channelCache, channelId, $"«@m-{member.Id}»", [ma]);
    }

    // if (Utils.StartsWithAny(content, "v/sayhitovictor"))
    // {
    //     await Utils.SendReplyAsync(channelCache, channelId, $"clanker/meow/bot/native/roadmap/bug/github");
    // }

    if (Utils.StartsWithAny(content, "v/echo "))
    {
        if (message.AuthorUserId != client.Me.OwnerId) return;
        string msg = string.Join(" ", split[1..]);
        await Utils.SendReplyAsync(channelCache, channelId, $"{msg}");
    }

    if (Utils.StartsWithAny(content, "v/join "))
    {
        if (message.AuthorUserId != client.Me.OwnerId) return;
        if (!long.TryParse(split[1], out var planetIdToJoin))
        {
            await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}» v/join [planetId] [optional inviteCode]");
            return;
        }
        if (split.Length < 2)
        {
            bool joinResult = (await client.PlanetService.JoinPlanetAsync(planetIdToJoin)).Success.ToString().ToLower().Equals("true");
            await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}» {(joinResult ? "Joined planet" : "Failed to join planet")}");

            await joinChannels();
        } else
        {
            string inviteCode = split[2];
            bool joinResult = (await client.PlanetService.JoinPlanetAsync(planetIdToJoin, inviteCode)).Success.ToString().ToLower().Equals("true");
            await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}» {(joinResult ? $"Joined planet {(await client.PlanetService.FetchPlanetAsync(planetIdToJoin)).Name}" : "Failed to join planet")}");

            await joinChannels();
        }
    }

    if (Utils.StartsWithAny(content, "v/leave "))
    {
        if (message.AuthorUserId != client.Me.OwnerId) return;
        if (!long.TryParse(split[1], out var planetIdToLeave))
        {
            await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}» v/leave [planetId]");
            return;
        }
        Planet planetToLeave;
        try {
            planetToLeave = await client.PlanetService.FetchPlanetAsync(planetIdToLeave);
        } catch (Exception exception)
        {
            await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}» Not a member of that planet!");
            return;
        }
        bool leaveResult = (await client.PlanetService.LeavePlanetAsync(planetToLeave)).Success.ToString().ToLower().Equals("true");
        await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}» {(leaveResult ? $"Left planet {planetToLeave.Name}" : "Failed to leave planet")}");

        await joinChannels();
    }

    if (Utils.StartsWithAny(content, "v/refreshchannels"))
    {
        if (message.AuthorUserId != client.Me.OwnerId) return;

        await Utils.SendReplyAsync(channelCache, channelId, $"«@m-{member.Id}» Refreshing channels...");
        
        await joinChannels();
    }
};

Console.WriteLine("Listening for messages...");
await Task.Delay(Timeout.Infinite);