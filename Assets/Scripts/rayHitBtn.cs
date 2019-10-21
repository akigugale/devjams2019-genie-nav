using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class rayHitBtn : MonoBehaviour
{
    public AudioClip[] aClips;
    public AudioSource myAudioSource;
    string btnName;
    public Text notification;
    // Start is called before the first frame update
    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit)){
                btnName = hit.transform.name;
                switch(btnName)
                {
                    case "Directions":
                        myAudioSource.clip = aClips[0];
                        myAudioSource.Play();
                        StartCoroutine(openGivenUrl("https://maps.google.com/maps?q=Shoppers+Stop", 1f));
                        break;
                    case "Website":
                        myAudioSource.clip = aClips[0];
                        myAudioSource.Play();
                        // wait(4);
                        // Invoke("openUrl", 1);
                        StartCoroutine(openGivenUrl("https://www.shoppersstop.com", 1f));
                        break;
                    case "Info":
                        myAudioSource.clip = aClips[0];
                        myAudioSource.Play();
                        StartCoroutine(ShowMessage("Shoppers Stop AR Advertisment for MetroAR.", 2));
                        break;
                    case "CollectCoupon":
                        myAudioSource.clip = aClips[0];
                        myAudioSource.Play();
                        StartCoroutine(ShowMessage("Collected Coupon for 50% discount !", 2));

                        break;
                    default:
                        break;

                }
            }
        }

    }

    IEnumerator openGivenUrl(string url, float delayTime){
        yield return new WaitForSeconds(delayTime);
        Application.OpenURL(url);
    }

     IEnumerator ShowMessage (string message, float delay) {
        notification.text = message;
        notification.enabled = true;
        yield return new WaitForSeconds(delay);
        notification.enabled = false;
    }

    void openUrl() {
        Application.OpenURL("https://www.shoppersstop.com/");
    }
    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        // Code to execute after the delay
    }
}


