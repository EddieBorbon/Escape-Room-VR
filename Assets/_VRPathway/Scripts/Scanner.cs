using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using TMPro;

public class Scanner : XRGrabInteractable
{
    [Header("Scanner Data")]
    public Animator animator;
    public LineRenderer laserRenderer;
    public TextMeshProUGUI targetName;
    public TextMeshProUGUI targetDetails;

    [Header("Scanning Materials")]
    public Material scanningMaterial; // Material to apply when scanning

    private string defaultTargetName = "Ready to Scan";
    private string defaultTargetDetails = "";

    private Material[] originalMaterials; // Original materials of the currently scanned object
    private Renderer currentObjectRenderer; // Renderer of the currently scanned object

    protected override void Awake(){
        base.Awake();
        ScannerActivated(false);
    }

    private void Start(){
        animator.SetBool("Opened", true);
    }

    protected virtual void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        animator.SetBool("Opened", true); 
        ResetToDefault();
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        animator.SetBool("Opened", false);
        RevertObjectMaterial();
    }

    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args); 
        ScannerActivated(true);
    }

    protected override void OnDeactivated(DeactivateEventArgs args)
    {
        base.OnDeactivated(args);
        ScannerActivated(false);
        ResetToDefault();
    }

    private void ScannerActivated(bool isActivated){
        laserRenderer.gameObject.SetActive(isActivated);

        if(!isActivated){
            ResetToDefault();
        }
    }

    private void ResetToDefault() {
        targetName.SetText(defaultTargetName);
        targetDetails.SetText(defaultTargetDetails);
        RevertObjectMaterial();
    }

    private void ScanForObjects(){
        RaycastHit hit;

        Vector3 worldHit = laserRenderer.transform.position + laserRenderer.transform.forward * 1000.0f;

        if(Physics.Raycast(laserRenderer.transform.position, laserRenderer.transform.forward, out hit)){
            worldHit = hit.point;

            // Update the target information
            targetName.SetText(hit.collider.name);

            // Calculate and display details
            float distance = Vector3.Distance(laserRenderer.transform.position, hit.point);
            Vector3 size = hit.collider.bounds.size;
            float angle = Vector3.Angle(laserRenderer.transform.forward, hit.normal);

            // Set all details into the targetDetails TextMeshPro
            targetDetails.SetText(
                $"Position: {hit.collider.transform.position}\n" +
                $"Distance: {distance:F2}m\n" +
                $"Size: {size:F2}\n" +
                $"Angle: {angle:F2}°"
            );

            // Revert the material of the previously scanned object if there is one
            if (currentObjectRenderer != null && currentObjectRenderer != hit.collider.GetComponent<Renderer>())
            {
                RevertObjectMaterial();
            }

            // Change the material of the current object being scanned
            currentObjectRenderer = hit.collider.GetComponent<Renderer>();
            if (currentObjectRenderer != null)
            {
                if (originalMaterials == null) // Save original materials only if it's the first time scanning this object
                {
                    originalMaterials = currentObjectRenderer.materials;
                }
                Material[] newMaterials = new Material[originalMaterials.Length];
                for (int i = 0; i < newMaterials.Length; i++) {
                    newMaterials[i] = scanningMaterial;
                }
                currentObjectRenderer.materials = newMaterials; // Apply the scanning material to all submeshes
            }
        } else {
            ResetToDefault();
        }

        laserRenderer.SetPosition(1, laserRenderer.transform.InverseTransformPoint(worldHit));
    }

    private void RevertObjectMaterial()
    {
        // Revert the object's material to its original state
        if (currentObjectRenderer != null && originalMaterials != null)
        {
            currentObjectRenderer.materials = originalMaterials;
            originalMaterials = null; // Clear original materials so they're only stored when scanning a new object
            currentObjectRenderer = null; // Clear the object reference
        }
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if(laserRenderer.gameObject.activeSelf){
            ScanForObjects();
        } else {
            RevertObjectMaterial(); // Ensure material reverts when not scanning
        }
    }
}
