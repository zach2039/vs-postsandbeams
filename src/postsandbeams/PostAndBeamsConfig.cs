using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostsAndBeams
{
    public class PostsAndBeamsConfig
    {
        public string unstableFallingSupportableDropChanceText = "Chance that blocks with behavior UnstableFallingSupportable can fall when a neighboring block is updated";
        public float unstableFallingSupportableDropChance = 0.1f;

        public string canUnstableFallingSupportableCascadeText = "Whether or not blocks with behavior UnstableFallingSupportable can cause similar blocks to fall without checking for support";
        public bool canUnstableFallingSupportableCascade = true;

        public string patchCrackedRockToApplyUnstableFallingSupportableText = "Patch cracked rock to apply behavior UnstableFallingSupportable";
        public bool patchCrackedRockToApplyUnstableFallingSupportable = true;

        public PostsAndBeamsConfig() { }

        public static PostsAndBeamsConfig Current { get; set; }

        public static PostsAndBeamsConfig GetDefault()
        {
            PostsAndBeamsConfig defaultConfig = new PostsAndBeamsConfig();

            defaultConfig.unstableFallingSupportableDropChanceText.ToString();
            defaultConfig.unstableFallingSupportableDropChance = 0.1f;

            defaultConfig.canUnstableFallingSupportableCascadeText.ToString();
            defaultConfig.canUnstableFallingSupportableCascade = true;

            defaultConfig.patchCrackedRockToApplyUnstableFallingSupportableText.ToString();
            defaultConfig.patchCrackedRockToApplyUnstableFallingSupportable = true;

            return defaultConfig;
        }
    }
}