using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
//using UnityEngine.UIElements;

public class InstructionsARSceneController : MonoBehaviour
{
    [Header("UI Elements")]
    public Animator instructionsAnimator;
    public TextMeshProUGUI instructionsText;
    public Image instructionsImage;
    public Button skipButton;
    public Button nextButton;

    [Header("Instruction Data")]
    public Sprite[] instructionImages;
    public string[] instructionTexts;

    [Header("Timing")]
    public float delayBeforeInput = 1f;

    [Header("Audio")]
    private AudioController audioController;
    [SerializeField] private string tapSound = "ScreenTap";
    [SerializeField] private string finalSound = "Start";

    [Header("Scene Prefab")]
    [SerializeField] private GameObject nextScenePrefab;

    private int currentInstructionIndex = 0;
    private bool inputEnabled = false;
    private bool isSkipped = false;

    private void Start()
    {
        audioController = FindFirstObjectByType<AudioController>();
        if (instructionImages.Length == 0 || instructionTexts.Length == 0 || instructionsAnimator == null)
        {
            Debug.LogError("Missing required components or data!");
            return;
        }

        // Initialize the first instruction
        SetInstruction(0);
        PlayAnimation("InstructionsEaseIn");

        // Assign skip button action
        skipButton.onClick.AddListener(SkipInstructions);
        nextButton.onClick.AddListener(NextInstruction);
    }
    private void NextInstruction()
    {
        if (!isSkipped)
        {
            audioController.PlaySound(tapSound);
            PlayAnimation("InstructionsEaseOut");

            // Wait for the animation to finish easing out before proceeding
            StartCoroutine(WaitAndAdvanceInstruction());
            Debug.Log("Tap!!");
        }
    }

    private void SetInstruction(int index)
    {
        if (index >= 0 && index < instructionImages.Length && index < instructionTexts.Length)
        {
            instructionsImage.sprite = instructionImages[index];
            instructionsText.text = instructionTexts[index];
        }
        else
        {
            Debug.LogError("Instruction index out of bounds!");
        }
    }

    private void PlayAnimation(string triggerName)
    {
        if (instructionsAnimator != null)
        {
            instructionsAnimator.SetTrigger(triggerName);
        }
    }

    private System.Collections.IEnumerator WaitAndAdvanceInstruction()
    {
        yield return new WaitForSeconds( 0.5f); // Adjust if ease-out animation duration changes

        currentInstructionIndex++;
        Debug.Log("Instructions Controller Index: " + currentInstructionIndex);
        if (currentInstructionIndex < instructionImages.Length)
        {
            // Show next instruction
            SetInstruction(currentInstructionIndex);
            PlayAnimation("InstructionsEaseIn");
            Debug.Log("Next Instruction.");
        }
        else
        {
            // Trigger despawn animation
            Debug.Log("Despawn.");
            audioController.PlaySound(finalSound);
            PlayAnimation("InstructionsDespawn");

            //Fallback
            Invoke(nameof(OnInstructionsARAnimatorExit), 1f);
        }
    }
    // Skip button functionality
    public void SkipInstructions()
    {
        //DisableInput(); // Disable input to prevent conflicts
        Debug.Log("Skip!");
        isSkipped = true;
        audioController.PlaySound(finalSound);
        PlayAnimation("InstructionsDespawn");
        Debug.Log("Skipped!");
    }
    // Called at the end of the despawn animation via Animation Event
    public void OnInstructionsARAnimatorExit()
    {
        Debug.Log("Instructions UI has despawned. Transitioning to game...");
        // Additional logic for transitioning to the game can be added here
        Instantiate(nextScenePrefab);
        Destroy(gameObject);
    }
}
