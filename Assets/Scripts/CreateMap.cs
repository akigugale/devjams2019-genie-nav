﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.XR.iOS;
using System.Runtime.InteropServices;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CustomShapeManager))]
public class CreateMap : MonoBehaviour, PlacenoteListener {

    public Text debugText;

    private string MAP_NAME = "GenericMap";

    private CustomShapeManager shapeManager;

    private bool shouldRecordWaypoints = false;
    private bool shouldSaveMap = true;
    private bool mARInit = false;

    private UnityARSessionNativeInterface mSession;

    private LibPlacenote.MapMetadataSettable mCurrMapDetails;

    private BoxCollider mBoxColliderDummy;
    private SphereCollider mSphereColliderDummy;
    private CapsuleCollider mCapColliderDummy;

    public Text mapname;
    private String fileName = "maps.txt";

    // Use this for initialization
    void Start() {

        shapeManager = GetComponent<CustomShapeManager>();

        Input.location.Start();

        mSession = UnityARSessionNativeInterface.GetARSessionNativeInterface();
        StartARKit();
        FeaturesVisualizer.EnablePointcloud();
        LibPlacenote.Instance.RegisterListener(this);
    }

    void OnDisable() {
    }

    // Update is called once per frame
    void Update() {
        if (!mARInit && LibPlacenote.Instance.Initialized())
        {
            Debug.Log("Ready To Start!");
            mARInit = true;

            return;
        }

        if (shouldRecordWaypoints) {
            Transform player = Camera.main.transform;
            //create waypoints if there are none around
            Collider[] hitColliders = Physics.OverlapSphere(player.position, 1f);
            int i = 0;
            while (i < hitColliders.Length) {
                if (hitColliders[i].CompareTag("waypoint")) {
                    return;
                }
                i++;
            }
            Vector3 pos = player.position;
            Debug.Log(player.position);
            pos.y = -.5f;
            shapeManager.AddShape(pos, Quaternion.Euler(Vector3.zero), false);
        }
    }

    public void CreateDestination() {
        shapeManager.AddDestinationShape();
    }

    private void StartARKit() {
        Debug.Log("Initializing ARKit");
        Application.targetFrameRate = 60;
        ConfigureSession();
    }


    private void ConfigureSession() {
#if !UNITY_EDITOR
		ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration ();

		if (UnityARSessionNativeInterface.IsARKit_1_5_Supported ()) {
			config.planeDetection = UnityARPlaneDetection.HorizontalAndVertical;
		} else {
			config.planeDetection = UnityARPlaneDetection.Horizontal;
		}

		config.alignment = UnityARAlignment.UnityARAlignmentGravity;
		config.getPointCloudData = true;
		config.enableLightEstimation = true;
		mSession.RunWithConfig (config);
#endif
    }

    public void OnStartNewClick()
    {
        MAP_NAME = mapname.text;
        Debug.Log("MAP - NAME: " + MAP_NAME);


        if (!File.Exists(fileName))
        {
            var sr = File.CreateText(fileName);
            Debug.Log(fileName+" already exists.");
            sr.WriteLine (MAP_NAME);
            sr.Close();
        }
        else {
            StreamWriter sr = new StreamWriter(fileName, true);
            sr.WriteLine (MAP_NAME);
            sr.Close();
        }
        // sr.WriteLine ("I can write ints {0} or floats {1}, and so on.", 1, 4.2);
        ConfigureSession();

        if (!LibPlacenote.Instance.Initialized())
        {
            Debug.Log("SDK not yet initialized");
            return;
        }

        Debug.Log("Started Session");
        LibPlacenote.Instance.StartSession();

        //start drop waypoints
        Debug.Log("Dropping Waypoints!!");
        shouldRecordWaypoints = true;
    }

    public void OnSaveMapClick() {
        OverwriteExistingMap();
    }

