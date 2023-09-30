using System;
using System.Collections.Generic;
using System.Linq;
using GameChat.UI;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ChatImages.Patches
{

    [HarmonyPatch(typeof(UIMessageStackHandler), "HireMessage")]
    internal sealed class UIMessageStackHandlerPatch
    {
        internal static Dictionary<UIChatMessage, float> heights = new Dictionary<UIChatMessage, float>();

        internal static void Postfix(UIChatMessageManager.MessageReference messageRef, RectTransform parent,
            ref UIChatMessage __result)
        {
            switch (messageRef)
            {
                case UIChatMessageManager.ChatMessageReference reference:
                    Debug.Log(reference.ChatMessage);

                    string workingString = reference.ChatMessage.Trim().Replace("\\", "/");

                    if (ChatImagesPlugin.Extensions.Any(workingString.EndsWith)  
                        && Uri.TryCreate(workingString, UriKind.Absolute, out var outUri)
                        && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))
                    {
                        var texts = __result.GetComponentsInChildren<TextMeshProUGUI>();
                        foreach (var text in texts)
                        {
                            text.text = "<size=0>";
                        }

                        var original = __result.transform.GetChild(0);
                        var rec = original.GetComponent<RectTransform>();

                        var g = Object.Instantiate(new GameObject("Handout"));

                        var image = g.AddComponent<Image>();

                        var texture = ChatImagesPlugin.GetRemoteTexture(workingString);

                        var multiplier = 3 * rec.rect.width / texture.width;

                        Rect r = new Rect(0, 0, texture.width, texture.height);
                        Vector2 p = new Vector2(0.5f, 0.5f);

                        image.sprite = Sprite.Create(texture, r, p);
                        g.transform.localScale = new Vector3(multiplier, multiplier);

                        g.transform.SetParent(__result.transform, false);
                        g.SetActive(true);

                        heights[__result] = texture.height / multiplier;

                        __result.UpdateHeight();
                    }
                    
                    break;
                case UIChatMessageManager.DiceResultsReference reference:
                    Debug.Log("Dice Result");
                    break;
                case UIChatMessageManager.EventMessageReference reference:
                    Debug.Log("Event Message");
                    break;
            }


        }
    }

    [HarmonyPatch(typeof(UIChatMessageChat), "UpdateHeight")]
    internal sealed class UIChatMessageChatPatch
    {
        internal static bool Prefix(ref UIChatMessageChat __instance)
        {
            if (__instance.transform.childCount == 4)
            {
                var width = __instance.Rect.sizeDelta.x;
                __instance.Rect.sizeDelta = new Vector2(width, UIMessageStackHandlerPatch.heights[__instance]);
                __instance.Rect.ForceUpdateRectTransforms();
                return false;
            }
            return true;
        }
    }
}