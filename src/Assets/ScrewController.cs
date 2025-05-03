using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))] // Ensures a Rigidbody is present
public class ScrewController : MonoBehaviour
{
    public bool isScrewedIn = true;
    public float unscrewDistance = 0.1f;
    public float unscrewRotation = 720f; // Degrees to rotate (e.g., 720 for two full turns)
    public float unscrewTime = 1.0f;     // Duration of the unscrew animation

    private Vector3 initialLocalPosition; // Use local position relative to parent
    private Quaternion initialLocalRotation;// Use local rotation relative to parent
    private Transform originalParent;      // To store the original parent
    private Rigidbody rb;               // Reference to the Rigidbody

    private bool isInteractable = false;
    private bool isMoving = false;

    void Awake() // Get components early
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        originalParent = transform.parent;      // Store original parent
        initialLocalPosition = transform.localPosition; // Store initial local position
        initialLocalRotation = transform.localRotation; // Store initial local rotation

        // Ensure Rigidbody starts kinematic
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        else
        {
            // This should not happen due to [RequireComponent] but good practice
            Debug.LogError("Rigidbody component not found!", this);
        }
    }

    // Called by the ScrewdriverTip trigger or similar mechanism
    public void SetInteractable(bool value)
    {
        isInteractable = value;
    }

    // Called by the Screwdriver's interaction script when activated
    public void TryUnscrew()
    {
        // Check conditions: interactable, screwed in, not already moving, and has Rigidbody
        if (isInteractable && isScrewedIn && !isMoving && rb != null)
        {
            StartCoroutine(UnscrewSequence());
        }
    }

    IEnumerator UnscrewSequence()
    {
        isMoving = true;
        isScrewedIn = false; // Mark as unscrewed

        // --- Calculate Target Position (Local) ---
        // Uses the screw's local forward axis (blue arrow in Unity) for movement direction
        Vector3 targetLocalPosition = initialLocalPosition - Vector3.right * unscrewDistance;

        // --- Rotation Parameters ---
        float rotationRate = 0f;
        if (unscrewTime > 0)
        {
            rotationRate = unscrewRotation / unscrewTime; // Degrees per second
        }
        // Rotates around the local X-axis (red arrow in Unity)
        Vector3 rotationAxis = Vector3.forward; // Local forward axis (blue arrow in Unity)

        float elapsedTime = 0f;

        // --- Animate Unscrewing ---
        while (elapsedTime < unscrewTime)
        {
            float fraction = (unscrewTime > 0) ? elapsedTime / unscrewTime : 1f;

            // 1. Interpolate Position (using local coordinates)
            transform.localPosition = Vector3.Lerp(initialLocalPosition, targetLocalPosition, fraction);

            // 2. Apply Rotation Incrementally (local axis)
            if (Time.deltaTime > 0 && rotationRate != 0)
            {
                float frameRotation = rotationRate * Time.deltaTime;
                transform.Rotate(rotationAxis, frameRotation, Space.Self);
            }

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // --- Set Final Transform State Precisely ---
        transform.localPosition = targetLocalPosition;
        // Set final rotation based on total degrees around the chosen axis (X in this case)
        transform.localRotation = initialLocalRotation * Quaternion.Euler(unscrewRotation, 0, 0);


        // --- Detach and Enable Physics ---
        // 1. Unparent (keeps world position)
        if (transform.parent != null) // Only unparent if it has a parent
        {
            transform.SetParent(null, true);
        }

        // 2. Enable physics
        rb.isKinematic = false;
        // Optional: Ensure gravity is enabled if not set in Inspector (usually it is)
        // rb.useGravity = true;


        isMoving = false; // Finished the sequence
    }

    // Function to reset the screw to its initial state
    public void ResetScrew()
    {
        StopAllCoroutines(); // Stop animation if running
        isMoving = false;

        // Disable physics before moving/re-parenting
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;        // Reset physics movement
            rb.angularVelocity = Vector3.zero; // Reset physics rotation
        }

        // Re-parent and reset local transform
        transform.SetParent(originalParent, true); // Re-attach using world position preservation
        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;

        isScrewedIn = true; // Mark as screwed back in
    }
}