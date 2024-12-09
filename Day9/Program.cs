var input = File.ReadAllText("input.txt").Trim();

Part1(input);
Part2(input);

static void Part1(string input)
{
    const int Empty = -1;
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

    Console.WriteLine($"Part 1 Checksum: {checksum}");

    static void Print(List<int> memory) => Console.WriteLine(string.Join("", memory.Select(b => b == Empty ? "." : b.ToString())));
}

static void Part2(string input)
{
    var memory = new List<int>();

    var fileLengths = new Dictionary<int, int>();
    {
        var fileId = 0;
        var isFile = true;
        foreach (var c in input)
        {
            var len = Parse(c);
            for (var i = 0; i < len; i++)
                memory.Add(isFile ? fileId : -len); // store the length for easy access later

            if (isFile)
            {
                fileLengths[fileId] = len;
                fileId++;
            }

            isFile = !isFile;
        }
    }

    //Print(memory);

    var endIndex = memory.Count - 1;
    var moved = new HashSet<int>();
    while (endIndex >= 0)
    {
        //var emptyIndex = 0;
        //while (memory[emptyIndex] >= 0) emptyIndex++;
        while (memory[endIndex] < 0) endIndex--;
        var fileId = memory[endIndex];

        //if (endIndex >= emptyIndex) break;

        var fileLength = fileLengths[fileId];
        var emptyIndex = -1;
        var available = -1;
        for (var nextEmpty = 0; nextEmpty < endIndex; nextEmpty++)
        {
            if (memory[nextEmpty] >= 0) continue;
            available = -memory[nextEmpty];
            if (available >= fileLength)
            {
                emptyIndex = nextEmpty;
                break;
            }
        }

        if (!moved.Add(fileId))
        {
            Console.WriteLine($"already moved {fileId}");
            endIndex -= fileLength;
            continue;
        }

        if (emptyIndex == -1)
        {
            Console.WriteLine($"can't move {fileId}. {endIndex} - {fileLength}");
            endIndex -= fileLength;
            continue;
        }

        for (var i = 0; i < fileLength; i++)
        {
            (memory[endIndex], memory[emptyIndex]) = (memory[emptyIndex], memory[endIndex]);
            emptyIndex++;
            endIndex--;
        }

        for (var i = fileLength; i < available; i++)
            memory[emptyIndex++] = fileLength - available;

        //Print(memory);
    }

    //Print(memory);

    long checksum = 0;
    for (var i = 0; i < memory.Count; i++)
    {
        if (memory[i] < 0) continue;
        checksum += memory[i] * i;
    }

    Console.WriteLine($"Part 2 Checksum: {checksum}");

    static void Print(List<int> memory)
    {
        foreach (var x in memory)
        {
            if (x >= 0)
            {
                Console.Write(x);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(-x);
                Console.ResetColor();
            }
        }
        Console.WriteLine();
    }
}

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
