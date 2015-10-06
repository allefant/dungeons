using UnityEngine;
using System;

static class Util {
    static public void Print (string format, params object[] args) {
        Debug.Log (String.Format (format, args));
    }
}

