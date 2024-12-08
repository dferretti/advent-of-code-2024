using System.Collections.Immutable;

var part1Sum = 0L;
foreach (var line in File.ReadAllLines("input.txt"))
{
    var parts = line.Split(':');
    var testValue = long.Parse(parts[0]);
    var values = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray();
    var validCombos = GetMatchingOperators(testValue, values);
    if (validCombos.Any())
    {
        Console.WriteLine($"Test value: {testValue}");
        part1Sum += testValue;
        foreach (var combo in validCombos)
        {
            Console.WriteLine(string.Join(" ", combo.Select(o => o switch { Operator.Add => "+", Operator.Multiply => "*", _ => throw new InvalidOperationException() })));
        }
    }
}

Console.WriteLine($"Part 1 sum: {part1Sum}");

static List<List<Operator>> GetMatchingOperators(long target, ReadOnlySpan<long> values)
{
    var validCombos = GetMatchingOperators(target, values, [[]]);
    return validCombos.Select(PopToList).ToList();

    static List<ImmutableStack<Operator>> GetMatchingOperators(long target, ReadOnlySpan<long> values, List<ImmutableStack<Operator>> operators)
    {
        var lastValue = values[^1];

        // if we are at the front of the list, and the 1 value equals the target, then the input sets of operators are valid, so return them
        if (values.Length == 1)
            return lastValue == target ? operators : ([]);

        (var q, var rem) = Math.DivRem(target, lastValue);

        var result = new List<ImmutableStack<Operator>>();

        if (rem == 0)
        {
            // if target is divisible by last value, then last operator may be multiply
            // so push Multiply on to each current set of operators and recurse down
            var o = operators.Select(operators => operators.Push(Operator.Multiply)).ToList();
            result.AddRange(GetMatchingOperators(q, values[..^1], o));
        }

        // push Add on to each current set of operators and recurse down
        var o2 = operators.Select(operators => operators.Push(Operator.Add)).ToList();
        result.AddRange(GetMatchingOperators(target - lastValue, values[..^1], o2));

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

enum Operator { Add, Multiply }
