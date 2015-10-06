using System;
using System.Linq;
using System.Collections.Generic;

class Tuple<T1, T2> {
    public T1 a;
    public T2 b;

    public Tuple (T1 a, T2 b) {
        this.a = a;
        this.b = b;
    }
}

public class TileType {
    public List<Picture> variants = new List<Picture> ();
    /*

    A transition can be up to 6 tiles, one for each of the 6 neighbors. A single picture
    can cover more than one neighbor though - in case all 64 possible transitions are
    present only a single picture would be draw each time. For example transition "63"
    means a transition all around, tansition "28" means transition to tiles  2, 3, 4 and
    transition 32 means only transition to tile 5.
        __
     __/0 \__
    /5 \__/1 \
    \__/  \__/
    /4 \__/2 \
    \__/3 \__/
       \__/

        __
     __/1 \__
    /32\__/2 \
    \__/  \__/
    /16\__/4 \
    \__/8 \__/
       \__/

    If the tile is a wall, transitions work differently. Each hexagon vertex which is
    convex (one of the three hexagons is a wall) gets a convex vertex overlay. At the same
    time each vertex which is concave (two walls) gets a different vertex overlay. When
    a tile is drawn the two top vertices are handled.

    left:

     __/1 \__
     2 \__/   
     __/4 \__
       \  /

    64 0 -
    65 1 convex bl
    66 2 convex r
    67 3 concave tl
    68 4 convex tl
    69 5 concave r
    70 6 concave bl
    71 7 -

    right:

    72 0 -
    73 1 convex br
    74 2 convex l
    75 3 concave tr
    76 4 convex tr
    77 5 concave l
    78 6 concave br
    79 7 -

     __/1 \__
       \__/2  
     __/4 \__
       \  /

    */
    public Dictionary<int, Picture> transitions = new Dictionary<int, Picture> ();
    public bool wall;
    public bool animated;
    public bool tiled;
    public bool visible;
    public int layer;
}

public class HexTile {
    public TileType tile;
    public int variant;
    public TileType obj;
    public float dark;
    public int visible;
}

public class TileTypes {
    readonly Dictionary<string, TileType> by_name = new Dictionary<string, TileType> ();

    TileType add (string name, string path = "", string list = "") {
        var tt = new TileType ();
        by_name.Add (name, tt);
 
        var pics = from x in list.Split ('\n')
                         where x.Trim ().Length > 0
                         select x.Trim ();
        foreach (var t in pics) {
            tt.variants.Add (new Picture (path + t));
        }
        return tt;
    }

    delegate Tuple<int, string> GetCode (string name);

    static void add_transitions (TileType tt, string path = "", string list = "") {

        GetCode trans = name => {
            int x = 0;
            Func<string, bool> has = a => name.Contains ("-" + a + "-") || name.Contains ("-" + a + "."); 
            if (has ("n"))
                x += 1;
            if (has ("ne"))
                x += 2;
            if (has ("se"))
                x += 4;
            if (has ("s"))
                x += 8;
            if (has ("sw"))
                x += 16;
            if (has ("nw"))
                x += 32;
            if (has ("convex-bl"))
                x = 65;
            if (has ("convex-r"))
                x = 66;
            if (has ("concave-tl"))
                x = 67;
            if (has ("convex-tl"))
                x = 68;
            if (has ("concave-r"))
                x = 69;
            if (has ("concave-bl"))
                x = 70;
            if (has ("convex-br"))
                x = 73;
            if (has ("convex-l"))
                x = 74;
            if (has ("concave-tr"))
                x = 75;
            if (has ("convex-tr"))
                x = 76;
            if (has ("concave-l"))
                x = 77;
            if (has ("concave-br"))
                x = 78; 
      
            return new Tuple<int, string> (x, path + name);
        };
        var transitions = from x in list.Split ('\n')
                                where x.Trim ().Length > 0
                                select trans (x.Trim ());
        foreach (var t in transitions) {
            tt.transitions.Add (t.a, new Picture (t.b));
        }
    }

