using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    double timeInstantiated;
    public float assignedTime;
    public bool clicked;
    void Start()
    {
        timeInstantiated = SongManager.GetAudioSourceTime();
    }

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (clicked)
            {
                Debug.Log("Vurdu");
                ScoreManager.Hit();
                Destroy(this.gameObject);
               // inputIndex++;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        clicked = true;
    }

    // Update is called once per frame
    void Update()
    {
        double timeSinceInstantiated = SongManager.GetAudioSourceTime() - timeInstantiated;
        float t = (float)(timeSinceInstantiated / (SongManager.Instance.noteTime * 2));

        
        if (t > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(Vector3.up * SongManager.Instance.noteSpawnY, Vector3.up * SongManager.Instance.noteDespawnY, t); 
            GetComponent<SpriteRenderer>().enabled = true;
        }
    }
}
