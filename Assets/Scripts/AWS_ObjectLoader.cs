using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class AWS_ObjectLoader : MonoBehaviour 
{
    public string bucketName = "your-bucket-name";
    public string region = "your-bucket-region";
    public string url = "https://s3-{0}.amazonaws.com/{1}/";

    private List<GameObject> fbxObjects = new List<GameObject>();

    async void Start()
    {
        await FetchAllFBXObjects(bucketName, "models/fbx/");
    }

    async Task<List<GameObject>> FetchAllFBXObjects(string bucketName, string prefix)
    {
        string url = $"https://{bucketName}.s3.amazonaws.com/{prefix}/";

        UnityWebRequest request = UnityWebRequest.Get(url);
        UnityWebRequestAsyncOperation operation = request.SendWebRequest();

        while (!operation.isDone)
        {
            await Task.Delay(100);
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log($"Error: {request.error}");
            return null;
        }

        List<GameObject> fbxObjects = new List<GameObject>();

        string response = request.downloadHandler.text;
        string[] lines = response.Split('\n');

        foreach (string line in lines)
        {
            if (line.Contains(".fbx"))
            {
                string[] words = line.Split('"');
                string objectName = words[1];
                string objectUrl = url + objectName;

                UnityWebRequest objectRequest = UnityWebRequest.Get(objectUrl);
                UnityWebRequestAsyncOperation objectOperation = objectRequest.SendWebRequest();

                while (!objectOperation.isDone)
                {
                    await Task.Delay(100);
                }

                if (objectRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log($"Error: {objectRequest.error}");
                }
                else
                {
                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(objectRequest);
                    GameObject fbxObject = bundle.LoadAsset<GameObject>(objectName);
                    fbxObjects.Add(fbxObject);
                }
            }
        }

        return fbxObjects;
    }
}

