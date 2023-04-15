using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using UnityEngine.XR.Interaction.Toolkit;
using System.Threading.Tasks;
using Dummiesman;
using System.IO;
using UnityEditor.ShaderGraph.Serialization;
using System.Linq;

public class API : MonoBehaviour
    {
        private string bucketName = "stl-loader-adarsh";
         private string accessKey;
         private string secretKey;
        const string BaseModelURL = "https://stl-loader-adarsh.s3.amazonaws.com/";

        public string userName;
        public string repoName;
        public string branchName;
        public string personalAccessToken;

        public Material bonesMaterial;
        public string layerName = "Grabable";
        public UI_button uiButtonTemplate;
        public Transform UI_Holder_CONTENT;
        public Transform SPAWNEDOBJ_Transform;
        public List<GameObject> UI_Buttons = new List<GameObject>();


        private async void Start()
        {
            UI_Buttons.Clear();
            StartCoroutine(GetListOfObjectsAsync());
        }
        public IEnumerator GetListOfObjectsAsync()
        {
            print("Trying to access github files");
            string url = "https://api.github.com/repos/" + userName + "/" + repoName + "/git/trees/" + branchName + "?recursive=1&path=Assets/Scripts/";
            print(url);
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization", "token " + personalAccessToken);
            request.SendWebRequest(); // use the async version of SendWebRequest

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(request.error);
            }
            else
            {
                while(!request.isDone)
                {
                    print(".");
                    yield return null;
                }
                print("Resquest success : "+request.result);
                string json = request.downloadHandler.text;
                List<string> files = new List<string>();

                Regex regex = new Regex("\"path\":\"([^\"]+)\"");
                MatchCollection matches = regex.Matches(json);
                
                print("Got the data! : " + json);
                GitHubTreeResponse treeResponse = JsonUtility.FromJson<GitHubTreeResponse>(request.downloadHandler.text);

                List<GitHubObject> targetFolderObjects = treeResponse.tree
                   .Where(obj => obj.path.StartsWith("Assets/Scripts/"))
                   .ToList();

                if (targetFolderObjects.Count > 0)
                {
                    Debug.Log("Found " + targetFolderObjects.Count + " objects in folder: " + "Assets/Scripts");
                    foreach (GitHubObject obj in targetFolderObjects)
                    {
                        Debug.Log("Object path: " + obj.path + ", URL: " + obj.url);
                    }
                }
                else
                {
                    Debug.LogError("No objects found in folder: " + "Assets/Scripts");
                }
        /*GameObject newUI = Instantiate(uiButtonTemplate.gameObject);
        newUI.GetComponent<UI_button>().ModelName = (string)ch;
        newUI.transform.SetParent(UI_Holder_CONTENT.transform);
        newUI.transform.localPosition = Vector3.zero;
        newUI.transform.localScale = Vector3.one;
        UI_Buttons.Add(newUI);
        newUI.GetComponent<UI_button>().DisplayName();*/
    }

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
    }
