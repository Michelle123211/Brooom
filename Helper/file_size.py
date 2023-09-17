import pathlib
import os


debug = False #or True

target = input("Target path: ")

print("Listing .cs files from path: " + target)
if debug:
    print("----------------")

path = pathlib.Path(target)

total_size = 0
for file in list(path.rglob("*.cs")):
    file_stats = os.stat(file)
    size = file_stats.st_size / 1024 # B converted to kB
    if debug:
        print(str(file)[len(target):] + ": " + str("%.2f" % size) + " kB") # 2 decimals
    total_size += size

if debug:
    print("----------------")
print("Total size: " + str("%.2f" % total_size) + " kB") # 2 decimals
