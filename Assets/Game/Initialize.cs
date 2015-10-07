using UnityEngine;
using System.Collections;
using System;

public class Initialize : MonoBehaviour {
    WorldMap world;
    public static Camera cam;

    void Start () {
        world = new WorldMap ();

        try {
            Picture.prepare_all ();
        } catch (UnityException e) {
            Debug.Log (e);
            world.invisible = true;
        }

        cam = GameObject.Find ("Main Camera").GetComponent<Camera> ();
        cam.orthographicSize = Screen.height / 2;
    }

    void Update () {
        var x = Input.GetAxis ("Horizontal");
        var y = Input.GetAxis ("Vertical");
        DrawMap.scroll (x * 4, y * 4);
        var rx = Screen.width / 2;
        var ry = Screen.height / 2;

        if (Input.GetMouseButtonDown (0)) {
            var mpos = Input.mousePosition;
            world.select (mpos.x - rx + DrawMap.xy.x,
                mpos.y - ry + DrawMap.xy.y);
        }

        if (Input.GetKeyDown ("f1")) {
            Debugging.draw_tiles = !Debugging.draw_tiles;
        }
        if (Input.GetKeyDown ("f2")) {
            Debugging.draw_transitions = !Debugging.draw_transitions;
        }
        if (Input.GetKeyDown ("f3")) {
            Debugging.draw_vertex_transitions = !Debugging.draw_vertex_transitions;
        }
        if (Input.GetKeyDown ("f4")) {
            Debugging.draw_darkness = !Debugging.draw_darkness;
        }
    }

    void OnRenderObject () {
        if (world == null)
            return;
        if (world.invisible)
            return;
        DrawMap.render_map (world);

    }

    public void OnButton1 () {
        world.up ();
    }

    public void OnButton2 () {
        world.down ();
    }

    static void Main () {
    }
}
