using Harmony;
using System;
using System.Reflection;
using Assets.Code;
using I2.Loc;
using UnityEngine;

/*
 * Huge thanks to all the great people helping me here, Especially Rakceyen and greenking2000 !!!
 * 
 */

namespace mods.CrownedRepublic
{
    public class ModMain : MonoBehaviour
    {
        public static KCModHelper Helper;
        public static AssetBundle AssetBundle;

        public static GameObject HouseOfParliamentPrefab;

        public static ModMain Inst;


        public void Preload(KCModHelper helper)
        {
            Inst = this;

            Helper = helper;
            string assetBundlePath = Helper.modPath + "/AssetBundle/";
            
            try
            {
                AssetBundle = KCModHelper.LoadAssetBundle(assetBundlePath, "house_of_parliament");
                Debug.Log(assetBundlePath);

                if (AssetBundle is null)
                {
                    Helper.Log($"Failed to load asset bundle at {assetBundlePath}");
                }
            }

            catch (Exception) 
            {
                Helper.Log(Helper.modPath);
            }


            if (AssetBundle != null)
            {

                //House_of_Parliament
                HouseOfParliamentPrefab = AssetBundle.LoadAsset("assets/workspace/House_of_Parliament.prefab") as GameObject;
                
                //=========================================================================//

                //attaches the Building script to the prefab
                HouseOfParliamentPrefab?.AddComponent<Building>();
                Building bHouseOfParliament = HouseOfParliamentPrefab.GetComponent<Building>();

                BuildingCollider houseOfParliamentCol = bHouseOfParliament.transform.Find("Offset").Find("House_of_Parliament").gameObject.AddComponent<BuildingCollider>();
                houseOfParliamentCol.Building = bHouseOfParliament;


                bHouseOfParliament.UniqueName = "house_of_parliament";
                bHouseOfParliament.customName = "House of Parliament";
                bHouseOfParliament.uniqueNameHash = "".GetHashCode();


                bHouseOfParliament.JobCategory = JobCategory.Castle;
                //bHouse_of_Parliament.placementSounds = new string[] { "castleplacement" };//replace with building sound
                //bHouse_of_Parliament.SelectionSounds = new string[] { "BuildingSelectCastleGate" };//replace with manor door sound
                bHouseOfParliament.skillUsed = "Castle";
                bHouseOfParliament.size = new Vector3(6f, 4f, 3f);

                //cost
                ResourceAmount cost = new ResourceAmount();
                cost.Set(FreeResourceType.Tree, 750);
                cost.Set(FreeResourceType.Stone, 800);
                cost.Set(FreeResourceType.Gold, 9000);
                cost.Set(FreeResourceType.IronOre, 50);
                SetField<Building>("Cost", bFlags, bHouseOfParliament, cost);

                //place people
                bHouseOfParliament.personPositions = new Transform[] { HouseOfParliamentPrefab.transform.Find("Offset").Find("P1") };
                bHouseOfParliament.DisplayModel = HouseOfParliamentPrefab;
                bHouseOfParliament.SubStructure = false;

                bHouseOfParliament.WorkersForFullYield = 50;//number of politicians
                bHouseOfParliament.BuildAllowedWorkers = 40;

                bHouseOfParliament.BuildShaderMinYAdjustment = 0f;
                bHouseOfParliament.BuildShaderMaxYAdjustment = 3f;

                bHouseOfParliament.Stackable = false;
                bHouseOfParliament.troopsCanWalkOn = false;
                bHouseOfParliament.transportCartsCanTravelOn = false;
                bHouseOfParliament.allowOverAndUnderAqueducts = false;
                bHouseOfParliament.allowCastleBlocksOnTop = 0;
                bHouseOfParliament.dragPlacementMode = 0;
                bHouseOfParliament.doBuildAnimation = true;
                bHouseOfParliament.MaxLife = 10;
                bHouseOfParliament.ignoreRoadCoverageForPlacement = true;

            }
            else
            {
                Helper.Log("Bundle not loaded");
            }

            //initializing harmony patches
            var harmony = HarmonyInstance.Create("harmony");
            harmony.PatchAll(Assembly.GetExecutingAssembly());


        }


        /*
        public static void BuildingCheck(Building building)
        {
            string bInfo = "Job Category: " + building.JobCategory.ToString() +
                "\nPlacement Sound: " + building.placementSounds.ToString() +
                "\nSelection Sound: " + building.SelectionSounds.ToString() +
                "\nSelected Ambient Sound: " + building.selectedAmbientSound.ToString()
                "\nUnique Name: " + building.UniqueName.ToString() +
                "\nCategory Name: " + building.CategoryName.ToString() +
                "\nDisplay Model: " + building.DisplayModel.ToString() +
                "\nSubstructure: " + building.SubStructure.ToString() +
                "\nSkill Used: " + building.skillUsed.ToString() +
                "\nSize: " + building.size.ToString() +
                "\nCost: " + building.GetCost().ToString() +
                "\n");
            helper.Log(bInfo);
        }
        */


        BindingFlags bFlags = BindingFlags.NonPublic | BindingFlags.Instance;

        public static void SetField<T>(string variable, BindingFlags flags, T source, object value)
        {
            FieldInfo fieldInfo = typeof(T).GetField(variable, flags);

            fieldInfo?.SetValue(source, value);
        }

        /*
         * [HarmonyPatch (typeof (Class))]
         * [HarmonyPatch ("Method")
         * public static class NamePatch
         */

        //inst prefabs
        [HarmonyPatch(typeof(GameState))]
        [HarmonyPatch("Awake")]
        public static class InternalPrefabsPatch
        {
            static void Postfix(GameState __instance)
            {
                //__instance.internalPrefabs.Add(inst.bHouse_of_Parliament);
                __instance.internalPrefabs.Add(HouseOfParliamentPrefab.GetComponent<Building>());
            }
        }

        //Add building to build UI
        [HarmonyPatch(typeof(BuildUI))]
        [HarmonyPatch("Start")]
        public static class BuildUIPatch
        {
            static void Postfix(BuildUI __instance)
            {
                __instance.AddBuilding(__instance.AdvTownTab, __instance.AdvTownTabVR, __instance.AdvTownTabConsole,
                    "house_of_parliament","road", Vector3.one);
            }
        }

        //Thought bubbles and descriptions
        [HarmonyPatch(typeof(LocalizationManager))]
        [HarmonyPatch("GetTranslation")]
        public static class LocalizationManagerPatch
        {
            static void Postfix(string Term, ref string __result)
            {
                switch (Term)
                {
                    case "Building house_of_parliament FriendlyName":
                        __result = "House of Parliament";
                        break;
                    case "Building house_of_parliament Description":
                        __result = "Declares the republic & hands over legislative power to parliament.";
                        break;
                    case "Building apartment ThoughtOnBuilt":
                        __result = "Astonished by the election for Parliament";
                        break;
                }
            }
        }

    }

}
