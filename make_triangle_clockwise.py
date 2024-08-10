import json

f = open("triangles.json", "r", encoding="utf-8")
data = f.readline()
f.close()

triangles = json.loads(data)
