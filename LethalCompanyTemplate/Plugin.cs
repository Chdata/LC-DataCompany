//#define PUBLIC_RELEASE

using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DataCompanyMod.Patches;
using DataCompanyMod.ScrapItems;
using HarmonyLib;
using UnityEngine;

namespace DataCompanyMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);

        internal static new ManualLogSource Logger;

        private ConfigEntry<int> configStaleBreadRarity;

        public static AssetBundle ChdataAssets;
        public static Item StaleBreadItem;

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;

            Logger.LogInfo("DATA COMPANY LOADED");
            Logger.LogInfo("DATA COMPANY LOADED");
            Logger.LogInfo("DATA COMPANY LOADED");

            configStaleBreadRarity = Config.Bind("Settings", "Stale Bread Rarity", 30, new ConfigDescription("How rare Stale Bread is to find. Lower values = More rare. 0 to MAYBE prevent spawning. Normal range is 0-100.", new AcceptableValueRange<int>(0, 10000)));

            LoadAssets();

            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(Cheats));
            harmony.PatchAll(typeof(StaleBread));
        }

        private void LoadAssets()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "datacompany");
            Logger.LogDebug(path);

            ChdataAssets = AssetBundle.LoadFromFile(path);
            if (ChdataAssets == null)
            {
                Logger.LogError("Failed to load Chdata's Assets Bundle.");
            }
            else
            {
                Logger.LogDebug("Succeeded in loading Chdata Assets Bundle.");
            }

            StaleBreadItem = ChdataAssets.LoadAsset<Item>("Assets/Import/StaleBread/StaleBread.asset");
            if (StaleBreadItem == null)
            {
                Logger.LogError("Failed to load Stale Bread item.");
            }
            else
            {
                Logger.LogDebug("Succeeded in loading Stale Bread item.");
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(StaleBreadItem.spawnPrefab);
                LethalLib.Modules.Items.RegisterScrap(StaleBreadItem, configStaleBreadRarity.Value, LethalLib.Modules.Levels.LevelTypes.All);
            }
        }




        // public static void SpawnBreadAtLocation(Vector3 spawnPos)
        // {
        //     //RoundManager.SpawnScrapInLevel()
        // 
        //     // Move it a little bit above the teleporter, otherwise it spawns underneath it.
        //     spawnPos.y += 1f;
        // 
        //     GameObject staleBreadObject = Object.Instantiate<GameObject>(StaleBreadItem.spawnPrefab, spawnPos, Quaternion.identity); // , RoundManager.spawnedScrapContainer
        // 
        //     GrabbableObject grabbableBread = staleBreadObject.GetComponent<GrabbableObject>();
        // 
        //     grabbableBread.transform.localPosition = Vector3.zero;
        //     grabbableBread.transform.localRotation = Quaternion.identity;
        //     grabbableBread.transform.localScale = Vector3.one;
        // 
        //     grabbableBread.transform.position = spawnPos;
        //     grabbableBread.transform.rotation = Quaternion.Euler(grabbableBread.itemProperties.restingRotation);
        //     grabbableBread.fallTime = 0f;
        //     grabbableBread.scrapValue = Random.Range(2, 7); // Value of 1 gets treated as 0 apparently?
        // 
        //     NetworkObject networkedBread = staleBreadObject.GetComponent<NetworkObject>();
        //     networkedBread.Spawn();
        // 
        //     DropGrabbableObjectAtPosition(grabbableBread, spawnPos);
        // 
        //     List<int> listValue = [];
        //     List<NetworkObjectReference> listNetwork = [];
        // 
        //     listValue.Add(grabbableBread.scrapValue);
        //     listNetwork.Add(networkedBread);
        // 
        //     grabbableBread.StartCoroutine(WaitForScrapToSpawnToSync(listNetwork.ToArray(), listValue.ToArray()));
        //     //RoundManager.Instance.SyncScrapValuesClientRpc(listNetwork.ToArray(), listValue.ToArray());
        // 
        //     Logger.LogInfo("Spawn New Bread 5");
        // }
        // 
        // // This is the bare minimum needed to spawn something according to ItemDropShip.OpenShipDoorsOnServer()
        // public static void DropItem(Vector3 spawnPos)
        // {
        //     GameObject obj = Object.Instantiate(StaleBreadItem.spawnPrefab, spawnPos, Quaternion.identity);
        //     obj.GetComponent<GrabbableObject>().fallTime = 0f;
        //     obj.GetComponent<NetworkObject>().Spawn();
        // }
        // 
        // 
        // 
        

        // public static void DropAndTeleportStaleBread(PlayerControllerB __instance, Vector3 teleportPos, bool itemsFall = true, bool disconnecting = false)
        // {
        //     for (int i = 0; i < __instance.ItemSlots.Length; i++)
        //     {
        //         GrabbableObject grabbableObject = __instance.ItemSlots[i];
        //         if (!((UnityEngine.Object)(object)grabbableObject != null))
        //         {
        //             continue;
        //         }
        // 
        //         //Logger.LogInfo(grabbableObject.GetType().ToString()); // PhysicsProp / KeyItem
        //         Logger.LogInfo(grabbableObject.itemProperties.itemName);
        // 
        //         if (grabbableObject.itemProperties.itemName != "Stale bread")
        //         {
        //             continue;
        //         }
        // 
        //         // This prevents the bread from teleporting below the floor.
        //         Vector3 adjustedTelePos = teleportPos;
        //         adjustedTelePos.y += 0.2f;
        // 
        //         // This is handled a little further below itemsFall
        //         //grabbableObject.transform.position = adjustedTelePos;
        //         //targetFloorPosition;
        //         //startFallingPosition;
        // 
        //         if (itemsFall)
        //         {
        //             grabbableObject.parentObject = null;
        //             grabbableObject.heldByPlayerOnServer = false;
        //             if (__instance.isInElevator)
        //             {
        //                 ((Component)(object)grabbableObject).transform.SetParent(__instance.playersManager.elevatorTransform, worldPositionStays: true);
        //             }
        //             else
        //             {
        //                 ((Component)(object)grabbableObject).transform.SetParent(__instance.playersManager.propsContainer, worldPositionStays: true);
        //             }
        // 
        //             __instance.SetItemInElevator(__instance.isInHangarShipRoom, __instance.isInElevator, grabbableObject);
        //             grabbableObject.EnablePhysics(enable: true);
        //             grabbableObject.EnableItemMeshes(enable: true);
        //             ((Component)(object)grabbableObject).transform.localScale = grabbableObject.originalScale;
        //             grabbableObject.isHeld = false;
        //             grabbableObject.isPocketed = false;
        //             grabbableObject.startFallingPosition = adjustedTelePos; // ((Component)(object)grabbableObject).transform.parent.InverseTransformPoint(((Component)(object)grabbableObject).transform.position);
        //             grabbableObject.FallToGround(randomizePosition: true);
        //             grabbableObject.fallTime = UnityEngine.Random.Range(-0.3f, 0.05f);
        //             if (__instance.IsOwner)
        //             {
        //                 grabbableObject.DiscardItemOnClient();
        //             }
        //             else if (!grabbableObject.itemProperties.syncDiscardFunction)
        //             {
        //                 grabbableObject.playerHeldBy = null;
        //             }
        //         }
        // 
        //         if (__instance.IsOwner && !disconnecting)
        //         {
        //             ((Behaviour)(object)HUDManager.Instance.holdingTwoHandedItem).enabled = false;
        //             ((Behaviour)(object)HUDManager.Instance.itemSlotIcons[i]).enabled = false;
        //             HUDManager.Instance.ClearControlTips();
        //             __instance.activatingItem = false;
        //         }
        // 
        //         __instance.ItemSlots[i] = null;
        //     }
        // 
        //     // if (__instance.isHoldingObject)
        //     // {
        //     //     __instance.isHoldingObject = false;
        //     //     if ((UnityEngine.Object)(object)__instance.currentlyHeldObjectServer != null)
        //     //     {
        //     //         MethodInfo methodInfo = __instance.GetType().GetMethod("SetSpecialGrabAnimationBool", BindingFlags.NonPublic | BindingFlags.Instance);
        //     //         methodInfo.Invoke(__instance, new object[] { false, __instance.currentlyHeldObjectServer });
        //     //     }
        //     // 
        //     //     __instance.playerBodyAnimator.SetBool("cancelHolding", value: true);
        //     //     __instance.playerBodyAnimator.SetTrigger("Throw");
        //     // }
        //     // 
        //     // __instance.twoHanded = false;
        //     // __instance.carryWeight = 1f;
        //     // __instance.currentlyHeldObjectServer = null;
        // }
    }
}




