using BepInEx;
using System;
using System.Security.Permissions;
using System.Security;
using System.Linq;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace R2API.Utils
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ManualNetworkRegistrationAttribute : Attribute
    {
    }
}

namespace RemoveEnemies
{
    [BepInPlugin("com.Moffein.RemoveEnemies", "RemoveEnemies", "1.0.0")]
    public class RemoveEnemiesPlugin : BaseUnityPlugin
    {
        public static string blacklistMasterList;
        public static HashSet<GameObject> blacklistedPrefabs = new HashSet<GameObject>();

        private void Awake()
        {
            ReadConfig();
            RoR2Application.onLoad += BuildBlacklist;
            On.RoR2.DCCSBlender.GetBlendedDCCS += DCCSBlender_GetBlendedDCCS;
        }

        private void ReadConfig()
        {
            blacklistMasterList = base.Config.Bind<string>(new ConfigDefinition("Master Blacklist", "Master Blacklist"), "",
                new ConfigDescription("List of Masters to blacklist. Format is MasterName separated by comma, case-sensitive.")).Value;
        }

        private void BuildBlacklist()
        {
            blacklistMasterList = new string(blacklistMasterList.ToCharArray().Where(c => !System.Char.IsWhiteSpace(c)).ToArray());
            string[] split = blacklistMasterList.Split(',');
            foreach (string str in split)
            {
                var index = MasterCatalog.FindMasterIndex(str);
                if (index != MasterCatalog.MasterIndex.none)
                {
                    blacklistedPrefabs.Add(MasterCatalog.GetMasterPrefab(index));
                }
            }
        }

        private DirectorCardCategorySelection DCCSBlender_GetBlendedDCCS(On.RoR2.DCCSBlender.orig_GetBlendedDCCS orig, DccsPool.Category dccsPoolCategory, ref Xoroshiro128Plus rng, ClassicStageInfo stageInfo, int contentSourceMixLimit, System.Collections.Generic.List<RoR2.ExpansionManagement.ExpansionDef> acceptableExpansionList)
        {
            var ret = orig(dccsPoolCategory, ref rng, stageInfo, contentSourceMixLimit, acceptableExpansionList);
            for (int i = 0; i < ret.categories.Length; i++)
            {
                ret.categories[i].cards = ret.categories[i].cards.Where(card => !blacklistedPrefabs.Contains(card.spawnCard.prefab)).ToArray();
            }
            return ret;
        }
    }
}
