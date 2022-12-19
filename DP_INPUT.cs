using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR;

[System.Serializable]
public enum InputFeature
{
    //
    // ժҪ:
    //     The primary face button being pressed on a device, or sole button if only one
    //     is available.
    primaryButton,
    //
    // ժҪ:
    //     The primary face button being touched on a device.
    primaryTouch,
    //
    // ժҪ:
    //     The secondary face button being pressed on a device.
    secondaryButton,
    //
    // ժҪ:
    //     The secondary face button being touched on a device.
    secondaryTouch,
    //
    // ժҪ:
    //     A binary measure of whether the device is being gripped.
    gripButton,
    //
    // ժҪ:
    //     A binary measure of whether the index finger is activating the trigger.
    triggerButton,
    //
    // ժҪ:
    //     Represents a menu button, used to pause, go back, or otherwise exit gameplay.
    menuButton,
    //
    // ժҪ:
    //     Represents the primary 2D axis being clicked or otherwise depressed.
    primary2DAxisClick,
    //
    // ժҪ:
    //     Represents the primary 2D axis being touched.
    primary2DAxisTouch,
    //
    // ժҪ:
    //     Represents the secondary 2D axis being clicked or otherwise depressed.
    secondary2DAxisClick,
    //
    // ժҪ:
    //     Represents the secondary 2D axis being touched.
    secondary2DAxisTouch,
    //
    // ժҪ:
    //     Use this property to test whether the user is currently wearing and/or interacting
    //     with the XR device. The exact behavior of this property varies with each type
    //     of device: some devices have a sensor specifically to detect user proximity,
    //     however you can reasonably infer that a user is present with the device when
    //     the property is UserPresenceState.Present.
    userPresence
}

public class DP_INPUT : MonoBehaviour
{
    int COUNT = 0;
   
    StringBuilder seconddataPosition = new StringBuilder();
    StringBuilder seconddataRay = new StringBuilder();
    public static bool startGame = false;
    public static int Triggertimes = 0;
    public GameObject cam = null;
    public GameObject rayTarget = null;
    public GameObject musicobj = null;
    public GameObject musicobj1 = null;
    public GameObject collisionPointCube = null;
    public Text ShowTXT = null;
    private Dictionary<InputFeature, InputFeatureUsage<bool>> inputFeatureUsageMap = new Dictionary<InputFeature, InputFeatureUsage<bool>>() {
        { InputFeature.primaryButton, CommonUsages.primaryButton },
        { InputFeature.primaryTouch, CommonUsages.primaryTouch },
        { InputFeature.secondaryButton, CommonUsages.secondaryButton },
        { InputFeature.secondaryTouch, CommonUsages.secondaryTouch },
        { InputFeature.gripButton, CommonUsages.gripButton },
        { InputFeature.triggerButton, CommonUsages.triggerButton },
        { InputFeature.menuButton, CommonUsages.menuButton },
        { InputFeature.primary2DAxisClick, CommonUsages.primary2DAxisClick },
        { InputFeature.primary2DAxisTouch, CommonUsages.primary2DAxisTouch },
        { InputFeature.secondary2DAxisClick, CommonUsages.secondary2DAxisClick },
        { InputFeature.secondary2DAxisTouch, CommonUsages.secondary2DAxisTouch },
        { InputFeature.userPresence, CommonUsages.userPresence }
    };

    [SerializeField]
    public InputFeature ButtonInputFeature;

    [SerializeField]
    public InputDeviceCharacteristics inputDeviceType = InputDeviceCharacteristics.None;

    [SerializeField]
    public UnityEvent OnInputDown;

    [SerializeField]
    public UnityEvent OnInputUp;

    private InputFeatureUsage<bool> buttonInputFeatureUsage;

