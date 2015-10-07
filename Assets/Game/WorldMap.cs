using System;
using System.Collections.Generic;


public class WorldMap {
    static readonly TileTypes tiletypes = new TileTypes ();
    static readonly Random random = new Random ();
    public bool invisible;
    public List<HexMap> dungeon = new List<HexMap> ();
    public int x, y, z;

    public WorldMap () {
        dungeon.Add (create_overworld (100, 100));
        dungeon.Add (create_dungeon_wall (100, 100, "wall-hewn"));
        dungeon.Add (create_dungeon_wall (100, 100, "earthy-wall-hewn"));
        dungeon.Add (create_dungeon_chasm (100, 100));
    }

    public void up () {
        if (z > 0)
            z--;
    }

    public void down () {
        if (z < 3)
            z++;
    }

    public void select (double x, double y) {
        dungeon [z].get_hex_position (new Position (x / 54, y / -36),
            out this.x, out this.y);
    }

    HexMap create_overworld (int w, int h) {
        var map = new HexMap (w, h);
        map.visibility = 9;
        var water = tiletypes.get ("water");
        var grass = tiletypes.get ("green");
        var pine = tiletypes.get ("pine");
        var deciduous_fall = tiletypes.get ("deciduous-fall");
        var deciduous = tiletypes.get ("deciduous");
        var mixed = tiletypes.get ("mixed");
        var mixed_fall = tiletypes.get ("mixed-fall");
        var building = tiletypes.get ("building");
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                var t = map.get (x, y);
                var r = random.Next ();
                t.variant = r;
                if (r % 100 < 80) {
                    t.tile = grass;
                    if (r % 100 < 5) {
                        t.obj = pine;
                    } else if (r % 100 < 10) {
                        t.obj = deciduous_fall;
                    } else if (r % 100 < 15) {
                        t.obj = deciduous;
                    } else if (r % 100 < 20) {
                        t.obj = mixed;
                    } else if (r % 100 < 25) {
                        t.obj = mixed_fall;
                    } else if (r % 100 < 30) {
                        t.obj = building;
                    }
                } else {
                    t.tile = water;
                }
            }
        }
        return map;
    }

    HexMap create_dungeon_wall (int w, int h, string walltype) {
        var map = new HexMap (w, h);
        map.visibility = 4;
        var floor = tiletypes.get ("floor");
        var wall = tiletypes.get (walltype);
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                var t = map.get (x, y);
                var r = random.Next ();
                t.variant = r;
                if (r % 100 < 20) {
                    t.tile = wall;
                } else {
                    t.tile = floor;
                }
            }
        }
        return map;
    }

    HexMap create_dungeon_chasm (int w, int h) {
        var map = new HexMap (w, h);
        map.visibility = 4;
        var floor = tiletypes.get ("floor");
        var abyss = tiletypes.get ("abyss");
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                var t = map.get (x, y);
                var r = random.Next ();
                t.variant = r;
                if (r % 100 < 20) {
                    t.tile = abyss;
                } else {
                    t.tile = floor;
                }
            }
        }
        return map;
    }
}
