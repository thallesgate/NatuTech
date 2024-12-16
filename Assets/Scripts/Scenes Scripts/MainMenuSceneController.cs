using UnityEngine;
using UnityEngine.UI;

public class MainMenuSceneController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animatorTrigger = "FadeOutTrigger";

    [Header("Audio")]
    private AudioController audioController;
    [SerializeField] private string tapSound = "ScreenTap";

    [Header("Scene Prefab")]
    [SerializeField] private GameObject playScenePrefab;
    [SerializeField] private GameObject settingsScenePrefab;
    private GameObject nextScene;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioController = FindFirstObjectByType<AudioController>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnClickPlay()
    {
        audioController.PlaySound(tapSound);
        nextScene = playScenePrefab;
        if (animator != null)
        {
            animator.SetTrigger(animatorTrigger);
        }
        Debug.Log("MainMenuController: Play Click!");
    }

    public void OnClickSettings()
    {
        audioController.PlaySound(tapSound);
        nextScene = settingsScenePrefab;
        if (animator != null)
        {
            animator.SetTrigger(animatorTrigger);
        }
        Debug.Log("MainMenuController: Settings Click!");
    }

    public void OnClickExit()
    {
        audioController.PlaySound(tapSound);
        Debug.Log("MainMenuController: Quit Click!");
        #if UNITY_EDITOR
            Debug.Log("Game would quit if this were a built application.");
        #else
            // Quit the application
            Application.Quit();
        #endif
    }

    public void OnAnimatorEaseOut()
    {
        if (nextScene != null)
        {
            GameObject sceneInstance = Instantiate(nextScene, GlobalPlacementData.position, GlobalPlacementData.rotation);
            sceneInstance.transform.localScale *= GlobalPlacementData.scale.x;
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("Next scene is null. Ensure a selection has been made.");
        }
    }
}