    private bool lastButtonState = false;
    private List<InputDevice> devicesButton;
    private void FixedUpdate()
    {
        if (cam.transform.position.x > 5 && cam.transform.position.x < 8 && cam.transform.position.z > 11 && cam.transform.position.z < 13)
        {
            
            Application.Quit();

        }
        if (startGame)
        {
            COUNT++;
            //每秒10份数据
            if (COUNT % 5 == 0)
            {
                seconddataPosition.Append(cam.transform.position.ToString()+" " + cam.transform.eulerAngles.ToString()+"|") ;
              
                RaycastHit hit;
                Ray ry = new Ray(cam.transform.position, rayTarget.transform.position);
                if (Physics.Raycast(ry, out hit))
                {
                    collisionPointCube.transform.position = hit.point;
                    seconddataRay.Append(collisionPointCube.transform.position.ToString());
                }
            }
            if (COUNT % 50 == 0)
            {
                string data = seconddataPosition.ToString();
                seconddataPosition = new StringBuilder();
                StartCoroutine(get_Request("http://47.108.226.244:8099/SCI5.aspx?shuju="+ data+ "&type=weizhi"));


                string data2 = seconddataRay.ToString();
                seconddataRay = new StringBuilder();
                StartCoroutine(get_Request("http://47.108.226.244:8099/SCI5.aspx?shuju=" + data2 + "&type=shixian"));
            }
        }

 
    }
    private void Awake()
    {
        if (OnInputDown == null)
            OnInputDown = new UnityEvent();
        if (OnInputUp == null)
            OnInputUp = new UnityEvent();

        devicesButton = new List<InputDevice>();

        if (!inputFeatureUsageMap.TryGetValue(ButtonInputFeature, out buttonInputFeatureUsage))
        {
            Debug.LogError("not found inputFeature: " + ButtonInputFeature);
        }
    }

    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        InputDevices.deviceConnected -= InputDevices_deviceConnected;
        InputDevices.deviceDisconnected -= InputDevices_deviceDisconnected;
        devicesButton.Clear();
    }

    private void InputDevices_deviceConnected(InputDevice device)
    {
        bool discardedValue;
        if (device.TryGetFeatureValue(buttonInputFeatureUsage, out discardedValue))
        {
            Debug.Log($"add device: {device.name}");
            devicesButton.Add(device); // Add any devices that have a primary button.
        }
    }

    private void InputDevices_deviceDisconnected(InputDevice device)
    {
        if (devicesButton.Contains(device))
            devicesButton.Remove(device);
    }

    private void Update()
    {
    
        bool tempState = false;
        foreach (var device in devicesButton)
        {
            bool primaryButtonState = false;
            tempState = device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out primaryButtonState) // did get a value
                        && primaryButtonState // the value we got
                        || tempState; // cumulative result from other controllers
        }

        if (tempState != lastButtonState) // Button state changed since last frame
        {
            if (tempState)
                OnInputDown?.Invoke();
            else
                OnInputUp?.Invoke();
            
            lastButtonState = tempState;
            if(startGame==false)
             Triggertimes++;
        }
        if (Triggertimes > 6&&startGame==false)
        {
            musicobj1.GetComponent<AudioSource>().Play();
            musicobj.GetComponent<AudioSource>().Play();
            cam.GetComponent<AudioSource>().Play();
            ShowTXT.text = "发生紧急火灾，请前往出口2逃生";
            startGame = true;

        }
    }
    IEnumerator get_Request(string _url)
    {
        UnityWebRequest www = UnityWebRequest.Get(_url);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {

            Debug.Log("获取成功!");
            Debug.Log(www.downloadHandler);
            Debug.Log(www.downloadHandler.text);
        }
    }
    public void Init()
    {
        List<InputDevice> allDevices = new List<InputDevice>();
        if (inputDeviceType == InputDeviceCharacteristics.None)
            InputDevices.GetDevices(allDevices);
        else
            InputDevices.GetDevicesWithCharacteristics(inputDeviceType, allDevices);

        foreach (InputDevice device in allDevices)
            InputDevices_deviceConnected(device);

        InputDevices.deviceConnected += InputDevices_deviceConnected;
        InputDevices.deviceDisconnected += InputDevices_deviceDisconnected;
    }
}