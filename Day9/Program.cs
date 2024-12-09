const int Empty = -1;
var input = File.ReadAllText("input.txt").Trim();

var memory = new List<int>();

var fileId = 0;
var isFile = true;
foreach (var c in input)
{
    var len = Parse(c);
    for (var i = 0; i < len; i++)
        memory.Add(isFile ? fileId : Empty);

    if (isFile) fileId++;
    isFile = !isFile;
}

Print(memory);

var emptyIndex = 0;
var endIndex = memory.Count - 1;
while (emptyIndex < endIndex)
{
    while (memory[emptyIndex] != Empty) emptyIndex++;
    while (memory[endIndex] == Empty) endIndex--;
    if (emptyIndex < endIndex)
    {
        memory[emptyIndex] = memory[endIndex];
        memory[endIndex] = Empty;
    }
}

Print(memory);

long checksum = 0;
for (var i = 0; i < memory.Count; i++)
{
    if (memory[i] == Empty) break;
    checksum += memory[i] * i;
}

Console.WriteLine($"Checksum: {checksum}");

static byte Parse(char c) => c switch
{
    '0' => 0,
    '1' => 1,
    '2' => 2,
    '3' => 3,
    '4' => 4,
    '5' => 5,
    '6' => 6,
    '7' => 7,
    '8' => 8,
    '9' => 9,
    _ => throw new InvalidOperationException($"unexpected char: {c}"),
};

static void Print(List<int> memory) => Console.WriteLine(string.Join("", memory.Select(b => b == Empty ? "." : b.ToString())));