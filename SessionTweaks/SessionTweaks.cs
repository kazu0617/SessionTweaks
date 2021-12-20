using HarmonyLib;
using NeosModLoader;
using System;
using FrooxEngine;
using BaseX;
using CloudX.Shared;
using FrooxEngine.UIX;

namespace SessionTweaks
{
    public class SessionTweaks : NeosMod
    {
        public override string Name => "SesisonTweaks";
        public override string Author => "kazu0617";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/kazu0617/SessionTweaks"; // this line is optional and can be omitted

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("net.kazu0617.sessiontweaks");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(SessionItem))]
        [HarmonyPatch("Update")]
        class SessionTweaksPatch
        {
            static void Postfix(SessionItem __instance)
            {
                foreach (Slot c in __instance.Slot.GetAllChildren())
                {
                    if( c.GetComponent<Text>() != null && c.GetComponent<Text>().Content == "Join")
                    {
                        //Source(Join) Button Resize
                        Slot DuplicatedSlot_Src = c.Parent;
                        DuplicatedSlot_Src.GetComponent<RectTransform>().AnchorMin.Value = new float2(0.8f, 0f);

                        //Create Orb Button
                        Slot DuplicatedSlot_Orb = c.Parent.Duplicate();
                        DuplicatedSlot_Orb.GetComponent<RectTransform>().AnchorMin.Value = new float2(0.6f, 0f);
                        DuplicatedSlot_Orb.GetComponent<RectTransform>().AnchorMax.Value = new float2(0.79f, 1f);

                        DuplicatedSlot_Orb.GetComponent<Button>().Destroy();
                        DuplicatedSlot_Orb.GetComponent<Image>().Destroy();

                        DuplicatedSlot_Orb.AttachComponent<Image>();

                        var newBtn_Orb = DuplicatedSlot_Orb.AttachComponent<Button>();
                        newBtn_Orb.LocalPressed += (IButton b, ButtonEventData _) =>
                        {
                            SessionInfo sessionInfo = __instance.SessionInfo;
                            World world = __instance.LocalUser.World.WorldManager.FocusedWorld;
                            world.RunSynchronously((Action)(() =>
                            {
                                Slot slot = world.RootSlot.LocalUserSpace.AddSlot("World Orb");
                                WorldOrb worldOrb = slot.AttachComponent<WorldOrb>();
                                worldOrb.ActiveSessionURLs = sessionInfo.GetSessionURLs();
                                worldOrb.ActiveUsers.Value = sessionInfo.JoinedUsers;
                                worldOrb.WorldName = sessionInfo.Name;
                                slot.PositionInFrontOfUser();
                            }));
                        };
                        DuplicatedSlot_Orb.GetComponentInChildren<Text>().Content.Value = "Orb";

                        //Create Copy Button
                        Slot DuplicatedSlot_Copy = c.Parent.Duplicate();
                        DuplicatedSlot_Copy.GetComponent<RectTransform>().AnchorMin.Value = new float2(0.4f, 0f);
                        DuplicatedSlot_Copy.GetComponent<RectTransform>().AnchorMax.Value = new float2(0.59f, 1f);

                        DuplicatedSlot_Copy.GetComponent<Button>().Destroy();
                        DuplicatedSlot_Copy.GetComponent<Image>().Destroy();

                        DuplicatedSlot_Copy.AttachComponent<Image>();

                        var newBtn_Copy = DuplicatedSlot_Copy.AttachComponent<Button>();
                        newBtn_Copy.LocalPressed += (IButton b, ButtonEventData _) =>
                        {
                            SessionInfo sessionInfo = __instance.SessionInfo;
                            World world = __instance.LocalUser.World.WorldManager.FocusedWorld;
                            world.RunSynchronously((Action)(() =>
                            {
                                b.World.InputInterface.Clipboard.SetText("neos-session:///" + sessionInfo.SessionId);
                            }));
                        };
                        DuplicatedSlot_Copy.GetComponentInChildren<Text>().Content.Value = "Copy";
                    }
                }

            }
        }
    }
}