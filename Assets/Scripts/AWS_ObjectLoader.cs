using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class AWS_ObjectLoader : MonoBehaviour
{
    public string s3ApiLink; // The API link to the S3 bucket object
    public float scale = 1f; // The scale of the instantiated game object
    private GameObject objectPrefab; // The prefab of the loaded 3D object

    IEnumerator Start()
    {
        UnityWebRequest www = UnityWebRequest.Get(s3ApiLink);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            string contentType = www.GetResponseHeader("Content-Type");

            if (contentType.StartsWith("text"))
            {
                Debug.Log(www.downloadHandler.text);
            }
            else if (contentType.StartsWith("application/octet-stream"))
            {
                objectPrefab = DownloadHandlerAssetBundle.GetContent(www).LoadAsset<GameObject>("YourObjectName");
                SpawnObject();
            }
            else
            {
                Debug.Log("Unsupported content type: " + contentType);
            }
        }
    }

    void SpawnObject()
    {
        // Instantiate the object as a game object
        GameObject newObject = Instantiate(objectPrefab);

        // Set the scale of the game object
        newObject.transform.localScale = new Vector3(scale, scale, scale);

        // Position the game object in the scene as needed
        newObject.transform.position = Vector3.zero; // Modify as needed
    }
}

