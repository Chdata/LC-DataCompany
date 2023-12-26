using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DataCompanyMod.ScrapItems
{
    internal class StaleBread
    {
        [HarmonyPatch(typeof(PlayerControllerB), "DropAllHeldItems")]
        [HarmonyPrefix]
        private static bool OnDropAllHeldItems(PlayerControllerB __instance, bool itemsFall, bool disconnecting)
        {
            //Plugin.Logger.LogInfo("Calling OnDropAllHeldItems");

            bool foundBread = false;

            for (int i = 0; i < __instance.ItemSlots.Length; i++)
            {
                GrabbableObject grabbableObject = __instance.ItemSlots[i];
                if (grabbableObject == null)
                {
                    continue;
                }

                if (grabbableObject.itemProperties.itemName == "Stale bread")
                {
                    foundBread = true;
                    continue;
                }

                if (itemsFall)
                {
                    grabbableObject.parentObject = null;
                    grabbableObject.heldByPlayerOnServer = false;
                    if (__instance.isInElevator)
                    {
                        grabbableObject.transform.SetParent(__instance.playersManager.elevatorTransform, worldPositionStays: true);
                    }
                    else
                    {
                        grabbableObject.transform.SetParent(__instance.playersManager.propsContainer, worldPositionStays: true);
                    }
                    __instance.SetItemInElevator(__instance.isInHangarShipRoom, __instance.isInElevator, grabbableObject);
                    grabbableObject.EnablePhysics(enable: true);
                    grabbableObject.EnableItemMeshes(enable: true);
                    grabbableObject.transform.localScale = grabbableObject.originalScale;
                    grabbableObject.isHeld = false;
                    grabbableObject.isPocketed = false;
                    grabbableObject.startFallingPosition = grabbableObject.transform.parent.InverseTransformPoint(grabbableObject.transform.position);
                    grabbableObject.FallToGround(randomizePosition: true);
                    grabbableObject.fallTime = Random.Range(-0.3f, 0.05f);
                    if (__instance.IsOwner)
                    {
                        grabbableObject.DiscardItemOnClient();
                    }
                    else if (!grabbableObject.itemProperties.syncDiscardFunction)
                    {
                        grabbableObject.playerHeldBy = null;
                    }
                }
                if (__instance.IsOwner && !disconnecting)
                {
                    HUDManager.Instance.holdingTwoHandedItem.enabled = false;
                    HUDManager.Instance.itemSlotIcons[i].enabled = false;
                    HUDManager.Instance.ClearControlTips();
                    __instance.activatingItem = false;
                }
                __instance.ItemSlots[i] = null;
            }

            if (__instance.isHoldingObject)
            {
                __instance.isHoldingObject = false;
                if (__instance.currentlyHeldObjectServer != null)
                {
                    //SetSpecialGrabAnimationBool(setTrue: false, currentlyHeldObjectServer);
                    MethodInfo methodInfo = __instance.GetType().GetMethod("SetSpecialGrabAnimationBool", BindingFlags.NonPublic | BindingFlags.Instance);
                    methodInfo.Invoke(__instance, [false, __instance.currentlyHeldObjectServer]);
                }
                __instance.playerBodyAnimator.SetBool("cancelHolding", value: true);
                __instance.playerBodyAnimator.SetTrigger("Throw");
            }
            __instance.activatingItem = false;
            __instance.twoHanded = false;
            __instance.carryWeight = 1f;
            __instance.currentlyHeldObjectServer = null;

            if (foundBread)
            {
                //Plugin.Logger.LogInfo("Spawn New Bread 1 (true)");
                __instance.StartCoroutine(SpawnBreadAtPlayer(__instance));
            }

            return false;
        }

        [HarmonyPatch(typeof(ShipTeleporter), "TeleportPlayerOutWithInverseTeleporter")]
        [HarmonyPostfix]
        private static void OnTeleportPlayerOutWithInverseTeleporter(ShipTeleporter __instance, int playerObj, Vector3 teleportPos)
        {
            //Plugin.Logger.LogDebug("OnTeleportPlayerOutWithInverseTeleporter");

            PlayerControllerB thePlayer = StartOfRound.Instance.allPlayerScripts[playerObj];

            if (thePlayer.isPlayerDead)
            {
                return;
            }

            //StartOfRound.Instance.allPlayerScripts[playerObj].TeleportPlayer(teleportPos);

            //Plugin.Logger.LogDebug("Try TeleportStaleBread Inverse");
            //__instance.StartCoroutine(TeleportStaleBread(thePlayer));

            foreach (GrabbableObject grabbable in Plugin.FindObjectsOfType<GrabbableObject>())
            {
                if (grabbable.itemProperties.itemName != "Stale bread")
                {
                    continue;
                }

                //Plugin.Logger.LogDebug($"Inverse found bread. Player: {thePlayer.serverPlayerPosition} Tele: {teleportPos} Bread: {grabbable.transform.position}");

                // We can't do this because the player has already had their items dropped by the teleporter at this point.
                //if (!PlayerHasGrabbableObjectInItemSlots(thePlayer, grabbable))
                //{
                //    continue;
                //}

                if (Vector3.Distance(grabbable.transform.position, __instance.teleportOutPosition.position) >= 2f)
                {
                    // Bread is too far away from the teleporter.
                    continue;
                }

                Vector3 adjustedTelePos = thePlayer.serverPlayerPosition; //transform.position; <-- this never changes and is not the player's position
                adjustedTelePos.y += 0.2f;

                // Doing this alone will not work.
                //grabbable.transform.position = adjustedTelePos;

                DropGrabbableObjectAtPosition(grabbable, adjustedTelePos);

                //Plugin.Logger.LogDebug($"Inverse moved bread. Player: {thePlayer.serverPlayerPosition} Tele: {teleportPos} Bread: {grabbable.transform.position}");
            }
        }

        static IEnumerator SpawnBreadAtPlayer(PlayerControllerB thePlayer)
        {
            //Plugin.Logger.LogInfo("Spawn New Bread 2");

            yield return new WaitForSeconds(0.5f);

            //Plugin.Logger.LogInfo("Spawn New Bread 3");

            if (thePlayer.isPlayerDead)
            {
                //Plugin.Logger.LogInfo("Spawn New Bread 4 DEAD");
                yield break;
            }

            //Plugin.Logger.LogInfo("Spawn New Bread 4");

            CreateBreadAtLocation(thePlayer.serverPlayerPosition);
        }

        // This is the bare minimum needed to spawn a new item with a value, according to RoundManager.SpawnScrapInLevel()
        public static void CreateBreadAtLocation(Vector3 spawnPos)
        {
            //Plugin.Logger.LogInfo("CreateBreadAtLocation()");

            // Move it a little bit above the teleporter, otherwise it spawns underneath it.
            spawnPos.y += 1f;

            GameObject obj = Object.Instantiate(Plugin.StaleBreadItem.spawnPrefab, spawnPos, Quaternion.identity);
            GrabbableObject component = obj.GetComponent<GrabbableObject>();
            component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
            component.fallTime = 0f;

            component.scrapValue = Random.Range(2, 7);
            NetworkObject component2 = obj.GetComponent<NetworkObject>();
            component2.Spawn();

            List<int> listValue = [];
            List<NetworkObjectReference> listNetwork = [];

            listValue.Add(component.scrapValue);
            listNetwork.Add(component2);

            // This is what fixes the scrap value so it syncs / appears for all players
            component.StartCoroutine(WaitForScrapToSpawnToSync([.. listNetwork], [.. listValue]));
        }

        static IEnumerator WaitForScrapToSpawnToSync(NetworkObjectReference[] spawnedScrap, int[] scrapValues)
        {
            yield return new WaitForSeconds(1f);
            RoundManager.Instance.SyncScrapValuesClientRpc(spawnedScrap, scrapValues);
        }

        public static void DropGrabbableObjectAtPosition(GrabbableObject grabbableObject, Vector3 telePos)
        {
            grabbableObject.parentObject = null;
            grabbableObject.heldByPlayerOnServer = false;
        
            PlayerControllerB thePlayer = grabbableObject.playerHeldBy;
            if (thePlayer != null)
            {
                if (thePlayer.isInElevator)
                {
                    (grabbableObject).transform.SetParent(thePlayer.playersManager.elevatorTransform, worldPositionStays: true);
                }
                else
                {
                    (grabbableObject).transform.SetParent(thePlayer.playersManager.propsContainer, worldPositionStays: true);
                }
        
                thePlayer.SetItemInElevator(thePlayer.isInHangarShipRoom, thePlayer.isInElevator, grabbableObject);
            }
        
            grabbableObject.EnablePhysics(enable: true);
            grabbableObject.EnableItemMeshes(enable: true);
            grabbableObject.transform.localScale = grabbableObject.originalScale;
            grabbableObject.isHeld = false;
            grabbableObject.isPocketed = false;
            grabbableObject.transform.position = telePos; // I needed to add this to make FallToGround() work.
            grabbableObject.startFallingPosition = telePos; // ((Component)(object)grabbableObject).transform.parent.InverseTransformPoint(((Component)(object)grabbableObject).transform.position);
            grabbableObject.FallToGround(randomizePosition: true);
            grabbableObject.fallTime = Random.Range(-0.3f, 0.05f);
        
            if (thePlayer != null)
            {
                if (thePlayer.IsOwner)
                {
                    grabbableObject.DiscardItemOnClient();
                }
                else if (!grabbableObject.itemProperties.syncDiscardFunction)
                {
                    grabbableObject.playerHeldBy = null;
                }
        
                int b = -1;
        
                for (int i = 0; i < thePlayer.ItemSlots.Length; i++)
                {
                    if (thePlayer.ItemSlots[i] == grabbableObject)
                    {
                        b = i;
                        break;
                    }
                }
        
                if (b != -1)
                {
                    if (thePlayer.IsOwner)
                    {
                        HUDManager.Instance.holdingTwoHandedItem.enabled = false;
                        HUDManager.Instance.itemSlotIcons[b].enabled = false;
                        HUDManager.Instance.ClearControlTips();
                        thePlayer.activatingItem = false;
                    }
        
                    thePlayer.ItemSlots[b] = null;
                }
            }
        }
    }
}
