using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DoorHandle : XRBaseInteractable
{
    [Header("Door Handle Data")]
    public Transform draggedTransform;
    public Vector3 localDragDirection;
    public float dragDistance;
    public int doorWeight = 20;

    // Audio settings
    [Header("Audio Settings")]
    public AudioClip doorOpenSound;
    private AudioSource audioSource;

    // Haptic settings
    [Header("Haptic Feedback")]
    public float maxHapticIntensity = 1.0f; // Max intensity of the vibration
    public float minHapticIntensity = 0.1f; // Min intensity of the vibration
    private XRBaseControllerInteractor controllerInteractor;

    // Visual references
    [Header("Visual References")]
    public LineRenderer handleToHandLine;
    public LineRenderer dragVectorLine;

    private Vector3 m_StartPosition;
    private Vector3 m_EndPosition;
    private Vector3 m_WorldDragDirection;

    private void Start()
    {
        // Initialize world drag direction
        m_WorldDragDirection = transform.TransformDirection(localDragDirection).normalized;

        // Set start and end positions
        m_StartPosition = draggedTransform.position;
        m_EndPosition = m_StartPosition + m_WorldDragDirection * dragDistance;

        // Initialize AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = doorOpenSound;
        audioSource.playOnAwake = false;

        // Initialize visual references
        handleToHandLine.gameObject.SetActive(false);
        dragVectorLine.gameObject.SetActive(false);

        // Initialize haptic feedback
        controllerInteractor = GetComponent<XRBaseControllerInteractor>();
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed && isSelected)
        {
            var interactorTransform = firstInteractorSelecting.GetAttachTransform(this);
            Vector3 selfToInteractor = interactorTransform.position - transform.position;
            float forceInDirectionOfDrag = Vector3.Dot(selfToInteractor, m_WorldDragDirection);
            bool dragToEnd = forceInDirectionOfDrag > 0.0f;
            float absoluteForce = Mathf.Abs(forceInDirectionOfDrag);
            float speed = absoluteForce / Time.deltaTime / doorWeight;

            // Move the door
            draggedTransform.position = Vector3.MoveTowards(draggedTransform.position,
                dragToEnd ? m_EndPosition : m_StartPosition,
                speed * Time.deltaTime);

            // Play the door open sound
            if (speed > 0 && !audioSource.isPlaying)
            {
                audioSource.Play();
            }

            // Provide haptic feedback
            if (controllerInteractor != null)
            {
                float intensity = Mathf.Lerp(minHapticIntensity, maxHapticIntensity, speed / dragDistance);
                controllerInteractor.SendHapticImpulse(intensity, 0.1f);
            }

            // Update visual lines
            handleToHandLine.SetPosition(0, transform.position);
            handleToHandLine.SetPosition(1, interactorTransform.position);
            dragVectorLine.SetPosition(0, transform.position);
            dragVectorLine.SetPosition(1, transform.position + forceInDirectionOfDrag * m_WorldDragDirection);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 worldDirection = transform.TransformDirection(localDragDirection);
        worldDirection.Normalize();
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + worldDirection * dragDistance);
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        handleToHandLine.gameObject.SetActive(true);
        dragVectorLine.gameObject.SetActive(true);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        handleToHandLine.gameObject.SetActive(false);
        dragVectorLine.gameObject.SetActive(false);
    }
}
