using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JointCreator : MonoBehaviour
{
    [SerializeField] private ConfigurableJointMotion jointAngularXMotion = ConfigurableJointMotion.Locked;
    [SerializeField] private ConfigurableJointMotion jointAngularYMotion = ConfigurableJointMotion.Limited;
    [SerializeField] private ConfigurableJointMotion jointAngularZMotion = ConfigurableJointMotion.Locked;
    [SerializeField] private float jointAngularYLimit = 10f;
    [SerializeField] private float jointSpring = 10f;
    [SerializeField] private float jointDamper = 10f;
    private float BREAK_FORCE;
    private Slider bf_slider;

    private void Start()
    {
        bf_slider = FindObjectOfType<Slider>();
    }
    void OnCollisionEnter(Collision collision)
    {
        BREAK_FORCE =  bf_slider.value;
        // Get the other object that we collided with
        GameObject otherObject = collision.gameObject;

        // Check if the other object has a JointCreator script attached
        JointCreator otherJointCreator = otherObject.GetComponent<JointCreator>();
        if (otherJointCreator == null)
        {
            // The other object does not have a JointCreator script, so we can't create a joint
            return;
        }

        // Check if the other object is already connected to us with a joint
        ConfigurableJoint[] existingJoints = otherObject.GetComponents<ConfigurableJoint>();
        foreach (ConfigurableJoint joint in existingJoints)
        {
            if (joint.connectedBody == GetComponent<Rigidbody>())
            {
                // The other object is already connected to us, so we don't need to create another joint
                return;
            }
        }

        // Check if we already have a joint with the other object
        ConfigurableJoint[] ourJoints = GetComponents<ConfigurableJoint>();
        foreach (ConfigurableJoint joint in ourJoints)
        {
            if (joint.connectedBody == otherObject.GetComponent<Rigidbody>())
            {
                // We already have a joint with the other object, so we don't need to create another joint
                return;
            }
        }

        // Create a joint component on this object and connect it to the other object
        ConfigurableJoint newJoint = gameObject.AddComponent<ConfigurableJoint>();
        newJoint.connectedBody = otherObject.GetComponent<Rigidbody>();

        // Set the anchor point of the joint to the contact point position
        ContactPoint contact = collision.contacts[0];
        Vector3 localAnchor = transform.InverseTransformPoint(contact.point);
        newJoint.anchor = localAnchor;

        // Set the joint's axis
        newJoint.axis = Vector3.forward;

        // Set the joint's motion and limits
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

}
