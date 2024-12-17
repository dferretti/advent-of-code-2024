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
    >=0 and <= 3 => program[pc + 1],
    4 => a,
    5 => b,
    6 => c,
    _ => throw new InvalidOperationException($"Unexpected combo operand: {program[pc + 1]}"),
};

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