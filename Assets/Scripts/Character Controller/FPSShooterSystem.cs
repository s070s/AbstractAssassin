using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FPSShooterSystem : MonoBehaviour
{
    [Header("Weapon Settings")]
    public List<Weapon> weapons;
    private int currentWeaponIndex = 0;
    public float weaponSwitchDelay = 0.5f;
    private bool isSwitchingWeapon = false;

    [Header("Shooting Settings")]
    public float fireRate = 0.1f;
    private float nextFireTime = 0f;

    [Header("Reloading Settings")]
    public float reloadTime = 2f;
    private bool isReloading = false;

    [Header("Grenade Settings")]
    public GameObject grenadePrefab;
    public float throwForce = 15f;
    public float grenadeDelay = 3f;
    public float grenadeDamage = 100f;
    public float grenadeRadius = 5f;

    [Header("Melee Settings")]
    public float meleeRange = 2f;
    public int meleeDamage = 50;
    public float meleeCooldown = 0.5f;
    private float nextMeleeTime = 0f;

    [Header("Aiming Settings")]
    public float normalFOV = 60f;
    public float aimedFOV = 40f;
    public float aimSpeed = 10f;

    [Header("References")]
    public Camera playerCamera;
    public Transform weaponHolder;
    Vector3 weaponHolderPosition;
    public Image crosshair;

    public GameObject impactEffect; // Impact effect
    public ParticleSystem muzzleFlash; // Muzzle flash effect

    private void Start()
    {
        weaponHolderPosition = weaponHolder.localPosition;
        if (weapons.Count > 0)
        {
            SwitchWeapon(0);
        }
    }

    private void Update()
    {

        if (isReloading) return;

        if (Input.GetKeyDown(KeyCode.Q) && !isSwitchingWeapon)
        {
            StartCoroutine(SwitchWeaponCoroutine());
        }

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime && !isReloading)
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            StartCoroutine(Reload());
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            ThrowGrenade();
        }

        if (Input.GetKeyDown(KeyCode.E) && Time.time >= nextMeleeTime)
        {
            MeleeAttack();
        }

        if (Input.GetButton("Fire2"))
        {
            AimDownSights(true);
        }
        else
        {
            AimDownSights(false);
        }
    }

    private void Shoot()
    {
        if (weapons[currentWeaponIndex].currentClipAmmo > 0)
        {
            weapons[currentWeaponIndex].currentClipAmmo--;
            nextFireTime = Time.time + fireRate;

            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }

            RaycastHit hit;
            if (Physics.Raycast(playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2)), out hit, weapons[currentWeaponIndex].range))
            {
                Debug.Log("Hit: " + hit.transform.name);
                if (impactEffect != null)
                {
                    Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                }
                HealthManager healthManager = hit.transform.GetComponent<HealthManager>();
                if (healthManager != null)
                {
                    healthManager.TakeDamage(weapons[currentWeaponIndex].damage);
                }

            }
        }
        else
        {
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        yield return new WaitForSeconds(reloadTime);

        weapons[currentWeaponIndex].currentClipAmmo = weapons[currentWeaponIndex].maxClipAmmo;
        isReloading = false;
        Debug.Log("Reloaded!");
    }

    private IEnumerator SwitchWeaponCoroutine()
    {
        isSwitchingWeapon = true;
        yield return new WaitForSeconds(weaponSwitchDelay);

        currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Count;
        SwitchWeapon(currentWeaponIndex);

        isSwitchingWeapon = false;
    }

    private void SwitchWeapon(int index)
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i].gameObject.SetActive(i == index);
        }
    }

    private void ThrowGrenade()
    {
        GameObject grenade = Instantiate(grenadePrefab, playerCamera.transform.position, playerCamera.transform.rotation);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);

        StartCoroutine(ExplodeGrenade(grenade));
    }
    ///Work on the visuals of the Grenade explosion
    private IEnumerator ExplodeGrenade(GameObject grenade)
    {
        yield return new WaitForSeconds(grenadeDelay);

        Collider[] hitColliders = Physics.OverlapSphere(grenade.transform.position, grenadeRadius);
        foreach (var hitCollider in hitColliders)
        {
            HealthManager healthManager = hitCollider.GetComponent<HealthManager>();
            if (healthManager != null)
            {
                float distance = Vector3.Distance(grenade.transform.position, hitCollider.transform.position);
                float damageMultiplier = 1 - (distance / grenadeRadius);
                float damage = grenadeDamage * damageMultiplier;
                healthManager.TakeDamage(damage);
            }
        }

        Destroy(grenade);
    }

    private void MeleeAttack()
    {
        nextMeleeTime = Time.time + meleeCooldown;

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, meleeRange))
        {
            Debug.Log("Melee hit: " + hit.transform.name);
            HealthManager healthManager = hit.transform.GetComponent<HealthManager>();
            if (healthManager != null)
            {
                healthManager.TakeDamage(meleeDamage);
            }
        }
    }

    private void AimDownSights(bool isAiming)
    {
        float targetFOV = isAiming ? aimedFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * aimSpeed);
        Vector3 targetPosition = weaponHolderPosition;
        if (isAiming)
        {
            targetPosition = weaponHolderPosition + Vector3.right * -0.4f + Vector3.up * -0.05f;
        }
        else
        {
            targetPosition = weaponHolderPosition;
        }

        weaponHolder.localPosition = Vector3.Lerp(weaponHolder.localPosition, targetPosition, Time.deltaTime * aimSpeed);
    }
}

[System.Serializable]
public class Weapon
{
    public string name;
    public GameObject gameObject;
    public int currentClipAmmo;
    public int maxClipAmmo;
    public float range;
    public float damage;
}