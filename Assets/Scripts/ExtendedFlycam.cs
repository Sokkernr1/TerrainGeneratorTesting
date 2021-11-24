using UnityEngine;
using System.Collections;

public class ExtendedFlycam : MonoBehaviour
{
    public float cameraSensitivity = 90;
    public float climbSpeed = 4;
    public float normalMoveSpeed = 10;
    public float slowMoveFactor = 0.25f;
    public float fastMoveFactor = 3;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    private TerrainGenerator myGen;
    private TexturizeTerrain myTexturizer;
    private bool isActive = false;

    private void Start()
    {
        myGen = FindObjectOfType<TerrainGenerator>();
        myTexturizer = FindObjectOfType<TexturizeTerrain>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Reloading Terrain...");
            myGen.InitiateTerrain();
        } else if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Reloading Textures...");
            myTexturizer.StartTexturizing();
        }
        
        if (isActive)
        {
            rotationX += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
            rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
            rotationY = Mathf.Clamp(rotationY, -90, 90);

            transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
            transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                transform.position += transform.forward * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
                transform.position += transform.right * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                transform.position += transform.forward * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
                transform.position += transform.right * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
            }
            else
            {
                transform.position += transform.forward * normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
                transform.position += transform.right * normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
            }


            if (Input.GetKey(KeyCode.Q)) { transform.position += transform.up * climbSpeed * Time.deltaTime; }
            if (Input.GetKey(KeyCode.E)) { transform.position -= transform.up * climbSpeed * Time.deltaTime; }


            
        }
        if (Input.GetKeyDown(KeyCode.Escape) && isActive)
        {
            Cursor.lockState = CursorLockMode.None;
            isActive = false;
            Debug.Log("Flycam now disabled");
            
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && !isActive)
        {
            Cursor.lockState = CursorLockMode.Locked;
            isActive = true;
            Debug.Log("Flycam now enabled");
        }
    }
}
