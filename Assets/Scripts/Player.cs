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

    //当前拥有的炸弹
    private int _bombs = 3;
    private int bombs {
        get { return _bombs; }
        set {
            Debug.Log("^^^^^^^^^^bombs changed");
            Debug.Log(GetComponent<NetworkIdentity>().netId);
            _bombs = value;
            Debug.Log(_bombs);
        }
    }
    //已经增加的炸弹数
    private int bombLimit = 0;

    //当前爆炸范围
    private int bombScope = 2;

    //加速数值
    private float speedup = 0f;

    // the diretion, last time, power of bouncing away by explosion
    private bool toLeft = false;
    private bool toRight = false;
    private bool toUp = false;
    private bool toDown = false;
    private float leftTime = 0f;
    private float rightTime = 0f;
    private float upTime = 0f;
    private float downTime = 0f;
    private int leftPower = 0;
    private int rightPower = 0;
    private int upPower = 0;
    private int downPower = 0;

    //time of invincibility
    private float immuneTime = 0f;

    //on banana and slip direction
    private bool bananaed = false;
    private Vector3 slipDir = Vector3.zero;

    //last player rotation
    private int lastBodyRotation;
    //Prefabs
    public GameObject bombPrefab;
    public GameObject weakwallPrefab;
    //JoyStick控制
    //private Image joystick;
    private FloatingJoystick joystick;
    //Cached components
    public Rigidbody rigidBody;
    private Transform myTransform;
    private Animator animator;

    // void Awake(){
    //     animator = myTransform.Find ("PlayerModel").GetComponent<Animator> ();
    // }
    // Use this for initialization
    void Start()
    {
        Debug.Log(GetComponent<NetworkIdentity>().playerControllerId);
        //Cache the attached components for better performance and less typing
        healthSlider = GetComponentInChildren<Slider>();
        rigidBody = GetComponent<Rigidbody>();
        myTransform = transform;
        joystick = FindObjectOfType<FloatingJoystick>();
        //animator = myTransform.Find ("PlayerModel").GetComponent<Animator> ();
        animator = GetComponent<Animator> ();
        if(isLocalPlayer){
            GameObject.FindGameObjectWithTag("bombControl").GetComponent<Button>().onClick.AddListener(this.OnClickBomb);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer)
        {
            UpdateMovement();
        }

    }

    private void FixedUpdate()
    {   
        //decrease the immune time
        if(immuneTime > 0) {
            immuneTime -= Time.deltaTime;
        }
        //slip to slipDir with a speed of 10f
        if(bananaed) {
            rigidBody.AddForce(slipDir * 10f, ForceMode.VelocityChange);
        }
        //the effect by exlposion
        if(toLeft) {
            leftTime = 0.5f + 0.1f * leftPower;
            toLeft = false;
        }
        if(toRight) {
            rightTime = 0.5f + 0.1f * rightPower;
            toRight = false;
        }
        if(toUp) {
            upTime = 0.5f + 0.1f * upPower;
            toUp = false;
        }
        if(toDown) {
            downTime = 0.5f + 0.1f * downPower;
            toDown = false;
        }
        if(leftTime > 0) {
            rigidBody.AddForce(new Vector3(-12f, 0, 0) * leftTime, ForceMode.VelocityChange);
            leftTime -= Time.deltaTime;
        }
        if (rightTime > 0)
        {
            rigidBody.AddForce(new Vector3(12f, 0, 0) * rightTime, ForceMode.VelocityChange);
            rightTime -= Time.deltaTime;
        }
        if (upTime > 0)
        {
            rigidBody.AddForce(new Vector3(0, 0, 12f) * upTime, ForceMode.VelocityChange);
            upTime -= Time.deltaTime;
        }
        if (downTime > 0)
        {
            rigidBody.AddForce(new Vector3(0, 0, -12f) * downTime, ForceMode.VelocityChange);
            downTime -= Time.deltaTime;
        }
    }

    private void UpdateMovement()
    {
        //animator.SetBool ("Walking", false); //Resets walking animation to idle

        if (!canMove)
        { //Return if player can't move
            return;
        }

        //Depending on the player number, use different input for moving
        // if (playerNumber == 1)
        // {
        UpdatePlayer1Movement();
        // } else
        // {
        //     UpdatePlayer2Movement ();
        // }
    }
    

    /// <summary>
    /// Updates Player 1's movement and facing rotation using the WASD keys and drops bombs using Space
    /// </summary>
    private void UpdatePlayer1Movement()
    {
        Vector3 dir = Vector3.zero;
        dir.x = joystick.Horizontal;
        dir.z = joystick.Vertical;
        rigidBody.velocity = new Vector3(dir.x * (moveSpeed + speedup), 0, dir.z * (moveSpeed + speedup));
        if (dir.x == 0 && dir.z == 0) {
            animator.SetBool("Walking", false);
        }else {
            animator.SetBool("Walking", true);
        }


        if (dir.x == 0 && dir.z == 0) {
            myTransform.rotation = Quaternion.Euler(0, lastBodyRotation, 0);
        } else {
            if (dir.x >= 0 && dir.z >= 0)
            {
                if (dir.x > dir.z)
                {
                    myTransform.rotation = Quaternion.Euler(0, 90, 0);
                    lastBodyRotation = 90;
                }
                else
                {
                    myTransform.rotation = Quaternion.Euler(0, 0, 0);
                    lastBodyRotation = 0;
                }
            }
            else if (dir.x >= 0 && dir.z < 0)
            {
                if (dir.x >= Math.Abs(dir.z))
                {
                    myTransform.rotation = Quaternion.Euler(0, 90, 0);
                    lastBodyRotation = 90;
                }
                else
                {
                    myTransform.rotation = Quaternion.Euler(0, 180, 0);
                    lastBodyRotation = 180;
                }

            }
            else if (dir.x < 0 && dir.z >= 0)
            {
                if (Math.Abs(dir.x) >= Math.Abs(dir.z))
                {
                    myTransform.rotation = Quaternion.Euler(0, 270, 0);
                    lastBodyRotation = 270;
                }
                else
                {
                    myTransform.rotation = Quaternion.Euler(0, 0, 0);
                    lastBodyRotation = 0;
                }
            }
            else if (dir.x < 0 && dir.z < 0)
            {
                if (Math.Abs(dir.x) >= Math.Abs(dir.z))
                {
                    myTransform.rotation = Quaternion.Euler(0, 270, 0);
                    lastBodyRotation = 270;
                }
                else
                {
                    myTransform.rotation = Quaternion.Euler(0, 180, 0);
                    lastBodyRotation = 180;
                }
            }
        }


        if (Input.GetKey (KeyCode.W))
        { //Up movement
            rigidBody.velocity = new Vector3 (rigidBody.velocity.x, rigidBody.velocity.y, (moveSpeed + speedup));
            myTransform.rotation = Quaternion.Euler (0, 0, 0);
            animator.SetBool ("Walking", true);
        }

        if (Input.GetKey (KeyCode.A))
        { //Left movement
            rigidBody.velocity = new Vector3 (-(moveSpeed + speedup), rigidBody.velocity.y, rigidBody.velocity.z);
            myTransform.rotation = Quaternion.Euler (0, 270, 0);
            animator.SetBool ("Walking", true);
        }

        if (Input.GetKey (KeyCode.S))
        { //Down movement
            rigidBody.velocity = new Vector3 (rigidBody.velocity.x, rigidBody.velocity.y, -(moveSpeed + speedup));
            myTransform.rotation = Quaternion.Euler (0, 180, 0);
            animator.SetBool ("Walking", true);
        }

        if (Input.GetKey (KeyCode.D))
        { //Right movement
            rigidBody.velocity = new Vector3 ((moveSpeed + speedup), rigidBody.velocity.y, rigidBody.velocity.z);
            myTransform.rotation = Quaternion.Euler (0, 90, 0);
            animator.SetBool ("Walking", true);
        }


        if (Input.GetKeyDown(KeyCode.Space))
        { //Drop bomb
            bombput();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) && weakwallPrefab) {
            CmdBuild();
        }
    }

    public void bombput()
    {
        if (canDropBombs)
        {
            DropBomb();
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

    private void DropBomb ()
    {
        if (bombPrefab && bombs > 0)
        { //Check if bomb prefab is assigned first
            canDropBombs = false;
            bombs--;
             //Create new bomb and snap it to a tile
            //GameObject bomb = Instantiate (bombPrefab,
            //    new Vector3 (Mathf.RoundToInt (myTransform.position.x), bombPrefab.transform.position.y, Mathf.RoundToInt (myTransform.position.z)),
            //    bombPrefab.transform.rotation) ;
            //bomb.GetComponent<Bomb>().initBomb(bombScope, gameObject);
            //NetworkServer.Spawn(bomb);
            CmdDrop(GetComponent<NetworkIdentity>().netId, bombScope);
        }
    }
    [Command]
    void CmdDrop(NetworkInstanceId id, int scope){
        GameObject bomb = Instantiate(bombPrefab,
                new Vector3(Mathf.RoundToInt(myTransform.position.x), bombPrefab.transform.position.y, Mathf.RoundToInt(myTransform.position.z)),
                bombPrefab.transform.rotation);
        bomb.GetComponent<Bomb>().initBomb(scope, id);
        NetworkServer.Spawn(bomb);
    }
    [Command]
    void CmdBuild() {
        GameObject weakwall = Instantiate(weakwallPrefab, new Vector3(Mathf.RoundToInt(myTransform.position.x), weakwallPrefab.transform.position.y, Mathf.RoundToInt(myTransform.position.z)),
                                          weakwallPrefab.transform.rotation);
        NetworkServer.Spawn(weakwall);
    }

    [Command]
    void CmdTakeDamage(){
        healthValue -= 1;
        RpcTakeDamage(healthValue);
    }

    [ClientRpc]
    void RpcTakeDamage(float healthValue){
        Debug.Log(healthValue + "RpcTakeDamage");
        healthSlider.value = healthValue;
    }

    public void OnTriggerEnter (Collider other)
    {
        if (!dead && other.CompareTag ("Explosion") && immuneTime <= 0)
        { //Not dead & hit by explosion
            Debug.Log ("P"  + " hit by explosion!");
            CmdTakeDamage();
            if (healthValue <= 0)
            {
                dead = true;
                //globalManager.PlayerDied (playerNumber); //Notify global state manager that this player died
                Destroy(gameObject);
            }

            Vector3 dir = other.gameObject.GetComponent<DestroySelf>().getDir();
            int power = other.gameObject.GetComponent<DestroySelf>().getPower();
            if (dir == Vector3.back) {
                downPower = power;
                toDown = true;
            }
            else if(dir == Vector3.forward) {
                upPower = power;
                toUp = true;
            }
            else if(dir == Vector3.left) {
                leftPower = power;
                toLeft = true;
            }
            else {
                rightPower = power;
                toRight = true;
            }
        }
        //on banana
        if (other.gameObject.CompareTag("Banana"))
        {
            bananaed = true;
            canMove = false;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        //stop sliding
        if ((collision.gameObject.CompareTag("Weakwall") || collision.gameObject.CompareTag("Block")) && bananaed)
        {
            bananaed = false;
            canMove = true;
            Debug.Log(" hit the wall!");
        }
    }
    public void onBanana(Vector3 dir) {
        slipDir = dir;
    }

    //become immunable
    public void toImmune() {
        immuneTime = 3f;
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
