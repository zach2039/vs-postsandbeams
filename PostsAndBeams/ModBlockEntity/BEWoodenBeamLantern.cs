using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace PostsAndBeams.ModBlockEntity
{
    // BE for woodenbeam-lantern. Renders the vanilla lantern body suspended below the
    // beam by reusing vanilla's ceiling.json shape (chain + body), shifted down so the
    // chain anchors at the beam underside (y=8) instead of the cell ceiling (y=16).
    //
    // Returns false from OnTesselation so the block's static beam shape continues to
    // render normally — we only ADD the lantern body to the chunk.
    //
    // Since the parent block (BlockWoodenBeamLantern) is a plain Block (not BlockLantern),
    // we look up the vanilla lantern block to drive GenMesh — that's where the
    // [textureCode] indexer lives that resolves #material/#lining/#glass against the BE's
    // runtime attributes.
    //
    // The translated body mesh is cached via ObjectCacheUtil per (size, material, lining,
    // glass) so chunk re-tesselation doesn't re-run GenMesh on every block.
    public class BEWoodenBeamLantern : BELantern
    {
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            // Catch-and-log: an exception here would corrupt the entire chunk's
            // tesselation and fire every frame the chunk re-meshes.
            try
            {
                MeshData mesh = GetBodyMesh(tesselator);
                if (mesh != null) mesher.AddMeshData(mesh, 1);
            }
            catch (Exception e)
            {
                this.Api?.Logger?.Error("[PostsAndBeams] BEWoodenBeamLantern.OnTesselation at {0}: {1}", this.Pos, e);
            }
            // Static beam shape still renders via the standard block-shape pipeline.
            return false;
        }

        private MeshData GetBodyMesh(ITesselatorAPI tesselator)
        {
            ICoreClientAPI capi = this.Api as ICoreClientAPI;
            if (capi == null) return null;

            // Snapshot mutable BE state once — see BEWoodenPostLantern for the rationale.
            string mat = this.material ?? "copper";
            string lin = this.lining ?? "plain";
            string gls = this.glass ?? "quartz";

            string size = this.Block?.Variant["size"] ?? "small";

            Block vanillaLantern = this.Api.World.GetBlock(new AssetLocation("game", "lantern-" + size + "-up"));
            BlockLantern lanternBlock = vanillaLantern as BlockLantern;
            if (lanternBlock == null) return null;

            Dictionary<string, MeshData> cache = ObjectCacheUtil.GetOrCreate(
                this.Api,
                "woodenbeamlantern-" + size + "-meshes",
                () => new Dictionary<string, MeshData>());

            string cacheKey = mat + "-" + lin + "-" + gls;
            if (cache.TryGetValue(cacheKey, out MeshData cached)) return cached;

            Shape bodyShape = Shape.TryGet(this.Api, "shapes/block/metal/lantern/" + size + "/ceiling.json");
            if (bodyShape == null) return null;

            MeshData bodyMesh = lanternBlock.GenMesh(capi, mat, lin, gls, bodyShape, tesselator);
            if (bodyMesh == null) return null;

            // Vanilla ceiling.json anchors the chain at y=16 (cell ceiling). The beam
            // underside is at y=8, so shift the entire shape down by 8 px. Translation
            // happens before caching so the cached mesh is already final-positioned.
            bodyMesh.Translate(0f, -8f / 16f, 0f);

            cache[cacheKey] = bodyMesh;
            return bodyMesh;
        }
    }
}
