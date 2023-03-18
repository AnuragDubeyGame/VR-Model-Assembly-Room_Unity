using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakForce_Manager : MonoBehaviour
{
    public void SetAllBreakForces(float value)
    {
        ConfigurableJoint[] joints = FindObjectsOfType<ConfigurableJoint>();
        foreach (ConfigurableJoint joint in joints)
        {
            joint.breakForce = value;
        }
    }
}
