var safeCount = 0;

foreach (var report in File.ReadLines("input.txt"))
{
    var levels = report.Split(' ').Select(int.Parse).ToList();
    var isIncreasing = levels[0] <= levels[1]; // don't need to handle == since that will be unsafe anyway
    var diffMultiplier = isIncreasing ? 1 : -1;

    var safe = true;
    for (var i = 1; i < levels.Count; i++)
    {
        if (diffMultiplier * (levels[i] - levels[i - 1]) is 1 or 2 or 3)
            continue;

        safe = false;
        break;
    }

    if (safe)
        safeCount++;
}

Console.WriteLine($"Safe: {safeCount}");