﻿using UnityEngine;
using System.Collections;


namespace Game {
	public class PlayerManager : MonoBehaviour {
		public enum Status {Run, Jump, BeHit, Idle};
		public Status currentStatus = Status.Idle;
		public string currentShieldStatus = "red";
	
		public bool isLand;
		public int jumpNum = 0;
		public int speed = 5;
		private int maxJumpNum = 2;
		private float stunTime = 0.9f;
		private int boosttime = 5;
		private int boostStack;
		
		private int bulletCatch;
		private float bulletTime;
		//1 = walk, 2 = jump, 3 = land
		public Animator mAnim;
		public Rigidbody2D mRigidBody;
		public BoxCollider2D mBoxCollider;
		public GameManager gameManager;
		public GameObject greenShield;
		public GameObject landParticle;
		public GameObject jumpParticle;
		public GameObject shieldParticle;
		public GameObject boostdParticle;
		private int jumpPower = 15;
		public MusicHandler mMusicHanlder;
		public MusicModel mMusicModel;

		private TrailRenderer mTrailRenderer;
		private float shieldChangePoint = -0.5f;
		private Sprite[] shieldSprites;
		delegate void ShieldMethod();
		ShieldMethod shieldHandler;

		// Use this for initialization
		void Start () {
			mRigidBody = GetComponent<Rigidbody2D>();
			mTrailRenderer = GetComponent<TrailRenderer>();
			mMusicModel = Camera.main.gameObject.GetComponent<MusicModel>();
			mMusicHanlder = gameObject.AddComponent<MusicHandler>();
			gameManager = Camera.main.gameObject.GetComponent<GameManager>();
			mBoxCollider = GetComponent<BoxCollider2D>();
			mAnim = GetComponent<Animator>();
			shieldDeviceDetector();
			shieldSprites = Resources.LoadAll<Sprite>("Game/shields");
		}

		void Update() {
			if (currentStatus != Status.Idle) {
				shieldHandler();
				if (Input.GetMouseButtonDown(0)) Jump();
			}
		}

		// Update is called once per frame
		void FixedUpdate () {
			if (currentStatus != Status.BeHit && currentStatus != Status.Idle) Move ();
		}

		void Jump() {
			float floorHeight = Screen.height - (Screen.height * 0.14f);
			float inputHeight = Input.mousePosition.y;
			float actualPower = (jumpNum == 0) ? jumpPower : jumpPower * 0.7f;
			if (jumpNum < maxJumpNum && inputHeight < floorHeight && currentStatus != Status.BeHit) {
				if (jumpNum == 0) {
					mRigidBody.velocity = new Vector2(mRigidBody.velocity.x, actualPower);					
				} else {
					mRigidBody.velocity = new Vector2(mRigidBody.velocity.x, actualPower);
				}
				mMusicHanlder.playSound(mMusicModel.jump);
				particleSwitcher(jumpParticle, true);
				jumpNum++;		
			}
		}
		
		void Move() {
			transform.Translate(transform.right * speed *  Time.deltaTime);
		}

		public void damage() {
			currentStatus = Status.BeHit;
			mAnim.SetBool("BeHit", true);
			StartCoroutine(ResumeRunStatus(stunTime));
			mRigidBody.velocity = new Vector2(mRigidBody.velocity.x, 0);
		}

		public void bulletBlock() {
			int boostBulletSecond = 3;
			int maxBulletNum = 4;
			bulletCatch++;
			if (bulletTime > Time.time && bulletCatch >= maxBulletNum) {
				bulletCatch = 0;
			}
			bulletTime = Time.time + 3;
		}

		public void boostSpeed() {
			mTrailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
			particleSwitcher(boostdParticle, true);
			speed = 8;
			boostStack++;
			GameObject.Find("Barriers").GetComponent<MusicHandler>().playSound(mMusicModel.powerUp);
			StartCoroutine(resumeNormalSpeed(boosttime));
		}

		IEnumerator resumeNormalSpeed(float boosttime) {
			yield return new WaitForSeconds(boosttime);
			boostStack--;
			if (boostStack <= 0) {
				speed = 5;
				mTrailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
				particleSwitcher(boostdParticle, false);
				boostStack = 0;
			}
		}

		IEnumerator ResumeRunStatus(float waitTime) {
			yield return new WaitForSeconds(waitTime);
			currentStatus = Status.Run;
			mAnim.SetBool("BeHit", false);
		}


		void OnCollisionEnter2D(Collision2D coll) {
			if (coll.gameObject.tag == "Enemy") {
				if (currentStatus == Status.Jump) {
					jumpNum = 0;
					mRigidBody.velocity = new Vector2(mRigidBody.velocity.x, jumpPower);
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
				mMusicHanlder.playSound(mMusicModel.changeShield);
			}
		}
		
	}
}