    void OverwriteExistingMap() {
        if (!LibPlacenote.Instance.Initialized()) {
            Debug.Log("SDK not yet initialized");
            return;
        }

        // Overwrite map if it exists.
        LibPlacenote.Instance.SearchMaps(MAP_NAME, (LibPlacenote.MapInfo[] obj) => {
            bool foundMap = false;
            foreach (LibPlacenote.MapInfo map in obj) {
                if (map.metadata.name == MAP_NAME) {
                    foundMap = true;
                    LibPlacenote.Instance.DeleteMap(map.placeId, (deleted, errMsg) => {
                        if (deleted) {
                            Debug.Log("Deleted ID: " + map.placeId);
                            SaveCurrentMap();
                        } else {
                            Debug.Log("Failed to delete ID: " + map.placeId);
                        }
                    });
                }
            }

            if (!foundMap) {
                SaveCurrentMap();
            }
        });
    }

    void SaveCurrentMap() {
        if (shouldSaveMap) {
            shouldSaveMap = false;

            if (!LibPlacenote.Instance.Initialized()) {
                Debug.Log("SDK not yet initialized");
                return;
            }

            bool useLocation = Input.location.status == LocationServiceStatus.Running;
            LocationInfo locationInfo = Input.location.lastData;

            Debug.Log("Saving...");

            debugText.text = "uploading...";
            LibPlacenote.Instance.SaveMap(
                (mapId) => {
                    LibPlacenote.Instance.StopSession();

                    LibPlacenote.MapMetadataSettable metadata = new LibPlacenote.MapMetadataSettable();
                    metadata.name = MAP_NAME;
                    Debug.Log("Saved Map Name: " + metadata.name);

                    JObject userdata = new JObject();
                    metadata.userdata = userdata;

                    JObject shapeList = GetComponent<CustomShapeManager>().Shapes2JSON();

                    userdata["shapeList"] = shapeList;

                    if (useLocation) {
                        metadata.location = new LibPlacenote.MapLocation();
                        metadata.location.latitude = locationInfo.latitude;
                        metadata.location.longitude = locationInfo.longitude;
                        metadata.location.altitude = locationInfo.altitude;
                    }
                    LibPlacenote.Instance.SetMetadata(mapId, metadata);
                    mCurrMapDetails = metadata;
                },
                (completed, faulted, percentage) => {
                    if (completed) {
                        Debug.Log("Upload Complete:" + mCurrMapDetails.name);
                        // debugText.text = "upload complete!!";
                        StartCoroutine(ShowMessage("Upload Complete for " + mCurrMapDetails.name, 2));
                        SceneManager.LoadScene("Home");
                    } else if (faulted) {
                        Debug.Log("Upload of Map Named: " + mCurrMapDetails.name + "faulted");
                    } else {
                        Debug.Log("Uploading Map Named: " + mCurrMapDetails.name + "(" + percentage.ToString("F2") + "/1.0)");
                    }
                }
            );
        }
    }

    public void OnPose(Matrix4x4 outputPose, Matrix4x4 arkitPose) { }

    public void OnStatusChange(LibPlacenote.MappingStatus prevStatus, LibPlacenote.MappingStatus currStatus) {
        Debug.Log("prevStatus: " + prevStatus.ToString() + " currStatus: " + currStatus.ToString());
        if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.LOST) {
            Debug.Log("Localized");
            //			GetComponent<ShapeManager> ().LoadShapesJSON (mSelectedMapInfo.metadata.userdata);
        } else if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.WAITING) {
            Debug.Log("Mapping");
        } else if (currStatus == LibPlacenote.MappingStatus.LOST) {
            Debug.Log("Searching for position lock");
        } else if (currStatus == LibPlacenote.MappingStatus.WAITING) {
            if (GetComponent<CustomShapeManager>().shapeObjList.Count != 0) {
                GetComponent<CustomShapeManager>().ClearShapes();
            }
        }
    }

    IEnumerator ShowMessage (string message, float delay) {
        debugText.text = message;
        debugText.enabled = true;
        yield return new WaitForSeconds(delay);
        debugText.enabled = false;
    }
}
