using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.XR.Interaction.Toolkit;

public class API : MonoBehaviour
{
    
    const string BundleFolder = "https://stl-loader-adarsh.s3.amazonaws.com/Models/";
    public Material bonesMaterial;
    public string layerName = "Grabable";
    public List<GameObject> UI_Buttons = new List<GameObject>();

    public void GetBundleObject(string assetName, UnityAction<GameObject> callback, Transform bundleParent)
    {
        StartCoroutine(GetDisplayBundleRoutine(assetName, callback, bundleParent));
    }

    IEnumerator GetDisplayBundleRoutine(string assetName, UnityAction<GameObject> callback, Transform bundleParent)
    {

        string bundleURL = BundleFolder + assetName;

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