using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // Needed for XR Grab Interactable events

public class ScrewdriverInteraction : MonoBehaviour
{
    // Assign the child 'ScrewdriverTip' GameObject here in the Inspector
    public GameObject tipObject;

    private ScrewController currentScrew = null;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    void Start()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable == null)
        {
            Debug.LogError("XRGrabInteractable not found on this GameObject!");
            return;
        }

        // --- Hook into the 'activated' event ---
        grabInteractable.activated.AddListener(PerformAction);
        // --- Hook into the trigger events on the tip ---
        if (tipObject != null)
        {
            TipTriggerHandler tipHandler = tipObject.AddComponent<TipTriggerHandler>(); // Add helper component
            tipHandler.interactionScript = this; // Give it a reference back to this script
        }
        else
        {
            Debug.LogError("Tip Object not assigned in the Inspector!");
        }
    }

    // --- Called by the 'activated' event from XR Grab Interactable ---
    public void PerformAction(ActivateEventArgs arg0)
    {
        Debug.Log("Screwdriver Activated!");
        // Instead of confetti, try to interact with the screw
        if (currentScrew != null)
        {
            currentScrew.TryUnscrew();
        }
        else
        {
            Debug.Log("Not touching a screw.");
            // Maybe play an 'error' sound here?
        }
    }

    // --- Called by TipTriggerHandler when trigger enters ---
    public void TipEnteredScrew(ScrewController screw)
    {
        currentScrew = screw;
        currentScrew.SetInteractable(true); // Tell the screw it can be interacted with
    }

    // --- Called by TipTriggerHandler when trigger exits ---
    public void TipExitedScrew(ScrewController screw)
    {
        if (currentScrew == screw) // Make sure we are exiting the screw we thought we were touching
        {
            currentScrew.SetInteractable(false); // Tell the screw it's no longer interactable
            currentScrew = null;
        }
    }

    // --- Clean up listener when destroyed ---
    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.activated.RemoveListener(PerformAction);
        }
    }


    // --- Helper Component to put on the Tip GameObject ---
    // This keeps trigger logic separate and reports back to the main script
    public class TipTriggerHandler : MonoBehaviour
    {
        public ScrewdriverInteraction interactionScript; // Assign this automatically in Start()

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Screw")) // Use the tag!
            {
                ScrewController screw = other.GetComponent<ScrewController>();
                if (screw != null && interactionScript != null)
                {
                    interactionScript.TipEnteredScrew(screw);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Screw"))
            {
                ScrewController screw = other.GetComponent<ScrewController>();
                if (screw != null && interactionScript != null)
                {
                    interactionScript.TipExitedScrew(screw);
                }
            }
        }
    }
}