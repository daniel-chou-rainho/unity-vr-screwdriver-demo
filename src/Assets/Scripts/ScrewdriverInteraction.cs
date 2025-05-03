using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // Explicit namespace

public class ScrewdriverInteraction : MonoBehaviour
{
    public GameObject tipObject;
    private ScrewController currentScrew = null;
    private XRGrabInteractable grabInteractable;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            Debug.LogError("XRGrabInteractable not found!", this);
            return;
        }
        grabInteractable.activated.AddListener(PerformAction);

        if (tipObject != null)
        {
            // Get or add the TipTriggerHandler
            if (!tipObject.TryGetComponent<TipTriggerHandler>(out var tipHandler))
            {
                tipHandler = tipObject.AddComponent<TipTriggerHandler>();
            }
            tipHandler.interactionScript = this;
        }
        else
        {
            Debug.LogError("Tip Object not assigned!", this);
        }
    }

    // Called by XRGrabInteractable's 'activated' event
    void PerformAction(ActivateEventArgs arg0)
    {
        currentScrew?.TryUnscrew(); // If currentScrew is not null, call TryUnscrew
    }

    // Called by TipTriggerHandler
    public void TipEnteredScrew(ScrewController screw)
    {
        currentScrew = screw;
        currentScrew?.SetInteractable(true);
    }

    // Called by TipTriggerHandler
    public void TipExitedScrew(ScrewController screw)
    {
        if (currentScrew == screw) // Only process exit if it's the screw we were tracking
        {
            currentScrew?.SetInteractable(false);
            currentScrew = null;
        }
    }

    void OnDestroy()
    {
        // Use null-conditional operator for safety
        grabInteractable?.activated.RemoveListener(PerformAction);
    }

    // Helper component automatically added to the tip object
    public class TipTriggerHandler : MonoBehaviour
    {
        public ScrewdriverInteraction interactionScript; // Set by the main script

        private void OnTriggerEnter(Collider other)
        {
            // Use TryGetComponent for combined get and null check
            if (interactionScript != null && other.CompareTag("Screw") && other.TryGetComponent<ScrewController>(out var screw))
            {
                interactionScript.TipEnteredScrew(screw);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (interactionScript != null && other.CompareTag("Screw") && other.TryGetComponent<ScrewController>(out var screw))
            {
                interactionScript.TipExitedScrew(screw);
            }
        }
    }
}