using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
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
                Helper.Log($"SUCCESS: Loaded AssetBundle at: {assetBundlePath}");

                if (AssetBundle is null)
                {
                    Helper.Log($"FAILURE: Didn't load asset bundle at {assetBundlePath}");
                }
            }

            catch (Exception e) 
            {
                Helper.Log($"FAILURE: Error occurred: {e}");
            }


            if (AssetBundle != null)
            {
                Helper.Log(ListAllAssets(AssetBundle, newLine: true));
                
                //House_of_Parliament
                // string houseOfParliamentPrefabLocation = "assets/kcassets/workspace/house_of_parliament.prefab";
                // HouseOfParliamentPrefab = AssetBundle.LoadAsset(houseOfParliamentPrefabLocation) as GameObject;
                HouseOfParliamentPrefab = LoadPrefabLazy(AssetBundle, "house_of_parliament.prefab");
                if (HouseOfParliamentPrefab != null)
                {
                    SetupHousesParliament();
                    
                    // Only load harmony if prefab(s) have loaded correctly
                    HarmonyInstance harmony = HarmonyInstance.Create("CrownedRepublic");
                    harmony.PatchAll(Assembly.GetExecutingAssembly());
                    Helper.Log("SUCCESS: Loaded HouseOfParliamentPrefab");
                }
                else
                {
                    Helper.Log("FAILURE: HouseOfParliamentPrefab not loaded (Likely not found)");
                }
            }
            else
            {
                Helper.Log("FAILURE: AssetBundle not loaded");
            }
        }

        public void SetupHousesParliament()
        {
            //attaches the Building script to the prefab
            if (HouseOfParliamentPrefab is null) throw new Exception("HouseOfParliamentPrefab is null!\n" +
                                                                     "Make sure it is loaded BEFORE running this method");
            
            HouseOfParliamentPrefab.AddComponent<Building>();
            Building bHouseOfParliament = HouseOfParliamentPrefab.GetComponent<Building>();

            BuildingCollider houseOfParliamentCol = bHouseOfParliament.transform.Find("Offset")
                .Find("House_of_Parliament").gameObject.AddComponent<BuildingCollider>();
            houseOfParliamentCol.Building = bHouseOfParliament;


            bHouseOfParliament.UniqueName = "house_of_parliament";
            bHouseOfParliament.customName = "House of Parliament";
            bHouseOfParliament.uniqueNameHash = bHouseOfParliament.UniqueName.GetHashCode();


            bHouseOfParliament.JobCategory = JobCategory.Castle;
            //bHouse_of_Parliament.placementSounds = new string[] { "castleplacement" };//replace with building sound
            //bHouse_of_Parliament.SelectionSounds = new string[] { "BuildingSelectCastleGate" };//replace with manor door sound
            bHouseOfParliament.skillUsed = "Castle";
            bHouseOfParliament.size = new Vector3(6f, 4f, 3f);

            // Cost
            ResourceAmount cost = new ResourceAmount();
            cost.Set(FreeResourceType.Tree, 750);
            cost.Set(FreeResourceType.Stone, 800);
            cost.Set(FreeResourceType.Gold, 9000);
            cost.Set(FreeResourceType.IronOre, 50);
            SetField("Cost", bFlags, bHouseOfParliament, cost);

            // Place people
            bHouseOfParliament.personPositions = new[]
            {
                HouseOfParliamentPrefab.transform.Find("Offset").Find("P1")
            };
            bHouseOfParliament.DisplayModel = HouseOfParliamentPrefab;
            bHouseOfParliament.SubStructure = false;

            bHouseOfParliament.WorkersForFullYield = 50; // Number of "Politicians" TODO In game name defined where?
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
            
            BuildingCheck(bHouseOfParliament);
        }

        
        public static void BuildingCheck(Building building)
        {
            // string bInfo = "Job Category: " + building.JobCategory.ToString() +
            //     "\nPlacement Sound: " + building.placementSounds.ToString() +
            //     "\nSelection Sound: " + building.SelectionSounds.ToString() +
            //     "\nSelected Ambient Sound: " + building.selectedAmbientSound.ToString() +
            //     "\nUnique Name: " + building.UniqueName.ToString() +
            //     "\nCategory Name: " + building.CategoryName.ToString() +
            //     "\nDisplay Model: " + building.DisplayModel.ToString() +
            //     "\nSubstructure: " + building.SubStructure.ToString() +
            //     "\nSkill Used: " + building.skillUsed.ToString() +
            //     "\nSize: " + building.size.ToString() +
            //     "\nCost: " + building.GetCost().ToString() +
            //     "\n");

            string bInfo = $"UniqName:   {building.UniqueName}\n" +
                           $"DisplayNm:  {building.DisplayModel}\n" +
                           $"JobCat:     {building.JobCategory}\n" +
                           $"PlaceSound: {building.placementSounds}\n" +
                           $"SelecSound: {building.SelectionSounds}\n" +
                           $"CatName:    {building.CategoryName}\n" +
                           $"Substrucut: {building.SubStructure}\n" +
                           $"SkillUsed:  {building.skillUsed}\n" +
                           $"Size:       {building.size}\n" +
                           $"Cost:       {building.GetCost(Player.inst.PlayerLandmassOwner)}";
            
            Helper.Log(bInfo);
        }

        /// <summary>
        /// Returns string of all Assets in given AssetBundle
        /// </summary>
        /// <param name="assetBundle"></param>
        /// <param name="delimiter"></param>
        /// <param name="newLine"></param>
        /// <returns></returns>
        public string ListAllAssets(AssetBundle assetBundle, string delimiter=" | ", bool newLine=false)
        {
            List<string> assets = GetAllAssets(assetBundle);

            return assets.Join(delimiter: !newLine ? delimiter : "\n");
        }

        public static List<string> GetAllAssets(AssetBundle assetBundle)
        {
            return assetBundle.GetAllAssetNames().ToList();
        }

        /// <summary>
        /// Loads prefab from any location inside an AssetBundle. Just requires the name of the prefab (Or most of it)
        /// </summary>
        /// <param name="assetBundle"></param>
        /// <param name="prefabName">.prefab is optional</param>
        /// <returns></returns>
        public static GameObject LoadPrefabLazy(AssetBundle assetBundle, string prefabName)
        {
            foreach (string assetName in GetAllAssets(assetBundle))
            {
                if (assetName.Contains(prefabName))
                {
                    return assetBundle.LoadAsset(assetName) as GameObject;
                }
            }

            throw new Exception($"{prefabName} was not found inside {assetBundle}");
        }


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
