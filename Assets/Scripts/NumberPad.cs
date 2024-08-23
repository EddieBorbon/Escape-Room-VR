using UnityEngine;
using TMPro;
using System.Collections;

public class NumberPad : MonoBehaviour
{
    public string Sequence; // The correct code sequence
    public Transform KeycardSpawnLocation; // The location where the keycard will be spawned
    public GameObject KeycardPrefab; // The keycard prefab to spawn
    public TextMeshProUGUI InputDisplayText; // The TMP text component to display the entered code
    public AudioClip CorrectCodeSound; // Audio clip for correct code feedback
    public AudioClip IncorrectCodeSound; // Audio clip for incorrect code feedback
    public AudioClip KeycardSound; // Audio clip for keycard appearance
    public AudioClip ButtonPressSound; // Audio clip for button press feedback
    public AudioSource AudioSource; // AudioSource component for playing sounds
    public float MoveDuration = 10f; // Duration for the keycard to move
    public float InitialDelay = 5f; // Delay before keycard starts moving

    private string m_CurrentEnteredCode = ""; // The code that the user is currently entering
    private bool isProcessing = false; // To check if the code processing is in progress

    // Call this method for numeric buttons
    public void ButtonPressed(int valuePressed)
    {
        // Play the button press sound
        PlaySound(ButtonPressSound);

        // Check if processing is not in progress
        if (!isProcessing)
        {
            // Check if the length of the entered code is less than the correct sequence length
            if (m_CurrentEnteredCode.Length < Sequence.Length)
            {
                // Append the pressed number to the current entered code
                m_CurrentEnteredCode += valuePressed;

                // Update the display text with the current code
                UpdateDisplay();
            }
        }
    }

    // Call this method for special buttons
    public void SpecialButtonPressed(string buttonType)
    {
        if (!isProcessing) // Only proceed if not already processing
        {
            if (buttonType == "OK")
            {
                CheckCode(); // Call CheckCode() if the button type is "OK"
            }
            else if (buttonType == "X")
            {
                ResetCode(); // Call ResetCode() if the button type is "X"
            }
        }
    }

    public void CheckCode()
    {
        if (isProcessing) return; // Exit if already processing

        isProcessing = true; // Set processing flag to true

        // Check if the entered code matches the correct sequence
        if (m_CurrentEnteredCode == Sequence)
        {
            Debug.Log("Code correct!");
            InputDisplayText.text = "Code Correct";
            PlaySound(CorrectCodeSound); // Play the correct code sound
            StartCoroutine(SpawnKeycardWithDelay()); // Start coroutine to spawn keycard with delay
        }
        else
        {
            Debug.Log("Code incorrect!");
            InputDisplayText.text = "Code Incorrect";
            PlaySound(IncorrectCodeSound); // Play the incorrect code sound
            StartCoroutine(ResetCodeAfterDelay(3f)); // Start coroutine to reset code after a delay
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (AudioSource != null && clip != null)
        {
            AudioSource.PlayOneShot(clip); // Play the audio clip
        }
    }

    private IEnumerator ResetCodeAfterDelay(float delay)
    {
        // Wait for the specified amount of time
        yield return new WaitForSeconds(delay);

        // Reset the code after the delay
        ResetCode();
        isProcessing = false; // Reset processing flag after resetting code
    }

    private IEnumerator SpawnKeycardWithDelay()
    {
        // Play the keycard sound immediately
        PlaySound(KeycardSound);

        // Wait for the initial delay before starting the movement
        yield return new WaitForSeconds(InitialDelay);

        // Spawn the keycard at the specified location
        GameObject keycard = Instantiate(KeycardPrefab, KeycardSpawnLocation.position, KeycardSpawnLocation.rotation);

        // Start the movement coroutine
        StartCoroutine(MoveKeycard(keycard, new Vector3(KeycardSpawnLocation.position.x, KeycardSpawnLocation.position.y, 0.35f), MoveDuration));

        // Reset the code after spawning the keycard
        ResetCode();
        isProcessing = false; // Reset processing flag after spawning keycard
    }

    private IEnumerator MoveKeycard(GameObject keycard, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = keycard.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Move the keycard towards the target position
            keycard.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final position is set
        keycard.transform.position = targetPosition;
    }

    public void ResetCode()
    {
        // Clear the entered code and reset the display
        m_CurrentEnteredCode = "";
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        // Update the TextMeshPro display with the current entered code
        InputDisplayText.text = m_CurrentEnteredCode;
    }
}
