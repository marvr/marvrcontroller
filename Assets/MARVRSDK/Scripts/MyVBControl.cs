using UnityEngine;
using System.Collections;
using Vuforia;

/// <summary>
/// 虚拟枪支控制脚本
/// </summary>
public class MyVBControl : MonoBehaviour, IVirtualButtonEventHandler
{
    private Vector3 target_position;
    private Vector3 target_position_origin;
    private Vector3 interval_position;

    // 记录Target欧拉角变换
    // 初始位置欧拉角
    private Vector3 euler_origin;
    // 间隔欧拉角
    private Vector3 euler_interval;

    // 用于记录位移转换的角度
    private float x_angle;
    private float y_angle;
    private float z_angle;

    // 用于记录欧拉角变换
    private float pitch;//偏航角
    private float roll;//翻转角
    private float yaw;//俯仰角

    // 枪模型1,平移
    //	public GameObject gun;
    // 枪模型2，单独旋转
    public GameObject gun2;

    // 界面
    private bool isClocked = false;

    // 记录枪支的初始位置
    private Vector3 gun_position_origin;
    private Vector3 gun2_position_origin;
    // 记录手枪的初始角度
    private Quaternion gun2_orientation_origin;

    private DefaultTrackableEventHandler myTrackable;
    private Quaternion gun_rotation;
    private Quaternion gun2_rotation;
    // 枪的位置
    private Vector3 gun_position;

    // 差补
    private float offset_x = 0;
    private float offset_y = 0;
    private float offset_z = 0;

    // 设置阈值
    private float threshold = 3;
    private float threshold2 = 1;

    private float FILTER_FACTOR = 0.3F;

    float x;
    float y;
    float z;

    // 记录陀螺仪初始位置
    private float y_origin;

    // 枪的初始位置
    private Quaternion gun_origin;

    private bool isFire = false;

    // Use this for initialization
    void Start()
    {
        //		VFGyro.RegisterCallback (OnGyroChanged);

        VirtualButtonBehaviour[] vbs = GetComponentsInChildren<VirtualButtonBehaviour>();
        for (int i = 0; i < vbs.Length; i++)
        {
            vbs[i].RegisterEventHandler(this);
        }
        // 记录初始位置
        target_position_origin = transform.position;
        euler_origin = transform.eulerAngles;

        pre_interval = target_position_origin;
        pre_interval_euler = euler_origin;

        // 记录枪支初始位置
        //		gun_position_origin = gun.transform.position;
        //				gun2_position_origin = gun2.transform.position;
        gun2_position_origin = gun2.transform.localPosition;
        // 获取初始欧拉角
        gun2_orientation_origin = gun2.transform.localRotation;

        myTrackable = GetComponent<DefaultTrackableEventHandler>();

        // 初始位置
        //y_origin = VFGyro.Find().transform.rotation.y;
        //		gun_origin = gun.transform.localRotation;
    }

    private Vector3 pre_interval;
    private Vector3 pre_interval_euler;

