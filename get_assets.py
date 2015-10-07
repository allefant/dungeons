#!/usr/bin/env python3
import argparse, subprocess

def run(x):
    print(" ".join(x))
    subprocess.check_call(x)

parser = argparse.ArgumentParser()
parser.add_argument("-w", required = True)
options = parser.parse_args()

p = "images/terrain/water"
for water in """
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
    """.splitlines():
    water = water.strip()
    if not water:
        continue
    out = "Assets/Resources/" + p + "/" + water
    run(["cp", options.w + "/data/core/" + p + "/" + water, out])
    run(["mogrify", "-crop", "324x144+0+0", out])
