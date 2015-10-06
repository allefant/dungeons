
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

/*
	The map is drawn tile by tile. To draw transitions, we only consider neighbors
	in a higher up layer. For example consider these layers:

	water < grass < mountain < snow
	
	If we draw a grass tile, we check our neighbors. If we encounter water we
	ignore it. If we encounter at least one mountain, we draw a transition
	to all mountains. Then if we also have at least one snow neighbor, we
	draw a snow transition on top of the mountain transition.
*/

public static class DrawMap {
    static public Position xy = new Position (0, 0);
    static Material darkness;
    static Picture cursor;

    public static void scroll (double sx, double sy) {
        xy.x += sx;
        xy.y += sy;
    }

    /* Get the pixel position of the center of the tile at diamond grid
     * x/y. The hexagon center is the diamond origin.
     * 
     */
    static Position getpos (HexMap map, int x, int y) {
        var s = map.tile_to_screen (new Position (x, y));
        s.x *= 54;
        s.x -= xy.x;
        s.y *= -36;
        s.y -= xy.y;
        return s;
    }

    static void drawC (Picture pic, Position pos) {
        Texture2D tex = pic.tex;
        float sx = (float)(pos.x);
        float sy = (float)(pos.y);
        var r = new Rect (sx - tex.width / 2, sy + tex.height / 2,
                    tex.width, -tex.height);
        Graphics.DrawTexture (r, tex); 
    }

    static void drawTiled (Picture pic, Position pos) {
        Texture2D tex = pic.tex;
        float sx = (float)(pos.x);
        float sy = (float)(pos.y);
        var r = new Rect (sx - 36, sy + 36, 72, -72);
        float tx = (float)(xy.x + sx) / tex.width;
        float ty = (float)(xy.y + sy) / tex.height;
        var sr = new Rect (tx, ty, 72.0f / tex.width, 72.0f / tex.height);
        Graphics.DrawTexture (r, tex, sr, 0, 0, 0, 0); 
    }

    static void drawTL (Picture pic, Position pos) {
        Texture2D tex = pic.tex;
        float sx = (float)(pos.x);
        float sy = (float)(pos.y);
        var r = new Rect (sx, sy,
                    tex.width, -tex.height);
        Graphics.DrawTexture (r, tex); 
    }

    struct DrawInfo {
        public Position s;
        public int x, y;

        public DrawInfo (Position s, int x, int y) {
            this.s = s;
            this.x = x;
            this.y = y;
        }
    }

    /* Note: This just runs over positions, so at the edge it will include outside
     * positions.
     */
    static IEnumerable<DrawInfo> drawer (HexMap map) {
        double rx = Screen.width / 2;
        double ry = Screen.height / 2;
        int x1, y1;
        double left = -rx;// + 100;
        double top = ry;// - 100;
        double right = rx - 128; // -100;
        double bottom = -ry;// + 100;
        map.get_hex_position (new Position (
            (xy.x + left) / 54, (xy.y + top) / -36), out x1, out y1);

        var s1 = getpos (map, x1, y1);
        if (s1.y - top < 0) {
            y1--;
            s1.x += 54;
            s1.y += 36;
        }
        for (; ;) {
            var x = x1;
            var y = y1;
            var s = s1;
            for (; ;) {
                if (s.y + 36 < bottom)
                    yield break;
                if (s.x - 36 > right)
                    break;

                yield return new DrawInfo (s, x, y);

                x++;
                y--;
                s.x += 108;
            }
            var flip = s1.x - left > 18;
            if (flip) {
                y1++;
                s1.x -= 54;
                s1.y -= 36;
            } else {
                x1++;
                s1.x += 54;
                s1.y -= 36;
            }
        }
    }

    static bool is_wall (HexTile t) {
        return t != null && t.tile != null && t.tile.wall;
    }

