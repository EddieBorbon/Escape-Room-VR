using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XRContent.Interaction;

public class BalloonInflator : XRGrabInteractable
{
    [Header("Balloon Data")]
    public Transform attachPoint;
    public Balloon balloonPrefab;
    
    private Balloon m_BalloonInstance; 
    private XRBaseController m_Controller;
    private OnVelocity m_OnVelocity;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        m_BalloonInstance = Instantiate(balloonPrefab, attachPoint);
        var controllerInteractor = args.interactorObject as XRBaseControllerInteractor;
        m_Controller = controllerInteractor.xrController;

        if (m_Controller != null)
        {
            m_Controller.SendHapticImpulse(1, 0.5f);
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        if (m_BalloonInstance != null)
        {
            Destroy(m_BalloonInstance.gameObject);
        }
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (isSelected && m_Controller != null && m_BalloonInstance != null)
        {
            // Inflate the balloon and adjust haptics dynamically
            float inflateAmount = Mathf.Lerp(1.0f, 4.0f, m_Controller.activateInteractionState.value);
            m_BalloonInstance.transform.localScale = Vector3.one * inflateAmount;

            float hapticIntensity = Mathf.Abs(m_Controller.activateInteractionState.value - m_Controller.uiPressInteractionState.value);
            m_Controller.SendHapticImpulse(hapticIntensity, 0.1f);
        }
    }
}
