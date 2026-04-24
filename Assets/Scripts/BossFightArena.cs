using UnityEngine;
using Cinemachine;

public class BossFightArena : MonoBehaviour
{
    [Header("Paredes")]
    [SerializeField] private Collider2D leftWall;
    [SerializeField] private Collider2D rightWall;

    [Header("Boss")]
    [SerializeField] private GuerreroDeCera boss;

    [Header("Camara")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CinemachineConfiner2D confiner2D;
    [SerializeField] private Collider2D normalCameraBounds;
    [SerializeField] private Collider2D bossRoomCameraBounds;

    [Header("Opciones")]
    [SerializeField] private bool openWallsWhenBossDies = true;
    [SerializeField] private bool unlockCameraWhenBossDies = true;
    [SerializeField] private bool disableTriggerAfterActivation = true;

    private bool activated;
    private bool bossDefeatedHandled;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated)
            return;

        if (!other.CompareTag("Player"))
            return;

        activated = true;

        CloseWalls();
        LockCameraToBossRoom();

        if (disableTriggerAfterActivation)
        {
            Collider2D ownCollider = GetComponent<Collider2D>();
            if (ownCollider != null)
                ownCollider.enabled = false;
        }
    }

    private void Update()
    {
        if (!activated || bossDefeatedHandled || boss == null)
            return;

        if (boss.IsDead())
            HandleBossDefeated();
    }

    private void CloseWalls()
    {
        if (leftWall != null)
            leftWall.isTrigger = false;

        if (rightWall != null)
            rightWall.isTrigger = false;
    }

    private void OpenWalls()
    {
        if (leftWall != null)
            leftWall.isTrigger = true;

        if (rightWall != null)
            rightWall.isTrigger = true;
    }

    private void LockCameraToBossRoom()
    {
        if (confiner2D == null || bossRoomCameraBounds == null)
            return;

        confiner2D.m_BoundingShape2D = bossRoomCameraBounds;
        confiner2D.InvalidateCache();
    }

    private void UnlockCamera()
    {
        if (confiner2D == null || normalCameraBounds == null)
            return;

        confiner2D.m_BoundingShape2D = normalCameraBounds;
        confiner2D.InvalidateCache();
    }

    private void HandleBossDefeated()
    {
        bossDefeatedHandled = true;

        if (openWallsWhenBossDies)
            OpenWalls();

        if (unlockCameraWhenBossDies)
            UnlockCamera();
    }
}