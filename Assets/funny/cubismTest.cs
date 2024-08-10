using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Live2D.Cubism.Framework.Json;
using Live2D.Cubism.Core;
using System;
using System.IO;
using Live2D.Cubism.Core.Unmanaged;

public class cubismTest : MonoBehaviour
{
    // Start is called before the first frame update
    CubismDrawable[] drawables;
    void Start()
    {
        var path = Application.streamingAssetsPath + "/capy3/capy3.model3.json";
        CubismModel3Json model3Json = CubismModel3Json.LoadAtPath(path, BuiltinLoadAssetAtPath);
        CubismModel model = model3Json.ToModel();
        // CubismMoc moc = CubismMoc.CreateFrom(model3Json.Moc3);
        // CubismTaskableModel taskableModel = new CubismTaskableModel(moc);
        // CubismUnmanagedModel unmanagedModel = taskableModel.UnmanagedModel;
        // Debug.Log(unmanagedModel.Drawables.IndexCounts);
        drawables = model.Drawables;
        foreach (CubismDrawable draw in drawables){
            Debug.Log("funny");
            Debug.Log(draw.VertexPositions.Length);
            foreach(Vector3 vertex in draw.VertexPositions){
                Debug.Log(vertex);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // public voidResetModel(CubismModel model){
    //     Moc = moc;
    //     name = moc.name;
    //     TaskableModel = new CubismTaskableModel(moc);

    //         if (TaskableModel == null || TaskableModel.UnmanagedModel == null)
    //         {
    //             return;
    //         }

    //         // Create and initialize proxies.
    //         var parameters = CubismParameter.CreateParameters(TaskableModel.UnmanagedModel);
    //         var parts = CubismPart.CreateParts(TaskableModel.UnmanagedModel);
    //         var drawables = CubismDrawable.CreateDrawables(TaskableModel.UnmanagedModel);


    //         parameters.transform.SetParent(transform);
    //         parts.transform.SetParent(transform);
    //         drawables.transform.SetParent(transform);


    //         Parameters = parameters.GetComponentsInChildren<CubismParameter>();
    //         Parts = parts.GetComponentsInChildren<CubismPart>();
    //         Drawables = drawables.GetComponentsInChildren<CubismDrawable>();

    //         CanvasInformation = new CubismCanvasInformation(TaskableModel.UnmanagedModel);
    // }
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
    public void SetOffsets(){
        // compare closest meshes (seperate between different ones?), link weights?
        // find 4 closest vertex for each point, map to them I guess (kind of stupid)
        foreach (CubismDrawable draw in drawables){
            // differentiate by type, just assuming it's 1 atm
            foreach(Vector3 vertex in draw.VertexPositions){
                
            }
        }
    }
}