//public static bool PlayerHasGrabbableObjectInItemSlots(PlayerControllerB thePlayer, GrabbableObject findGrabbableObject)
//{
//    for (int i = 0; i < thePlayer.ItemSlots.Length; i++)
//    {
//        if (thePlayer.ItemSlots[i] == findGrabbableObject)
//        {
//            return true;
//        }
//    }
//    return false;
//}

// SetPlayerTeleporterId(___playersBeingTeleported, playerControllerB, -1);
// DropSomeItems(playerControllerB, true);
// if ((bool)UnityEngine.Object.FindObjectOfType<AudioReverbPresets>())
// {
//     UnityEngine.Object.FindObjectOfType<AudioReverbPresets>().audioPresets[2].ChangeAudioReverbForPlayer(playerControllerB);
// }
// 
// playerControllerB.isInElevator = false;
// playerControllerB.isInHangarShipRoom = false;
// playerControllerB.isInsideFactory = true;
// playerControllerB.averageVelocity = 0f;
// playerControllerB.velocityLastFrame = Vector3.zero;
// StartOfRound.Instance.allPlayerScripts[playerObj].TeleportPlayer(teleportPos);
// StartOfRound.Instance.allPlayerScripts[playerObj].beamOutParticle.Play();
// __instance.shipTeleporterAudio.PlayOneShot(__instance.teleporterBeamUpSFX);
// StartOfRound.Instance.allPlayerScripts[playerObj].movementAudio.PlayOneShot(__instance.teleporterBeamUpSFX);
// if ((UnityEngine.Object)(object)playerControllerB == (UnityEngine.Object)(object)GameNetworkManager.Instance.localPlayerController)
// {
//     UnityEngine.Debug.Log("Teleporter shaking camera");
//     HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
// }
// 
// return;

