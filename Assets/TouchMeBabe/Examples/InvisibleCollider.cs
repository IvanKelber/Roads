using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleCollider : MonoBehaviour, ITappable, ISwipeable
{
    private AudioSource audioSource;
    private void Start() {
        audioSource = GetComponent<AudioSource>();
    }

    public void OnTap() {
        Debug.Log("Tapping invisible collider");
    }

    public void OnSwipe(SwipeInfo swipe) {
        audioSource.Play();
    }
}
