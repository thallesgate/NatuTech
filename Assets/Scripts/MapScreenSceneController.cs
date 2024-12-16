using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class MapScreenSceneController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Animator animator;
    [SerializeField] private string easeOutTrigger = "SelectMap";
    [SerializeField] private string clickOutTrigger = "SelectOut";

    [Header("Audio")]
    private AudioController audioController;
    [SerializeField] private string tapSound = "ScreenTap";

    [Header("Scene Prefabs")]
    [SerializeField] private GameObject mainMenuPrefab;
    [SerializeField] private float scaleFactor = 2.0f;

    [System.Serializable]
    public class LevelPrefab
    {
        public GameObject prefab;
        public int levelNumber;
        public string selectAnimationTrigger;
        public string temaMusical;
    }

    [SerializeField] private List<LevelPrefab> levels = new List<LevelPrefab>();

    private GameObject nextScene;
    private string nextMusic;
    private bool isClickOutActive = false;
    private int lastSelectedMap = -1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        audioController = FindFirstObjectByType<AudioController>();
    }

    // Triggered when a level button is selected
    public void OnSelectMap(int selection)
    {
        if (selection < 0 || selection >= levels.Count)
        {
            Debug.LogError("MapScreenController: Invalid selection index: " + selection);
            return;
        }

        // Reset ClickOut state
        isClickOutActive = false;

        // Prevent retriggering the same map's animation
        if (selection == lastSelectedMap)
        {
            Debug.Log("MapScreenController: Same map selected again, ignoring trigger.");
            return;
        }

        lastSelectedMap = selection;

        audioController.PlaySound(tapSound);
        LevelPrefab selectedLevel = levels[selection];
        nextScene = selectedLevel.prefab;
        nextMusic = selectedLevel.temaMusical;

        if (animator != null && !string.IsNullOrEmpty(selectedLevel.selectAnimationTrigger))
        {
            animator.ResetTrigger(clickOutTrigger); // Ensure ClickOut is reset
            animator.SetTrigger(selectedLevel.selectAnimationTrigger);
        }

        Debug.Log("MapScreenController: Selected level " + selectedLevel.levelNumber);
    }

    // Triggered when the map is confirmed
    public void OnMapConfirm()
    {
        audioController.PlaySound(tapSound);
        audioController.PlaySound(nextMusic);
        if (animator != null)
        {
            animator.SetTrigger(easeOutTrigger);
        }

        Debug.Log("MapScreenController: Confirmed selection, loading level.");
    }

    // Triggered when the back button is clicked
    public void OnClickBack()
    {
        audioController.PlaySound(tapSound);

        nextScene = mainMenuPrefab;

        if (animator != null)
        {
            animator.SetTrigger(easeOutTrigger);
        }

        Debug.Log("MapScreenController: Back to main menu.");
    }

    public void OnClickOut()
    {
        if (isClickOutActive)
        {
            Debug.Log("MapScreenController: ClickOut trigger already active.");
            return;
        }

        isClickOutActive = true;
        lastSelectedMap = -1; // Reset last selected map on ClickOut
        audioController.PlaySound(tapSound);

        if (animator != null)
        {
            animator.ResetTrigger(easeOutTrigger); // Reset other triggers to prevent conflicts
            animator.SetTrigger(clickOutTrigger);
        }

        Debug.Log("MapScreenController: Click out!");
    }

    // Called by the ease-out animation event
    public void OnAnimatorEaseOut()
    {
        isClickOutActive = false;

        if (nextScene != null)
        {
            GameObject sceneInstance = Instantiate(nextScene, GlobalPlacementData.position, GlobalPlacementData.rotation);
            GlobalPlacementData.scale.x *= 2f;
            GlobalPlacementData.scale.y *= 2f;
            GlobalPlacementData.scale.z *= 2f;
            sceneInstance.transform.localScale *= GlobalPlacementData.scale.x;
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("Next scene is null. Ensure a selection has been made.");
        }
    }
}
