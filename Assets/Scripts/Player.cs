using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isGrounded;
    public bool isSprinting;

    private Transform cam;
    private World world;

    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;

    public float playerWidth = 0.15f;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum;
    private bool jumpRequest;

    private void Start() {

        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();

    }

    private void FixedUpdate()
    {
        calculateVelocity();

        if (jumpRequest)
            jump();

        transform.Rotate(Vector3.up * mouseHorizontal * 6);
        cam.Rotate(Vector3.right * -mouseVertical * 6);
        transform.Translate(velocity, Space.World);
    }

    private void Update() 
    {
        getPlayerInputs();
    }

    void jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    private void calculateVelocity()
    {
        // Affect vertical momentum with gravity
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        // if we're sprinting
        if (isSprinting)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        else
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.deltaTime * walkSpeed;

        // Apply vertical momentum (falling/jumping)
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        // se houver movimento no eixo x e ouver algo á frente
        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;

        if (velocity.y < 0)
            velocity.y = checkDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = checkUpSpeed(velocity.y);

    }

    private void getPlayerInputs() 
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
            isSprinting = true;            
        if (Input.GetButtonUp("Sprint"))
            isSprinting = false;

        if (isGrounded && Input.GetButtonDown("Jump"))
            jumpRequest = true;

    }

    private float checkDownSpeed(float downSpeed)
    {
        if (
            world.checkForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.checkForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.checkForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
            world.checkForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
        ) {
            isGrounded = true;
            return 0;
        }
        else {
            isGrounded = false;
            return downSpeed;
        }

    }
    

    private float checkUpSpeed(float upSpeed)
    {
        if (
            world.checkForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
            world.checkForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
            world.checkForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)) ||
            world.checkForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth))
        ) {
            return 0;
        }
        else {
            return upSpeed;
        }

    }
    
    public bool front 
    {
        get { 
            if (
                world.checkForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                world.checkForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth))
            ) {
                return true;
            } 
            else {
                return false;
            }

        }
    }

    public bool back 
    {
        get { 
            if (
                world.checkForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                world.checkForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth))
            ) {
                return true;
            } 
            else {
                return false;
            }

        }
    }

    public bool right 
    {
        get { 
            if (
                world.checkForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                world.checkForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z))
            ) {
                return true;
            } 
            else {
                return false;
            }

        }
    }

    public bool left 
    {
        get { 
            if (
                world.checkForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                world.checkForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z))
            ) {
                return true;
            } 
            else {
                return false;
            }

        }
    }

}
