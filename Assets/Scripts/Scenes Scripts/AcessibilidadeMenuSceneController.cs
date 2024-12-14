using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AcessibilidadeMenuSceneController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animatorTrigger = "FadeOutTrigger";

    [Header("Audio")]
    private AudioController audioController;
    [SerializeField] private string tapSound = "ScreenTap";

    [Header("Scene Prefab")]
    [SerializeField] private GameObject mainMenuScene;
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
    public void OnClickBack()
    {
        audioController.PlaySound(tapSound);
        nextScene = mainMenuScene;
        if (animator != null)
        {
            animator.SetTrigger(animatorTrigger);
        }
        Debug.Log("AcessibilidadeController: Settings Click!");
    }

    public void OnAnimatorEaseOut()
    {
        if (nextScene != null)
        {
            GameObject sceneInstance = Instantiate(nextScene, GlobalPlacementData.position, GlobalPlacementData.rotation);
            sceneInstance.transform.localScale *= GlobalPlacementData.scale.x;
            Destroy(gameObject);
        }
    }
}
