var stones = File.ReadAllText("input.txt").Split(' ').Select(long.Parse).ToArray();

var cache = new Dictionary<(long, long), long>();
var part1Count = stones.Sum(s => Step(s, 25, cache));
Console.WriteLine($"Part 1 count: {part1Count}");
var part2Count = stones.Sum(s => Step(s, 75, cache));
Console.WriteLine($"Part 2 count: {part2Count}");
Console.WriteLine($"Cache size: {cache.Count}");

static long Step(long stone, int blinkCountdown, Dictionary<(long, long), long> cache)
{
    if (blinkCountdown == 0) return 1;

    var key = (stone, blinkCountdown);
    if (cache.TryGetValue(key, out var cached)) return cached;

    long[] nextGen = (stone, TrySplit(stone, out var left, out var right)) switch
    {
        (0, _) => [1],
        (_, true) => [left, right],
        _ => [stone * 2024],
    };

    var result = nextGen.Sum(x => Step(x, blinkCountdown - 1, cache));

    if (cache.Count < 1_000_000)
        cache[key] = result;

    return result;
}

static bool TrySplit(long x, out long left, out long right)
{
    var str = x.ToString();
    if (str.Length % 2 == 0)
    {
        left = long.Parse(str[..(str.Length / 2)]);
        right = long.Parse(str[(str.Length / 2)..]);
        return true;
    }

    left = right = 0;
    return false;
}