using System.Collections.Immutable;

var part1Sum = 0L;
var part2Sum = 0L;
foreach (var line in File.ReadAllLines("input.txt"))
{
    var parts = line.Split(':');
    var testValue = long.Parse(parts[0]);
    var values = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray();
    var part1Combos = GetMatchingOperators(testValue, values, false);
    //var part2Combos = new List<List<Operator>>(); // GetMatchingOperators(testValue, values, true);
    var part2Combos = GetMatchingOperators(testValue, values, true);
    if (part1Combos.Any())
    {
        Console.WriteLine($"1 - Test value: {testValue}");
        part1Sum += testValue;
        part2Sum += testValue;
        foreach (var combo in part1Combos)
        {
            Console.WriteLine(string.Join(" ", combo.Select(o => o switch { Operator.Add => "+", Operator.Multiply => "*", _ => throw new InvalidOperationException() })));
        }
    }
    else if (part2Combos.Any())
    {
        Console.WriteLine($"2 - Test value: {testValue}");
        part2Sum += testValue;
        foreach (var combo in part2Combos)
        {
            Console.WriteLine(string.Join(" ", combo.Select(o => o switch { Operator.Add => "+", Operator.Multiply => "*", Operator.Concat => "||", _ => throw new InvalidOperationException() })));
        }
    }
}

Console.WriteLine($"Part 1 sum: {part1Sum}");
Console.WriteLine($"Part 2 sum: {part2Sum}");

static List<List<Operator>> GetMatchingOperators(long target, ReadOnlySpan<long> values, bool includePart2)
{
    var validCombos = GetMatchingOperators(target, values, [[]], includePart2);
    return validCombos.Select(PopToList).ToList();

    static List<ImmutableStack<Operator>> GetMatchingOperators(long target, ReadOnlySpan<long> values, List<ImmutableStack<Operator>> operators, bool includePart2)
    {
        var lastValue = values[^1];

        // if we are at the front of the list, and the 1 value equals the target, then the input sets of operators are valid, so return them
        if (values.Length == 1)
            return lastValue == target ? operators : ([]);


        var result = new List<ImmutableStack<Operator>>();

        (var q, var rem) = Math.DivRem(target, lastValue);
        if (rem == 0)
        {
            // if target is divisible by last value, then last operator may be multiply
            // so push Multiply on to each current set of operators and recurse down
            var o = operators.Select(operators => operators.Push(Operator.Multiply)).ToList();
            result.AddRange(GetMatchingOperators(q, values[..^1], o, includePart2));
        }

        // push Add on to each current set of operators and recurse down
        if (target >= lastValue)
        {
            var o2 = operators.Select(operators => operators.Push(Operator.Add)).ToList();
            result.AddRange(GetMatchingOperators(target - lastValue, values[..^1], o2, includePart2));
        }

        var targetStr = target.ToString();
        var lastValueStr = lastValue.ToString();
        if (includePart2 && targetStr.EndsWith(lastValueStr) && targetStr.Length > lastValueStr.Length)
        {
            // push Concat on to each current set of operators and recurse down
            var o3 = operators.Select(operators => operators.Push(Operator.Concat)).ToList();
            var newTarget = long.Parse(targetStr[..^lastValueStr.Length]);
            result.AddRange(GetMatchingOperators(newTarget, values[..^1], o3, includePart2));
        }

        return result;
    }
}

static List<T> PopToList<T>(ImmutableStack<T> stack)
{
    var list = new List<T>(stack.Count());
    while (!stack.IsEmpty)
    {
        list.Add(stack.Peek());
        stack = stack.Pop();
    }

    return list;
}

enum Operator { Add, Multiply, Concat }