    public TileTypes () {
        var p = "images/terrain/grass/";
        var tt = add ("green", p, @"
            green.png
            green2.png
            green3.png
            green4.png
            green5.png
            green6.png
            green7.png
            green8.png
                ");
        tt.layer = 1;
        add_transitions (tt, p, @"
            green-ne.png
            green-ne-se.png
            green-ne-se-s.png
            green-n-ne.png
            green-n-ne-se.png
            green-n.png
            green-nw-n-ne.png
            green-nw-n.png
            green-nw.png
            green-se.png
            green-se-s.png
            green-se-s-sw.png
            green-s.png
            green-s-sw-nw-n-ne.png
            green-s-sw-nw-n-ne-se.png
            green-s-sw-nw.png
            green-s-sw.png
            green-sw-nw-n-ne-se.png
            green-sw-nw-n.png
            green-sw-nw.png
            green-sw.png
                ");
                              
        p = "images/terrain/forest/";
        add ("pine", p, @"
            pine.png
            pine2.png
            pine3.png
            pine4.png
            pine-small.png
            pine-small2.png
            pine-sparse.png
            pine-sparse2.png
            pine-sparse3.png
            pine-sparse4.png
            pine-sparse-small.png
            ");

        add ("deciduous-fall", p, @"
            deciduous-fall.png
            deciduous-fall2.png
            deciduous-fall3.png
            deciduous-fall-small.png
            deciduous-fall-sparse.png
            deciduous-fall-sparse2.png
            deciduous-fall-sparse3.png
            deciduous-fall-sparse-small.png
            ");

        add ("deciduous", p, @"
            deciduous-summer.png
            deciduous-summer2.png
            deciduous-summer3.png
            deciduous-summer4.png
            deciduous-summer-small.png
            deciduous-summer-sparse.png
            deciduous-summer-sparse2.png
            deciduous-summer-sparse3.png
            deciduous-summer-sparse-small.png
            ");

        add ("mixed-fall", p, @"
            mixed-fall.png
            mixed-fall2.png
            mixed-fall-small.png
            mixed-fall-sparse.png
            mixed-fall-sparse2.png
            mixed-fall-sparse-small.png
            ");

        add ("mixed", p, @"
            mixed-summer.png
            mixed-summer2.png
            mixed-summer-small.png
            mixed-summer-sparse.png
            mixed-summer-sparse2.png
            mixed-summer-sparse-small.png
            ");

        p = "images/terrain/village/";
        add ("building", p, @"
        camp.png
        cave.png
        cave2.png
        cave3.png
        coast.png
        coast2.png
        coast3.png
        coast4.png
        coast5-A01.png
        coast5-A02.png
        coast5-A03.png
        coast5-A04.png
        desert.png
        desert2.png
        desert3.png
        desert4.png
        desert-camp.png
        desert-oasis-1.png
        desert-oasis-2.png
        desert-oasis-3.png
        drake1.png
        drake1-A01.png
        drake1-A02.png
        drake1-A03.png
        drake2.png
        drake2-A01.png
        drake2-A02.png
        drake2-A03.png
        drake2-A04.png
        drake3.png
        drake4.png
        drake5.png
        dwarven.png
        dwarven2.png
        dwarven3.png
        dwarven4.png
        elven.png
        elven2.png
        elven3.png
        elven4.png
        elven-snow.png
        elven-snow2.png
        elven-snow3.png
        elven-snow4.png
        human.png
        human2.png
        human3.png
        human4.png
        human-city.png
        human-city2.png
        human-city3.png
        human-city4.png
        human-city-ruin.png
        human-city-ruin2.png
        human-city-ruin3.png
        human-city-ruin4.png
        human-city-snow.png
        human-city-snow2.png
        human-city-snow3.png
        human-city-snow4.png
        human-cottage-ruin.png
        human-cottage-ruin2.png
        human-cottage-ruin3.png
        human-cottage-ruin4.png
        human-hills.png
        human-hills-ruin.png
        human-snow.png
        human-snow2.png
        human-snow3.png
        human-snow4.png
        human-snow-hills.png
        hut.png
        hut2.png
        hut-snow.png
        hut-snow2.png
        igloo.png
        igloo2.png
        log-cabin.png
        log-cabin2.png
        log-cabin3.png
        log-cabin4.png
        log-cabin-snow.png
        orc.png
        orc2.png
        orc3.png
        orc4.png
        orc-snow.png
        orc-snow2.png
        orc-snow3.png
        orc-snow4.png
        swampwater.png
        swampwater2.png
        swampwater3.png
        tropical-forest.png
        tropical-forest2.png
        tropical-forest3.png
        ");

        p = "images/terrain/cave/";
        add ("floor", p, @"
            floor.png
            floor2.png
            floor3.png
            floor4.png
            floor5.png
            floor6.png
            ");

        tt = add ("wall-hewn", p, @"
            wall-rough.png
            ");
        tt.wall = true;
        add_transitions (tt, p, @"
            wall-hewn-concave-bl.png
            wall-hewn-concave-br.png
            wall-hewn-concave-l.png
            wall-hewn-concave-r.png
            wall-hewn-concave-tl.png
            wall-hewn-concave-tr.png
            wall-hewn-convex-bl.png
            wall-hewn-convex-br.png
            wall-hewn-convex-l.png
            wall-hewn-convex-r.png
            wall-hewn-convex-tl.png
            wall-hewn-convex-tr.png
            ");

        tt = add ("earthy-wall-hewn", p, @"
            wall-rough.png
            ");
        tt.wall = true;
        add_transitions (tt, p, @"
            earthy-wall-hewn-concave-bl.png
            earthy-wall-hewn-concave-br.png
            earthy-wall-hewn-concave-l.png
            earthy-wall-hewn-concave-r.png
            earthy-wall-hewn-concave-tl.png
            earthy-wall-hewn-concave-tr.png
            earthy-wall-hewn-convex-bl.png
            earthy-wall-hewn-convex-br.png
            earthy-wall-hewn-convex-l.png
            earthy-wall-hewn-convex-r.png
            earthy-wall-hewn-convex-tl.png
            earthy-wall-hewn-convex-tr.png
            ");

        p = "images/terrain/chasm/";
        tt = add ("abyss", p, @"
            abyss.png          
            abyss2.png
            abyss3.png
            abyss4.png
            abyss5.png
            abyss6.png
            abyss7.png
            ");
        tt.wall = true;
        add_transitions (tt, p, @"
            regular-concave-bl.png
            regular-concave-br.png
            regular-concave-l.png
            regular-concave-r.png
            regular-concave-tl.png
            regular-concave-tr.png
            regular-convex-bl.png
            regular-convex-br.png
            regular-convex-l.png
            regular-convex-r.png
            regular-convex-tl.png
            regular-convex-tr.png
            ");

        p = "images/terrain/water/";
        tt = add ("water", p, @"
            water01.png
            water02.png
            water03.png
            water04.png
            water05.png
            water06.png
            water07.png
            water08.png
            water09.png
            water10.png
            water11.png
            water12.png
            water13.png
            water14.png
            water15.png
            water16.png
            water17.png
        ");
        tt.animated = true;
        tt.tiled = true;
        tt.layer = 0;
    }

    public TileType get (string name) {
        return by_name [name];
    }

    public TileType[] get_all () {
        return by_name.Values.ToArray ();
    }
}

