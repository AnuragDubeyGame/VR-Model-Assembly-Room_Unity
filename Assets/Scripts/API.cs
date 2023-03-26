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
using UnityEngine.InputSystem;


public class API : MonoBehaviour
{
    const string ModelFolder = "https://stl-loader-adarsh.s3.amazonaws.com/Models/";
    private string bucketName = "stl-loader-adarsh";
    private string objectKey = "talus-1";


    public Material bonesMaterial;
    public string layerName = "Grabable";
    public UI_button uiButtonTemplate;
    public Transform UI_Holder_CONTENT;
    public List<GameObject> UI_Buttons = new List<GameObject>();


    private async void Start()
    {
        UnityInitializer.AttachToGameObject(this.gameObject);
        await GetListOfObjectsAsync();
    }

    public async Task GetListOfObjectsAsync()
    {
        AmazonS3Client S3Client = new AmazonS3Client(new AnonymousAWSCredentials(), RegionEndpoint.USEast1);
        var listRequest = new ListObjectsRequest
        {
            BucketName = bucketName
        };
        S3Client.ListObjectsAsync(listRequest, (response) => { 
            if(response.Exception != null)
            {
                Debug.Log("Exception : "+response.Exception);
            }
            else
            {
                foreach (var s3Object in response.Response.S3Objects)
                {
                    Debug.Log(s3Object);
                }
            }
        });
    }




































/* foreach (string model in models)
{
GameObject newUI = Instantiate(uiButtonTemplate.gameObject);
newUI.GetComponent<UI_button>().ModelName = model;
newUI.transform.parent = UI_Holder_CONTENT.transform;
newUI.transform.position = Vector3.zero;
UI_Buttons.Add(newUI);
}.

IEnumerator LoadGLB()
{
// Download the GLB file from the remote server
using (WWW www = new WWW(glbUrl))
{
yield return www;
if (!string.IsNullOrEmpty(www.error))
{
Debug.LogError($"Failed to load GLB file from {glbUrl}: {www.error}");
yield break;
}

// Parse the GLB file using the GLTF package
GLTFRoot gltfRoot = null;
GLTFParser.ParseJson(Encoding.ASCII.GetString(www.bytes), out gltfRoot);

// Create a new GameObject and add a GLTFComponent to it
GameObject modelObject = new GameObject("GLB Model");
GLTFComponent gltfComponent = modelObject.AddComponent<GLTFComponent>();
gltfComponent.GLTFRoot = gltfRoot;
gltfComponent.FileName = glbUrl;

// Load the GLB model using the GLTFComponent
yield return gltfComponent.Load();
}
}
*/
public void GetBundleObject(string assetName, UnityAction<GameObject> callback, Transform bundleParent)
    {
        StartCoroutine(GetDisplayBundleRoutine(assetName, callback, bundleParent));
    }
    IEnumerator GetDisplayBundleRoutine(string assetName, UnityAction<GameObject> callback, Transform bundleParent)
    {

        string bundleURL = ModelFolder + assetName;

        UnityEngine.Debug.Log("Requesting bundle at " + bundleURL);

        //request asset bundle
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(bundleURL);
        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            UnityEngine.Debug.Log("Network error");
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