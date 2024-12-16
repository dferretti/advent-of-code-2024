var file = "input.txt";
Part1(file);
Part2(file);

static void Part1(string file)
{
    var lines = File.ReadLines(file);

    var tiles = lines
        .TakeWhile(line => !string.IsNullOrWhiteSpace(line))
        .Select(l => l.ToCharArray().Select(ParseTile).ToArray())
        .ToArray();
    var grid = new Grid(tiles);
    var robot = tiles
        .SelectMany((row, r) => row.Select((tile, c) => (Tile: tile, Position: new Position(r, c))))
        .Single(x => x.Tile == Tile.Robot)
        .Position;

    var moves = lines
        .SkipWhile(line => !string.IsNullOrWhiteSpace(line))
        .SelectMany(line => line.ToCharArray())
        .Select(ParseMove);

    grid.Print();

    var step = 0;
    foreach (var move in moves)
    {
        step++;
        var canMove = true;
        var numBoxes = 0;
        var cursor = robot;
        while (true)
        {
            cursor += move;
            numBoxes++;

            if (!grid.IsInbounds(cursor) || grid[cursor] == Tile.Wall)
            {
                canMove = false;
                break;
            }
            else if (grid[cursor] == Tile.Empty)
            {
                break;
            }
        }

        if (!canMove) continue;

        grid[robot + move * numBoxes] = Tile.Box;
        grid[robot + move] = Tile.Robot;
        grid[robot] = Tile.Empty;
        robot += move;

        //Console.WriteLine($"Step {step}: {move.Display}");
        //grid.Print();
    }

    Console.WriteLine($"Part 1 sum: {grid.SumCoords()}");
}

static void Part2(string file)
{
    var lines = File.ReadLines(file);

    var tiles = lines
        .TakeWhile(line => !string.IsNullOrWhiteSpace(line))
        .Select(l => l.ToCharArray().Select(ParseTile).SelectMany(Expand).ToArray())
        .ToArray();
    var grid = new Grid(tiles);
    var robot = tiles
        .SelectMany((row, r) => row.Select((tile, c) => (Tile: tile, Position: new Position(r, c))))
        .Single(x => x.Tile == Tile.Robot)
        .Position;

    var moves = lines
        .SkipWhile(line => !string.IsNullOrWhiteSpace(line))
        .SelectMany(line => line.ToCharArray())
        .Select(ParseMove);

    grid.Print();

    var step = 0;
    foreach (var move in moves)
    {
        step++;
        var canMove = true;
        Stack<(Position Position, Tile Replacement)> positions = new([(robot, Tile.Empty)]);
        List<(Position Position, Tile Replacement)> nextSet = [(robot, Tile.Empty)];
        while (true)
        {
            nextSet = nextSet
                .Where(x => grid[x.Position] != Tile.Empty)
                .Select(x => (x.Position + move, grid[x.Position]))
                .ToList();

            // if moving up or down, see if we are at the edge of a box
            if (move.R != 0)
            {
                /*
                 * [][]  []
                 *  []  []
                 *   [][]
                 *    []
                 *     @
                 */
                for (var i = 0; i < nextSet.Count; i++)
                {
                    if (grid[nextSet[i].Position] == Tile.RightBox)
                    {
                        if (i == 0 || nextSet[i - 1].Position.C != nextSet[i].Position.C - 1)
                            nextSet.Insert(i, (nextSet[i].Position + Move.Left, Tile.Empty));
                    }
                    else if (grid[nextSet[i].Position] == Tile.LeftBox)
                    {
                        if (i == nextSet.Count - 1 || nextSet[i + 1].Position.C != nextSet[i].Position.C + 1)
                            nextSet.Insert(i + 1, (nextSet[i].Position + Move.Right, Tile.Empty));
                    }
                }
            }

            foreach (var x in nextSet) positions.Push(x);

            if (nextSet.Any(x => !grid.IsInbounds(x.Position) || grid[x.Position] == Tile.Wall))
            {
                canMove = false;
                break;
            }
            else if (nextSet.All(x => grid[x.Position] == Tile.Empty))
            {
                break;
            }
        }

        if (!canMove) continue;

        while (positions.TryPop(out var pos))
            grid[pos.Position] = pos.Replacement;

        robot += move;

        // Console.Clear();
        // Console.WriteLine($"Step {step}: {move.Display}");
        // grid.Print();
        // Console.ReadLine();
        // Thread.Sleep(50);
    }

    grid.Print();
    Console.WriteLine($"Part 2 sum: {grid.SumCoords()}");
}

static Move ParseMove(char c) => c switch
{
    '^' => Move.Up,
    'v' => Move.Down,
    '<' => Move.Left,
    '>' => Move.Right,
    _ => throw new InvalidOperationException($"Unexpected move: {c}"),
};

static Tile ParseTile(char c) => c switch
{
    '.' => Tile.Empty,
    '#' => Tile.Wall,
    'O' => Tile.Box,
    '@' => Tile.Robot,
    _ => throw new InvalidOperationException($"Unexpected tile: {c}"),
};

static Tile[] Expand(Tile tile) => tile switch
{
    Tile.Wall => [Tile.Wall, Tile.Wall],
    Tile.Box => [Tile.LeftBox, Tile.RightBox],
    Tile.Empty => [Tile.Empty, Tile.Empty],
    Tile.Robot => [Tile.Robot, Tile.Empty],
    _ => throw new InvalidOperationException(),
};

enum Tile { Empty, Wall, Box, Robot, LeftBox, RightBox }
record Position(int R, int C)
{
    public static Position operator +(Position p, Move m) => new(p.R + m.R, p.C + m.C);
}
record Move(int R, int C, char Display)
{
    public static readonly Move Up = new(-1, 0, '^');
    public static readonly Move Down = new(1, 0, 'v');
    public static readonly Move Left = new(0, -1, '<');
    public static readonly Move Right = new(0, 1, '>');
    public static Move operator *(Move m, int n) => new(m.R * n, m.C * n, m.Display);
}
class Grid(Tile[][] tiles)
{
    public Tile this[Position p]
    {
        get => tiles[p.R][p.C];
        set => tiles[p.R][p.C] = value;
    }

    public bool IsInbounds(Position p) =>
        p.R >= 0 && p.R < tiles.Length && p.C >= 0 && p.C < tiles[0].Length;

    public void Print()
    {
        foreach (var row in tiles)
        {
            foreach (var tile in row)
            {
                Console.Write(tile switch
                {
                    Tile.Empty => '.',
                    Tile.Wall => '#',
                    Tile.Box => 'O',
                    Tile.Robot => '@',
                    Tile.LeftBox => '[',
                    Tile.RightBox => ']',
                    _ => throw new InvalidOperationException(),
                });
            }
            Console.WriteLine();
        }
    }

    public int SumCoords() => tiles
        .SelectMany((row, r) => row.Select((tile, c) => (r, c, tile)))
        .Sum(x => x.tile is Tile.Box or Tile.LeftBox ? (100 * x.r) + x.c : 0);
}