/*[HarmonyPatch(typeof(ShipTeleporter), "SetPlayerTeleporterId")]
[HarmonyPostfix]
static void OnSetPlayerTeleporterId(PlayerControllerB playerScript, int teleporterId)
{
    Logger.LogDebug($"OnSetPlayerTeleporterId {playerScript}: {teleporterId}");
    if (teleporterId == 2) // Inverse teleporter
    {
        playerScript.StartCoroutine(TeleportStaleBread(playerScript));

        // This isn't really doing what I want, so my next idea is this:
        // Teleport all bread within range of the inverse teleporter when it activates.
    }
}*/

/*static IEnumerator TeleportStaleBread(PlayerControllerB __instance)
{
    Logger.LogDebug("$TeleportStaleBread 1 Player {__instance}");

    var savedItemSlots = new GrabbableObject[__instance.ItemSlots.Length];

    for (int i = 0; i < __instance.ItemSlots.Length; i++)
    {
        GrabbableObject grabbableObject = __instance.ItemSlots[i];
        if (grabbableObject == null)
        {
            Logger.LogDebug($"TeleportStaleBread 1 grabbableObject was NULL {i} Player {__instance}");
            continue;
        }

        //Logger.LogInfo(grabbableObject.GetType().ToString()); // PhysicsProp / KeyItem
        //Logger.LogInfo(grabbableObject.itemProperties.itemName);

        if (grabbableObject.itemProperties.itemName != "Stale bread")
        {
            Logger.LogDebug($"TeleportStaleBread 1 grabbableObject was {i} {grabbableObject.itemProperties.itemName} Player {__instance}");
            savedItemSlots[i] = null;
            continue;
        }

        Logger.LogDebug($"TeleportStaleBread 1 Save {i} Player {__instance}");

        savedItemSlots[i] = grabbableObject;
    }

    // I don't know what I can put here to get some kind of timestamp from the game
    //Logger.LogInfo($"The current game time is: {}");

    Logger.LogDebug($"TeleportStaleBread 2 Player {__instance}");

    yield return new WaitForSeconds(3.5f);

    Logger.LogDebug($"TeleportStaleBread 3 Player {__instance}");

    //Logger.LogInfo($"The current game time is: {}");

    Logger.LogDebug($"TeleportStaleBread savedItemSlots Length: {savedItemSlots.Length} Player {__instance}");

    for (int i = 0; i < savedItemSlots.Length; i++)
    {
        Logger.LogDebug($"TeleportStaleBread 3 savedItemSlots i: {i} Player {__instance}");

        GrabbableObject grabbableObject = savedItemSlots[i];
        if (grabbableObject == null)
        {
            Logger.LogDebug($"TeleportStaleBread 3 savedItemSlots i: {i} NULL {grabbableObject} Player {__instance}");
            continue;
        }

        // Teleport the stale bread to the player's position

        Logger.LogDebug($"TeleportStaleBread 3 savedItemSlots TELEPORT BREAD {i} Player {__instance}");

        // Move it upwards a little to prevent it from spawning in the floor
        Vector3 adjustedTelePos = __instance.thisPlayerBody.transform.position;
        adjustedTelePos.z += 0.2f;

        //grabbableObject.transform.position = adjustedTelePos;
        DropGrabbableObjectAtPosition(grabbableObject, adjustedTelePos);
    }
}*/



