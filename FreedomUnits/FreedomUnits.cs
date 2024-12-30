using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;
using UnityEngine;

namespace FreedomUnits
{
    [HarmonyPatch]
    public class FreedomUnits : ModBehaviour
    {
        const float METERS_TO_FEET = 3.280839895f;
        const float FEET_PER_YARD = 3f;
        const float FEET_PER_MILE = 5280f;
        const float FEET_PER_FOOTBALL_FIELD = 300f;

        static bool extraFreedomMode = false;

        void Start()
        {
            ModHelper.Console.WriteLine($"Applying Imperial Measurements", MessageType.Success);

            new Harmony("Hawkbar.FreedomUnits").PatchAll(Assembly.GetExecutingAssembly());
        }

        public override void Configure(IModConfig config)
        {
            extraFreedomMode = ModHelper.Config.GetSettingsValue<bool>("EXTRA FREEDOM MODE");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(CanvasMarker), nameof(CanvasMarker.UpdateDistanceText))]
        static void CanvasMarker_UpdateDistanceText(CanvasMarker __instance)
        {
            if (__instance._stringBuilder == null) return;
            var meters = __instance.GetMarkerDistance();
            var feet = Mathf.Round(meters * METERS_TO_FEET);

            __instance._stringBuilder.Length = 0;
            __instance._stringBuilder.Append(__instance._label);
            __instance._stringBuilder.Append(" ");

            if (extraFreedomMode)
            {
                var footballFields = Mathf.Round(feet / FEET_PER_FOOTBALL_FIELD * 10f) / 10f;
                __instance._stringBuilder.Append(footballFields);
                __instance._stringBuilder.Append(" football fields");
            }
            else if (feet < 100f)
            {
                __instance._stringBuilder.Append(feet);
                __instance._stringBuilder.Append("ft");
            }
            else if (feet < FEET_PER_MILE)
            {
                var yards = Mathf.Round(feet / FEET_PER_YARD);
                __instance._stringBuilder.Append(yards);
                __instance._stringBuilder.Append("yd");
            }
            else if (feet < FEET_PER_MILE * 100f)
            {
                var miles = Mathf.Round(feet / FEET_PER_MILE * 10f) / 10f;
                __instance._stringBuilder.Append(miles);
                __instance._stringBuilder.Append("mi");
            }
            else
            {
                __instance._stringBuilder.Append(" ");
                __instance._stringBuilder.Append("ERROR");
            }

            __instance._mainTextField.text = __instance._stringBuilder.ToString();
            if (__instance._nextMarker != null && __instance._nextMarker.IsVisible())
            {
                __instance._offScreenIndicator.SetText(UITextLibrary.GetString(UITextType.MultipleSignal));
            }
            else
            {
                __instance._offScreenIndicator.SetText(__instance._stringBuilder.ToString());
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ReferenceFrameGUI), nameof(ReferenceFrameGUI.UpdateRelativeMotion))]
        static void ReferenceFrameGUI_UpdateRelativeMotion(ReferenceFrameGUI __instance)
        {
            __instance._stringBuilder.Length = 0;

            var reticuleWithState = __instance.GetReticuleWithState(LockOnReticule.LockState.LOCK);
            if (reticuleWithState == null) return;

            if (__instance._isMapView)
            {
                reticuleWithState.SetReadoutText(string.Empty);
                __instance._offScreenIndicator.SetText(string.Empty);
                return;
            }

            __instance._stringBuilder.Append(__instance._currentReferenceFrame.GetHUDDisplayName());
            if (__instance._stringBuilder.Length > 0)
            {
                __instance._stringBuilder.AppendLine();
            }

            var meters = (__instance._activeBody.GetWorldCenterOfMass() - __instance._currentReferenceFrame.GetPosition()).magnitude;
            var feet = Mathf.Round(meters * METERS_TO_FEET);

            if (extraFreedomMode)
            {
                var footballFields = Mathf.Round(feet / FEET_PER_FOOTBALL_FIELD * 10f) / 10f;
                __instance._stringBuilder.Append(footballFields);
                __instance._stringBuilder.Append(" football fields");
            }
            else if (feet < 100f)
            {
                __instance._stringBuilder.Append(feet);
                __instance._stringBuilder.Append("ft");
            }
            else if (feet < FEET_PER_MILE)
            {
                var yards = Mathf.Round(feet / FEET_PER_YARD);
                __instance._stringBuilder.Append(yards);
                __instance._stringBuilder.Append("yd");
            }
            else
            {
                var miles = Mathf.Round(feet / FEET_PER_MILE * 10f) / 10f;
                __instance._stringBuilder.Append(miles);
                __instance._stringBuilder.Append("mi");
            }

            __instance._stringBuilder.AppendLine();

            var metersPerSecond = __instance._orientedRelativeVelocity.z;
            var feetPerSecond = metersPerSecond * METERS_TO_FEET;
            var milesPerSecond = feetPerSecond / FEET_PER_MILE;
            var milesPerHour = milesPerSecond * 3600f;

            if (extraFreedomMode)
            {
                var fastballsPerMinute = milesPerSecond / 100f * 60f;
                __instance._stringBuilder.Append(Mathf.Round(fastballsPerMinute * 100f) / 100f);
                __instance._stringBuilder.Append(" fastballs per minute");
            }
            else
            {
                __instance._stringBuilder.Append(Mathf.Round(milesPerHour));
                __instance._stringBuilder.Append("mph");
            }

            reticuleWithState.SetReadoutText(__instance._stringBuilder.ToString());
            reticuleWithState.SetColorWithoutAlpha(__instance._reticuleColor);
            if (__instance._offScreenIndicator != null)
            {
                __instance._offScreenIndicator.SetText(__instance._stringBuilder.ToString());
            }
        }
    }

}
