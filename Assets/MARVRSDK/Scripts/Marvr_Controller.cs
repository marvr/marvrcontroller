using UnityEngine;
using System.Collections;
using Vuforia;

public class Marvr_Controller : MonoBehaviour {
    DefaultTrackableEventHandler mDefaultTrackableEventHandler = null;
    GameObject myGun;
    bool cameraOn = false;
    Vector3 origin_Pos_Gun;
    Vector3 origin_Pos_Target;
    bool get_Once = false;
    float scale_factor = 300;// 缩放因子

    // Use this for initialization
    void Start () {
        myGun = GameObject.Find("gun");
        mDefaultTrackableEventHandler = GetComponent<DefaultTrackableEventHandler>();
        setARCameraDepth(cameraOn);

        origin_Pos_Gun = myGun.transform.position;
    }
	
	// Update is called once per frame
	void Update () {
        if (DefaultTrackableEventHandler.isFindTarget)
        {
            if (!get_Once)
            {
                origin_Pos_Target = mDefaultTrackableEventHandler.mTrackableBehaviour.transform.position;
                get_Once = true;
            }
            myGun.transform.rotation = mDefaultTrackableEventHandler.mTrackableBehaviour.transform.rotation;
            //Debug.Log(mDefaultTrackableEventHandler.mTrackableBehaviour.transform.position);
            Debug.Log(mDefaultTrackableEventHandler.mTrackableBehaviour.transform.position - origin_Pos_Target);
            myGun.transform.position = origin_Pos_Gun + (mDefaultTrackableEventHandler.mTrackableBehaviour.transform.position - origin_Pos_Target)/scale_factor;
        }
        else {
            get_Once = false;
        }
	}

    void setARCameraDepth(bool on) {
        if (on)
        {
            GameObject.FindGameObjectWithTag("ARCamera").GetComponent<Camera>().depth = 1;

        }
        else {
            GameObject.FindGameObjectWithTag("ARCamera").GetComponent<Camera>().depth = -1;
        }
    }
    void OnGUI() {
        if (GUI.Button(new Rect(0, 0, 100, 100), "Hide")) {
            cameraOn = !cameraOn;
            setARCameraDepth(cameraOn);
        }
    }
}
