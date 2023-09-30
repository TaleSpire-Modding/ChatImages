using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using LordAshes;
using ModdingTales;
using PluginUtilities;
using Talespire;
using UnityEngine;

namespace ChatImages
{

    [BepInPlugin(Guid, "Chat Images Plugin", Version)]
    [BepInDependency(SetInjectionFlag.Guid)]
    [BepInDependency(ChatServicePlugin.Guid)]
    public class ChatImagesPlugin : BaseUnityPlugin
    {
        // constants
        private const string Guid = "org.hollofox.plugins.ChatImages";
        private const string Version = "0.9.3.0";
        private const string Prefix = "/img";
        internal static List<string> Extensions = new List<string>
        {
            ".png",
            ".jpg",
            ".bmp"
        };

        /// <summary>
        /// Awake plugin
        /// </summary>
        void Awake()
        {
            DoConfig(Config);
            
            // Load PUP
            ModdingUtils.Initialize(this, Logger, "HolloFoxes'");

            ChatServicePlugin.ChatMessageService.AddHandler(Guid,CmsHandler);
            ChatServicePlugin.ChatMessageService.AddHandler(Prefix,CmsHandler);

            var harmony = new Harmony(Guid);
            try
            {
                harmony.PatchAll();
                Debug.Log("Chat Images Plug-in loaded");
            }
            catch (Exception)
            {
                harmony.UnpatchSelf();
                Debug.Log("Chat Images Failed to patch");
            }
        }

        public static void SendImage(string uri)
        {
            ChatServicePlugin.ChatMessageService.SendMessage($"{Guid} {uri}",LocalPlayer.Id.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        private string CmsHandler(string chatMessage, string sender, SourceRole source)
        {

            chatMessage = chatMessage.Replace($"{Guid} ", "").Replace($"{Prefix} ", "").Replace(@"\", "/").Trim();

            if (!chatMessage.StartsWith("http://") && !chatMessage.StartsWith("https://"))
                return null;

            var acceptedExtension = Extensions.Any(chatMessage.EndsWith);
            if (!acceptedExtension)
                return null;
            
            return chatMessage;
        }

        private void DoConfig(ConfigFile config)
        {
            // Descriptions for configs
        }

        internal static Dictionary<string, Texture2D> cachedTexture2Ds = new Dictionary<string, Texture2D>();


        /// <summary>
        /// Downloads an image to a Texture
        /// </summary>
        /// <param name="mediaUrl">URL of the image</param>
        /// <returns></returns>
        internal static Texture2D GetRemoteTexture(string url)
        {
            using (var webClient = new WebClient())
            {
                byte[] imageBytes = webClient.DownloadData(url);
                Texture2D tex = new Texture2D(0,0);
                tex.LoadImage(imageBytes);
                Debug.Log($"Downloaded {url}");
                cachedTexture2Ds[url] = tex;
                return tex;
            }
            return null;
        }
    }
}
