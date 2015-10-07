using System;
using UnityEngine;

public class Visibility {
    public Mesh mesh;
    public HexMap map;

    int[] visible1;
    int[] visible2;
    int[] marker;
    int vindex;
    int max_index;
    int vx, vy;
    float fading;
    float fade_d;

    const int MAX_VISIBILITY_RADIUS = 9 + 2;

    public int get_index (int x, int y) {
        int i = 0;
        foreach (var t in HexMap.hexagon(0, 0, MAX_VISIBILITY_RADIUS, 0, true)) {
            if (t.x == x && t.y == y)
                return i;
            i++;
        }
        return -1;
    }

    public void precalculate () {
        int n = HexMap.hexagon_size (MAX_VISIBILITY_RADIUS, true);
        visible1 = new int[n];
        visible2 = new int[n];
        marker = new int[n];
        max_index = n;
        foreach (var t in HexMap.hexagon(0, 0, MAX_VISIBILITY_RADIUS, 0, true)) {
            int d1 = HexMap.direction_1 (-t.x, -t.y);
            int d2 = HexMap.direction_2 (-t.x, -t.y);
            int x = t.x, y = t.y;
            HexMap.add_dir (ref x, ref y, d1);
            int v1 = get_index (x, y);
            x = t.x;
            y = t.y;
            HexMap.add_dir (ref x, ref y, d2);
            int v2 = get_index (x, y);
            visible1 [vindex] = v1;
            visible2 [vindex] = v2;
            vindex++;
        }
    }

    public void calculate (int tx, int ty) {
        int i = vindex++;
        if (tx < 0 || ty < 0 || tx >= map.w || ty >= map.h) {
            return;
        }
        var r = map.get (tx, ty);
        if (r.tile.voided)
            return;
        if (r.outside) {
            r.outside = false;
        }

        // This does the "line of sight" logic (i.e. everything along the 6 hexagon
        // lines is visible.
        int v = 1;
        if (i < max_index) {
            if (i > 6) {
                int i1 = visible1 [i];
                v = marker [i1];
                int i2 = visible2 [i];
                int v2 = marker [i2];
                if (v2 > v) {
                    v = v2;
                }
            }
        } else {
            v = 0;
        }
        if (!r.tile.unblocked) {
            marker [i] = 0;
        } else {
            marker [i] = v;
        }
        r.go_visible = v > 0;
    }

    public void hide (int tx, int ty) {
        var r = map.get (tx, ty);
        r.go_visible = false;
        r.outside = true;
    }

    public void unhide (int tx, int ty) {
        var r = map.get (tx, ty);
        r.outside = false;
    }

    public void darken_outside (int tx, int ty) {
        var r = map.get (tx, ty);
        if (r.outside) {
            r.darkness = 1;
        }
    }

    public void fade (int tx, int ty) {
        var r = map.get (tx, ty);
        if (r.go_visible) {
            r.darkness -= fade_d;
            if (r.darkness < 0)
                r.darkness = 0;
        } else {
            r.darkness += fade_d;
            if (r.darkness > 1)
                r.darkness = 1;
        }
    }

    public Visibility () {
        precalculate ();
    }

    public void update (WorldMap world, Position scroll) {

        bool need_recalc = true;
        if (this.map == world.dungeon [world.z] &&
            this.vx == world.x &&
            this.vy == world.y) {
            need_recalc = false;
            if (fading <= 0)
                return;
        } else {
            fading = 1f;
        }
            
        if (this.map == world.dungeon [world.z] && need_recalc) {
            foreach (var p in HexMap.hexagon(vx, vy, map.visibility + 1, 0, true)) {
                hide (p.x, p.y);
            }
            foreach (var p in HexMap.hexagon(world.x, world.y, map.visibility + 1, 0, true)) {
                unhide (p.x, p.y);
            }
            foreach (var p in HexMap.hexagon(vx, vy, map.visibility + 1, 0, true)) {
                darken_outside (p.x, p.y);
            }
        }
        this.map = world.dungeon [world.z];
        this.vx = world.x;
        this.vy = world.y;

        if (need_recalc) {
            vindex = 0;
            foreach (var p in HexMap.hexagon(vx, vy, map.visibility, 0, true)) {
                calculate (p.x, p.y);
            }
        }

        fade_d = Time.deltaTime;
        fading -= fade_d;
        foreach (var p in HexMap.hexagon(vx, vy, map.visibility + 1, 0, true)) {
            fade (p.x, p.y);
        }

        mesh = new Mesh ();
        int n = HexMap.hexagon_size (map.visibility + 2, true);
        var vertices = new Vector3[n * 7];
        var triangles = new int[n * 6 * 3];
        var colors = new Color32[n * 7];
        int vi = 0;
        int ti = 0;
        Action<Position, float> v = (Position p, float dark) => {
            vertices [vi] = new Vector3 ((float)(p.x + scroll.x),
                (float)(p.y + scroll.y));
            colors [vi] = new Color32 (0, 0, 0, (byte)(dark * 255));
            vi++;
        };
        Action<int,int,int> t = (int a, int b, int c) => {
            triangles [ti++] = a + vi - 7;
            triangles [ti++] = b + vi - 7;
            triangles [ti++] = c + vi - 7;
        };

        foreach (var p in HexMap.hexagon(vx, vy, map.visibility + 2, 0, true)) {
            var r = map.get (p.x, p.y);
            bool[] unblocked = new bool[7];
            float[] ndark = new float[7];
            unblocked [6] = r.tile.unblocked;
            ndark [6] = r.darkness;
            bool blank = unblocked [6] && (ndark [6] <= 0);
            for (int d = 0; d < 6; d++) {
                var rn = map.get_neighbor (p.x, p.y, d);
               
                unblocked [d] = rn.tile.unblocked;
                ndark [d] = rn.darkness;
                blank = blank && (ndark [d] <= 0) && unblocked [d];
            }

            if (blank)
                continue;

            var s = DrawMap.getpos (map, p.x, p.y);
            if (!unblocked [6]) {
                v (s, 1);
            } else
                v (s, r.darkness); // center
            /*
                    __
                 __/0 \__
                /5 \__/1 \
                \__/6 \__/
                /4 \__/2 \
                \__/3 \__/
                   \__/
            */
            bool unb = unblocked [5];
            float pdark = ndark [5];
            int[] cx = { -18, 18, 36, 18, -18, -36 };
            int[] cy = { 36, 36, 0, -36, -36, 0 };
            for (int d = 0; d < 6; d++) {
                Position s2 = new Position (s.x + cx [d], s.y + cy [d]);

                float dark = ndark [6];
                if (ndark [d] > dark)
                    dark = ndark [d];
                if (pdark > dark)
                    dark = pdark;


                if (!unb && !unblocked [d])
                    dark = 1;
                if (!unblocked [6] && (!unb || !unblocked [d]))
                    dark = 1;

                v (s2, dark);
                unb = unblocked [d];
                pdark = ndark [d];
            }
                
            t (0, 1, 2);
            t (0, 2, 3);
            t (0, 3, 4);
            t (0, 4, 5);
            t (0, 5, 6);
            t (0, 6, 1);
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors32 = colors;
    }
}


