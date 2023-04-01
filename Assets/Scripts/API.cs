using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.XR.Interaction.Toolkit;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using Amazon;
using System.IO;
using UnityEditor;

public class API : MonoBehaviour
{
    private string bucketName = "stl-loader-adarsh";
    [SerializeField] public string accessKey;
    [SerializeField] public string secretKey;
    const string BaseModelURL = "https://stl-loader-adarsh.s3.amazonaws.com/";

    public Material bonesMaterial;
    public string layerName = "Grabable";
    public UI_button uiButtonTemplate;
    public Transform UI_Holder_CONTENT;
    public List<GameObject> UI_Buttons = new List<GameObject>();


    private async void Start()
    {
        UnityInitializer.AttachToGameObject(this.gameObject);
        UI_Buttons.Clear();
        await GetListOfObjectsAsync();
    }
    public async Task GetListOfObjectsAsync()
    {
        AWSCredentials credentials = new BasicAWSCredentials(accessKey,secretKey);
        AmazonS3Client S3Client = new AmazonS3Client(credentials, RegionEndpoint.USEast1);
        var listRequest = new ListObjectsRequest
        {
            BucketName = bucketName
        };
        S3Client.ListObjectsAsync(listRequest, (response) =>
        {
            if (response.Exception != null)
            {
                Debug.Log("Exception : " + response.Exception);
            }
            else
            {
                Debug.Log(response.Response.S3Objects.Count);
                foreach (var s3Object in response.Response.S3Objects)
                {
                    Debug.Log(s3Object.Key);
                    GameObject newUI = Instantiate(uiButtonTemplate.gameObject);
                    newUI.GetComponent<UI_button>().ModelName = s3Object.Key;
                    newUI.transform.parent = UI_Holder_CONTENT.transform;
                    newUI.transform.localPosition = Vector3.zero;
                    newUI.transform.localScale = Vector3.one;
                    UI_Buttons.Add(newUI);
                    newUI.GetComponent<UI_button>().DisplayName();
                }
            }
        });
    }

    public IEnumerator LoadGLB(string assetName)
    {
        string bundleURL = BaseModelURL + assetName;

        // Download the FBX file
        WWW www = new WWW(bundleURL);
        yield return www;

        // Check for errors
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError(www.error);
            yield break;
        }

        // Save the FBX file to disk
        string filePath = Path.Combine(Application.persistentDataPath, assetName);
        File.WriteAllBytes(filePath, www.bytes);

        // Import the FBX file into Unity
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
        Instantiate(model);

    }
    private void OnApplicationQuit()
    {
        string path = Application.persistentDataPath;
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
        Debug.Log("Persistent data deleted.");
    }

















    public void GetBundleObject(string assetName, UnityAction<GameObject> callback, Transform bundleParent)
    {
        StartCoroutine(GetDisplayBundleRoutine(assetName, callback, bundleParent));
    }
    IEnumerator GetDisplayBundleRoutine(string assetName, UnityAction<GameObject> callback, Transform bundleParent)
    {
        string bundleURL = BaseModelURL + assetName;

        Debug.Log("Requesting bundle at " + bundleURL);

        //request asset bundle
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(bundleURL);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Network error");
        }
        else
        {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            if (bundle != null)
            {
                string rootAssetPath = bundle.GetAllAssetNames()[0];
                GameObject fbxObject = Instantiate(bundle.LoadAsset(rootAssetPath) as GameObject, bundleParent);
                fbxObject.transform.localPosition = Vector3.zero;
                fbxObject.GetComponent<Renderer>().material = bonesMaterial;
                fbxObject.AddComponent<JointCreator>();

                XRGrabInteractable xrI = fbxObject.AddComponent<XRGrabInteractable>();
                int layerIndex = LayerMask.NameToLayer(layerName);
                xrI.interactionLayerMask = 1 << layerIndex;

                xrI.movementType = XRBaseInteractable.MovementType.VelocityTracking;
                xrI.useDynamicAttach = true;
                fbxObject.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
                fbxObject.transform.parent = null;
                bundle.Unload(false);
                callback(fbxObject);

                foreach (var item in UI_Buttons)
                {
                    if(assetName == item.transform.name)
                    {
                       GameObject loading = item.transform.Find("Loading").gameObject;
                        loading.SetActive(false);
                    }
                }
            }
            else
            {
                UnityEngine.Debug.Log("Not a valid asset bundle");
            }
        }
    }
}