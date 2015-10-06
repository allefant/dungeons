
using System.Collections.Generic;
using UnityEngine;

public class Picture {
    static List<Picture> all = new List<Picture> ();
    string name;
    public Texture2D tex;

    public Picture (string name) {
        if (name.EndsWith (".png"))
            name = name.Remove (name.Length - 4);
        this.name = name;
        all.Add (this);
    }

    static public void prepare_all () {
        foreach (var pic in all) {
            pic.prepare ();
        }
    }

    public void prepare () {
        if (tex != null)
            return;
        tex = Resources.Load<Texture2D> (name);
        if (tex == null) {
            Util.Print ("{0} = {1}", tex, name);
            throw new UnityException ();
        }
    }
}
