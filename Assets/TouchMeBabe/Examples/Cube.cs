using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour, ITappable, ISwipeable
{

    private bool popping = false;

    private Vector3 velocity = Vector3.zero;

    private AudioSource audioSource;

    private void Start() {
        audioSource = GetComponent<AudioSource>();
    }
    public void OnTap() {
        audioSource.Play();
    }

    public void OnSwipe(SwipeInfo swipe) {
        velocity += swipe.GetWorldVelocity(Camera.main, true);
    }

    public void Update() {
        if(velocity.magnitude > 0) {
            Vector3 translate = velocity * Time.deltaTime;
            transform.Translate(translate);
            velocity -= translate;
        }
    }

    IEnumerator Pop(float totalDuration, float finalScale) {
        popping = true;
        float duration = totalDuration/3;
        Vector3 startScale = this.transform.localScale;
        float startTime = Time.time;
        float endTime = startTime + duration;
        float currentScale = 1;
        while(Time.time < endTime || currentScale < finalScale) {
            currentScale += Time.deltaTime * finalScale/duration;
            this.transform.localScale = startScale * currentScale;
            yield return null;
        }
        currentScale = finalScale;
        this.transform.localScale = startScale * finalScale;
        yield return new WaitForSeconds(duration);

        float descendTime = Time.time;
        float endDescendTime = descendTime + duration;

        while(Time.time < endDescendTime || currentScale > 1) {
            currentScale -= Time.deltaTime * 1/duration;
            this.transform.localScale = startScale * currentScale;
            yield return null;
        }
        this.transform.localScale = startScale;
        popping = false;
    }
}
