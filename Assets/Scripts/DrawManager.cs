using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class DrawManager : MonoBehaviour
{
    public float drawZoneDistance;
    public float instMetaBallInterval;
    public float ballDensity;
    float instMetaBallClock = 0f;
    int framesWithoutDraw;
    bool isDrawing = false;
    

    public GameObject drawContainer;
    Container container;
    public GameObject metaBallPref;
    List<GameObject> metaBalls = new List<GameObject>();
    List<Vector3> metaBallPositions = new List<Vector3>();

    //For CheckInDrawZone();
    Vector3 lowestPosition;
    Vector3 highestPosition;
    void Start()
    {
        container = drawContainer.GetComponent<Container>();
    }


    void Update()
    {
        if (isDrawing && framesWithoutDraw > 2)
        {
            Debug.Log("stopped drawing");
            InstantiateDrawing(true);
            isDrawing = false;
        }
        framesWithoutDraw++;
    }

    public void Draw(Vector3 position)
    {
        isDrawing = true;
        instMetaBallClock += Time.deltaTime;
        framesWithoutDraw = 0;


        if (instMetaBallClock >= instMetaBallInterval)
        {
            instMetaBallClock = 0;

            if (!CheckInDrawZone(position))
            {
                Debug.Log("Out of drawzone");
                InstantiateDrawing(false);
            }
            if (metaBallPositions.Count == 0)
            {

                lowestPosition = position;
                highestPosition = position;
            }

            AddPosition(position);
        }
    }
    
    void InstantiateDrawing (bool instantiateAsEndedDrawing)
    {
        container.InstantiateMetaBalls(metaBallPositions, lowestPosition);
        if (instantiateAsEndedDrawing)
        {
            metaBallPositions.Clear();
        }
        else
        {
            int keepingBallsCount = 5;
            int removeCount = metaBallPositions.Count - keepingBallsCount;
            for (int i = 0; i < removeCount; i++)
            {
                metaBallPositions.RemoveAt(0);
            }
            lowestPosition = metaBallPositions[0];
            highestPosition = metaBallPositions[0];
            for (int i = 1; i < keepingBallsCount; i++)
            {
                CheckInDrawZone(metaBallPositions[i]);
            }

        }
    }
    
    bool CheckInDrawZone (Vector3 newPosition)
    {
        if (metaBallPositions.Count == 0)
        {
            lowestPosition = newPosition;
            highestPosition = newPosition;
        } else {
            foreach (Vector3 position in metaBallPositions)
            {

                if (newPosition.x < lowestPosition.x)
                {
                    lowestPosition.x = newPosition.x;
                }
                else if (newPosition.x > highestPosition.x)
                {
                    highestPosition.x = newPosition.x;
                }
                if (newPosition.y < lowestPosition.y)
                {
                    lowestPosition.y = newPosition.y;
                }
                else if (newPosition.y > highestPosition.y)
                {
                    highestPosition.y = newPosition.y;
                }
                if (newPosition.z < lowestPosition.z)
                {
                    lowestPosition.z = newPosition.z;
                }
                else if (position.z > highestPosition.z)
                {
                    highestPosition.z = newPosition.z;
                }

                //Check distance across area
                if (highestPosition.x - lowestPosition.x > drawZoneDistance)
                {
                    return false;
                }
                else if (highestPosition.y - lowestPosition.y > drawZoneDistance)
                {
                    return false;
                }
                else if (highestPosition.z - lowestPosition.z > drawZoneDistance)
                {
                    return false;
                }
            }

        }
        return true;
    }
    void AddPosition(Vector3 position)
    {
        if (metaBallPositions.Count > 0)
        {
            if ((position - metaBallPositions[metaBallPositions.Count-1]).magnitude > ballDensity)
            {
                metaBallPositions.Add(position);
            }
        } 
        else 
        { 
        metaBallPositions.Add(position);
        }
    }
}