// The game calls this when you press the teleporter button. It will target the player currently being viewed by the map screen in the ship.
/*[HarmonyPatch(typeof(ShipTeleporter), "beamUpPlayer")]
[HarmonyPostfix]
private static void PostBeamUpPlayer(ShipTeleporter __instance)
{
    Logger.LogDebug("PostBeamUpPlayer");

    PlayerControllerB playerToBeamUp = StartOfRound.Instance.mapScreen.targetedPlayer;
    if (playerToBeamUp == null)
    {
        Logger.LogDebug("Targeted player is null");
        return;
    }

    // If player is dead, skip
    if (playerToBeamUp.deadBody != null)
    {
        return;
    }

    //DropAndTeleportStaleBread(playerToBeamUp, __instance.teleporterPosition.position);

    // What I need to do here is... cache all the bread... and teleport it afterwards.

    // This gets called as soon as you hit the button... not when the player teleports moments later
    Logger.LogDebug("Try TeleportStaleBread Teleporter");

    //__instance.StartCoroutine(TeleportStaleBread(playerToBeamUp));
}*/

//[HarmonyPatch(typeof(PlayerControllerB), "TeleportPlayer")]
//[HarmonyPrefix]
//public static void PreTeleportPlayer(int damageNumber, ref int ___health)
//{
//    ___health = 100;
//    //Logger.LogInfo($"Taking {damageNumber} damage.");
//}

//[HarmonyPatch("TeleportPlayer")]
//[HarmonyPrefix]
//private static void PreTeleportPlayer(ShipTeleporter __instance, ref int[] ___playersBeingTeleported, int playerObj, Vector3 teleportPos)
//{
//}
