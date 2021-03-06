﻿using UnityEngine;
using System.Collections;

public class MovementController : MonoBehaviour {
	
	
	public float speed = 2.5f;
	public float jumpPower = 23f;
	public float gravity = 50.0f;
	public float minVerticalPower = -30;
	public ActionState state;
	public int attackCombo = 0;
	private Vector2 moveDirection = Vector2.zero;
	private CharacterController controller;
	private float verticalPower = 0;
	private Animator _animator;
	public float tempPosition;
	private float newTime = 0;
	private Raycast raycast;
	public bool attacking = false;
	public int wjCounter;
	
	void Start ()
	{
		tempPosition = transform.position.x;
		controller  = GetComponent<CharacterController>();
		raycast = GetComponentInChildren<Raycast> ();
		_animator = GetComponentInChildren<Animator>();
	}
	
	public enum ActionState{
		
		Stand,
		Jump,
		WallJump,
		BackJump,
		AttackGround,
		AttackJump,
	}
	
	void Update() {

		Orientation();
		Attack ();
		Move ();
		StepDownPlatform ();
	}
	
	void Move(){

		if (raycast.IsGrounded()){
			wjCounter = 0;
		}

		if (raycast.IsGrounded() && !attacking) {
			
			state = ActionState.Stand;
			
			if (Input.GetButtonDown ("Jump") && !Input.GetKey(KeyCode.S)){
				
				_animator.SetBool("jump", true);
				verticalPower = jumpPower;
				state = ActionState.Jump;
			}
		}
		
		else if (!raycast.IsGrounded() && state != ActionState.WallJump && 
		       						      state != ActionState.BackJump &&
		       							  state != ActionState.AttackJump){
			state = ActionState.Jump;
		}
		
		switch (state){
			
		case ActionState.Stand:

			_animator.SetBool("jump", false);
			_animator.SetFloat("walk", Mathf.Abs(Input.GetAxis("Horizontal")));
			moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
			moveDirection *= speed;
			break;
			
		case ActionState.Jump:
			
			_animator.SetBool("jump", true);
			moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
			moveDirection *= speed;
			
			WallJump();

			break;
			
		case ActionState.WallJump:

			if (DistanceFromObject (raycast.raycastPoint) > 8) {
				state = ActionState.BackJump;
			}

			WallJump();

			break;
			
		case ActionState.BackJump:

			if (Input.GetButton("Horizontal")){
				moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
				moveDirection *= speed + 10;
				state = ActionState.Jump;
			}

			WallJump();

			break;
		}

		if (verticalPower >= minVerticalPower){

			verticalPower -= gravity * Time.deltaTime;
		}
		else {
			verticalPower = minVerticalPower;
		}

		moveDirection.y = verticalPower;
	
		controller.Move(moveDirection * Time.deltaTime);
	//	print (moveDirection.y);
	}

	void WallJump(){

		if (raycast.CanWallJump()){
//			print (raycast.CanWallJump());
			if (Input.GetButtonDown ("Jump") && Input.GetAxisRaw("Horizontal") != 0){
				if (state != ActionState.BackJump){
					state = ActionState.WallJump;
				}
				wjCounter += 1;
				print ("Vert Antes " + verticalPower);
				verticalPower = jumpPower; //+ 3 * wjCounter;

				print ("Vert Depois " + verticalPower);
				moveDirection = transform.TransformDirection(-Vector3.right.x + 0.2f, 0, 0);
				moveDirection *= speed;
			}
		}
	}
	
	void Orientation(){
		
		if (transform.position.x > tempPosition) {
			transform.eulerAngles = new Vector3(0,0,0);
		}
		else if (transform.position.x < tempPosition){
			transform.eulerAngles = new Vector3(0, 180, 0);
		}
		tempPosition = transform.position.x;
	}
	
	float DistanceFromObject (Vector2 ObjectPosition){
		
		float distance = Vector2.Distance (ObjectPosition, transform.position);
		//		print (distance);
		return distance;
	}
	
	void Attack(){
		
		if (Input.GetButtonDown("Fire1")){
			
			switch (state){
			case ActionState.Stand:
				AttackGround();
				break;
				
			case ActionState.AttackGround:
				AttackGround();
				break;
				
			case ActionState.Jump:
				AttackJump();
				break;
				
			case ActionState.AttackJump:
				AttackJump();
				break;
				
			case ActionState.WallJump:
				AttackJump();
				break;
				
			case ActionState.BackJump:
				AttackJump();
				break;
			}
		}
		
		if (attackCombo == 0){
			_animator.SetInteger("combo", 0);
		}
	}
	
	void AttackGround (){
		
		attacking = true;
		StopWalking();
		state = ActionState.AttackGround;
		attackCombo += 1;
		_animator.SetInteger("combo", attackCombo);
	}
	
	void AttackJump(){
		
		state = ActionState.AttackJump;
		attackCombo = 1;
		_animator.SetInteger("combo", attackCombo);
	}
	
	void StopWalking(){
		
		moveDirection *= 0;
		_animator.SetFloat("walk", 0);
	}
	
	void StepDownPlatform(){
		
		if (raycast.IsGrounded ()) {
			if (raycast.IsGrounded().collider.name == "Platform_OneWay"){
				GameObject oneWayPlatform = raycast.IsGrounded().collider.gameObject;
				
				if (Input.GetButtonDown ("Jump") && Input.GetKey(KeyCode.S)){
					
					oneWayPlatform.GetComponentInChildren<OneWayCollision>().BehaviorActive = false;
					Physics.IgnoreCollision(this.gameObject.collider, oneWayPlatform.collider);
				}
			}
		}
	}
}
