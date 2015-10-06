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

    public int calculate (int tx, int ty) {
        int i = vindex++;
        if (tx < 0 || ty < 0 || tx >= map.w || ty >= map.h) {
            return 0;
        }
        var r = map.get (tx, ty);

        // This does the "line of sight" logic (i.e. everything along the 6 hexagon
        // lines is visible.
        int v = 1;
        if (i <= max_index) {
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
        if (!r.tile.visible) {
            marker [i] = 0;
        } else {
            marker [i] = v;
        }
        if (v > 0) {
            r.visible = 1;
        } else {
            r.visible = 0;
        }
        return 0;
    }

    public Visibility (HexMap map) {
        mesh = new Mesh ();
        int n = map.hexagon_size (4, true);
        var vertices = new Vector3[n * 7];
        var triangles = new int[n * 6 * 3];
        var colors = new Color32[n * 7];
        int vi = 0;
        int ti = 0;
        Action<Position> v = (Position p) => {
            vertices [vi] = new Vector3 ((float)p.x, (float)p.y);
            colors [vi] = new Color32 (0, 0, 0, 255);
            vi++;
        };
        Action<int,int,int> t = (int a, int b, int c) => {
            triangles [ti++] = a + vi - 7;
            triangles [ti++] = b + vi - 7;
            triangles [ti++] = c + vi - 7;
        };
        foreach (var p in map.hexagon(0, 0, 4, 0, true)) {
            Position s = new Position (p.x, p.y);
            s = map.tile_to_screen (s);
            s.x *= 54;
            s.y *= -36;
            v (s); // center
            s.x += 18;
            s.y += 36;
            v (s); // top right
            s.x += 18;
            s.y -= 36;
            v (s); // right
            s.x -= 18;
            s.y -= 36;
            v (s); // bottom right
            s.x -= 36;
            v (s); // bottom left
            s.x -= 18;
            s.y += 36;
            v (s); // left
            s.x += 18;
            s.y += 36;
            v (s); // top left
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


