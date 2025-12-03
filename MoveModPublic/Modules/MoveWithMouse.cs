using Hazel;
using MoveModPublic.Extensions;
using UnityEngine;

namespace MoveModPublic.Modules;

public class MoveWithMouse : MonoBehaviour
{
    private PlayerControl selectedPlayer;
    private bool isDragging;
    private int fixedUpdateCounter = 0;
    private int fixedUpdateSkipRate => MVPlugin.Instance?.MoveRateLimit?.Value ?? 0;
    private int activeTouchId = -1;
    private Vector2 touchPosition;

    public void Update()
    {
        if (PlayerControl.LocalPlayer == null)
        {
            return;
        }

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            HandleTouchDrag();
        }
        else
        {
            HandleMouseDrag();
        }

        if (isDragging && selectedPlayer != null)
        {
            DragObject(false, touchPosition);
        }
    }

    private void HandleMouseDrag()
    {
        isDragging = Input.GetMouseButton(1) && PlayerControl.LocalPlayer.CanMove;
        touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(1))
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                player.GetComponent<BoxCollider2D>().enabled = true;
            }
            StartDragging(touchPosition);
        }

        if (Input.GetMouseButtonUp(1))
        {
            StopDragging();
        }
    }

    private void HandleTouchDrag()
    {
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                touchPosition = Camera.main.ScreenToWorldPoint(touch.position);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        if (activeTouchId == -1)
                        {
                            StartDragging(touchPosition, touch.fingerId);
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (touch.fingerId == activeTouchId)
                        {
                            StopDragging();
                        }
                        break;
                }
            }
        }
    }

    public void FixedUpdate()
    {
        fixedUpdateCounter++;

        if (fixedUpdateCounter >= fixedUpdateSkipRate)
        {
            fixedUpdateCounter = 0;
            if (isDragging && selectedPlayer != null)
            {
                DragObject(true, touchPosition);
            }
        }
    }

    public void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            StopDragging();
        }
    }

    public void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            StopDragging();
    }

    void StartDragging(Vector3 position, int touchId = -1)
    {
        Vector3 mousePosition = position;
        mousePosition.z = 0f;

        Collider2D[] hitColliders = Physics2D.OverlapPointAll(mousePosition);

        foreach (Collider2D hitCollider in hitColliders)
        {
            PlayerControl playerControl = hitCollider.GetComponent<PlayerControl>();
            if (playerControl != null && ((AmongUsClient.Instance.AmHost && !MVConstants.DisableModdedProtocol) || TutorialManager.InstanceExists))
            {
                if (playerControl.walkingToVent || playerControl.onLadder)
                {
                    continue;
                }
                playerControl.ToggleHighlight(true, RoleTeamTypes.Crewmate);
                selectedPlayer = playerControl;
                activeTouchId = touchId;
                if (selectedPlayer.inVent)
                {
                    selectedPlayer.MyPhysics.RpcBootFromVent(MVPlugin.GetLastVentId(selectedPlayer));
                }
                break;
            }
        }
    }

    void StopDragging()
    {
        try
        {
            selectedPlayer.ToggleHighlight(false, RoleTeamTypes.Crewmate);
        }
        catch{}
        selectedPlayer = null;
        isDragging = false;
        activeTouchId = -1;
    }

    void DragObject(bool networked, Vector2 position)
    {
        if (networked)
        {
            selectedPlayer.NetTransform.RpcTeleport(position);
        }
        else
        {
            if (selectedPlayer.inVent)
            {
                selectedPlayer.MyPhysics.RpcBootFromVent(MVPlugin.GetLastVentId(selectedPlayer));
            }
            selectedPlayer.NetTransform.body.position = position;
            selectedPlayer.transform.position = position;
            selectedPlayer.NetTransform.body.velocity = Vector2.zero;
        }
    }
}