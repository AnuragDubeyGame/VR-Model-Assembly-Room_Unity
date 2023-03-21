using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.XR.Interaction.Toolkit;

public class API : MonoBehaviour
{
    
    const string ModelFolder = "https://stl-loader-adarsh.s3.amazonaws.com/Models/";
    public Material bonesMaterial;
    public string layerName = "Grabable";
    public UI_button uiButtonTemplate;
    public Transform UI_Holder_CONTENT;
    public List<GameObject> UI_Buttons = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(GetListOfObjects(ModelFolder));
    }
    public void GetBundleObject(string assetName, UnityAction<GameObject> callback, Transform bundleParent)
    {
        StartCoroutine(GetDisplayBundleRoutine(assetName, callback, bundleParent));
    }
    private IEnumerator GetListOfObjects(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            while(!request.isDone)
            {
                yield return null;
            }
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Models Request Success");
                print("Data Received : "+ request.downloadHandler.text);
                string[] files = request.downloadHandler.text.Split('\n');
                foreach (string file in files)
                {
                    Debug.Log("File: " + file);
                }
               /* string[] models = request.downloadHandler.text.Split('\n');

                foreach (string model in models)
                {
                    GameObject newUI = Instantiate(uiButtonTemplate.gameObject);
                    newUI.GetComponent<UI_button>().ModelName = model;
                    newUI.transform.parent = UI_Holder_CONTENT.transform;
                    newUI.transform.position = Vector3.zero;
                    UI_Buttons.Add(newUI);
                }*/
            }
            else
            {
                Debug.Log("Models Request Failed: " + request.error);
            }
        }
    }
    IEnumerator GetDisplayBundleRoutine(string assetName, UnityAction<GameObject> callback, Transform bundleParent)
    {

        string bundleURL = ModelFolder + assetName;

        Debug.Log("Requesting bundle at " + bundleURL);

        //request asset bundle
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(bundleURL);
        yield return www.SendWebRequest();

        if (www.isNetworkError)
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
                Debug.Log("Not a valid asset bundle");
            }
        }
    }
}