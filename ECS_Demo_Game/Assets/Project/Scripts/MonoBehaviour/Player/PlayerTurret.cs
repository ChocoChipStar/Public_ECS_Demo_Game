using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTurret : MonoBehaviour
{
    [SerializeField]
    private GameObject bulletPrfab = null;

    [SerializeField]
    private Transform bulletStorageTrans = null;

    [SerializeField]
    private Transform[] GunPortTrans = new Transform[0];

    private float measureTime = 0.0f;

    private const float GenerateInterval = 0.025f;

    private void FixedUpdate()
    {
        if(!Keyboard.current.spaceKey.isPressed)
        {
            return;
        }

        measureTime += Time.deltaTime;
        if (measureTime <= GenerateInterval)
        {
            return;
        }
        measureTime = 0.0f;

        GenerateBullet();
    }

    private void GenerateBullet()
    {
        for (int i = 0; i < GunPortTrans.Length; i++)
        {
            var isActivated = false;
            foreach (Transform transform in bulletStorageTrans)
            {
                if (!transform.gameObject.activeSelf)
                {
                    transform.SetPositionAndRotation(GunPortTrans[i].position, GunPortTrans[i].rotation);
                    transform.gameObject.SetActive(true);
                    isActivated = true;
                    MonoBehaviourCountUp.instance.AddBulletCount();
                    break;
                }
            }

            if(isActivated)
            {
                continue;
            }
            
            var instance = Instantiate(bulletPrfab, GunPortTrans[i].position, GunPortTrans[i].rotation);
            instance.transform.SetParent(bulletStorageTrans);
            MonoBehaviourCountUp.instance.AddBulletCount();
        }
    }
}
