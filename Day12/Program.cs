var chars = File.ReadAllLines("input.txt").Select(line => line.ToCharArray()).ToArray();
var plots = chars.Select((row, r) => row.Select((col, c) => GetPlot(chars, r, c)).ToArray()).ToArray();

var regions = new List<Region>();
var regionMap = chars.Select(row => new Region[row.Length]).ToArray();

var outside = new Region('-');
for (var r = 0; r < chars.Length; r++)
{
    for (var c = 0; c < chars.Length; c++)
    {
        var plot = plots[r][c]; // GetPlot(chars, r, c);
        var type = plot.Type; // chars[r][c];
        var northRegion = r > 0 ? regionMap[r - 1][c] : outside;
        var westRegion = c > 0 ? regionMap[r][c - 1] : outside;

        if (northRegion.Type == type && westRegion.Type == type)
        {
            northRegion.AddPlot(plot, plots);

            // if 2 regions are joined by this new corner plot, merge them and remove the west region
            if (northRegion != westRegion)
            {
                foreach (var p in westRegion.Plots) regionMap[p.R][p.C] = northRegion;
                regions.Remove(westRegion);
                northRegion.Merge(westRegion);
            }

            regionMap[r][c] = northRegion;
        }
        else if (northRegion.Type == type)
        {
            northRegion.AddPlot(plot, plots);
            regionMap[r][c] = northRegion;
        }
        else if (westRegion.Type == type)
        {
            westRegion.AddPlot(plot, plots);
            regionMap[r][c] = westRegion;
        }
        else
        {
            var region = new Region(type);
            region.AddPlot(plot, plots);
            regions.Add(region);
            regionMap[r][c] = region;
        }
    }
}

// foreach (var region in regions) Console.WriteLine($"Region {region.Type}: {region.Area} plots, {region.Perimeter} fences, {region.Price} price");
// foreach (var region in regions) Console.WriteLine($"Region {region.Type}: {region.Area} plots, {region.Sides} Sides, {region.Price2} price2");
Console.WriteLine($"Part 1 total price: {regions.Sum(region => region.Price)}");
Console.WriteLine($"Part 2 total price: {regions.Sum(region => region.Price2)}");

/*
 * B A C
 * A A C
 * 
 * B B B A C
 * A A B A C
 * A A A A C
 */

static Plot GetPlot(char[][] chars, int r, int c)
{
    var type = chars[r][c];
    var n = (r <= 0 || chars[r - 1][c] != type);
    var e = (c >= chars[r].Length - 1 || chars[r][c + 1] != type);
    var s = (r >= chars.Length - 1 || chars[r + 1][c] != type);
    var w = (c <= 0 || chars[r][c - 1] != type);
    var fence = new Fence(n, e, s, w);
    return new(type, r, c, fence);
}

record Plot(char Type, int R, int C, Fence Fences);

class Region(char type)
{
    private readonly HashSet<Plot> _plots = [];

    public char Type => type;

    public int Area => _plots.Count;

    public int Perimeter => _plots.Sum(plot => plot.Fences.Count);

    public int Price => Area * Perimeter;

    public int Sides { get; private set; }

    public int Price2 => Area * Sides;

    public void AddPlot(Plot plot, Plot[][] chars)
    {
        _plots.Add(plot);

        // if it has a north fence, check the left neighbor to see if it is a new side
        if (plot.Fences.N)
        {
            var left = plot.C > 0 ? chars[plot.R][plot.C - 1] : null;
            if (left == null || left.Type != plot.Type || !left.Fences.N) Sides++;
        }

        if (plot.Fences.E)
        {
            var up = plot.R > 0 ? chars[plot.R - 1][plot.C] : null;
            if (up == null || up.Type != plot.Type || !up.Fences.E) Sides++;
        }

        if (plot.Fences.S)
        {
            var left = plot.C > 0 ? chars[plot.R][plot.C - 1] : null;
            if (left == null || left.Type != plot.Type || !left.Fences.S) Sides++;
        }

        if (plot.Fences.W)
        {
            var up = plot.R > 0 ? chars[plot.R - 1][plot.C] : null;
            if (up == null || up.Type != plot.Type || !up.Fences.W) Sides++;
        }
    }

    public void Merge(Region region)
    {
        _plots.UnionWith(region._plots);
        region._plots.Clear();
        Sides += region.Sides;
    }

    public IEnumerable<Plot> Plots => _plots;
}

record Fence(bool N, bool E, bool S, bool W)
{
    public int Count => (N ? 1 : 0) + (E ? 1 : 0) + (S ? 1 : 0) + (W ? 1 : 0);
}