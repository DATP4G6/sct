import sys

content = ""
nFiles = int(sys.argv[1]) if len(sys.argv) >= 2 else 22
with open("template.sct", "r") as f:
    content = f.read()
for i in range(nFiles):
    filename = "benchmark_files/" + "bm" + str(i) + ".sct"
    count = 0 if i == 0 else str((2 ** i)*1000)
    newContent = content.replace("##COUNT##", str(count))
    with open(filename, "w") as f:
        f.write(newContent)
