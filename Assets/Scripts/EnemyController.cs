// derived from http://www.unifycommunity.com/wiki/index.php?title=PhysicsFPSWalker

using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour {
		
	private Animation animation;
	
	public AnimationClip idleAnimation;
	public AnimationClip walkAnimation;
	public AnimationClip deathAnimation;
	public AnimationClip hurtAnimation;
	
	public float walkAnimationSpeed = 1.0f;
	public float walkMaxAnimationSpeed  = 0.75f;
	public float jumpAnimationSpeed = 1.15f;

	// These variables are for adjusting in the inspector how the object behaves
	public float maxWalkSpeed = 6.0f;
	public float walkForce     = 8.0f;
 
	public float hurtCooldown = 1.5f;
	private float hurtTime = 0.0f;
	
	public int health = 2;
 
	// These variables are there for use by the script and don't need to be edited
	public enum EnemyState {
		Idle = 0,
		Walking = 1,
		Dead = 2,
		Hurt = 3
	}
	public EnemyState enemyState = 0;
	
	private bool isMoving = false;
	private Vector3 originalOrientation = Vector3.zero;
	private Vector3 charOrientation = Vector3.zero;
	[HideInInspector]
	public bool isControllable = true; // shut down controls if necesary
	private Vector3 addForce = Vector3.zero;
 
	private GameObject mainCamera;
	private Vector3 attackDirection;
	
	public bool printPos = true;
	
	void Awake ()
	{
			// Don't let the Physics Engine rotate this physics object so it doesn't fall over when running

		rigidbody.freezeRotation = true;
		charOrientation = transform.TransformDirection(Vector3.forward);
		originalOrientation = transform.TransformDirection(Vector3.forward);
				
		animation = (Animation) GetComponent("Animation");
		if(!animation) Debug.Log("The character you would like to control doesn't have animations. Moving her might look weird.");
		if(!idleAnimation) {
			animation = null;
			Debug.Log("No idle animation found. Turning off animations.");
		}
		if(!walkAnimation) {
			animation = null;
			Debug.Log("No walk animation found. Turning off animations.");
		}
		if(!deathAnimation) {
			animation = null;
			Debug.Log("No death animation found and the character has canJump enabled. Turning off animations.");
		}
		if(!hurtAnimation) {
			Debug.Log(gameObject.name + ": EnemyController: no hurtAnimation found, assign one in the inspector.");
		}
		
	}
	
	void Start() {
		
	}
	
	void OnLevelWasLoaded() {

	}
 
	void OnTriggerStay (Collider other)
	{
		if (other.gameObject.tag == "Player") {
			//Debug.Log("player detected");
			attackDirection = transform.position - other.gameObject.transform.position;
			//if (printPos) Debug.Log(attackDirection);
		}
	}
	
	void OnTriggerExit( Collider other) {
		if (other.gameObject.tag == "Player") {
			//Debug.Log("player detected");
			attackDirection =Vector3.zero;
		}
	}
	
	void OnCollisionEnter(Collision other) {
		
	}
 
	void OnCollisionExit (Collision other)
	{

	}
 
	public virtual float horizontal
	{
		get
		{
			return attackDirection.normalized.x;
		}
	}
	
	void UpdateSmoothedMovementDirection() {
		
		if (mainCamera == null) {
			mainCamera = (GameObject) GameObject.FindWithTag("MainCamera");
		}
		Transform cameraTransform = mainCamera.transform;

		float h = horizontal; //Input.GetAxisRaw("Horizontal");
		//if (h > 0.0f) Debug.Log(h);
	
		bool wasMoving = isMoving;
		isMoving = rigidbody.velocity.magnitude > 0.1 ;
		
		// Target direction relative to the camera
		if (h != 0.0) charOrientation = h <0.0 ? cameraTransform.TransformDirection(originalOrientation) : cameraTransform.TransformDirection(-1.0f*originalOrientation);
	
		// Grounded controls
		if (enemyState != EnemyState.Hurt)
		{
			//	determine character state
			enemyState = EnemyState.Walking;
			if (rigidbody.velocity.sqrMagnitude < 0.01f) {
				enemyState = EnemyState.Idle;
			}
		
		}		
		else if (enemyState == EnemyState.Hurt) {
			if (Time.time - hurtTime > hurtCooldown) {
				enemyState = EnemyState.Idle;
			}				
		}
	}
	
	
	
	public void HurtEnemy() {
		hurtTime = Time.time;
		enemyState = EnemyState.Hurt;
	}

	// This is called every physics frame
	void FixedUpdate ()
	{
		if (!isControllable)
		{
			return;
		}
		
		//attackDirection = Vector3.zero;

		UpdateSmoothedMovementDirection();
		Vector3 lookTarget = new Vector3(charOrientation.x+transform.position.x, transform.position.y, transform.position.z + charOrientation.z);
		transform.LookAt(lookTarget);
 
		// If the object is grounded and isn't moving at the max speed or higher apply force to move it
		if(Mathf.Abs(horizontal) > 0.0f && rigidbody.velocity.magnitude < maxWalkSpeed)
		{
			addForce = Vector3.right*  -horizontal ;
			//if (printPos) Debug.Log("h " + horizontal);
			addForce.y = 0.05f;
			rigidbody.AddForce(addForce*walkForce);
			//if (printPos) Debug.Log("add force " + addForce);
		}
		
		// ANIMATION sector
		if(animation) {
				if (enemyState == EnemyState.Idle) {
					animation.CrossFade(idleAnimation.name);
				}
				else if(enemyState == EnemyState.Walking) {
						animation[walkAnimation.name].speed = Mathf.Clamp(rigidbody.velocity.magnitude*walkAnimationSpeed, 0.0f, walkMaxAnimationSpeed);
						animation.CrossFade(walkAnimation.name);	
				}
				else if(enemyState == EnemyState.Dead) {
						animation.CrossFade(deathAnimation.name);
				}
				if (enemyState == EnemyState.Hurt) {
					animation.CrossFade(hurtAnimation.name);
				}
		}
		// ANIMATION sector
	}
	
	public void KillEnemy() {
		isControllable = false;
		PlayDeathAnimation();
	}
	
	public void PlayDeathAnimation() {
		animation.Play(deathAnimation.name);
		Debug.Log("Play death animation");
	}
	
	public bool DeathAnimationFinished() {
		if (animation.IsPlaying(deathAnimation.name)) {
			return false;
		}
		return true;
	}
}