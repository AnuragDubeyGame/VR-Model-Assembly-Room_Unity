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
using Dummiesman;

    public class API : MonoBehaviour
    {
        private string bucketName = "stl-loader-adarsh";
         private string accessKey;
         private string secretKey;
        const string BaseModelURL = "https://stl-loader-adarsh.s3.amazonaws.com/";

        private S_keys skeys;

        public Material bonesMaterial;
        public string layerName = "Grabable";
        public UI_button uiButtonTemplate;
        public Transform UI_Holder_CONTENT;
        public Transform SPAWNEDOBJ_Transform;
        public List<GameObject> UI_Buttons = new List<GameObject>();


        private async void Start()
        {
            UnityInitializer.AttachToGameObject(this.gameObject);
            UI_Buttons.Clear();
            skeys = FindObjectOfType<S_keys>();
            accessKey = skeys.AccessKey;
            secretKey = skeys.SecretKey;
            await GetListOfObjectsAsync();
        }
        public async Task GetListOfObjectsAsync()
        {
            AWSCredentials credentials = new BasicAWSCredentials(accessKey, secretKey);
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
                    foreach (var s3Object in response.Response.S3Objects)
                    {
                        GameObject newUI = Instantiate(uiButtonTemplate.gameObject);
                        newUI.GetComponent<UI_button>().ModelName = s3Object.Key;
                        newUI.transform.SetParent(UI_Holder_CONTENT.transform);
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
            UnityWebRequest www = UnityWebRequest.Get(bundleURL);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error downloading FBX model: " + www.error);
                yield break;
            }

            string savePath = Application.persistentDataPath + "/" + assetName;
            System.IO.File.WriteAllBytes(savePath, www.downloadHandler.data);
            print("Downloaded : "+www.downloadedBytes+"Bytes");

            var loadedObject = new OBJLoader().Load(savePath);
            foreach (var item in UI_Buttons)
            {
                if (assetName == item.GetComponent<UI_button>().ModelName)
                {
                    GameObject loading = item.transform.Find("Loading").gameObject;
                    loading.SetActive(false);
                }
            }

            GameObject MainOBJ = loadedObject.transform.GetChild(0).gameObject;
            MainOBJ.transform.SetParent(SPAWNEDOBJ_Transform);
            MainOBJ.transform.localPosition = Vector3.zero;
            MainOBJ.transform.localScale = new Vector3(0.003f, 0.003f, 0.003f);
            Destroy(loadedObject,0.2f);

            MainOBJ.transform.SetParent(null);
            MainOBJ.GetComponent<Renderer>().material = bonesMaterial;

            MainOBJ.AddComponent<JointCreator>();
            MeshCollider mc = MainOBJ.AddComponent<MeshCollider>();
            mc.convex = true;
            XRGrabInteractable xrI = MainOBJ.AddComponent<XRGrabInteractable>();
            int layerIndex = LayerMask.NameToLayer(layerName);
            MainOBJ.layer = layerIndex;

            xrI.movementType = XRBaseInteractable.MovementType.VelocityTracking;
            xrI.useDynamicAttach = true;
            MainOBJ.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
            MainOBJ.transform.parent = null;

            yield return null;

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
                        if (assetName == item.transform.name)
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
