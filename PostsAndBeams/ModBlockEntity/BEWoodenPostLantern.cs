using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace PostsAndBeams.ModBlockEntity
{
    // Custom BELantern subclass that renders the lantern as two combined meshes:
    //   1. The block's own shape — a hand-authored bracket+post (no lantern body).
    //   2. Vanilla's ground lantern body shape, translated to the bracket terminus.
    // This avoids forking vanilla's lantern body geometry; if vanilla restyles the
    // lantern in a future VS update, our hybrid auto-tracks.
    //
    // Combined meshes are cached via ObjectCacheUtil keyed on
    // (block-cache-key, orientation, material, lining, glass). Without the cache every
    // chunk re-tesselation would re-run two GenMesh calls per hybrid block.
    public class BEWoodenPostLantern : BELantern
    {
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            // Catch-and-log: an exception here would corrupt the entire chunk's
            // tesselation and fire every frame the chunk re-meshes.
            MeshData mesh;
            try
            {
                mesh = GetCombinedMesh(tesselator);
            }
            catch (Exception e)
            {
                this.Api?.Logger?.Error("[PostsAndBeams] BEWoodenPostLantern.OnTesselation at {0}: {1}", this.Pos, e);
                // Don't delegate to base: BELantern.OnTesselation reads BlockLantern's
                // private tmpTextureSource which is only populated by a successful GenMesh
                // call. In this catch path GenMesh hasn't run, so base would NPE every
                // frame the chunk re-meshes. Let the block's static JSON shape render.
                return false;
            }
            if (mesh == null) return false;
            mesher.AddMeshData(mesh, 1);
            return true;
        }

        private MeshData GetCombinedMesh(ITesselatorAPI tesselator)
        {
            BlockLantern blockLantern = this.Block as BlockLantern;
            ICoreClientAPI capi = this.Api as ICoreClientAPI;
            if (blockLantern == null || capi == null) return null;
            if (this.Block.Shape?.Base == null) return null;

            // Snapshot mutable BE state once. OnTesselation runs on the tesselator
            // thread; DidPlace may write these fields concurrently from the main
            // thread. String reference reads are atomic in .NET, so each individual
            // read is safe — but reading the trio piecemeal across the method risks
            // mixing old and new attribute sets. Take a single snapshot up front.
            string mat = this.material ?? "copper";
            string lin = this.lining ?? "plain";
            string gls = this.glass ?? "quartz";

            string orientation = this.Block.Variant["horizontalorientation"] ?? "south";
            string size = this.Block.Variant["size"] ?? "small";

            Dictionary<string, MeshData> cache = ObjectCacheUtil.GetOrCreate(
                this.Api,
                blockLantern.baseCacheKey + "-" + orientation + "-meshes",
                () => new Dictionary<string, MeshData>());

            string cacheKey = mat + "-" + lin + "-" + gls;
            if (cache.TryGetValue(cacheKey, out MeshData cached)) return cached;

            // Bracket+post mesh from the block's own JSON shape. Vanilla GenMesh's
            // null-shape path strips the domain of Block.Shape.Base when reconstructing
            // the path, so lookups for non-vanilla-domain shapes fail. Load explicitly,
            // mirroring the vanilla idiom (non-mutating copy with prefix/suffix).
            AssetLocation bracketPath = this.Block.Shape.Base.CopyWithPathPrefixAndAppendixOnce("shapes/", ".json");
            Shape bracketShape = Shape.TryGet(this.Api, bracketPath);
            if (bracketShape == null) return null;

            MeshData bracketPostMesh = blockLantern.GenMesh(capi, mat, lin, gls, bracketShape, tesselator);
            if (bracketPostMesh == null) return null;

            // Body from vanilla ground.json. Optional: if missing, render bracket only.
            MeshData combined = bracketPostMesh;
            Shape bodyShape = Shape.TryGet(this.Api, "shapes/block/metal/lantern/" + size + "/ground.json");
            if (bodyShape != null)
            {
                MeshData bodyMesh = blockLantern.GenMesh(capi, mat, lin, gls, bodyShape, tesselator);
                if (bodyMesh != null)
                {
                    bodyMesh.Translate(ComputeBodyOffset(orientation, size));
                    combined = bracketPostMesh.Clone();
                    combined.AddMeshData(bodyMesh);
                }
            }

            cache[cacheKey] = combined;
            return combined;
        }

        // Per-orientation, per-size offset applied to vanilla's ground body mesh so it
        // lands at our bracket's terminus. Values are in block units (1 block = 16 px).
        private static Vec3f ComputeBodyOffset(string orientation, string size)
        {
            float outward = (size == "large") ? 9f / 16f : 8f / 16f;
            float dy = 5f / 16f;

            float dx = 0f, dz = 0f;
            switch (orientation)
            {
                case "south": dz = +outward; break;
                case "north": dz = -outward; break;
                case "east":  dx = +outward; break;
                case "west":  dx = -outward; break;
            }
            return new Vec3f(dx, dy, dz);
        }
    }
}
