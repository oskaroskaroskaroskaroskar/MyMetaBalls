using UnityEngine;

public class MouseDraw : MonoBehaviour
{
    public DrawManager drawManager;
    bool mouseDown = false;
    void Start()
    {
        drawManager = GameObject.FindFirstObjectByType<DrawManager>();
    }
    void Update()
    {
        if(Input.GetMouseButtonDown(0)) 
        {
            mouseDown = true;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            mouseDown = false;
        }

        if (mouseDown)
        {
            drawManager.Draw(MouseTracker.worldPos);
        }
    }
}
