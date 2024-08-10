import json

f = open("github_triangles.json", "r", encoding="utf-8")
data_str = ""
for line in f:
    data_str += line
data = json.loads(data_str)
f.close()

triangles = data
print(len(triangles))
based_triangles = []
#remove repeated
for i in range(len(triangles)):
    curr_tri = triangles[i]
    based_triangles.append(curr_tri[0])
    based_triangles.append(curr_tri[1])
    based_triangles.append(curr_tri[2])
    
f = open("github_triangles2.json", 'w', encoding='utf-8')
f.write(json.dumps(based_triangles))
f.close()