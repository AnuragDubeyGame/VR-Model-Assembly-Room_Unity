using System.Collections;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.XR.Interaction.Toolkit;

public class API : MonoBehaviour
{
    
    const string BundleFolder = "https://stl-loader-adarsh.s3.amazonaws.com/Models/";
    public Material bonesMaterial;
    public Preset XRI_Preset;

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
                GameObject arObject = Instantiate(bundle.LoadAsset(rootAssetPath) as GameObject, bundleParent);
                arObject.transform.localPosition = Vector3.zero;
                arObject.GetComponent<Renderer>().material = bonesMaterial;
                arObject.AddComponent<JointCreator>();
                XRGrabInteractable xrI = arObject.AddComponent<XRGrabInteractable>();
                XRI_Preset.ApplyTo(xrI);
                arObject.transform.parent = null;
                bundle.Unload(false);
                callback(arObject);
            }
            else
            {
                Debug.Log("Not a valid asset bundle");
            }
        }
    }
}