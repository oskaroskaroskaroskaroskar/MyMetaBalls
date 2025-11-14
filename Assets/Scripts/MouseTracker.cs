using UnityEngine;

public class MouseTracker : MonoBehaviour
{
    public static Vector3 worldPos;
    public float fixedZ = 10f; // distance from the camera

    void Update()
    {
        Plane plane = new Plane(Vector3.forward, fixedZ); // plane at world Z = 0
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float distance))
        {
            worldPos = ray.GetPoint(distance);
            //Debug.Log("Mouse world position on plane: " + worldPos);
        }
    }

}