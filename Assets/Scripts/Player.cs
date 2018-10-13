using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    // 最多拥有的炸弹数：初始炸弹数 bombs + MAX_BOMBS
    private readonly int MAX_BOMBS = 5;
    private readonly int MAX_SCOPE = 7;
    private readonly float MAX_SPEED = 5f;
    //Manager
    public GlobalStateManager globalManager;
    public float healthValue = 100;
    public Slider healthSlider;
    
    public float moveSpeed = 5f;
    public bool canDropBombs = true;
    //Can the player drop bombs?
    public bool canMove = true;
    //Can the player move?
    public bool dead = false;
    //Is this player dead?

     //当前拥有的炸弹数
    private int bombs = 3;
    //已经增加的炸弹数
    private int bombLimit = 0;

    //当前爆炸范围
    private int bombScope = 2;

    //加速数值
    private float speedup = 0f;
    //Prefabs
    public GameObject bombPrefab;
    //JoyStick控制
    //private Image joystick;
    private VirtualJoyStick joystick;
    //Cached components
    private Rigidbody rigidBody;
    private Transform myTransform;
    //private Animator animator;

    // void Awake(){
    //     animator = myTransform.Find ("PlayerModel").GetComponent<Animator> ();
    // }
    // Use this for initialization
    void Start ()
    {
        //Cache the attached components for better performance and less typing
        healthSlider = GetComponentInChildren<Slider>();
        rigidBody = GetComponent<Rigidbody> ();
        myTransform = transform;
        joystick = GameObject.FindGameObjectWithTag("control").GetComponent<VirtualJoyStick>();
        //animator = myTransform.Find ("PlayerModel").GetComponent<Animator> ();
        //animator = GetComponent<Animator> ();
        GameObject.FindGameObjectWithTag("bombControl").GetComponent<Button>().onClick.AddListener(this.OnClickBomb);
    }

    // Update is called once per frame
    void Update ()
    {
        if(isLocalPlayer){
            UpdateMovement ();
        }
        
    }

    private void UpdateMovement ()
    {
        //animator.SetBool ("Walking", false); //Resets walking animation to idle

        if (!canMove)
        { //Return if player can't move
            return;
        }

        //Depending on the player number, use different input for moving
        // if (playerNumber == 1)
        // {
        UpdatePlayer1Movement ();
        // } else
        // {
        //     UpdatePlayer2Movement ();
        // }
    }

    /// <summary>
    /// Updates Player 1's movement and facing rotation using the WASD keys and drops bombs using Space
    /// </summary>
    private void UpdatePlayer1Movement ()
    {
        Vector3 dir = Vector3.zero;
        dir.x = joystick.Horizontal();
        dir.z = joystick.Vertical();
        rigidBody.velocity = new Vector3(dir.x*moveSpeed, 0, dir.z*moveSpeed);

        /*
        if (Input.GetKey (KeyCode.W))
        { //Up movement
            rigidBody.velocity = new Vector3 (rigidBody.velocity.x, rigidBody.velocity.y, moveSpeed);
            //myTransform.rotation = Quaternion.Euler (0, 0, 0);
            //animator.SetBool ("Walking", true);
        }

        if (Input.GetKey (KeyCode.A))
        { //Left movement
            rigidBody.velocity = new Vector3 (-moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
            //myTransform.rotation = Quaternion.Euler (0, 270, 0);
            //animator.SetBool ("Walking", true);
        }

        if (Input.GetKey (KeyCode.S))
        { //Down movement
            rigidBody.velocity = new Vector3 (rigidBody.velocity.x, rigidBody.velocity.y, -moveSpeed);
            //myTransform.rotation = Quaternion.Euler (0, 180, 0);
            //animator.SetBool ("Walking", true);
        }

        if (Input.GetKey (KeyCode.D))
        { //Right movement
            rigidBody.velocity = new Vector3 (moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
            //myTransform.rotation = Quaternion.Euler (0, 90, 0);
            //animator.SetBool ("Walking", true);
        }
        */

        if (Input.GetKeyDown (KeyCode.Space))
        { //Drop bomb
            bombput();
        }
    }

    public void bombput()
    {
        if (canDropBombs)
        {
            CmdDropBomb();
        }
    }

    public void OnClickBomb()
    {
        bombput();
    }

    /// <summary>
    /// Updates Player 2's movement and facing rotation using the arrow keys and drops bombs using Enter or Return
    /// </summary>
    // private void UpdatePlayer2Movement ()
    // {
    //     if (Input.GetKey (KeyCode.UpArrow))
    //     { //Up movement
    //         rigidBody.velocity = new Vector3 (rigidBody.velocity.x, rigidBody.velocity.y, moveSpeed);
    //         myTransform.rotation = Quaternion.Euler (0, 0, 0);
    //         animator.SetBool ("Walking", true);
    //     }

    //     if (Input.GetKey (KeyCode.LeftArrow))
    //     { //Left movement
    //         rigidBody.velocity = new Vector3 (-moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
    //         myTransform.rotation = Quaternion.Euler (0, 270, 0);
    //         animator.SetBool ("Walking", true);
    //     }

    //     if (Input.GetKey (KeyCode.DownArrow))
    //     { //Down movement
    //         rigidBody.velocity = new Vector3 (rigidBody.velocity.x, rigidBody.velocity.y, -moveSpeed);
    //         myTransform.rotation = Quaternion.Euler (0, 180, 0);
    //         animator.SetBool ("Walking", true);
    //     }

    //     if (Input.GetKey (KeyCode.RightArrow))
    //     { //Right movement
    //         rigidBody.velocity = new Vector3 (moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
    //         myTransform.rotation = Quaternion.Euler (0, 90, 0);
    //         animator.SetBool ("Walking", true);
    //     }

    //     if (canDropBombs && (Input.GetKeyDown (KeyCode.KeypadEnter) || Input.GetKeyDown (KeyCode.Return)))
    //     { //Drop Bomb. For Player 2's bombs, allow both the numeric enter as the return key or players without a numpad will be unable to drop bombs
    //         CmdDropBomb ();
    //     }
    // }

    /// <summary>
    /// Drops a bomb beneath the player
    /// </summary>
    [Command]
    private void CmdDropBomb ()
    {
        if (bombPrefab && bombs > 0)
        { //Check if bomb prefab is assigned first
            canDropBombs = false;
            bombs--;
            // Create new bomb and snap it to a tile
            GameObject bomb = Instantiate (bombPrefab,
                new Vector3 (Mathf.RoundToInt (myTransform.position.x), bombPrefab.transform.position.y, Mathf.RoundToInt (myTransform.position.z)),
                bombPrefab.transform.rotation) ;
            bomb.GetComponent<Bomb>().initBomb(bombScope, gameObject);
            NetworkServer.Spawn(bomb);
        }
    }

    [Command]
    void CmdTakeDamage(){
        healthValue -= 10;
        RpcTakeDamage(healthValue);
    }

    [ClientRpc]
    void RpcTakeDamage(float healthValue){
        Debug.Log(healthValue + "RpcTakeDamage");
        healthSlider.value = healthValue;
    }

    public void OnTriggerEnter (Collider other)
    {
        if (!dead && other.CompareTag ("Explosion"))
        { //Not dead & hit by explosion
            Debug.Log ("P"  + " hit by explosion!");
            CmdTakeDamage();
            if(healthValue <= 0){
                dead = true;
                //globalManager.PlayerDied (playerNumber); //Notify global state manager that this player died
                Destroy (gameObject);
            }
            
        }
    }
    // interact with bombs
    public void enableDrop()
    {
        canDropBombs = true;
    }

    public void bombExploded()
    {
        bombs++;
    }

    // change player's attributes
    public void speedUp()
    {   if(speedup < MAX_SPEED) {
            speedup++;
        }
    }

    public void addBombs(){
        if (bombLimit < MAX_BOMBS)
        {
            bombs++;
            bombLimit++;
        }
    }
    public void powerUp(){
        if(bombScope < MAX_SCOPE) {
            bombScope++;
        }
    }
}
