var rules = new HashSet<Rule>();
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

var correctSum = 0;
var incorrectSum = 0;

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
    {
        correctSum += update[update.Length / 2];
    }
    else
    {
        Console.WriteLine($"Correcting: {string.Join(',', update)}");

        // build new corrected list. start with just the first element
        var corrected = new List<int> { update[0] };
        var remaining = new Queue<int>(update.Skip(1)); // remaining to be placed in corrected

        while (remaining.TryDequeue(out var x))
        {
            // try to place x in corrected, using rules avalable between x and the elements of corrected
            // if ambiguous, requeue x to remaining and try another value
            Console.WriteLine($"Trying to add {x} to [{string.Join(',', corrected)}]");

            if (TryAddFromLeft(x)) continue;
            if (TryAddFromRight(x)) continue;

            Console.WriteLine($"Ambiguous: {x}");
            remaining.Enqueue(x);
        }

        Console.WriteLine($"Corrected: [{string.Join(',', corrected)}]\n----");
        incorrectSum += corrected[corrected.Count / 2];

        bool TryAddFromLeft(int x)
        {
            // we are moving left to right in the corrected list. if we find a rule that says X must come before corrected[i], we insert X at this position
            // since we can't move right any more
            for (var i = 0; i < corrected.Count; i++)
            {
                var stopRule = new Rule(x, corrected[i]);
                if (rules.Contains(stopRule))
                {
                    corrected.Insert(i, x);
                    return true;
                }
            }

            return false;
        }

        bool TryAddFromRight(int x)
        {
            for (var i = corrected.Count - 1; i >= 0; i--)
            {
                var stopRule = new Rule(corrected[i], x);
                if (rules.Contains(stopRule))
                {
                    corrected.Insert(i + 1, x);
                    return true;
                }
            }

            return false;
        }
    }
}

Console.WriteLine($"Sum: {correctSum}");
Console.WriteLine($"Incorrect sum: {incorrectSum}");

record Rule(int Left, int Right);