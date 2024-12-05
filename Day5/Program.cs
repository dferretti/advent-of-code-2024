var rules = new List<Rule>();
var updates = new List<int[]>();

var readingRules = true;
foreach (var line in File.ReadAllLines("input.txt"))
{
    if (string.IsNullOrWhiteSpace(line))
    {
        readingRules = false;
        continue;
    }

    if (readingRules)
    {
        var parts = line.Split("|");
        rules.Add(new Rule(int.Parse(parts[0]), int.Parse(parts[1])));
    }
    else
    {
        updates.Add(line.Split(",").Select(int.Parse).ToArray());
    }
}

var sum = 0;

foreach (var update in updates)
{
    var inverseIndex = new Dictionary<int, int>(); // page number => index in the update array
    for (var i = 0; i < update.Length; i++)
        inverseIndex[update[i]] = i;

    var correctlyOrdered = true;
    foreach (var rule in rules)
    {
        if (!inverseIndex.TryGetValue(rule.Left, out var leftIndex) || !inverseIndex.TryGetValue(rule.Right, out var rightIndex))
            continue; // rule does not apply, page is not part of update

        if (leftIndex > rightIndex)
        {
            correctlyOrdered = false;
            break;
        }
    }

    if (correctlyOrdered)
        sum += update[update.Length / 2];
}

Console.WriteLine($"Sum: {sum}");

record Rule(int Left, int Right);