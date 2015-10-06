using System.Collections.Generic;
using System;

public struct Position {
    public double x, y;

    public Position (double x, double y) {
        this.x = x;
        this.y = y;
    }
}


/*
        __
     __/0 \__
    /5 \__/1 \
    \__/  \__/
    /4 \__/2 \
    \__/3 \__/
       \__/




             sx = -2  sx = -1  sx = 0   sx = 1   sx = 2
             |        |        |        |        |
                                 tx = -1
 sy =-2 _______________        _/  
                            _-' '-_
                         _-'       '-_    tx = 0
 sy =-1 ______        _-'   ______    '-_/ 
                   _-' '-_ /      \  _-' '-_
                _-'       /-_     _\'       '-_    tx = 1
 sy = 0 _    _-'         /   '-_-'  \          '-_/ 
              '-_        \  _-' '-_ /         _-' \ 
                 '-_     _\'       /-_     _-'     ty = -1
 sy = 1 ______      '-_-'  \______/   '-_-'      
                       '-_           _-' \  
                          '-_     _-'     ty = 0
 sy = 2 _______________      '-_-'             
                                \
                                 ty = 1

    */

public class HexMap {
    HexTile[] map;
    public int w, h;
    //                   0   1  2  3  4   5 (6)
    public int[] nx = { -1, -0, 1, 1, 0, -1, 0 };
    public int[] ny = { -1, -1, 0, 1, 1, -0, 0 };

    public Position tile_to_screen (Position t) {
        return new Position (t.x - t.y, t.x + t.y);
    }

    /* An integer tile position always is a grid intersection of 4 adjacent
     * diamond tiles. The floating point position walks along the
     * x and y axis in a 45° angle.
     * 
     * Each diamond cell is 2 screen units wide and 2 screen units high.
     * 
     * Hexagons are always drawn centered on diamond grid intersections -
     * each hexagon therefore overlaps 4 diamond cells.
     */
    public Position screen_to_tile (Position s) {
        return new Position (s.x / 2 + s.y / 2, s.y / 2 - s.x / 2);
    }

    public bool is_left (double ax, double ay, double bx, double by) {
        return ax * by - ay * bx < 0;
    }

    public void get_hex_position (Position s, out int tx, out int ty) {
        /* Given an on-screen position, return the terrain tile beneath.
         */
        var g = screen_to_tile (s);
        int ax = (int)Math.Floor (g.x);
        int ay = (int)Math.Floor (g.y);
        g.x -= ax;
        g.y -= ay;
        double w = 36;
        double h = 36;
        double px = g.x * w - g.y * w;
        double py = g.x * h + g.y * h;
        if (py < h) {
            if (is_left (-w, -h * 2, px, py - h * 2)) {
                tx = ax;
                ty = ay + 1;
                return;
            }
            if (is_left (-w, h * 2, px - w, py)) {
                tx = ax + 1;
                ty = ay;
                return;
            }
            tx = ax;
            ty = ay;
            return;
        } else {
            if (is_left (w, -h * 2, px - (-w), py - h * 2)) {
                tx = ax;
                ty = ay + 1;
                return;
            }
            if (is_left (w, h * 2, px, py)) {
                tx = ax + 1;
                ty = ay;
                return;
            }
            tx = ax + 1;
            ty = ay + 1;
        }
    }

    public HexMap (int w, int h) {
        this.w = w;
        this.h = h;
        map = new HexTile[w * h];
        for (int i = 0; i < w * h; i++) {
            map [i] = new HexTile ();
        }
    }

    public HexTile get (int x, int y) {
        if (x < 0 || y < 0 || x >= w || y >= h)
            return null;
        return map [w * y + x];
    }

    public HexTile get_neighbor (int x, int y, int n) {
        return get (x + nx [n], y + ny [n]);
    }

    /*
        __
     __/0 \__
    /5 \__/1 \
    \__/6 \__/
    /4 \__/2 \
    \__/3 \__/
       \__/
*/
   
    public int direction_1 (int dx, int dy) {
        /* Return one of the two likely directions. direction_2 returns the other direction. If a
         * straight line is possible with some direction, both will return that same
         * direction. If both dx and dy are zero, returns 6.
         */
        if (dx > 0) {
            if (dy > 0) {
                return 3;
            }
            if (dy < 0) {
                return 1;
            }
            return 2;
        } else if (dx < 0) {
            if (dy > 0) {
                return 4;
            }
            if (dy < 0) {
                return 0;
            }
            return 5;
        }
        if (dy > 0) {
            return 4;
        }
        if (dy < 0) {
            return 1;
        }
        return 6;
    }

    public int direction_2 (int dx, int dy) {
        if (dx > 0) {
            if (dy > 0) {
                if (dx > dy) {
                    return 2;
                }
                if (dx < dy) {
                    return 4;
                }
                return 3;
            }
            if (dy < 0) {
                return 2;
            }
            return 2;
        }
        if (dx < 0) {
            if (dy > 0) {
                return 5;
            }
            if (dy < 0) {
                if (dx < dy) {
                    return 5;
                }
                if (dx > dy) {
                    return 1;
                }
                return 0;
            }
            return 5;
        }
        if (dy > 0) {
            return 4;
        }
        if (dy < 0) {
            return 1;
        }
        return 6;
    }

    public void add_dir (ref int x, ref int y, int dir, int n = 1) {
        /*
         * Given a position ''x/y'', and a direction ''dir'' from 0 to 6, return the
         * position n tiles in that dir in ''nx/ny''.
         */
        x += nx [dir] * n;
        y += ny [dir] * n;
    }

    public struct HexPos {
        public int x, y;

        public HexPos (int x, int y) {
            this.x = x;
            this.y = y;
        }
    };

    /*

            0
         5     0
      5     0     0
   5     5  __ 0     1
      5  __/0 \__ 1
   4    /5 \__/1 \   1
      4 \__/6 \__/1
   4    /4 \__/2 \   1
      4 \__/3 \__/2
   4     3 \__/2     2
      3     3     2 
         3     2 
            3


*/

    public int hexagon_size (int radius, bool filled) {
        if (radius == 0)
            return 1;
        if (filled) {
            if (radius % 2 == 0) {
                return 1 + 6 * (radius / 2) * (radius + 1);
            } else {
                return 1 + 6 * radius * ((radius - 1) / 2 + 1);
            }
        } else {
            return 6 * radius;
        }
    }

    public IEnumerable<HexPos> hexagon (int x, int y, int radius, int dir, bool filled) {
        /* Return all positions inside a hexagon with the given center and radius.
         * The first position is the center, then positions will spiral outwards,
         * starting with the given direction. A radius of 0 means only the center, a
         * radius of 1 also the 6 neighbor fields, and so on.
         */
        int run;
        if (filled || radius == 0) {
            run = 1;
            yield return new HexPos (x, y);
        } else {
            run = radius;
            add_dir (ref x, ref y, dir, radius - 1);
        }
        while (run <= radius) {
            add_dir (ref x, ref y, dir);
            dir = (dir + 1) % 6;
            for (int i = 0; i < 6; i++) {
                dir = (dir + 1) % 6;
                for (int j = 0; j < run; j++) {
                    yield return new HexPos (x, y);
                    add_dir (ref x, ref y, dir);
                }
            }
            dir = (dir + 5) % 6;
            run += 1;
        }
        yield break;
    }
}