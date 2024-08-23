using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
public class CardReader : XRSocketInteractor
{
    private Transform m_KeycardTransform;
    private Vector3 m_HoverEntry;
    private bool m_SwipIsValid;
    private float m_SwipeStartTime;

    public float requiredSwipeDistance = 0.2f;
    public float maxSwipeTime = 2.0f;
    public float alignmentThreshold = 0.1f;
    public float AllowedUprightErrorRange = 0.1f;

    public GameObject VisualLockToHide;
    public MonoBehaviour HandleToEnable;
    public Collider pathCollider; // Referencia al colider del trayecto
    public float snapDistance = 0.1f; // Distancia dentro de la cual se activa el snap

    public Renderer greenLightRenderer;
    public Renderer redLightRenderer;
    public Material greenLightEmissiveMaterial;
    public Material redLightEmissiveMaterial;
    public Material greenLightOriginalMaterial;
    public Material redLightOriginalMaterial;

    public AudioSource audioSource;
    public AudioClip successClip;
    public AudioClip errorClip;

    private Coroutine lightCoroutine;
    protected override void Start()
    {
        base.Start();
        // Intenta obtener el Collider del mismo GameObject o de un hijo
        pathCollider = GetComponentInChildren<Collider>();
        if (pathCollider == null)
        {
            Debug.LogError("PathCollider not assigned or found. Please assign a Collider.");
        }
    }
    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);

        m_KeycardTransform = args.interactableObject.transform;
        m_HoverEntry = m_KeycardTransform.position;
        m_SwipeStartTime = Time.time;
        m_SwipIsValid = true;

        Debug.Log("Keycard hovered: " + m_KeycardTransform.name);
        Debug.Log("Hover entry position: " + m_HoverEntry);
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);

        if (m_KeycardTransform == null)
        {
            Debug.LogWarning("KeycardTransform is null on hover exit.");
            return;
        }

        Vector3 entryToExit = m_KeycardTransform.position - m_HoverEntry;
        float swipeDuration = Time.time - m_SwipeStartTime;

        Debug.Log("Keycard exit position: " + m_KeycardTransform.position);
        Debug.Log("Distance traveled: " + entryToExit + ", Y component: " + entryToExit.y);
        Debug.Log("Swipe duration: " + swipeDuration + " seconds");

        if (m_SwipIsValid && entryToExit.y < -requiredSwipeDistance && swipeDuration <= maxSwipeTime)
        {
            if (VisualLockToHide != null)
            {
                VisualLockToHide.SetActive(false);
                Debug.Log("Visual lock hidden.");
            }

            if (HandleToEnable != null)
            {
                HandleToEnable.enabled = true;
                Debug.Log("Handle enabled.");
            }

            ChangeLightMaterial(greenLightRenderer, greenLightEmissiveMaterial, greenLightOriginalMaterial, true);
            ChangeLightMaterial(redLightRenderer, redLightOriginalMaterial, redLightEmissiveMaterial, false);

            PlaySound(successClip);
        }
        else
        {
            ChangeLightMaterial(greenLightRenderer, greenLightOriginalMaterial, greenLightEmissiveMaterial, false);
            ChangeLightMaterial(redLightRenderer, redLightEmissiveMaterial, redLightOriginalMaterial, true);

            PlaySound(errorClip);

            Debug.Log("Swipe not valid or distance not sufficient.");
        }

        m_KeycardTransform = null;
    }

    private void ChangeLightMaterial(Renderer lightRenderer, Material newMaterial, Material originalMaterial, bool isActive)
    {
        if (isActive)
        {
            lightRenderer.material = newMaterial;
            if (lightCoroutine != null)
            {
                StopCoroutine(lightCoroutine);
            }
            lightCoroutine = StartCoroutine(RevertMaterialAfterDelay(lightRenderer, originalMaterial, 3f));
        }
        else
        {
            lightRenderer.material = newMaterial;
        }
    }

    private IEnumerator RevertMaterialAfterDelay(Renderer lightRenderer, Material originalMaterial, float delay)
    {
        yield return new WaitForSeconds(delay);
        lightRenderer.material = originalMaterial;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    private void Update()
    {
        if (m_KeycardTransform != null)
        {
            SnapKeycardToPath();
            Vector3 keycardForward = m_KeycardTransform.forward;
            float dot = Vector3.Dot(keycardForward, Vector3.up);

            Debug.Log("Keycard forward vector: " + keycardForward);
            Debug.Log("Dot product: " + dot);

            if (dot < 1 - AllowedUprightErrorRange)
            {
                m_SwipIsValid = false;
                Debug.Log("Swipe invalid due to misalignment.");
            }
        }
    }

    private void SnapKeycardToPath()
    {
        if (m_KeycardTransform != null && pathCollider != null)
        {
            Vector3 cardPosition = m_KeycardTransform.position;
            if (pathCollider.bounds.Contains(cardPosition))
            {
                Vector3 closestPoint = pathCollider.ClosestPoint(cardPosition);
                if (Vector3.Distance(cardPosition, closestPoint) <= snapDistance)
                {
                    m_KeycardTransform.position = closestPoint;
                    m_KeycardTransform.rotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);
                }
            }
        }
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        Debug.Log("CanSelect called, returning false.");
        return false;
    }
}
