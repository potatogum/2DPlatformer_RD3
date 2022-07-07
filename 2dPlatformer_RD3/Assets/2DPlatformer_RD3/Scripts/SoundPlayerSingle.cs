using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundPlayerSingle: MonoBehaviour
{
    [SerializeField] private AudioClip sound;
    [SerializeField] [Range(0.0f, 1.0f)] private float volume = 1;
    [SerializeField] private bool disableOnTouch;

    private AudioSource audioSource;
    private Collider2D[] colliders;
    private SpriteRenderer spriteRenderer;
    private bool destroyOnFinish = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        colliders = GetComponents<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // There are more than 1 collider on the player, we want the bodyCollider (polygon)
        //if (collision is PolygonCollider2D)
        //{
            PlaySound();

            if (disableOnTouch)
            {
                spriteRenderer.enabled = false;
                destroyOnFinish = true;    
                foreach (Collider2D c in colliders)
                {
                    c.enabled = false;
                }
            }

        //}
    }

    private void FixedUpdate()
    {
        if (destroyOnFinish)
        {
            if (!audioSource.isPlaying)
            {
                Destroy(this.gameObject);
            }
        }
    }

    private void PlaySound()
    {
        if (!sound)
        {
            Debug.Log("AudioClip is missing", this);
            return;
        }
        //AudioSource.PlayClipAtPoint(pickupSound, Camera.main.transform.position);
        audioSource.PlayOneShot(sound, volume);
    }
}
