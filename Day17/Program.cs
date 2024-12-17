Part1();
Part2();

static void Part1()
{
    var (a, b, c, program) = Parse("input.txt");

    var pc = 0;
    var output = new List<int>();

    while (pc < program.Length)
    {
        switch (program[pc])
        {
            case 0:
                a = a / (1 << Combo());
                break;
            case 1:
                b = b ^ Literal();
                break;
            case 2:
                b = Combo() & 7;
                break;
            case 3:
                if (a != 0) pc = Literal() - 2;
                break;
            case 4:
                b ^= c;
                break;
            case 5:
                output.Add(Combo() & 7);
                break;
            case 6:
                b = a / (1 << Combo());
                break;
            case 7:
                c = a / (1 << Combo());
                break;
            default:
                throw new InvalidOperationException($"Unexpected opcode: {program[pc]}");
        }

        pc += 2;
    }

    Console.WriteLine(string.Join(",", output));


    int Literal() => program[pc + 1];

    int Combo() => program[pc + 1] switch
    {
        >= 0 and <= 3 => program[pc + 1],
        4 => a,
        5 => b,
        6 => c,
        _ => throw new InvalidOperationException($"Unexpected combo operand: {program[pc + 1]}"),
    };
}

static void Part2()
{
    //var (_, _, _, program) = Parse("input.txt");
    /*var max = (long)Math.Pow(8, 16);
    for (var a = (long)Math.Pow(8, 15); a < max; a++)
    {
        if (a % 10_000_000 == 0) Console.WriteLine(a);

        if (PrintsSelf2(a, program))
        {
            Console.WriteLine($"Part 2: {a}");
            break;
        }
    }*/

    /*

    2,4: B = A & 7
    1,6: B = B ^ 6
    7,5: C = A / (1 << B)
    4,4: B = B ^ C
    1,7: B = B ^ 7
    0,3: A = A / 8
    5,5: output B & 7
    3,0: if A != 0 goto 0

    - calls output once per loop
    - program length is 16, so loops 16 times
    - each loop, A is divided by 8
    - so A is between 8^15 and 8^16

    - last A is 0 and exits loop
    - second to last A is 1 to 7, and prints '0'
    - third to last A will be above value shifted left 3 bits, plus any combo of 3 bits, and prints '3', etc
    - loop backwards, build possible values of A that will print expected output, then shift left and find next A

*/

    List<long> possibleMatches = [0L];
    int[] program = [2, 4, 1, 6, 7, 5, 4, 4, 1, 7, 0, 3, 5, 5, 3, 0];
    for (var i = program.Length - 1; i >= 0; i--)
    {
        var expectedOutput = program[i];
        possibleMatches = possibleMatches.SelectMany(x => PossibleMatches(x, expectedOutput)).ToList();
    }

    Console.WriteLine($"Part 2: {possibleMatches.Min()}");

    static List<long> PossibleMatches(long a, int expectedOutput)
    {
        var matches = new List<long>();
        for (var b = 0; b < 8; b++)
        {
            var nextA = (a << 3) + b;
            var output = Step(nextA);
            if (output == expectedOutput)
                matches.Add(nextA);
        }
        return matches;
    }

    /*static bool PrintsSelf(long a, ReadOnlySpan<int> program)
    {
        var b = 0L;
        var c = 0L;
        var pc = 0;
        int operand;
        var output = new List<int>(program.Length);

        while (pc < program.Length)
        {
            operand = program[pc + 1];
            switch (program[pc])
            {
                case 0:
                    a = a / (1 << (int)Combo());
                    break;
                case 1:
                    b = b ^ Literal();
                    break;
                case 2:
                    b = Combo() & 7;
                    break;
                case 3:
                    if (a != 0) pc = Literal() - 2;
                    break;
                case 4:
                    b ^= c;
                    break;
                case 5:
                    output.Add((int)Combo() & 7);
                    if (output.Count > program.Length) return false;
                    if (output[^1] != program[output.Count - 1]) return false;
                    break;
                case 6:
                    b = a / (1 << (int)Combo());
                    break;
                case 7:
                    c = a / (1 << (int)Combo());
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected opcode: {program[pc]}");
            }

            pc += 2;
        }

        return output.Count == program.Length;

        int Literal() => operand;

        long Combo() => operand switch
        {
            >= 0 and <= 3 => operand,
            4 => a,
            5 => b,
            6 => c,
            _ => throw new InvalidOperationException($"Unexpected combo operand: {operand}"),
        };
    }

    static bool PrintsSelf2(long a, ReadOnlySpan<int> program)
    {
        var outputCount = 0;
        while (a != 0)
        {
            var output = Step(a);
            a /= 8;
            if (output != program[outputCount]) return false;
            outputCount++;
            if (outputCount > program.Length) return false;
        }

        return outputCount == program.Length;
    }*/

    static int Step(long a)
    {
        var b = a & 7;
        b ^= 6;
        var c = a / (1 << (int)b);
        b ^= c;
        b ^= 7;
        return (int)b & 7;
    }
}

static (int A, int B, int C, int[] Program) Parse(string file)
{
    var lines = File.ReadAllLines(file);
    var a = ParseRegister(lines[0]);
    var b = ParseRegister(lines[1]);
    var c = ParseRegister(lines[2]);

    var program = lines[4].Split(' ')[1].Split(',').Select(int.Parse).ToArray();

    return (a, b, c, program);

    static int ParseRegister(string line) => int.Parse(line.Split(' ')[2]);
}
