using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Live2D.Cubism.Framework.Json;
using Live2D.Cubism.Core;
using System;
using System.IO;
using Live2D.Cubism.Core.Unmanaged;
using TMPro;
using System.Linq;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine.UIElements;
using System.Threading;

public class cubismTest : MonoBehaviour
{
    // Start is called before the first frame update
    CubismDrawable[] drawables;
    List<Mesh> meshes;
    List<GameObject> gameObjects;
    List<List<float[]>> weights;
    List<List<int[]>> mappings;
    List<int[]> triangles;
    public int numMappings = 4;
    List<Vector3[]> vertexPositions;
    List<Vector2[]> vertexUvs;
    public Material mat;
    MaterialPropertyBlock materialPropertyBlock;
    public Texture2D texture;
    Vector3 meshTransform;
    Vector3 meshScale;
    Vector3[] givenVertexPositions;
    void Start()
    {
        meshTransform = new Vector3(0, 0, 0);
        meshScale = new Vector3(1, 1, 1);
        materialPropertyBlock = new MaterialPropertyBlock();
        var path = Application.streamingAssetsPath + "/putin2/putin2.model3.json";
        CubismModel3Json model3Json = CubismModel3Json.LoadAtPath(path, BuiltinLoadAssetAtPath);
        CubismModel model = model3Json.ToModel();
        drawables = model.Drawables;
        meshes = new List<Mesh>();
        gameObjects = new List<GameObject>();
        weights = new List<List<float[]>>();
        mappings = new List<List<int[]>>();
        triangles = new List<int[]>();
        vertexPositions = new List<Vector3[]>();
        vertexUvs = new List<Vector2[]>();
        foreach (CubismDrawable draw in drawables){
            GameObject currGameObject = new GameObject();
            gameObjects.Add(currGameObject);
            MeshRenderer meshRenderer = currGameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = mat;
            MeshFilter meshFilter = currGameObject.AddComponent<MeshFilter>();
            Mesh currMesh = new Mesh();
            meshFilter.mesh = currMesh;

            meshRenderer.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetTexture("_MainTex", texture);
            meshRenderer.SetPropertyBlock(materialPropertyBlock);
            vertexPositions.Add((Vector3[])draw.VertexPositions.Clone());
            vertexUvs.Add((Vector2[])draw.VertexUvs.Clone());
            triangles.Add((int[])draw.Indices.Clone());
            meshes.Add(currMesh);
            meshFilter.mesh = currMesh;
        }
        initializeMesh();
        Debug.Log("MADE MESH!!!!!!!!");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public static object BuiltinLoadAssetAtPath(Type assetType, string absolutePath)
    {
        if (assetType == typeof(byte[]))
        {
            return File.ReadAllBytes(absolutePath);
        }
        else if(assetType == typeof(string))
        {
            return File.ReadAllText(absolutePath);
        }
        else if (assetType == typeof(Texture2D))
        {
            var texture = new Texture2D(1,1);
            texture.LoadImage(File.ReadAllBytes(absolutePath));
            return texture;
        }
        throw new NotSupportedException();
    }
    public void calculateNewVertex(Vector3[] referencePoints){

        for (int i = 0; i < referencePoints.Length; i++){
            referencePoints[i].x *= meshScale.x;
            referencePoints[i].y *= meshScale.y;
            referencePoints[i].z = 0;
            referencePoints[i] += meshTransform;
        }
        for (int i = 0; i < meshes.Count; i++){
            Debug.Log(mappings.Count);
            Debug.Log(weights.Count);
            Debug.Log(string.Join(",", vertexPositions[i]));
            Debug.Log(string.Join(",", referencePoints));
            Debug.Log(string.Join(",", mappings[0][0]));
            Debug.Log(string.Join(",", weights[0][0]));
            Vector3[] currVertexPositions = new Vector3[vertexPositions[i].Length];
            for (int j = 0; j < mappings[i].Count; j++){
                Vector3 currVertex = new Vector3(0, 0, 0);
                for (int k = 0; k < numMappings; k++){
                    currVertex += referencePoints[mappings[i][j][k]] * weights[i][j][k];
                }
                currVertexPositions[j] = currVertex;
            }
            vertexPositions[i] = currVertexPositions;
            Debug.Log(string.Join(",", vertexPositions[i]));
            Debug.Log("----");
        }
    }
    public void initializeMesh(){
        for(int i = 0; i < meshes.Count; i++){
            meshes[i].MarkDynamic();
            meshes[i].Clear();
            meshes[i].SetVertices(vertexPositions[i]);
            meshes[i].SetUVs(0, vertexUvs[i]);
            // meshes[i].SetTriangles(triangles[i], 0);
            int[] funny = new int[vertexPositions[i].Length];
            for (int j = 0; j < vertexPositions[i].Length; j++){
                funny[j] = j;
            }
            meshes[i].SetIndices(funny, MeshTopology.Points, 0, true, 0);
        }
    }
    public void updateMesh(){
        for(int i = 0; i < meshes.Count; i++){
            // Debug.Log("updated cubism mesh");
            // meshes[i].Clear();
            meshes[i].SetVertices(vertexPositions[i]);
            // meshes[i].SetUVs(0, vertexUvs[i]);
            // meshes[i].SetTriangles(triangles[i], 0);

            int[] funny = new int[vertexPositions[i].Length];
            for (int j = 0; j < vertexPositions[i].Length; j++){
                funny[j] = j;
            }
            meshes[i].SetIndices(funny, MeshTopology.Points, 0, true, 0);

        }
    }
    public void SetOffsets(Vector3[] referencePoints, Mesh faceMesh){
        float maxX = referencePoints.Max(vec => vec.x);
        float maxY = referencePoints.Max(vec => vec.y);
        float minX = referencePoints.Min(vec => vec.x);
        float minY = referencePoints.Min(vec => vec.y);
        float selfMaxX = vertexPositions[0].Max(vec => vec.x);
        float selfMaxY = vertexPositions[0].Max(vec => vec.y);
        float selfMinX = vertexPositions[0].Min(vec => vec.x);
        float selfMinY = vertexPositions[0].Min(vec => vec.y);

        meshScale = new Vector3((maxX - minX)/(selfMaxX - selfMinX), (maxX - minX)/(selfMaxX - selfMinX), 1);
        meshTransform = new Vector3((maxX - selfMaxX)*meshScale.x, (maxY - selfMaxY)*meshScale.y, 0);
        

        // givenVertexPositions = (Vector3[])referencePoints.Clone();
        // GameObject currGameObject = new GameObject();
        // gameObjects.Add(currGameObject);
        // MeshRenderer meshRenderer = currGameObject.AddComponent<MeshRenderer>();
        // meshRenderer.material = mat;
        // MeshFilter meshFilter = currGameObject.AddComponent<MeshFilter>();
        // Mesh currMesh = new Mesh();
        // meshFilter.mesh = currMesh;

        // meshRenderer.GetPropertyBlock(materialPropertyBlock);
        // materialPropertyBlock.SetTexture("_MainTex", texture);
        // meshRenderer.SetPropertyBlock(materialPropertyBlock);
        // currMesh.SetVertices(givenVertexPositions);
        // currMesh.SetUVs(0, vertexUvs[0]);
        // // meshes[i].SetTriangles(triangles[i], 0);
        // int[] funny = new int[givenVertexPositions.Length];
        // for (int j = 0; j < givenVertexPositions.Length; j++){
        //     funny[j] = j;
        // }
        // currMesh.SetIndices(funny, MeshTopology.Points, 0, true, 0);



        Debug.Log("--- ");
        Debug.Log(meshTransform);
        Debug.Log(meshScale);

        for (int i = 0; i < referencePoints.Length; i++){
            referencePoints[i].x *= meshScale.x;
            referencePoints[i].y *= meshScale.y;
            referencePoints[i].z = 0;
            referencePoints[i] += meshTransform;
        }

        // compare closest meshes (seperate between different ones?), link weights?
        // find 4 closest vertex for each point, map to them I guess (kind of stupid)
        for (int k = 0; k < drawables.Length; k++){
            CubismDrawable draw = drawables[k];
            // differentiate by type, just assuming it's 1 atm
            List<int[]> mappingList = new List<int[]>();
            List<float[]> weightList = new List<float[]>();

            foreach(Vector3 vertex in draw.VertexPositions){
                int[] closest = new int[4];
                float[] pointDistance = new float[4];
                float[] pointWeight = new float[4];
                for (int i = 0; i < referencePoints.Length; i++){
                    Vector3 curr = referencePoints[i];
                    float currDistance = Vector3.Distance(curr, vertex);
                    if (i == 0){
                        pointDistance[0] = currDistance;
                        closest[0] = 0;
                    }
                    else if (i < 4){
                        for (int j = 0; j < i; j++){
                            if (currDistance < pointDistance[j]){
                                for (int a = i; a > j; a--){
                                    closest[a] = closest[a-1];
                                    pointDistance[a] = pointDistance[a-1];
                                }
                                closest[j] = i;
                                pointDistance[j] = currDistance;
                                break;
                            }
                        }
                    }
                    else{
                        for (int j = 0; j < 4; j++){
                            if (currDistance < pointDistance[j]){
                                for (int a = 3; a > j; a--){
                                    closest[a] = closest[a-1];
                                    pointDistance[a] = pointDistance[a-1];
                                }
                                closest[j] = i;
                                pointDistance[j] = currDistance;
                                break;
                            }
                        }
                    }
                }
                float totalDistance = 0;
                for (int i = 0; i < 4; i++){
                    totalDistance += pointDistance[i];
                }
                for (int i = 0; i < 4; i++){
                    pointWeight[i] = pointDistance[i] / totalDistance;
                }
                mappingList.Add(closest);
                weightList.Add(pointWeight);

                // Debug.Log(string.Join(",", closest));
                // Debug.Log(string.Join(",", pointDistance));
                // Debug.Log(string.Join(",", pointWeight));
                // Debug.Log("----");
            }
            mappings.Add(mappingList);
            weights.Add(weightList);
        }
    }
    public void DrawSinglePoint(){
        
    }
    public void drawMeshRepresentation(){
        for (int i = 0; i < drawables.Length; i++){
            GameObject currGameObject = new GameObject();
            gameObjects.Add(currGameObject);
            MeshRenderer meshRenderer = currGameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = mat;
            MeshFilter meshFilter = currGameObject.AddComponent<MeshFilter>();
            Mesh currMesh = new Mesh();
            meshFilter.mesh = currMesh;
            int[] funny = new int[vertexPositions[i].Length];
            for (int j = 0; j < vertexPositions[i].Length; j++){
                funny[j] = j;
            }
            currMesh.SetVertices(vertexPositions[i]);
            currMesh.SetIndices(funny, MeshTopology.Points, 0, true, 0);
        }
    }
}
