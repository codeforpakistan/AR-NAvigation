using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.XR.iOS;
using System.Runtime.InteropServices;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class searchreadmap : MonoBehaviour {
    private string mSelectedMapId
    {
        get
        {
            return mSelectedMapInfo != null ? mSelectedMapInfo.placeId : null;
        }
    }
    [SerializeField] GameObject mMapSelectedPanel;
    [SerializeField] float mMaxRadiusSearch;

    [SerializeField] Text mLabelText;
    private bool mReportDebug = false;
    [SerializeField] GameObject mMappingButtonPanel;
    [SerializeField] Text mRadiusLabel;
    [SerializeField] GameObject mExitButton;
    private UnityARSessionNativeInterface mSession;
    private bool mFrameUpdated = false;
    private UnityARCamera mARCamera;
    public Text nnn;
    

    // Use this for initialization
    [SerializeField] RectTransform mListContentParent;
    [SerializeField] GameObject mMapListPanel;
    [SerializeField] GameObject mInitButtonPanel;
    [SerializeField] Slider mRadiusSlider;
    [SerializeField] GameObject mListElement;
    [SerializeField] ToggleGroup mToggleGroup;
    private LibPlacenote.MapInfo mSelectedMapInfo;
    // public  libnotetocanvas liblib;
    [SerializeField] GameObject mPlaneDetectionToggle;
    [SerializeField] PlacenoteARGeneratePlane mPNPlaneManager;


    public void load_map()
    {
        //Input.location.Start();
        nnn.text = "location" + Input.location.lastData.ToString();

        //if (!LibPlacenote.Instance.Initialized())
        //{
        //    Debug.Log("SDK not yet initialized   initilizing now");
        //    liblib.StartARKit();

        // //   return;

        //}

        //liblib.DeleteMaps();
        //foreach (Transform t in mListContentParent.transform)
        //{
        //    Destroy(t.gameObject);
        //}



        //mMapListPanel.SetActive(true);
        //mInitButtonPanel.SetActive(false);
        //mRadiusSlider.gameObject.SetActive(true);
        //LibPlacenote.Instance.ListMaps((mapList) => {
        //    // render the map list!
        //    foreach (LibPlacenote.MapInfo mapId1 in mapList)
        //    {
        //     //   Debug.Log("Map metadata "+ mapId1.metadata.userdata.ToString(Formatting.None));
        //       // if (mapId1.metadata.userdata != null)
        //        //{
        //         //   Debug.Log(mapId1.metadata.userdata.ToString(Formatting.None) + "aaa");
        //        //}
        //        AddMapToList(mapId1);
        //        Debug.Log("mapId1" + mapId1.metadata.userdata);
        //    }
        //});
        Input.location.Stop();
        LibPlacenote.Instance.StopSession();


        Application.LoadLevel(2);
    }
    public void home() {
        Application.LoadLevel(0);
    }
    public void OnDeleteMapClicked()
    {
        nnn.text = "delete btn clicked";

        if (!LibPlacenote.Instance.Initialized())
        {
            Debug.Log("SDK not yet initialized");
            return;
        }

        mLabelText.text = "Deleting Map ID: " + mSelectedMapId;
        LibPlacenote.Instance.DeleteMap(mSelectedMapId, (deleted, errMsg) => {
            if (deleted)
            {
                mMapSelectedPanel.SetActive(false);
                mLabelText.text = "Deleted ID: " + mSelectedMapId;
                OnListMapClick();
            }
            else
            {
                mLabelText.text = "Failed to delete ID: " + mSelectedMapId;
            }
        });
    }
    public void OnListMapClick()
    {
        if (!LibPlacenote.Instance.Initialized())
        {
            Debug.Log("SDK not yet initialized");
            return;
        }

        foreach (Transform t in mListContentParent.transform)
        {
            Destroy(t.gameObject);
        }


        mMapListPanel.SetActive(true);
        mInitButtonPanel.SetActive(false);
        mRadiusSlider.gameObject.SetActive(true);
        LibPlacenote.Instance.ListMaps((mapList) => {
            // render the map list!
            foreach (LibPlacenote.MapInfo mapId1 in mapList)
            {
                if (mapId1.metadata.userdata != null)
                {
                    Debug.Log(mapId1.metadata.userdata.ToString(Formatting.None));
                }
                AddMapToList(mapId1);
            }
        });
    }

    IEnumerator Start1()
    {  //Input.location.Start(100);
        Debug.Log("corouten started" );

      //  First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        { yield break; }

        // Start service before querying location
        Input.location.Start();
        Debug.Log("location started");
        // Wait until service initializes
        Debug.Log(Input.location.status.ToString());
        int maxWait = 10;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
            Debug.Log("max wait" + maxWait);
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
        }
        nnn.text = "location" + Input.location.lastData.latitude.ToString() + Input.location.lastData.longitude.ToString();
        OnRadiusSelect();
        // Stop service if there is no need to query location updates continuously
        // Input.location.Stop();
    }
    public GameObject iiii;
    void Start () {
       // LibPlacenote.Instance.Initialized();
        iiii = GameObject.Find("ARKitWorldTrackingRemoteConnection");
        mRadiusSlider.value = 0.5f;
        
        //ResetSlider();
        StartCoroutine(Start1());     
        mSession = UnityARSessionNativeInterface.GetARSessionNativeInterface();
        UnityARSessionNativeInterface.ARFrameUpdatedEvent += ARFrameUpdated;
        Application.targetFrameRate = 60;
        //ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration();
        //config.planeDetection = UnityARPlaneDetection.Horizontal;
        //config.alignment = UnityARAlignment.UnityARAlignmentGravity;
        //config.getPointCloudData = true;
        //config.enableLightEstimation = true;
        //mSession.RunWithConfig(config);

    }

    private void ARFrameUpdated(UnityARCamera camera)
    {
        // testbug.ss("mframe reached");
        mFrameUpdated = true;
        Debug.Log("mframe from ar ftn" + mFrameUpdated);
        mARCamera = camera;
    }
    void AddMapToList(LibPlacenote.MapInfo mapInfo)
    {
        Debug.Log("adding map to list 1");
        GameObject newElement = Instantiate(mListElement) as GameObject;
        Debug.Log("adding map to list 2" );

        MapInfoElement listElement = newElement.GetComponent<MapInfoElement>();
        Debug.Log("adding map to list 3");

        listElement.Initialize(mapInfo, mToggleGroup, mListContentParent, (value) => {
            Debug.Log("adding map to list 4");

            OnMapSelected(mapInfo);
            Debug.Log("adding map to list 5" + mapInfo);

        });
    }
    void OnMapSelected(LibPlacenote.MapInfo mapInfo)
    {
        mSelectedMapInfo = mapInfo;
        mMapSelectedPanel.SetActive(true);
        mRadiusSlider.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update () {
        
		
	}
    private void ConfigureSession(bool clearPlanes)
    {
#if !UNITY_EDITOR
		ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration ();

		if (mPlaneDetectionToggle.GetComponent<Toggle>().isOn) {
			if (UnityARSessionNativeInterface.IsARKit_1_5_Supported ()) {
				config.planeDetection = UnityARPlaneDetection.HorizontalAndVertical;
			} else {
				config.planeDetection = UnityARPlaneDetection.Horizontal;
			}
			mPNPlaneManager.StartPlaneDetection ();
		} else {
			config.planeDetection = UnityARPlaneDetection.None;
			if (clearPlanes) {
				mPNPlaneManager.ClearPlanes ();
			}
		}

		config.alignment = UnityARAlignment.UnityARAlignmentGravity;
		config.getPointCloudData = true;
		config.enableLightEstimation = true;
		mSession.RunWithConfig (config);
#endif
    }
    public void ResetSlider()
    {
        mRadiusSlider.value = 0.0f;
        mRadiusLabel.text = "Distance Filter: Off";
    }
    public void OnLoadMapClicked()
    {        nnn.text = "load btn clicked";
        

        ConfigureSession(false);
        Debug.Log("loadmap and config session value");

        if (!LibPlacenote.Instance.Initialized())
        {
            Debug.Log("SDK not yet initialized");
            return;
        }

        ResetSlider();
        Debug.Log("resetslider");
        nnn.text = nnn.text+"slider reset";
        mLabelText.text = "Loading Map ID: " + mSelectedMapId;
        LibPlacenote.Instance.LoadMap(mSelectedMapId,
            (completed, faulted, percentage) => {

                nnn.text = nnn.text + " load map  called ";
                if (completed)
                {
                    nnn.text = nnn.text + " load map  completed ";

                    mMapSelectedPanel.SetActive(false);
                    mMapListPanel.SetActive(false);
                    mInitButtonPanel.SetActive(false);
                    mMappingButtonPanel.SetActive(true);
                    mExitButton.SetActive(true);
                     mPlaneDetectionToggle.SetActive(true);
                    LibPlacenote.Instance.StartSession(true);
                    Debug.Log("instance start session called");
                   
                    
                    if (mReportDebug)
                    {
                        Debug.Log("mreportdebug"+mReportDebug);

                        LibPlacenote.Instance.StartRecordDataset(
                            (datasetCompleted, datasetFaulted, datasetPercentage) => {
                                Debug.Log("datasetCompleted, datasetFaulted, datasetPercentage" + datasetCompleted+ datasetFaulted+ datasetPercentage);
                                if (datasetCompleted)
                                {
                                    mLabelText.text = "Dataset Upload Complete";
                                }
                                else if (datasetFaulted)
                                {
                                    mLabelText.text = "Dataset Upload Faulted";
                                }
                                else
                                {
                                    mLabelText.text = "Dataset Upload: " + datasetPercentage.ToString("F2") + "/1.0";
                                }
                            });
                        Debug.Log("Started Debug Report");
                    }
                    Debug.Log("Loaded ID: " + mSelectedMapInfo.placeId + "...Starting session");

                    LibPlacenote.Instance.StartSession(true);
                    Debug.Log("else of mreportdebug");

                    mLabelText.text = "Loaded ID: " + mSelectedMapId;
                    Debug.Log("loaded id is"+ mSelectedMapId);


                }

                else if (faulted)
                {
                    mLabelText.text = "Failed to load ID: " + mSelectedMapId;

                }
                else
                {
                    mLabelText.text = "Map Download: " + percentage.ToString("F2") + "/1.0";
                }
            }
        );
        mMapListPanel.SetActive(false);
    }
    public void OnRadiusSelect()
    {
       // mRadiusSlider.value = 0.5f;

        Debug.Log("Map search:" + mRadiusSlider.value.ToString("F2"));
        LocationInfo locationInfo = Input.location.lastData;


        float radiusSearch = mRadiusSlider.value * mMaxRadiusSearch;
        mRadiusLabel.text = "Distance Filter: " + (radiusSearch / 1000.0).ToString("F2") + " km";

        LibPlacenote.Instance.SearchMaps(locationInfo.latitude, locationInfo.longitude, radiusSearch,
            (mapList) => {
                foreach (Transform t in mListContentParent.transform)
                {
                    Destroy(t.gameObject);
                }
                // render the map list!
                foreach (LibPlacenote.MapInfo mapId1 in mapList)
                {
                    //if (mapId1.metadata.userdata != null)
                    //{
                    //    Debug.Log(mapId1.metadata.userdata.ToString(Formatting.None));
                    //}
                    AddMapToList(mapId1);
                }
            });
    }
    public void OnExitClick()
    {
        mInitButtonPanel.SetActive(true);
        mExitButton.SetActive(false);
        mPlaneDetectionToggle.SetActive (false);
        mMappingButtonPanel.SetActive(false);

        //clear all existing planes
       mPNPlaneManager.ClearPlanes();
        mPlaneDetectionToggle.GetComponent<Toggle>().isOn = false;

        LibPlacenote.Instance.StopSession();
    }
}
