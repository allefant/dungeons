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
        var a = Initialize.cam.ScreenToWorldPoint (new Vector3 (rx / 2, ry / 2, 10));
        var b = Initialize.cam.ScreenToWorldPoint (new Vector3 (rx * 3 / 2, ry * 3 / 2, 10));
        var c = Initialize.cam.ScreenToWorldPoint (new Vector3 (rx * 3 / 2, ry / 2, 10));
        var d = Initialize.cam.ScreenToWorldPoint (new Vector3 (rx / 2, ry * 3 / 2, 10));
        Debug.DrawLine (a, b, Color.red, 0, false);
        Debug.DrawLine (c, d, Color.red, 0, false);

        if (Input.GetMouseButtonDown (0)) {
            var mpos = Input.mousePosition;
            world.select (mpos.x - rx + DrawMap.xy.x,
                mpos.y - ry + DrawMap.xy.y);
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
