var safeCount = 0;

foreach (var report in File.ReadLines("input.txt"))
{
    var levels = report.Split(' ').Select(int.Parse).ToList();

    var safe = IsSafe(levels);
    for (var i = 0; !safe && i < levels.Count; i++)
        safe |= IsSafe(OmitIndex(levels, i));

    Console.WriteLine(report + " : " + safe);

    if (safe)
        safeCount++;
}

Console.WriteLine($"Safe: {safeCount}");

static bool IsSafe(IEnumerable<int> levels)
{
    using var enumerator = levels.GetEnumerator();
    if (!enumerator.MoveNext())
        return true;

    var previous = enumerator.Current;

    if (!enumerator.MoveNext())
        return true;

    var current = enumerator.Current;

    var isIncreasing = previous <= current; // don't need to handle == since that will be unsafe anyway
    var diffMultiplier = isIncreasing ? 1 : -1;

    if (!IsStepOk(previous, current, diffMultiplier))
        return false;

    while (enumerator.MoveNext())
    {
        (previous, current) = (current, enumerator.Current);

        if (!IsStepOk(previous, current, diffMultiplier))
            return false;
    }

    return true;

    static bool IsStepOk(int previous, int current, int diffMultiplier)
        => diffMultiplier * (current - previous) is 1 or 2 or 3;
}

static IEnumerable<T> OmitIndex<T>(IEnumerable<T> source, int index)
    => source.Index().Where(pair => pair.Index != index).Select(pair => pair.Item);