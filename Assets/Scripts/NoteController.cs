using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteController : MonoBehaviour
{
    float speed = 0f;
    public float MinY = -180;

    void Update()
    {
        if (transform.position.y < this.MinY)
        {
            Destroy(this.gameObject);
        }
        else
        {
            transform.Translate(
                0,
                -1 * this.speed * UnityEngine.Time.deltaTime,
                0);
        }
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }
}
