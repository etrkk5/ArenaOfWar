using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.CrossPlatformInput;



[RequireComponent (typeof (WeaponManager))]
public class PlayerShoot : NetworkBehaviour {

    private const string PLAYER_TAG = "Player";

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private LayerMask mask;

    private PlayerWeapon currentWeapon;
    private WeaponManager weaponManager;

    void Start()
    {
        if (cam == null)
        {
            Debug.LogError("PlayerShoot: No camera referenced!");
            this.enabled = false;
        }

        weaponManager = GetComponent<WeaponManager>();
    }

    void Update()
    {
        currentWeapon = weaponManager.GetCurrentWeapon();

        if (PauseMenu.IsOn)
            return;

        if (currentWeapon.fireRate <= 0f)
        {
            if (CrossPlatformInputManager.GetButtonDown("rightButton"))
            {
                Shoot();
            }
        }
        else
        {
            if (CrossPlatformInputManager.GetButtonDown("rightButton"))
            {
                InvokeRepeating("Shoot", 0f, 1f / currentWeapon.fireRate);
            } else if(CrossPlatformInputManager.GetButtonUp("rightButton"))
            {
                CancelInvoke("Shoot");
            }
        }
    }
    
    // Called on the server when a player shoots.
    [Command]
    void CmdOnShoot()
    {
        RpcDoShootEffect();
    }

    // Called on all clients when we need to do a shoot effect.
    [ClientRpc]
    void RpcDoShootEffect()
    {
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();
    }

    // Called on the server when we hit something takes in the hit point an the normal of the surface.
    [Command]
    void CmdOnHit(Vector3 _pos, Vector3 _normal)
    {
        RpcDoHitEffect(_pos, _normal);
    }

    // Called on all clients, here we can spawn in cool effects.
    [ClientRpc]
    void RpcDoHitEffect(Vector3 _pos, Vector3 _normal)
    {
        GameObject _hitEffect = (GameObject) Instantiate(weaponManager.GetCurrentGraphics().hitEffectPrefab, _pos, Quaternion.LookRotation(_normal));
        Destroy(_hitEffect, 2f);
    }

    [Client]
    void Shoot()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        // Shooting, call the OnShoot method on the server.
        CmdOnShoot();

        RaycastHit _hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out _hit, currentWeapon.range, mask))
        {
            if (_hit.collider.tag == PLAYER_TAG)
            {
                CmdPlayerShot(_hit.collider.name, currentWeapon.damage);
            }

            // We hit something, call the OnHit method on the server.
            CmdOnHit(_hit.point, _hit.normal);
        }
    }

    [Command]
    void CmdPlayerShot(string _playerID, int _damage)
    {
        Debug.Log(_playerID + " has been shot.");

        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage);
    }

}
