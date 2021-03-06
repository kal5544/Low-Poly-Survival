﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GunController : MonoBehaviour
{
	// Enum
	enum firemode
	{
		Burst,
		Semi,
		FullAuto,
		BoltAction,
		PumpActionShotgun,

		FullAutoShotgun,
		BurstSemi,
		BurstAuto,
		SemiAuto,
		BurstSemiAuto
	}

	// UI
		[Header("UI")] 
	[Tooltip("The Text UI to dispaly your ammo")] [SerializeField] Text AmmoHud;
	// See In Inspector
		[Header("Extra Variables")] 
	[Tooltip("Particle To Be Played When A Bullet Hits A Surface")] [SerializeField] List<hitParticle> HitParticle = new List<hitParticle>();
	[Space(5f)]
	[SerializeField] List<bulletHit> BulletHit = new List<bulletHit>();
	[Space(5f)]
	[SerializeField] GameObject MuzzleFlash;

	// AimDownSights Controller
		[Header("Aim / Hip")]
	[Tooltip("FOV Of The Main Camera When In Aimed Down Sights")] [SerializeField] int PlayerAimFOV = 60;
	[Tooltip("FOV Of The Gun Camera When In Aimed Down Sights")] [SerializeField] int GunAimFOV = 40;
		[Space(6)]
	[Tooltip("FOV Of The Main Camera When In Hip Fire Mode")] [SerializeField] int PlayerHipFOV = 90;
	[Tooltip("FOV Of the Gun Camera Whne In Hip Fire Mode")] [SerializeField] int GunHipFOV = 60;
		[Space(6)]
	[Tooltip("Your Main Camera")] [SerializeField] Camera PlayerCam;
	[Tooltip("Camera Your Gun Is Attached To Except The Main Camera (Keep Blank If You Don't Have One)")] [SerializeField] Camera GunCam;
	// Sounds
		[Header("Sounds")]
	[Tooltip("AudioSource That Will Play The Sounds")] [SerializeField] AudioSource audioSource;
	[Tooltip("AudioClip That Is Played When The Gun Is Fired")] [SerializeField] AudioClip FireSound;
	[Tooltip("AudioClip That Is Played When The Gun Is Relaoded")] [SerializeField] AudioClip ReloadSound;

	// Animations
		[Header("Animations")]
	[Tooltip("Animation Played To Aim Down Sights")] [SerializeField] AnimationClip AimAnim;
	[Tooltip("Animation Played To Leave Aim Down Sights")] [SerializeField] AnimationClip DeAimAnim;
	[Tooltip("Animation Played When Gun Is Reloading")] [SerializeField] AnimationClip ReloadAnim;

	// Gun Stats
		[Space(10)]
		[Header("Gun Stats")] 
	[Tooltip("Maximum Amount Of Damage Per Shot")] [SerializeField] int MaxDamage = 15;
	[Tooltip("Minimum Amount Of Damage Per Shot")] [SerializeField] int MinDamage = 10;
	[Tooltip("Firing Distance")] [SerializeField] int Distance = 500;
	[Tooltip("Guns Durability. Keep At 0 If You Dont Want Durability")] [SerializeField] int Durability = 1500;
	[Tooltip("Ammo Per Clip")] [SerializeField] int Ammo = 30;
	[Tooltip("Amount Of Clips To Start With")] [SerializeField] int Clips = 3;
	[Tooltip("Guns Firing Capabilities")] [SerializeField] firemode FireMode = firemode.Semi; 
	[Tooltip("Guns Fire Rate")] [SerializeField] float FireRate = 0.3f;
	[Tooltip("How Long The Reload Takes")] [SerializeField] float ReloadSpeed = 3;
		[Space(10)]
		[Header("Spread")] 
	public TwoFloatValues HipSpread = new TwoFloatValues();
		[Space(5)]
	public TwoFloatValues AimSpread = new TwoFloatValues();

	//Keys
		[Space(10)]
		[Header("KeyBindings")] 
	public KeyCode shoot = KeyCode.Mouse0;
	public KeyCode aim = KeyCode.Mouse1;
	public KeyCode reload = KeyCode.R;


	// Hide In Inspector
	int OverAllAmmo;
	int curAmmo;
	bool CanFire = true;
	bool Reloading = false;
	bool Aiming = false;

	void Awake ()
	{
		Aiming = false;
		CanFire = true;
		curAmmo = Ammo;
		OverAllAmmo = Clips * Ammo;
	}
	
	void Update ()
	{
		if(Input.GetKeyDown(aim) && AimAnim != null && DeAimAnim != null && Reloading == false)
		{
			Aiming = !Aiming;
		}
		if(Aiming && AimAnim != null && DeAimAnim != null)
		{
			gameObject.GetComponent<Animation>().CrossFade(AimAnim.name, 0.5f);
			if(PlayerCam != null)
			{
				PlayerCam.fieldOfView = PlayerAimFOV;
			}
			if(GunCam != null)
			{
				GunCam.fieldOfView = GunAimFOV;
			}
		}
		else if(!Aiming && AimAnim != null && DeAimAnim != null && Reloading == false)
		{
			gameObject.GetComponent<Animation>().CrossFade(DeAimAnim.name, 1f);
			if(PlayerCam != null)
			{
				PlayerCam.fieldOfView = PlayerHipFOV;
			}
			if(GunCam != null)
			{
				GunCam.fieldOfView = GunHipFOV;
			}
		}

		if(AmmoHud != null)
		{
			AmmoHud.text = curAmmo + " / " + OverAllAmmo;
		}

		if(
			FireMode != firemode.BurstAuto || 
			FireMode != firemode.FullAuto || 
			FireMode != firemode.BurstSemiAuto || 
			FireMode != firemode.FullAutoShotgun || 
			FireMode != firemode.SemiAuto
			)
		{

			if(Input.GetKeyDown(reload) && curAmmo < Ammo && OverAllAmmo > 0 && !Reloading)
			{
				StartCoroutine(Reload());
			}

			if(Input.GetKey(shoot) && CanFire && curAmmo > 0 && !Reloading)
			{
				if(FireMode == firemode.FullAuto)
				{
					Shoot("FullAuto");
				}
				else if(FireMode == firemode.FullAutoShotgun)
				{
					Shoot("FullAutoShotgun");
				}
			}
			if(Input.GetKeyDown(shoot) && CanFire && curAmmo > 0)
			{
				if(FireMode == firemode.Burst)
				{
					Shoot("Burst");
				}
				else if(FireMode == firemode.Semi)
				{
					Shoot("Semi");
				}
				else if(FireMode == firemode.BoltAction)
				{
					Shoot("BoltAction");
				}
				else if(FireMode == firemode.PumpActionShotgun)
				{
					Shoot("PumpActionShotgun");
				}
			}
		}
	}


	void Shoot (string FireType)
	{
		Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2 + Random.Range(-HipSpread.min,HipSpread.Max), Screen.height / 2 + Random.Range(-HipSpread.min,HipSpread.Max)));
		if(!Aiming)
		{
			ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2 + Random.Range(-HipSpread.min,HipSpread.Max), Screen.height / 2 + Random.Range(-HipSpread.min,HipSpread.Max)));
		}
		else if(Aiming)
		{
			ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2 + Random.Range(-AimSpread.min,AimSpread.Max), Screen.height / 2 + Random.Range(-AimSpread.min,AimSpread.Max)));
		}
		RaycastHit hit = new RaycastHit();
		CanFire = false;
		if(FireType == "Semi")
		{
			if(Physics.Raycast(ray,out hit,Distance,Physics.DefaultRaycastLayers))
			{
				Debug.DrawLine(ray.origin, hit.point);
			}
			curAmmo--;
			StartCoroutine(FRate());

		}

		if(FireType == "BurstFire")
		{
			if(Physics.Raycast(ray,out hit,Distance,Physics.DefaultRaycastLayers))
			{
				Debug.DrawLine(ray.origin, hit.point);
			}
			curAmmo--;
		}

		if(FireType == "Burst")
		{
			StartCoroutine(Burst());
		}

		if(FireType == "FullAuto")
		{
			if(Physics.Raycast(ray,out hit,Distance,Physics.DefaultRaycastLayers))
			{
				Debug.DrawLine(ray.origin, hit.point);
			}
			else
			{

			}
			curAmmo--;
			StartCoroutine(FRate());
		}

		if(FireSound != null && audioSource != null)
		{
			audioSource.clip = FireSound;
			audioSource.Play();
		}

		if(MuzzleFlash != null)
		{
			StartCoroutine(Muzzle());
		}

		if(HitParticle.Count > 0 && hit.point != null && hit.transform != null)
		{
			Instantiate(HitParticle[Random.Range(0,HitParticle.Count)].HitParticle, hit.point, Quaternion.FromToRotation(Vector3.up,hit.normal));
		}

		if(BulletHit != null && hit.transform != null)
		{ 
			GameObject BulletHitObj = Instantiate(BulletHit[Random.Range(0,BulletHit.Count)].HitObject, hit.point, Quaternion.FromToRotation(Vector3.up,hit.normal)) as GameObject;
			BulletHitObj.transform.TransformDirection(new Vector3(0.1f,0.1f,0.1f));
			BulletHitObj.transform.parent = hit.collider.transform;
		}

		if(hit.transform != null && hit.transform.GetComponent<Rigidbody>())
		{
			hit.collider.GetComponent<Rigidbody>().AddForce(transform.TransformDirection(Vector3.forward)*600);
		}
	}

	//IEnumerators
	
	IEnumerator Burst()
	{
		Shoot("BurstFire");
		yield return new WaitForSeconds(FireRate / 2);
		Shoot("BurstFire");
		yield return new WaitForSeconds(FireRate / 2);
		Shoot("BurstFire");
		yield return new WaitForSeconds(FireRate / 2);
		CanFire = true;
		StopCoroutine(Burst());
	}
	
	IEnumerator FRate(){
		yield return new WaitForSeconds(FireRate);
		CanFire = true;
		StopCoroutine(FRate());
	}
	
	IEnumerator Reload()
	{
		if(Aiming)
		{
			Aiming = false;
			if(PlayerCam != null)
			{
				PlayerCam.fieldOfView = PlayerHipFOV;
			}
			if(GunCam != null)
			{
				GunCam.fieldOfView = GunHipFOV;
			}
		}
		Reloading = true;
		CanFire = false;
		if(ReloadSound != null && audioSource != null)
		{
			audioSource.clip = ReloadSound;
			audioSource.Play();
		}
		if(ReloadAnim != null)
		{
			gameObject.GetComponent<Animation>().CrossFade(ReloadAnim.name);
		}
		yield return new WaitForSeconds(ReloadSpeed);
		if(curAmmo != 0)
		{
			Clips--;
			OverAllAmmo = OverAllAmmo - 30 + curAmmo;
			curAmmo = Ammo;
		}
		else
		{
			// 20/90
			Clips--;
			OverAllAmmo = OverAllAmmo - 30;
			curAmmo = Ammo;
		}
		CanFire = true;
		Reloading = false;
		StopCoroutine(Reload());
	}

	IEnumerator Muzzle()
	{
		MuzzleFlash.SetActive(true);
		yield return new WaitForSeconds(0.1f);
		MuzzleFlash.SetActive(false);
		StopCoroutine(Muzzle());
	}

}



// Classes

[System.Serializable]
public class bulletHit
{
	public GameObject HitObject;
	public LayerMask Layer; 
}

[System.Serializable]
public class hitParticle
{
	public GameObject HitParticle;
	public LayerMask Layer;
}

[System.Serializable]
public class TwoFloatValues
{
	public float min;
	public float Max;
}

[System.Serializable]
public class TwoIntValues
{
	public int min;
	public int Max;
}


