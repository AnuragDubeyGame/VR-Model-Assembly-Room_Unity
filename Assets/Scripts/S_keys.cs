using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Keys", menuName = "MyGame/Keys")]
public class S_keys : ScriptableObject
{
    [SerializeField] private string accessKey;
    [SerializeField] private string secretKey;

    public string AccessKey
    {
        get { return accessKey; }
        set { accessKey = value; }
    }

    public string SecretKey
    {
        get { return secretKey; }
        set { secretKey = value; }
    }
}