    public static void render_map (WorldMap world) {
        HexMap map = world.dungeon [world.z];
        // draw tiled layers
        foreach (var di in drawer(map)) {
            var t = map.get (di.x, di.y);
            if (t == null || t.tile == null)
                continue;
            int v = t.variant;
            if (t.tile.animated) {
                v = (int)(Time.time * 10);
            }
            v %= t.tile.variants.Count;
            if (t.tile.tiled) {
                drawTiled (t.tile.variants [v], di.s);
            }
        }

        // draw tiles + transitions
        foreach (var di in drawer(map)) {
            var t = map.get (di.x, di.y);
            int layer = 0;
            if (t != null && t.tile != null) {
                layer = t.tile.layer;
                int v = t.variant;
                if (t.tile.animated) {
                    v = (int)(Time.time * 10);
                }
                v %= t.tile.variants.Count;
                if (t.tile.tiled) {
                } // already drawn in previous pass
                else
                    drawC (t.tile.variants [v], di.s);
            }

            // draw transitions to higher layers
            TileType[] around = { null, null, null, null, null, null };
            for (int j = 0; j < 6; j++) {
                var nt = map.get_neighbor (di.x, di.y, j);
                if (nt == null)
                    continue;
                if (nt.tile == null)
                    continue;
                if (nt.tile.layer <= layer)
                    continue;
                around [j] = nt.tile;
            }
            for (;;) {
                // find next highest layer
                TileType nt = null;
                foreach (var x in around.Where(x => x != null)) {
                    if (nt == null || x.layer < nt.layer) {
                        nt = x;
                    }
                }
                if (nt == null)
                    break;

                // find transition mask
                int m = 0;
                for (int j = 0; j < 6; j++) {
                    if (around [j] == null)
                        continue;
                    if (around [j] == nt) {
                        m += 1 << j;
                    }
                }

                if (nt.transitions.ContainsKey (m)) {
                    // we have a single transition handling the entire mask
                    drawC (nt.transitions [m], di.s);
                    for (int j = 0; j < 6; j++) {
                        if (around [j] == nt)
                            around [j] = null;
                    }
                } else {
                    // draw all the individual transitions instead
                    for (int j = 0; j < 6; j++) {
                        if (around [j] == nt) {
                            around [j] = null;
                            drawC (nt.transitions [1 << j], di.s);
                        }
                    }
                }
            }
        }

        // draw vertex transitions
        foreach (var di in drawer(map)) {
            var t = map.get (di.x, di.y);
            var nt0 = map.get_neighbor (di.x, di.y, 0);
            var nt1 = map.get_neighbor (di.x, di.y, 1);
            var nt5 = map.get_neighbor (di.x, di.y, 5);
            // top left
            var wall = t;
            if (!is_wall (wall)) {
                wall = nt0;
                if (!is_wall (wall)) {
                    wall = nt5;
                    if (!is_wall (wall)) {
                        wall = null;
                    }
                }
            }
            if (wall != null) {
                int m = 64;
                if (nt0 != null && nt0.tile == wall.tile)
                    m += 1;
                if (nt5 != null && nt5.tile == wall.tile)
                    m += 2;
                if (t != null && t.tile == wall.tile)
                    m += 4;
                if (wall.tile.transitions.ContainsKey (m)) {
                    drawTL (wall.tile.transitions [m], new Position (di.s.x - 90, di.s.y + 144));
                }
            }

            // top right
            wall = t;
            if (!is_wall (wall)) {
                wall = nt0;
                if (!is_wall (wall)) {
                    wall = nt1;
                    if (!is_wall (wall)) {
                        wall = null;
                    }
                }
            }
            if (wall != null) {
                int m = 72;
                if (nt0 != null && nt0.tile == wall.tile)
                    m += 1;
                if (nt1 != null && nt1.tile == wall.tile)
                    m += 2;
                if (t != null && t.tile == wall.tile)
                    m += 4;
                if (wall.tile.transitions.ContainsKey (m)) {
                    drawTL (wall.tile.transitions [m], new Position (di.s.x - 36, di.s.y + 108));
                }
            }
        }

        // draw objects
        foreach (var di in drawer(map)) {
            var t = map.get (di.x, di.y);
            if (t != null && t.obj != null) {
                int v = t.variant % t.obj.variants.Count;
                drawC (t.obj.variants [v], di.s); 
            }
        }

        // draw visibility darkness
        if (darkness == null) {
            darkness = Resources.Load<Material> ("Darkness");
        }
        darkness.SetPass (0);
        var vi = new Visibility (map);
        Graphics.DrawMeshNow (vi.mesh, new Vector3 ((float)-xy.x, (float)-xy.y, 0), Quaternion.identity);

        draw_cursor (world);
    }

    static void draw_cursor (WorldMap world) {
        if (cursor == null) {
            cursor = new Picture ("images/misc/hover-hex.png");
            cursor.prepare ();
        }
        var p = getpos (world.dungeon [world.z], world.x, world.y);
        drawC (cursor, p);
    }
}

