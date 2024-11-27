using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class HomeScreenController : MonoBehaviour
{
    [SerializeField] private InputAction tap;
    [SerializeField] private string sceneToLoad = "PlacementScene";
    [SerializeField] private float delayTime = 2.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
            tap.Disable();
            SceneManager.LoadScene(sceneToLoad);
            Debug.Log("Click!");
        }
    }
}
