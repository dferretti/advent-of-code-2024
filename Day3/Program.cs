using System.Text.RegularExpressions;

var input = File.ReadAllText("input.txt");
var sum = 0;
foreach (Match match in MyRegex.Regex.Matches(input))
{
    var a = int.Parse(match.Groups[1].Value);
    var b = int.Parse(match.Groups[2].Value);
    Console.WriteLine($"mul({a},{b}) = {a * b}");
    sum += a * b;
}
Console.WriteLine($"Sum: {sum}");

static partial class MyRegex
{
    [GeneratedRegex(@"mul\((\d{1,3}),(\d{1,3})\)")]
    public static partial Regex Regex { get; }
}