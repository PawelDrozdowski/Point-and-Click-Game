using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameTag : MonoBehaviour
{
    Camera myCam;
    void Start()
    {
        myCam = FindObjectOfType<Camera>();
    }

    void LateUpdate()
    {
        FollowMouse();
    }

    private void FollowMouse()
    {
        //move to position in pixels / resolution * resolution in game units
        transform.position = (Vector2)myCam.ScreenToWorldPoint(Input.mousePosition);
    }
}
