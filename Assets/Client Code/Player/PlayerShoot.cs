using UnityEngine;

public struct ShootSnapshot
{
    public int Tick;
}

public class PlayerShoot : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        ClientSend.PlayerShoot(new()
        {
            Tick = Client.Instance.serverTick
        });
    }
}