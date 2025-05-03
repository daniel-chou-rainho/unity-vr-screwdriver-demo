using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class ScrewController : MonoBehaviour
{
    public bool isScrewedIn = true;
    public float unscrewDistance = 0.1f;
    public float unscrewRotation = 720f;
    public float unscrewTime = 1.0f;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private Transform originalParent;
    private Rigidbody rb;
    private bool isInteractable = false;
    private bool isMoving = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        else
        {
            Debug.LogError("Rigidbody component not found!", this);
            return; // Stop if no Rigidbody
        }

        originalParent = transform.parent;
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }

    public void SetInteractable(bool value)
    {
        isInteractable = value;
    }

    public void TryUnscrew()
    {
        if (isInteractable && isScrewedIn && !isMoving && rb != null)
        {
            StartCoroutine(UnscrewSequence());
        }
    }

    IEnumerator UnscrewSequence()
    {
        isMoving = true;
        isScrewedIn = false;

        Vector3 targetLocalPosition = initialLocalPosition - Vector3.right * unscrewDistance;
        float rotationRate = (unscrewTime > 0) ? unscrewRotation / unscrewTime : 0f;
        Vector3 rotationAxis = Vector3.forward; // Local forward axis for rotation animation

        float elapsedTime = 0f;
        while (elapsedTime < unscrewTime)
        {
            float fraction = (unscrewTime > 0) ? elapsedTime / unscrewTime : 1f;
            transform.localPosition = Vector3.Lerp(initialLocalPosition, targetLocalPosition, fraction);

            if (Time.deltaTime > 0 && rotationRate != 0)
            {
                transform.Rotate(rotationAxis, rotationRate * Time.deltaTime, Space.Self);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetLocalPosition;
        // Apply final rotation relative to initial state around local X
        transform.localRotation = initialLocalRotation * Quaternion.Euler(unscrewRotation, 0, 0);

        if (transform.parent != null)
        {
            transform.SetParent(null, true);
        }

        rb.isKinematic = false;
        // rb.useGravity = true; // Optional: Ensure gravity is on if needed

        isMoving = false;
    }

    public void ResetScrew()
    {
        StopAllCoroutines();
        isMoving = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.SetParent(originalParent, true);
        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;
        isScrewedIn = true;
    }
}