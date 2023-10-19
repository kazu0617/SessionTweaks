using HarmonyLib;
using ResoniteModLoader;
using System;
using FrooxEngine;
using FrooxEngine.UIX;
using Elements.Core;
using SkyFrost.Base;
using System.Collections.Generic;

namespace SessionTweaks
{
    public class SessionTweaks : ResoniteMod
    {
        public override string Name => "SesisonTweaks";
        public override string Author => "kazu0617";
        public override string Version => "3.0.0";
        public override string Link => "https://github.com/kazu0617/SessionTweaks"; // this line is optional and can be omitted

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> Enable = new("enabled", "Enables this mod. this config must re-start all.", () => true);
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> Orb = new("Orb", "Add Orb Button. This button allows to spawn Session Orb from Contacts Tab.", () => true);
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> Open = new("Open", "Add Open Button. This one allows to Join session without focusing.", () => true);
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> Copy = new("Copy", "Add Copy Button. This one allow to Copy session url for paste to external sites.", () => false);

        private static ModConfiguration Config;
        public override void OnEngineInit() {
            Harmony harmony = new Harmony("net.kazu0617.sessiontweaks");
            Config = GetConfiguration();
            if (Config.GetValue(Enable)) harmony.PatchAll();
        }

        [HarmonyPatch(typeof(SessionItem))]
        [HarmonyPatch("Update")]
        class SessionTweaks_SessionItemPatch {
            static void Postfix(SessionItem __instance) {
                if (!Config.GetValue(Enable) || !Config.GetValue(Orb) && !Config.GetValue(Open) && !Config.GetValue(Copy)) return;
                foreach (Slot TargetSlot in __instance.Slot.GetAllChildren()) {
                    if (TargetSlot.GetComponent<Text>() != null && TargetSlot.GetComponent<Text>().Content == "Join") {
                        //Source(Join) Button Resize
                        Slot DuplicatedSlot_Src = TargetSlot.Parent;
                        DuplicatedSlot_Src.GetComponent<RectTransform>().AnchorMin.Value = new float2(0.8f, 0f);

                        //Create Open Button
                        if (Config.GetValue(Open)) {

                            Slot DuplicatedSlot_Open = DuplicatedSlot_Src.Duplicate();
                            DuplicatedSlot_Open.GetComponent<RectTransform>().AnchorMin.Value = new float2(0.6f, 0f);
                            DuplicatedSlot_Open.GetComponent<RectTransform>().AnchorMax.Value = new float2(0.79f, 1f);

                            DuplicatedSlot_Open.GetComponent<Image>().Destroy();
                            Image ImageComponent = DuplicatedSlot_Open.AttachComponent<Image>();
                            ImageComponent.Sprite.Value = DuplicatedSlot_Src.GetComponent<Image>().Sprite.Value;
                            ImageComponent.Tint.Value = DuplicatedSlot_Src.GetComponent<Image>().Tint.Value;
                            ImageComponent.NineSliceSizing.Value = DuplicatedSlot_Src.GetComponent<Image>().NineSliceSizing.Value;
                            ImageComponent.FillRect.Value = DuplicatedSlot_Src.GetComponent<Image>().FillRect.Value;
                            ImageComponent.PreserveAspect.Value = DuplicatedSlot_Src.GetComponent<Image>().PreserveAspect.Value;

                            DuplicatedSlot_Open.GetComponentInChildren<Text>().Color.Value = RadiantUI_Constants.Neutrals.MIDLIGHT;
                            DuplicatedSlot_Open.GetComponentInChildren<Text>().Content.Value = "Open";

                            DuplicatedSlot_Open.GetComponent<Button>().Destroy();
                            DuplicatedSlot_Open.AttachComponent<Button>().LocalPressed += (IButton b, ButtonEventData _) => {
                                SessionInfo sessionInfo = __instance.Engine.Cloud.Sessions.TryGetInfo(__instance.SessionInfo.SessionId) ?? __instance.SessionInfo;
                                World world = __instance.LocalUser.World.WorldManager.FocusedWorld;
                                world.RunSynchronously(() => {
                                    if (sessionInfo.HasEnded)
                                        return;
                                    Userspace.OpenWorld(new WorldStartSettings() {
                                        URIs = sessionInfo.GetSessionURLs(),
                                        HostUserId = sessionInfo.HostUserId,
                                        GetExisting = true,
                                        AutoFocus = false
                                    });
                                });
                            };
                        }
                        
                        //Create Orb Button
                        if (Config.GetValue(Orb))
                        {
                            Slot DuplicatedSlot = DuplicatedSlot_Src.Duplicate();
                            DuplicatedSlot.GetComponent<RectTransform>().AnchorMin.Value = new float2(0.4f, 0f);
                            DuplicatedSlot.GetComponent<RectTransform>().AnchorMax.Value = new float2(0.59f, 1f);

                            DuplicatedSlot.GetComponent<Image>().Destroy();
                            Image ImageComponent = DuplicatedSlot.AttachComponent<Image>();
                            ImageComponent.Sprite.Value = DuplicatedSlot_Src.GetComponent<Image>().Sprite.Value;
                            ImageComponent.Tint.Value = DuplicatedSlot_Src.GetComponent<Image>().Tint.Value;
                            ImageComponent.NineSliceSizing.Value = DuplicatedSlot_Src.GetComponent<Image>().NineSliceSizing.Value;
                            ImageComponent.FillRect.Value = DuplicatedSlot_Src.GetComponent<Image>().FillRect.Value;
                            ImageComponent.PreserveAspect.Value = DuplicatedSlot_Src.GetComponent<Image>().PreserveAspect.Value;

                            DuplicatedSlot.GetComponentInChildren<Text>().Color.Value = RadiantUI_Constants.Neutrals.MIDLIGHT;
                            DuplicatedSlot.GetComponentInChildren<Text>().Content.Value = "Orb";

                            DuplicatedSlot.GetComponent<Button>().Destroy();
                            DuplicatedSlot.AttachComponent<Button>().LocalPressed += (IButton b, ButtonEventData _) => {
                                SessionInfo sessionInfo = __instance.Engine.Cloud.Sessions.TryGetInfo(__instance.SessionInfo.SessionId) ?? __instance.SessionInfo;
                                World world = __instance.LocalUser.World.WorldManager.FocusedWorld;
                                world.RunSynchronously(() => {
                                    if (sessionInfo.HasEnded)
                                        return;
                                    Slot slot = world.RootSlot.LocalUserSpace.AddSlot("World Orb");
                                    WorldOrb worldOrb = slot.AttachComponent<WorldOrb>();
                                    worldOrb.ActiveSessionURLs = sessionInfo.GetSessionURLs();
                                    worldOrb.ActiveUsers.Value = sessionInfo.JoinedUsers;
                                    worldOrb.WorldName = sessionInfo.Name;
                                    worldOrb.ThumbnailTexURL = new Uri(sessionInfo.ThumbnailUrl);
                                    slot.PositionInFrontOfUser();
                                });
                            };
                        }

                        //Create CopySessionURL Button
                        if (Config.GetValue(Copy)) {

                            Slot DuplicatedSlot_Copy = DuplicatedSlot_Src.Duplicate();
                            DuplicatedSlot_Copy.GetComponent<RectTransform>().AnchorMin.Value = new float2(0.2f, 0f);
                            DuplicatedSlot_Copy.GetComponent<RectTransform>().AnchorMax.Value = new float2(0.39f, 1f);

                            DuplicatedSlot_Copy.GetComponent<Image>().Destroy();
                            Image ImageComponent = DuplicatedSlot_Copy.AttachComponent<Image>();
                            ImageComponent.Sprite.Value = DuplicatedSlot_Src.GetComponent<Image>().Sprite.Value;
                            ImageComponent.Tint.Value = DuplicatedSlot_Src.GetComponent<Image>().Tint.Value;
                            ImageComponent.NineSliceSizing.Value = DuplicatedSlot_Src.GetComponent<Image>().NineSliceSizing.Value;
                            ImageComponent.FillRect.Value = DuplicatedSlot_Src.GetComponent<Image>().FillRect.Value;
                            ImageComponent.PreserveAspect.Value = DuplicatedSlot_Src.GetComponent<Image>().PreserveAspect.Value;

                            DuplicatedSlot_Copy.GetComponentInChildren<Text>().Color.Value = RadiantUI_Constants.Neutrals.MIDLIGHT;
                            DuplicatedSlot_Copy.GetComponentInChildren<Text>().Content.Value = "Copy URL";

                            DuplicatedSlot_Copy.GetComponent<Button>().Destroy();
                            DuplicatedSlot_Copy.AttachComponent<Button>().LocalPressed += (IButton b, ButtonEventData _) => {
                                SessionInfo sessionInfo = __instance.SessionInfo;
                                World world = __instance.LocalUser.World.WorldManager.FocusedWorld;
                                world.RunSynchronously(() => {
                                    if (!__instance.InputInterface.IsClipboardSupported)
                                        return;
                                    b.World.InputInterface.Clipboard.SetText(__instance.Cloud.ApiEndpoint + "/open/session/" + sessionInfo.SessionId);
                                });
                            };
                        }
                    }
                }

            }
        }

