// grid of plots including what fences each square has
Plot[][] plots = GetPlots("input.txt");

// grid of regions, initially empty. built top-left to bottom-right
// r = regionMap[r][c] means that plot[r][c] belongs to r. many cells will contain the same region instances
Region[][] regionMap = plots.Select(row => new Region[row.Length]).ToArray();

// distinct set of regions. using List so we can check order of regions added vs example in problem description
List<Region> regions = [];

for (var r = 0; r < plots.Length; r++)
{
    for (var c = 0; c < plots.Length; c++)
    {
        var plot = plots[r][c];
        var northRegion = r > 0 ? regionMap[r - 1][c] : null;
        var westRegion = c > 0 ? regionMap[r][c - 1] : null;

        if (northRegion?.Type == plot.Type && westRegion?.Type == plot.Type)
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
        else if (northRegion?.Type == plot.Type)
        {
            northRegion.AddPlot(plot, plots);
            regionMap[r][c] = northRegion;
        }
        else if (westRegion?.Type == plot.Type)
        {
            westRegion.AddPlot(plot, plots);
            regionMap[r][c] = westRegion;
        }
        else
        {
            var region = new Region(plot.Type);
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


static Plot[][] GetPlots(string fileName)
{
    var chars = File.ReadAllLines(fileName).Select(line => line.ToCharArray()).ToArray();
    return chars.Select((row, r) => row.Select((col, c) => GetPlot(chars, r, c)).ToArray()).ToArray();

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

    public void AddPlot(Plot plot, Plot[][] plots)
    {
        _plots.Add(plot);

        var left = plot.C > 0 ? plots[plot.R][plot.C - 1] : null;
        var up = plot.R > 0 ? plots[plot.R - 1][plot.C] : null;

        // if it has a north fence, check the left neighbor to see if it is a new side
        if (plot.Fences.N && (left == null || left?.Type != plot.Type || !left.Fences.N))
            Sides++;

        if (plot.Fences.E && (up == null || up.Type != plot.Type || !up.Fences.E))
            Sides++;

        if (plot.Fences.S && (left == null || left.Type != plot.Type || !left.Fences.S))
            Sides++;

        if (plot.Fences.W && (up == null || up.Type != plot.Type || !up.Fences.W))
            Sides++;
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