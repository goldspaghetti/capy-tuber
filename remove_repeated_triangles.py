import json

f = open("triangles.json", "r", encoding="utf-8")
data = f.readline()
f.close()

triangles = json.loads(data)
print(len(triangles))
based_triangles = []
#remove repeated
for i in range(len(triangles)):
    curr_tri = triangles[i]
    for j in range(len(triangles)-i-1):
        tri = triangles[j]
        repeated = True
        for k in range(3):
            if not (curr_tri[k] in tri):
                repeated = False
    if not repeated:
        based_triangles.append(curr_tri[0])
        based_triangles.append(curr_tri[1])
        based_triangles.append(curr_tri[2])
    
f = open("triangles3.json", 'w', encoding='utf-8')
f.write(json.dumps(based_triangles))
f.close()