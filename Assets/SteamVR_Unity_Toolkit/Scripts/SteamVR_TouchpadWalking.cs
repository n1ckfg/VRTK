using UnityEngine;
using System.Collections;

public class SteamVR_TouchpadWalking : MonoBehaviour {
    public float deceleration = 0.1f;
    public float acceleration = 3f;

    private float movementSpeed = 0f;
    private float strafeSpeed = 0f;
    private int listenerInitTries = 5;

    private Transform headset;
    private Vector2 touchAxis;

    private Rigidbody rb;
    private BoxCollider bc;

    private GameObject floorTouching;

    private void Start () {
        headset = GetHeadset();
        CreateCollider();
        InitListeners();
    }

    private Transform GetHeadset()
    {
#if (UNITY_5_4_OR_NEWER)
        return GameObject.FindObjectOfType<SteamVR_Camera>().GetComponent<Transform>();
#endif
        return GameObject.FindObjectOfType<SteamVR_GameView>().GetComponent<Transform>();
    }

    private void InitListeners()
    {
        SteamVR_ControllerEvents[] controllers = GameObject.FindObjectsOfType<SteamVR_ControllerEvents>();
        if (controllers.Length == 0)
        {
            if (listenerInitTries > 0)
            {
                listenerInitTries--;
                Invoke("InitListeners", 0.25f);
            }
            else
            {
                Debug.LogError("A GameObject must exist with a SteamVR_ControllerEvents script attached to it");
                return;
            }
        }

        foreach (SteamVR_ControllerEvents controller in controllers)
        {
            controller.TouchpadAxisChanged += new ControllerClickedEventHandler(DoTouchpadAxisChanged);
            controller.TouchpadUntouched += new ControllerClickedEventHandler(DoTouchpadUntouched);
        }
    }

    private void CreateCollider()
    {
        rb = this.gameObject.AddComponent<Rigidbody>();
        rb.mass = 100;
        rb.freezeRotation = true;

        bc = this.gameObject.AddComponent<BoxCollider>();
        bc.center = new Vector3(0f, 1f, 0f);
        bc.size = new Vector3(0.25f, 1f, 0.25f);

        this.gameObject.layer = 2;
    }

    private void DoTouchpadAxisChanged(object sender, ControllerClickedEventArgs e)
    {
        touchAxis = e.touchpadAxis;
    }

    private void DoTouchpadUntouched(object sender, ControllerClickedEventArgs e)
    {
        touchAxis = Vector2.zero;
    }

    private void CalculateSpeed(ref float speed, float inputValue)
    {
        if (inputValue != 0f)
        {
            speed = (acceleration * inputValue);
        }
        else
        {
            Decelerate(ref speed);
        }
    }

    private void Decelerate(ref float speed)
    {
        if (speed > 0)
        {
            speed -= Mathf.Lerp(deceleration, acceleration, 0f);
        }
        else if (speed < 0)
        {
            speed += Mathf.Lerp(deceleration, -acceleration, 0f);
        }
        else
        {
            speed = 0;
        }
    }

    private void Move()
    {
        Vector3 movement = headset.transform.forward * movementSpeed * Time.deltaTime;
        Vector3 strafe = headset.transform.right * strafeSpeed * Time.deltaTime;
        float fixY = this.transform.position.y;
        this.transform.position += (movement + strafe);
        this.transform.position = new Vector3(this.transform.position.x, fixY, this.transform.position.z);
    }

    private void UpdateCollider()
    {
        float playAreaHeightAdjustment = 0.01f;
        float newBCYSize = headset.transform.position.y - this.transform.position.y;
        float newBCYCenter = (newBCYSize != 0 ? (newBCYSize / 2) + playAreaHeightAdjustment: 0);

        bc.size = new Vector3(bc.size.x, newBCYSize, bc.size.z);
        bc.center = new Vector3(headset.localPosition.x, newBCYCenter, headset.localPosition.z);
    }

    private void Update()
    {
        UpdateCollider();
    }

    private void FixedUpdate()
    {
        CalculateSpeed(ref movementSpeed, touchAxis.y);
        CalculateSpeed(ref strafeSpeed, touchAxis.x);
        Move();
    }
}