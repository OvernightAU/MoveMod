using HarmonyLib;
using UnityEngine;

namespace MoveModPublic.Modules;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public class Zoom
{
    private static bool ResetButtons = false;
    public static bool ShouldKeepZoom
    {
        get
        {
            return !MeetingHud.Instance;
        }
    }
    private static readonly float zoomSpeed = 100f;

    public static void Postfix()
    {
        if (MVPlugin.Instance.IgnoreZoom)
        {
            return;
        }

        if (ShouldKeepZoom && AmongUsClient.Instance.AmHost)
        {
            if (Camera.main.orthographicSize > 3f)
                ResetButtons = true;

            if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.CanMove)
            {
                if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    HandlePinchZoom();
                }
                else
                {
                    HandleScrollZoom();
                }
            }
        }
        else
        {
            SetZoomSize(reset: true);
        }

        if (HudManager.Instance == null) return;

        bool isUhh = Camera.main.orthographicSize < 3.01f;

        if (PlayerControl.LocalPlayer != null)
        {
            HudManager.Instance.ShadowQuad.gameObject.SetActive(isUhh && !PlayerControl.LocalPlayer.Data.IsDead);
        }
    }

    private static void HandleScrollZoom()
    {
        float scrollDelta = Input.mouseScrollDelta.y * -zoomSpeed;
        if (scrollDelta != 0)
        {
            float targetSize = Mathf.Clamp(Camera.main.orthographicSize + scrollDelta * Time.deltaTime, 3f, 18f);
            Camera.main.orthographicSize = targetSize;
            HudManager.Instance.UICamera.orthographicSize = targetSize;
        }
    }

    private static void HandlePinchZoom()
    {
        if (Input.touchCount >= 3)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

            float prevTouchDeltaMag = (touch0PrevPos - touch1PrevPos).magnitude;
            float touchDeltaMag = (touch0.position - touch1.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            if (deltaMagnitudeDiff != 0)
            {
                float targetSize = Mathf.Clamp(Camera.main.orthographicSize + deltaMagnitudeDiff * Time.deltaTime, 3f, 18f);
                Camera.main.orthographicSize = targetSize;
                HudManager.Instance.UICamera.orthographicSize = targetSize;
            }
        }
    }

    private static void SetZoomSize(bool times = false, bool reset = false)
    {
        if (reset)
        {
            Camera.main.orthographicSize = 3.0f;
            HudManager.Instance.UICamera.orthographicSize = 3.0f;
            HudManager.Instance.Chat.transform.localScale = Vector3.one;
        }
        else
        {
            float targetSize = Mathf.Clamp(Camera.main.orthographicSize + (Input.mouseScrollDelta.y > 0 ? -1f : 1f), 3f, 18f);
            float easedSize = EaseInOutQuad(Camera.main.orthographicSize, targetSize, zoomSpeed * Time.deltaTime);
            Camera.main.orthographicSize = easedSize;
            HudManager.Instance.UICamera.orthographicSize = easedSize;
        }

        if (ResetButtons)
        {
            ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
            ResetButtons = false;
        }
    }

    private static float EaseInOutQuad(float start, float end, float value)
    {
        value /= 0.5f;
        end -= start;
        if (value < 1) return end / 2 * value * value + start;
        value--;
        return -end / 2 * (value * (value - 2) - 1) + start;
    }
}