        [HarmonyPatch(typeof(ContactsDialog))]
        [HarmonyPatch("AddMessage")]
        class SessionTweaks_ContactsDialogPatch {
            static void Postfix(UIBuilder ___messagesUi, ContactsDialog __instance, ref Image __result, Message message) {
                if (!Config.GetValue(Enable) || !Config.GetValue(Orb) && !Config.GetValue(Open) && !Config.GetValue(Copy)) return;

                List<Slot> child = ___messagesUi.Current.GetAllChildren();
                foreach (Slot TargetSlot in child) {
                    if (TargetSlot.GetComponent<Text>() == null || message.MessageType != SkyFrost.Base.MessageType.SessionInvite) continue;

                    if (TargetSlot.GetComponent<Text>().Content == "Join") {
                        //Source(Join) Button Resize
                        Slot DuplicatedSlot_Src = TargetSlot.Parent;
                        DuplicatedSlot_Src.GetComponent<RectTransform>().AnchorMin.Value = new float2(0.8f, 0f);

                        //Create Open Button
                        if(Config.GetValue(Open)) {
                            Slot DuplicatedSlot_Open = TargetSlot.Parent.Duplicate();
                            DuplicatedSlot_Open.GetComponent<RectTransform>().AnchorMin.Value = new float2(0.6f, 0f);
                            DuplicatedSlot_Open.GetComponent<RectTransform>().AnchorMax.Value = new float2(0.79f, 1f);

                            DuplicatedSlot_Open.GetComponent<Image>().Destroy();
                            Image ImageComponent = DuplicatedSlot_Open.AttachComponent<Image>();
                            ImageComponent.Sprite.Value = DuplicatedSlot_Src.GetComponent<Image>().Sprite.Value;
                            ImageComponent.Tint.Value = DuplicatedSlot_Src.GetComponent<Image>().Tint.Value;
                            ImageComponent.NineSliceSizing.Value = DuplicatedSlot_Src.GetComponent<Image>().NineSliceSizing.Value;
                            ImageComponent.FillRect.Value = DuplicatedSlot_Src.GetComponent<Image>().FillRect.Value;
                            ImageComponent.PreserveAspect.Value = DuplicatedSlot_Src.GetComponent<Image>().PreserveAspect.Value;

                            DuplicatedSlot_Open.GetComponentInChildren<Text>().Color.Value = RadiantUI_Constants.Neutrals.MIDLIGHT;
                            DuplicatedSlot_Open.GetComponentInChildren<Text>().Content.Value = "Open";

                            DuplicatedSlot_Open.GetComponent<Button>().Destroy();
                            DuplicatedSlot_Open.AttachComponent<Button>().LocalPressed += (IButton b, ButtonEventData _) =>
                            {
                                SessionInfo sessionInfo = message.ExtractContent<SessionInfo>();
                                World world = __instance.LocalUser.World.WorldManager.FocusedWorld;
                                world.RunSynchronously(() =>
                                {
                                    if (sessionInfo.HasEnded)
                                        return;
                                    Userspace.OpenWorld(new WorldStartSettings()
                                    {
                                        URIs = sessionInfo.GetSessionURLs(),
                                        HostUserId = sessionInfo.HostUserId,
                                        GetExisting = true,
                                        AutoFocus = false
                                    });
                                });
                            };
                        }
                        
                        //Create Orb Button
                        if(Config.GetValue(Orb)) {
                            Slot DuplicatedSlot = TargetSlot.Parent.Duplicate();
                            DuplicatedSlot.GetComponent<RectTransform>().AnchorMin.Value = new float2(0.4f, 0f);
                            DuplicatedSlot.GetComponent<RectTransform>().AnchorMax.Value = new float2(0.59f, 1f);

                            DuplicatedSlot.GetComponent<Image>().Destroy();
                            Image ImageComponent = DuplicatedSlot.AttachComponent<Image>();
                            ImageComponent.Sprite.Value = DuplicatedSlot_Src.GetComponent<Image>().Sprite.Value;
                            ImageComponent.Tint.Value = DuplicatedSlot_Src.GetComponent<Image>().Tint.Value;
                            ImageComponent.NineSliceSizing.Value = DuplicatedSlot_Src.GetComponent<Image>().NineSliceSizing.Value;
                            ImageComponent.FillRect.Value = DuplicatedSlot_Src.GetComponent<Image>().FillRect.Value;
                            ImageComponent.PreserveAspect.Value = DuplicatedSlot_Src.GetComponent<Image>().PreserveAspect.Value;

                            DuplicatedSlot.GetComponentInChildren<Text>().Color.Value = RadiantUI_Constants.Neutrals.MIDLIGHT;
                            DuplicatedSlot.GetComponentInChildren<Text>().Content.Value = "Orb";

                            DuplicatedSlot.GetComponent<Button>().Destroy();
                            DuplicatedSlot.AttachComponent<Button>().LocalPressed += (IButton b, ButtonEventData _) =>
                            {
                                SessionInfo sessionInfo = message.ExtractContent<SessionInfo>();
                                World world = __instance.LocalUser.World.WorldManager.FocusedWorld;
                                world.RunSynchronously(() =>
                                {
                                    Slot slot = world.RootSlot.LocalUserSpace.AddSlot("World Orb");
                                    WorldOrb worldOrb = slot.AttachComponent<WorldOrb>();
                                    worldOrb.ActiveSessionURLs = sessionInfo.GetSessionURLs();
                                    worldOrb.ActiveUsers.Value = sessionInfo.JoinedUsers;
                                    worldOrb.WorldName = sessionInfo.Name;
                                    worldOrb.ThumbnailTexURL = new Uri(sessionInfo.ThumbnailUrl);
                                    slot.PositionInFrontOfUser();
                                });
                            };
                        }
                        
                        //Create Copy Button
                        if(Config.GetValue(Copy)) {

                            Slot DuplicatedSlot_Copy = TargetSlot.Parent.Duplicate();
                            DuplicatedSlot_Copy.GetComponent<RectTransform>().AnchorMin.Value = new float2(0.2f, 0f);
                            DuplicatedSlot_Copy.GetComponent<RectTransform>().AnchorMax.Value = new float2(0.39f, 1f);

                            DuplicatedSlot_Copy.GetComponent<Image>().Destroy();
                            Image ImageComponent = DuplicatedSlot_Copy.AttachComponent<Image>();
                            ImageComponent.Sprite.Value = DuplicatedSlot_Src.GetComponent<Image>().Sprite.Value;
                            ImageComponent.Tint.Value = DuplicatedSlot_Src.GetComponent<Image>().Tint.Value;
                            ImageComponent.NineSliceSizing.Value = DuplicatedSlot_Src.GetComponent<Image>().NineSliceSizing.Value;
                            ImageComponent.FillRect.Value = DuplicatedSlot_Src.GetComponent<Image>().FillRect.Value;
                            ImageComponent.PreserveAspect.Value = DuplicatedSlot_Src.GetComponent<Image>().PreserveAspect.Value;

                            DuplicatedSlot_Copy.GetComponentInChildren<Text>().Color.Value = RadiantUI_Constants.Neutrals.MIDLIGHT;
                            DuplicatedSlot_Copy.GetComponentInChildren<Text>().Content.Value = "Copy URL";

                            DuplicatedSlot_Copy.GetComponent<Button>().Destroy();
                            DuplicatedSlot_Copy.AttachComponent<Button>().LocalPressed += (IButton b, ButtonEventData _) =>
                            {
                                SessionInfo sessionInfo = message.ExtractContent<SessionInfo>();
                                World world = __instance.LocalUser.World.WorldManager.FocusedWorld;
                                world.RunSynchronously(() =>
                                {
                                    if (!__instance.InputInterface.IsClipboardSupported)
                                        return;
                                    b.World.InputInterface.Clipboard.SetText(__instance.Cloud.ApiEndpoint + "/open/session/" + sessionInfo.SessionId);
                                });
                            };
                        }
                    }
                }
            }
        }
    }
}
