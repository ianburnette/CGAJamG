using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour {
    #region Private Variables
    [Header("References")]
    [SerializeField] bool[] inputs;
    bool u, d, l, r;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator anim;

    [Header("Behavior")]
    [SerializeField] float speed;
    [SerializeField] float animationMagnitudeDivisor;
    [SerializeField] Vector3[] rotations;

    [Header("Raycasting")]
    [SerializeField] Vector2[] raycastDirections;
    [SerializeField] float raycastLength , destinationLength, stopDistance;
    [SerializeField] bool[] hits;
    [SerializeField] LayerMask mask;


    #endregion

    #region Public Properties

    #endregion

    #region Unity Functions
    void Start () {

	}
	
	void Update () {
        GetPlayerInput();
        CheckSurroundings();
       // if (CanMove() && AnyInput())
            ApplyPlayerMovement();
        Animate();
	}
    #endregion

    #region Custom Functions
	void GetPlayerInput()
    {
        //inputs[0] = Input.GetButtonDown("up");
        //inputs[1] = Input.GetButtonDown("down");
        //inputs[2] = Input.GetButtonDown("left");
        //inputs[3] = Input.GetButtonDown("right");
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (v != 0)
            if (v > 0)
                SetInput(0);
            else
                SetInput(1);
        else
        {
            ReleaseInput(0);
            ReleaseInput(1);
        }
        if (h != 0)
            if (h > 0)
                SetInput(3);
            else
                SetInput(2);
        else
        {
            ReleaseInput(2);
            ReleaseInput(3);
        }

    }
    void ApplyPlayerMovement()
    {
        RaycastHit2D destinationHit = Physics2D.Raycast(transform.position, EvaluateDirection(), destinationLength, mask);
        if (destinationHit.distance > stopDistance)
        {
            rb.velocity = EvaluateDirection() * speed;
            print("dist is " + destinationHit.distance);
        }
        else {
            rb.velocity = Vector2.zero;
            print("dist is small " + destinationHit.distance);
        }

    }
    void CheckSurroundings()
    {
        for (int i = 0; i < raycastDirections.Length; i++)
        {
            if (Physics2D.Raycast(transform.position, raycastDirections[i], raycastLength, mask))
            {
                if (!hits[i])
                    Align(i);
                hits[i] = true;
            }
            else
                hits[i] = false;
        }
    }
    bool AnyInput()
    {
        bool ret = false;
        foreach (bool inp in inputs)
            if (inp)
                ret = true;
        return ret;
    }
    bool CanMove()
    {
        int hitCount = 0;
        foreach (bool hit in hits)
            if (hit)
                hitCount++;
        if (hitCount > 0 && rb.velocity.magnitude == 0)
            return true;
        else
            return false;
    }
    void Animate()
    {
        anim.SetBool("moving", rb.velocity.magnitude == 0 ? false : true);
        anim.speed = ScaleVelocity(rb.velocity.magnitude);
    }
    float ScaleVelocity(float incomingMagnitude)
    {
        float mag = incomingMagnitude / animationMagnitudeDivisor;
        return mag;
    }
    void SetInput(int incomingInput)
    {
        for (int i = 0; i < inputs.Length; i++)
            inputs[i] = false;
        inputs[incomingInput] = true;
    }
    void ReleaseInput(int cancellingInput)
    {
        inputs[cancellingInput] = false;
    }
    Vector2 EvaluateDirection()
    {
        Vector2 ret = Vector2.zero;
        for (int i = 0; i<raycastDirections.Length; i++)
        {
            if (inputs[i])
            {
                ret = raycastDirections[i];
                Align(i);
            }
        }
        return ret;
    }
    void Align(int dir)
    {
        transform.rotation = Quaternion.Euler(rotations[dir]);
    }
    #endregion
}
