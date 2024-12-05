using System.Text.RegularExpressions;

var input = File.ReadAllText("input.txt");
var sum = 0;
var enabled = true;
foreach (Match match in MyRegex.Regex.Matches(input))
{
    var func = match.Groups["func"].Value;

    if (func == "do")
    {
        enabled = true;
        continue;
    }

    if (func == "don't")
    {
        enabled = false;
        continue;
    }

    if (!enabled)
        continue;

    var a = int.Parse(match.Groups[1].Value);
    var b = int.Parse(match.Groups[2].Value);
    Console.WriteLine($"mul({a},{b}) = {a * b}");
    sum += a * b;
}
Console.WriteLine($"Sum: {sum}");

static partial class MyRegex
{
    [GeneratedRegex(@"(?<func>mul)\((\d{1,3}),(\d{1,3})\)|(?<func>do)\(\)|(?<func>don't)\(\)")]
    public static partial Regex Regex { get; }
}