using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class TouchButton : XRBaseInteractable
{
    private Color originalColor;
    public Color hoverColor; // Color when the button is pressed

    private Renderer m_RendererToChange;
    public NumberPad linkedKeypad; // Reference to the NumberPad script

    public int ButtonNumber; // Number of the button to send to NumberPad
    public bool isSpecialButton; // True if this is a special button (OK or X)
    public string ButtonType; // Type of the special button ("OK" or "X")

    public float moveDistance = 0.1f; // Distance to move the button on press
    public float moveDuration = 0.1f; // Duration of the movement

    private Vector3 originalPosition; // Original position of the button

    protected override void Awake()
    {
        base.Awake();
        m_RendererToChange = GetComponent<Renderer>();
        originalPosition = transform.localPosition; // Store the original position

        // Store the original color of the button
        if (m_RendererToChange != null)
        {
            originalColor = m_RendererToChange.material.color;
        }
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        if (m_RendererToChange != null)
        {
            // Change the color to hoverColor when the button is pressed
            m_RendererToChange.material.color = hoverColor;
        }

        // Move the button backwards along the Y-axis
        StartCoroutine(MoveButton(originalPosition + new Vector3(0, -moveDistance, 0), moveDuration));

        // Notify the NumberPad that a button has been pressed
        if (linkedKeypad != null)
        {
            if (isSpecialButton)
            {
                linkedKeypad.SpecialButtonPressed(ButtonType); // Call SpecialButtonPressed() for OK or X
            }
            else
            {
                linkedKeypad.ButtonPressed(ButtonNumber); // Handle numeric button press
            }
        }
        else
        {
            Debug.LogWarning("LinkedKeypad is not assigned!");
        }
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        if (m_RendererToChange != null)
        {
            // Restore the original color when the button is released
            m_RendererToChange.material.color = originalColor;
        }

        // Move the button back to its original position
        StartCoroutine(MoveButton(originalPosition, moveDuration));
    }

    private IEnumerator MoveButton(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPosition; // Ensure it ends exactly at the target position
    }
}