    private float getAngle(float origin)
    {
        if (origin > 180)
        {
            return (origin - 360);
        }
        else {
            return origin;
        }
    }
    // Update is called once per frame
    void Update()
    {
        // 保存欧拉角
        euler_interval = transform.eulerAngles - euler_origin;

        if (euler_interval.x > 180)
        {
            euler_interval.x = euler_interval.x - 360;
        }

        if (euler_interval.y > 180)
        {
            euler_interval.y = euler_interval.y - 360;
        }

        if (euler_interval.z > 180)
        {
            euler_interval.z = euler_interval.z - 360;
        }

        // 低通滤波
        x = (float)(euler_interval.x * FILTER_FACTOR + x * (1 - FILTER_FACTOR));
        y = (float)(euler_interval.y * FILTER_FACTOR + y * (1 - FILTER_FACTOR));
        z = (float)(euler_interval.z * FILTER_FACTOR + z * (1 - FILTER_FACTOR));

        gun2_rotation = Quaternion.Euler(y, z + 90, x - 15);
        //gun2_rotation = Quaternion.Euler (y, 90, x-15);

        //		Debug.Log ("--->:::" + euler_interval);
        //		Debug.Log ("-->"+x+":"+y+":"+z);

        //将位移转换为欧拉角
        target_position = transform.position;

        // 记录位置实时变换
        interval_position = target_position - target_position_origin;

        // 滤波
        if (Mathf.Abs(interval_position.x - pre_interval.x) < threshold)
        {
            interval_position.x = pre_interval.x;
        }
        if (Mathf.Abs(interval_position.y - pre_interval.y) < threshold)
        {
            interval_position.y = pre_interval.y;
        }
        if (Mathf.Abs(interval_position.z - pre_interval.z) < threshold)
        {
            interval_position.z = pre_interval.z;
        }

        x_angle = Mathf.Atan(interval_position.x / 100) * Mathf.Rad2Deg;
        y_angle = Mathf.Atan(interval_position.y / 100) * Mathf.Rad2Deg;
        z_angle = Mathf.Atan(interval_position.z / 400) * Mathf.Rad2Deg;// 上下

        // 设置差补的值,偏移量
        if (DefaultTrackableEventHandler.isFindTarget)
        {
            offset_x = -45;
            offset_y = 30;
            offset_z = 0;
        }

        float offset_position = Mathf.Lerp(0, (300 - interval_position.y) / 200, Time.time);
        Debug.Log("----->" + DefaultTrackableEventHandler.isFindTarget);
        if (DefaultTrackableEventHandler.isFindTarget)
        {
            // gun2.transform.localPosition = new Vector3 (gun2_position_origin.x - Mathf.Lerp (0, (300 - interval_position.x) / 400, Time.time), gun2.transform.localPosition.y, gun2_position_origin.z + Mathf.Lerp (0, (300 - interval_position.y) / 300, Time.time)*0.5f);// 前后、左右
            if (myTrackable.mTrackableBehaviour.TrackableName.Equals("antvr-target"))
            {

                gun2.transform.localPosition = new Vector3(gun2_position_origin.x - Mathf.Lerp(0, (300 - interval_position.x) / 900, Time.time) + 0.3f, gun2_position_origin.y + 0.15f - Mathf.Lerp(0, (300 - interval_position.z) / 800, Time.time), gun2_position_origin.z + Mathf.Lerp(0, (300 - interval_position.y) / 1200, Time.time));// 前后、左右
                gun2_rotation = Quaternion.Euler(y, z + 90, x - 15);
                gun2.transform.localRotation = gun2_rotation;
            }
            else {
                gun2.transform.localPosition = new Vector3(gun2_position_origin.x - Mathf.Lerp(0, (300 - interval_position.x) / 900, Time.time), gun2_position_origin.y + 0.15f - Mathf.Lerp(0, (300 - interval_position.z) / 800, Time.time), gun2_position_origin.z + Mathf.Lerp(0, (300 - interval_position.y) / 1200, Time.time));// 前后、左右
                gun2.transform.localRotation = gun2_rotation;
            }
            //gun2.transform.localPosition = new Vector3(gun2_position_origin.x - Mathf.Lerp(0, (300 - interval_position.x) / 900, Time.time), gun2_position_origin.y + 0.15f - Mathf.Lerp(0, (300 - interval_position.z) / 800, Time.time), gun2_position_origin.z + Mathf.Lerp(0, (300 - interval_position.y) / 1200, Time.time));// 前后、左右
            //gun2.transform.localRotation = gun2_rotation;

        }
        else {
            // 如果标志丢失，则将枪的位置和角度设置为初始值
            gun2.transform.localPosition = gun2_position_origin;
            gun2.transform.localRotation = gun2_orientation_origin;
        }

        // 旋转 问题：怎么才能使枪既能随Camera也能绕自身坐标系旋转
        //gun_rotation = Quaternion.Euler ((-Mathf.Lerp (0, z_angle+42, Time.time) - offset_x)*0.4f, 0, offset_z);

        pre_interval = interval_position;
        pre_interval_euler = euler_interval;
    }

    // 当开枪的时候，锁定Target，保持位置不变
    public void OnButtonPressed(VirtualButtonAbstractBehaviour button)
    {
        switch (button.VirtualButtonName)
        {
            case "shoot":
                Debug.Log("Shoot!!!");
                break;
            default:
                break;
        }
    }

    private void fire()
    {
        Debug.Log("Shoot!!!");
    }

    // 释放按钮解锁
    public void OnButtonReleased(VirtualButtonAbstractBehaviour button)
    {
        isClocked = false;
    }
}