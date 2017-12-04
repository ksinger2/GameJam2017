using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public enum handState
{
    OPEN,
    CLOSED,
    NONE
}

public class WandController : MonoBehaviour
{

    [System.Serializable]
    public enum typeOfAction
    {
        TOUCH_TO_GRAB,
        TRIGGER_TO_GRAB
    }

    public handState stateOfHand;

    private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
    private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
    private Valve.VR.EVRButtonId Touchpad = Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad;
    private Valve.VR.EVRButtonId MenuButton = Valve.VR.EVRButtonId.k_EButton_ApplicationMenu;


    public SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
    private SteamVR_TrackedObject trackedObj;


    HashSet<InteractableItem> objects = new HashSet<InteractableItem>();
    public InteractableItem closestItem { get; private set; }


    public InteractableItem inHandItem { get; private set; }

    public bool held { get; private set; }

    public GameObject handModel;
    public typeOfAction grabType;

    public bool record;

    void Init()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        held = false;
        inHandItem = null;

        //OpenHand;
        if (handModel != null && !handModel.activeInHierarchy) handModel.SetActive(true);
        return;
    }

    void Start()
    {
        Init();
    }

    void Update()
    {
        //StartCoroutine(CheckAngle());
        if (held)
        {
            //Close hand
            if (inHandItem == null || inHandItem.isResponding)
            {
                //Open hand
                inHandItem = null;
                if (handModel != null && !handModel.activeInHierarchy) handModel.SetActive(true);
                held = false;

            }
        }
        if ((grabType == typeOfAction.TRIGGER_TO_GRAB && controller.GetPressDown(triggerButton)) || (grabType == typeOfAction.TOUCH_TO_GRAB))
        {
            float min = float.MaxValue;
            float distance;
            //Close Hand
            foreach (InteractableItem obj in objects)//Pick the closest object my controller is by if there are multiple objects near me with interactable item scripts attached
            {
                if (obj != null) 
                {
                    distance = (obj.transform.position - transform.position).sqrMagnitude;

                    if (distance < min)
                    {
                        min = distance;
                        closestItem = obj;
                    }
                    if (inHandItem != closestItem)
                    {
                        if (closestItem != null && inHandItem != null)
                        {
                            if (inHandItem.IsInteracting() && inHandItem.attachedWand != this)//If we are already interacting
                            {
                                inHandItem.EndInteraction(this);
                                held = false;

                            }
                            if (closestItem != null && !closestItem.IsInteracting() && !closestItem.isResponding && !held)
                            {
                                inHandItem = closestItem;
                                inHandItem.BeginInteraction(this);
                                held = true;
                            }
                        }
                        else if (closestItem != null)
                        {
                            inHandItem = closestItem;
                            inHandItem.BeginInteraction(this);
                            held = true;
                        }
                    }

                }
                else objects.RemoveWhere(ShouldRemove => obj);
            }

            if (handModel != null && handModel.activeInHierarchy) handModel.SetActive(false);

        }

        if ((controller.GetPressUp(triggerButton) && grabType == typeOfAction.TRIGGER_TO_GRAB) && inHandItem != null)//let go of item
        {
            held = false;
            //Open hand
            if (handModel != null && !handModel.activeInHierarchy) handModel.SetActive(true);

            inHandItem.EndInteraction(this);
        }
        else if ((controller.GetPressUp(triggerButton) && grabType == typeOfAction.TRIGGER_TO_GRAB) && !held)
        {
            //Open hand
            if (handModel != null && !handModel.activeInHierarchy) handModel.SetActive(true);

        }

        if (record)
        {
            //GlobalVariables.instance.recorder.RecordHand();
        }

    }

    void OnTriggerEnter(Collider collider)
    {
        InteractableItem collidedItem = collider.GetComponent<InteractableItem>();
        if (collidedItem)
        {
            objects.Add(collidedItem);
        }
    }
    void OnTriggerExit(Collider collider)
    {
        InteractableItem collidedItem = collider.GetComponent<InteractableItem>();

        if (collidedItem)
        {
            objects.RemoveWhere(ShouldRemove => collidedItem);
        }

    }

    private bool ShouldRemove(GameObject obj)
    {
        return obj == null;
    }


}
