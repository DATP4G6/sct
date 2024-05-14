#! /usr/bin/env nix-shell
#! nix-shell -i python3 -p python311Packages.matplotlib python311Packages.pandas python311Packages.numpy
import matplotlib.pyplot as plt
import matplotlib.animation as animation
import numpy as np
import pandas as pd
import json

# Output generated with:
# `dotnet run --project ../../SocietalConstructionTool -- ../../SocietalConstructionToolTests/TestFiles/BehaviourTests/TestGameOfLife.sct -o output.json`
df = pd.read_json("output.json", lines=True, typ="series").apply(pd.json_normalize).apply(lambda f: f.where(f["Species"] == "Cell"))
x_max = int(df[0]["Fields.x"].max()) + 1
y_max = int(df[0]["Fields.y"].max()) + 1

with open("output.json") as f:
    lines = f.readlines()
    generations = [json.loads(l) for l in lines]

# Make list of correctly sized matrices
pixels = [np.ones((x_max, y_max)) for g in generations]

for (m, g) in zip(pixels, generations):
    for c in filter(lambda c: c["Species"] == "Cell", g):
        x = c["Fields"]["x"]
        y = c["Fields"]["y"]
        m[x, y] = min(m[x, y], 0 if c["State"] == "Alive" else 1)

(fig, ax) = plt.subplots()

def update(frame):
    ax.set_xlim(0, 10)
    ax.imshow(pixels[frame], cmap="gray", vmin=0, vmax=1)

ani = animation.FuncAnimation(fig, func=update, frames=len(pixels))
#ani.save("ani.gif")
plt.show()
