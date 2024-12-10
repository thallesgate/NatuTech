using UnityEngine;

public class Flashbang : MonoBehaviour
{
    private bool isAnimatorExited = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnFlashbangAnimatorExit()
    {
        if (!isAnimatorExited)
        {
            Destroy(gameObject);
            isAnimatorExited = true;
        }
    }
}
