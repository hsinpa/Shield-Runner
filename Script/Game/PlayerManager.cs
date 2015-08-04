﻿using UnityEngine;
using System.Collections;


namespace Game {
	public class PlayerManager : MonoBehaviour {
		public enum Status {Run, Jump, BeHit};
		public Status currentStatus = Status.Run;
		public string currentShieldStatus = "red";
	
		public bool isLand;
		public int jumpNum = 0;
		public int speed = 5;
		private int maxJumpNum = 2;
		private float stunTime = 0.9f;
		private int boosttime = 5;
		//1 = walk, 2 = jump, 3 = land
		public Animator mAnim;
		public Rigidbody2D mRigidBody;
		public BoxCollider2D mBoxCollider;
		public GameObject greenShield;
		public GameObject landParticle;
		public GameObject jumpParticle;
		public GameObject shieldParticle;
		public GameObject boostdParticle;
		private TrailRenderer mTrailRenderer;
		private float shieldChangePoint = -0.5f;
		private Sprite[] shieldSprites;
		delegate void ShieldMethod();
		ShieldMethod shieldHandler;

		// Use this for initialization
		void Start () {
			mRigidBody = GetComponent<Rigidbody2D>();
			mTrailRenderer = GetComponent<TrailRenderer>();

			mBoxCollider = GetComponent<BoxCollider2D>();
			mAnim = GetComponent<Animator>();
			shieldDeviceDetector();
			shieldSprites = Resources.LoadAll<Sprite>("Game/shields");
		}

		void Update() {
			shieldHandler();
			if (Input.GetMouseButtonDown(0)) Jump();
			
		}

		// Update is called once per frame
		void FixedUpdate () {
			if (currentStatus != Status.BeHit) Move ();
		}

		void Jump() {
			float floorHeight = Screen.height - (Screen.height * 0.14f);
			float inputHeight = Input.mousePosition.y;
			int jumpPower = 10;
			float actualPower = (jumpNum == 0) ? jumpPower : jumpPower * 0.7f;
			if (jumpNum < maxJumpNum && inputHeight < floorHeight) {
				if (jumpNum == 0) {
					mRigidBody.velocity = new Vector2(mRigidBody.velocity.x, actualPower);					
				} else {
					mRigidBody.velocity = new Vector2(mRigidBody.velocity.x, actualPower);
				}
				particleSwitcher(jumpParticle, true);
				jumpNum++;		
			}
		}
		
		void Move() {
			transform.Translate(transform.right * speed *  Time.deltaTime);
		}

		public void damage() {
			currentStatus = Status.BeHit;
			mAnim.SetTrigger("BeHit");
			StartCoroutine(ResumeRunStatus(stunTime));
			mRigidBody.velocity = new Vector2(mRigidBody.velocity.x, 0);
		}

		public void boostSpeed() {
			mTrailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
			particleSwitcher(boostdParticle, true);
			speed = 8;
			StartCoroutine(resumeNormalSpeed());
		}

		IEnumerator resumeNormalSpeed() {
			yield return new WaitForSeconds(boosttime);
			speed = 5;
			mTrailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
			particleSwitcher(boostdParticle, false);
		}

		IEnumerator ResumeRunStatus(float waitTime) {
			yield return new WaitForSeconds(waitTime);
			currentStatus = Status.Run;
		}


		void OnCollisionEnter2D(Collision2D coll) {
			if (coll.gameObject.tag == "Enemy") {
				if (currentStatus == Status.Jump) {
					jumpNum = 0;
					mRigidBody.velocity = new Vector2(mRigidBody.velocity.x, 10);
					boostSpeed();
				}
			}
		}
		
		//=============================================== Practical Function ==============================
		
		
		public void particleSwitcher(GameObject particleObject, bool play) {
			particleObject.SetActive(play);
			ParticleSystem pObject = particleObject.GetComponent<ParticleSystem>();
			pObject.time = 0;
			pObject.Play();
		}

		void mobileShieldHandler() {
			if (Input.acceleration.y > shieldChangePoint) {
				chanageShieldStatus("green");
			} else {
				chanageShieldStatus("red");
			}
		}
		
		void desktopShieldHandler() {
			if (Input.GetKeyDown(KeyCode.Z)) chanageShieldStatus("red");
			if (Input.GetKeyDown(KeyCode.X)) chanageShieldStatus("green");
		}
		
		void shieldDeviceDetector() {
			if (SystemInfo.deviceType == DeviceType.Handheld) {
				shieldHandler = mobileShieldHandler;
			} else {
				shieldHandler = desktopShieldHandler;
			}
		}
		
		void chanageShieldStatus(string color) {
			if (currentShieldStatus != color) {
				currentShieldStatus = color;
				particleSwitcher(shieldParticle, true);
				shieldParticle.GetComponent<ParticleSystem>().startColor = (color == "red") ? Color.red : Color.green;
				greenShield.GetComponent<SpriteRenderer>().enabled = (color == "green") ? true : false;
			}
		}
		
	}
}