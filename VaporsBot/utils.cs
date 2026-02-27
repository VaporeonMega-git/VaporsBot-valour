using System.Globalization;
using Valour.Sdk.Models;

namespace VaporsBot
{
    public static class Utils
    {

        private static readonly Random random = Random.Shared;

        public static String RandomString(Dictionary<String, int> options)
        {
            int roll = random.Next(options.Values.Sum());
            foreach (var kvp in options)
            {
                if (kvp.Value <= 0)
                    continue;

                if (roll < kvp.Value)
                    return kvp.Key;

                roll -= kvp.Value;
            }

            Console.WriteLine("uh oh");
            return "";
        }

        public static bool IsSingleEmoji(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();

            var enumerator = StringInfo.GetTextElementEnumerator(input);
            int count = 0;

            while (enumerator.MoveNext())
                count++;

            return count == 1;
        }

        public static bool ContainsAny(string input, params string[] values)
        {
            var lower = input.ToLower();

            foreach (var value in values)
            {
                if (lower.Contains(value.ToLower()))
                    return true;
            };

            return false;
        }

        public static bool StartsWithAny(string input, params string[] values)
        {
            var lower = input.ToLower();

            foreach (var value in values)
            {
                if (lower.StartsWith(value.ToLower()))
                    return true;
            };

            return false;
        }

        public static async Task SendReplyAsync(Dictionary<long, Channel> channelCache, long channel, string reply)
        {
            if (channelCache.TryGetValue(channel, out var chan))
            {
                await chan.SendMessageAsync(reply);
            }
            else
            {
                Console.WriteLine($"Channel {channel} was not found in the cache.");
            };
        }
    };
};