using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
public class ScanTargetIndicator : MonoBehaviour,
                                            ITrackableEventHandler
{
    public GameObject scanCardIndicator;
    private TrackableBehaviour mTrackableBehaviour;

    void Start()
    {
        mTrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (mTrackableBehaviour)
        {
            mTrackableBehaviour.RegisterTrackableEventHandler(this);
        }
    }

    public void OnTrackableStateChanged(
                                    TrackableBehaviour.Status previousStatus,
                                    TrackableBehaviour.Status newStatus)
    {
        if (newStatus == TrackableBehaviour.Status.DETECTED ||
            newStatus == TrackableBehaviour.Status.TRACKED ||
            newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
        {
            Debug.Log("FOUNDDDDD ---- ");
            OnTrackingFound();
        }
        else
        {
            Debug.Log("LOSTTT -----");
            OnTrackingLost();
        }
    }

    void OnTrackingFound()
    {
        scanCardIndicator.SetActive(false);
    }

    void OnTrackingLost()
    {
        scanCardIndicator.SetActive(true);
    }

}
