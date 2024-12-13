using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class HomeScreenSceneController : MonoBehaviour
{
    [SerializeField] private InputAction tap;
    [SerializeField] private GameObject sceneToLoadPrefab;
    [SerializeField] private Animator animator;
    [SerializeField] private string animatorTrigger = "EaseOutTrigger";

    public Vector3 spawnPosition = new Vector3(0, 0, 0);
    public Quaternion spawnRotation = Quaternion.identity;

    [SerializeField] private float delayTime = 2.0f;

    private bool isAnimatorExited = false;

    private AudioController audioController;
    [SerializeField] private string tapSound = "Start";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioController = FindFirstObjectByType<AudioController>();
        tap = InputSystem.actions.FindAction("Spawn Object");
        StartCoroutine(EnableInputAfterDelay());
        //tap.canceled += OnClick;
    }
    void OnDestroy()
    {
        tap.performed -= OnClick;
    }

    private IEnumerator EnableInputAfterDelay()
    {
        yield return new WaitForSeconds(delayTime);
        tap.Enable();
        tap.performed += OnClick;
    }
    private void OnClick(InputAction.CallbackContext context)
    {
        //if (tap.ReadValue<bool>())
        if(context.phase == InputActionPhase.Performed)
        {
            audioController.PlaySound(tapSound);
            tap.Disable();
            if(animator != null)
            {
                animator.SetTrigger(animatorTrigger);
            }
            Debug.Log("Click!");
        }
    }

    public void HomeScreenOnAnimatorExit()
    {
        if (!isAnimatorExited && sceneToLoadPrefab != null)
        {
            Instantiate(sceneToLoadPrefab, spawnPosition, spawnRotation);
            Destroy(gameObject);
            isAnimatorExited = true;
        }
    }
}
