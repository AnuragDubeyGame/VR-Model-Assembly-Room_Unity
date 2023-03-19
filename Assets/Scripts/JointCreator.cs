using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class JointCreator : MonoBehaviour
{
    public event Action OnCFDestroyed;
    [SerializeField] private ConfigurableJointMotion jointAngularXMotion = ConfigurableJointMotion.Locked;
    [SerializeField] private ConfigurableJointMotion jointAngularYMotion = ConfigurableJointMotion.Limited;
    [SerializeField] private ConfigurableJointMotion jointAngularZMotion = ConfigurableJointMotion.Locked;
    [SerializeField] private float jointAngularYLimit = 10f;
    [SerializeField] private float jointSpring = 10f;
    [SerializeField] private float jointDamper = 10f;
    private float BREAK_FORCE;
    private Slider bf_slider;
    Rigidbody rb;
    private bool canTriggerEvent = true;
    public bool isConnected;
    private ConfigurableJoint myCF;
    private GameObject otherConnnectBody;

    private void Start()
    {
        bf_slider = FindObjectOfType<Slider>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        isConnected = false;
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Dustbin"))
        { 
            Destroy(gameObject);
        }
        BREAK_FORCE =  bf_slider.value;
        GameObject otherObject = collision.gameObject;

        JointCreator otherJointCreator = otherObject.GetComponent<JointCreator>();
        if (otherJointCreator == null)
        {
            return;
        }
        ConfigurableJoint[] existingJoints = otherObject.GetComponents<ConfigurableJoint>();
        foreach (ConfigurableJoint joint in existingJoints)
        {
            if (joint.connectedBody == GetComponent<Rigidbody>())
            {
                return;
            }
        }
        ConfigurableJoint[] ourJoints = GetComponents<ConfigurableJoint>();
        foreach (ConfigurableJoint joint in ourJoints)
        {
            if (joint.connectedBody == otherObject.GetComponent<Rigidbody>())
            {
                return;
            }
        }
        ConfigurableJoint newJoint = gameObject.AddComponent<ConfigurableJoint>();
        myCF = newJoint;
        OnCFDestroyed += JointCreator_OnCFDestroyed;
        canTriggerEvent = true;
        otherConnnectBody = otherObject;
        newJoint.connectedBody = otherObject.GetComponent<Rigidbody>();

        Rigidbody rigidbody1 = GetComponent<Rigidbody>();
        if (rigidbody1 != null)
        {
            rigidbody1.useGravity = true;
            isConnected = true;
        }
        Rigidbody rigidbody2 = newJoint.connectedBody.gameObject.GetComponent<Rigidbody>();
        if (rigidbody2 != null)
        {
            collision.gameObject.GetComponent<JointCreator>().isConnected = true;
            rigidbody2.useGravity = true;
        }
        ContactPoint contact = collision.contacts[0];
        Vector3 localAnchor = transform.InverseTransformPoint(contact.point);
        newJoint.anchor = localAnchor;

        newJoint.axis = Vector3.forward;
        newJoint.angularXMotion = jointAngularXMotion;
        newJoint.angularYMotion = jointAngularYMotion;
        newJoint.angularZMotion = jointAngularZMotion;
        newJoint.xMotion = ConfigurableJointMotion.Locked;
        newJoint.yMotion = ConfigurableJointMotion.Locked;
        newJoint.zMotion = ConfigurableJointMotion.Locked;
        newJoint.highAngularXLimit = new SoftJointLimit { limit = 0 };
        newJoint.lowAngularXLimit = new SoftJointLimit { limit = 0 };
        newJoint.angularYLimit = new SoftJointLimit { limit = jointAngularYLimit };

        newJoint.breakForce = BREAK_FORCE;
        // Set the joint's spring and damper values
        JointDrive jointDrive = new JointDrive();
        jointDrive.mode = JointDriveMode.PositionAndVelocity;
        jointDrive.positionSpring = jointSpring;
        jointDrive.positionDamper = jointDamper;
        newJoint.angularXDrive = jointDrive;
        newJoint.angularYZDrive = jointDrive;


    }

    private void JointCreator_OnCFDestroyed()
    {
        Rigidbody otherRB = otherConnnectBody.GetComponent<Rigidbody>();
        otherRB.gameObject.GetComponent<JointCreator>().isConnected = false;
        isConnected = false;
        rb.useGravity = false;
        rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero;
        otherRB.useGravity = false;
        otherRB.velocity = Vector3.zero; otherRB.angularVelocity = Vector3.zero;
        otherConnnectBody = null;
    }

    private void Update()
    {
        if (myCF == null && canTriggerEvent)
        {
            canTriggerEvent = false;
            OnCFDestroyed?.Invoke();
        }
        if(!isConnected)
        {
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        if(isConnected && rb.useGravity == false)
        {
            print("Setting Gravity");
            rb.useGravity = true;
        }
   
    }
}
