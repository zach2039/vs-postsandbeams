### v1.22.x-1.6.0

 - Add lantern attachment for wooden posts: right-click an empty post on any horizontal face with a vanilla lantern in hand to fuse them into a hybrid block; the lantern hugs the post on the clicked face
 - Add lantern attachment for wooden beams: right-click the underside of a beam with a vanilla lantern in hand to suspend a chain-and-lantern beneath it
 - Right-click either hybrid with an empty hand to detach the lantern; material/lining/glass attributes are preserved on the returned itemstack
 - Breaking either hybrid drops the lantern plus a wooden-post-ew item (matches the existing beam drop convention)
 - Normalise all wooden-post drops to woodenpost-{wood}-{bark}-ew via woodenpost.json drops field; every post variant (empty, n, ew, ne, etc.) now drops the same item the creative inventory provides
 - Lantern attachment refuses on connected posts (matches existing torchholder convention) and refuses to create a stacked-conflict pair with the other hybrid type
 - Narrow CanAttach.sides on woodenpost.json and woodenbeam.json to up/down so vanilla OmniAttachable can no longer redirect lantern placement onto post/beam sides where it would float
 - Add LanternAttachable behavior to woodenpost-torchholder.json so a torchholder-post refuses lantern clicks instead of letting OmniAttachable redirect to a nearby beam
 - Lantern body geometry is loaded from vanilla ground.json/ceiling.json at runtime via BlockLantern.GenMesh, so future vanilla lantern visual updates are picked up automatically
 - Mesh caching via ObjectCacheUtil keyed on (variant, material/lining/glass), matching the vanilla BELantern pattern
 - Multiplayer correctness: SetBlock + DidPlace run on both sides for client-side visual prediction; inventory consume and sound stay server-authoritative; DetachLantern is server-only
 - Add candle/glass/lining/material-diamond/material-grid texture defaults to lantern bracket shapes with explicit game: domain prefix (vanilla BlockLantern's tessellation merges in lantern body elements that reference these texture codes; without the defaults the candle inside the lantern rendered as an all-white quad)
 - BEWoodenPostLantern.OnTesselation no longer delegates to base.OnTesselation in error-fallback paths (base reads BlockLantern's tmpTextureSource, which is only populated by a successful GenMesh; falling back through it would NPE every frame the chunk re-meshes); returns false instead so the block's static JSON shape renders
 - Block OmniAttachable redirect when the hybrid block lookup returns null (e.g. compat-mod woods without a corresponding hybrid blocktype patch): both lantern-attach behaviors now consume the interaction with PreventDefault rather than falling through to vanilla, which would have placed the lantern on a neighbouring cell
 - Apply the same multiplayer-correctness pattern to the torchholder swap in BlockPost.OnBlockInteractStart: ExchangeBlock now runs on both sides for client visual prediction, and the inventory consume is gated on CurrentGameMode != Creative (was previously consuming torchholders even in creative mode)
 - Guard against null/short orientation values in BlockBehaviorBreakIfNotConnectedPost.IsConnectedAndFacingPost (currently safe with ns/we beams, defensive against future blocks that might omit the variant)
 - modinfo: description now mentions torch holders and lanterns; game dependency pinned to 1.22.0 minimum (the mod uses 1.22-only API surface and would crash at runtime on 1.21.x rather than refusing to load with the previous "*" wildcard)
 - README: drop "lantern attachment" from Future Plans (shipped); add Overview bullet describing the lantern interaction alongside the existing torchholder bullet

### v1.22.x-1.5.1

 - Update to VS-v1.22.2 (retarget to .NET 10)
 - Fix BlockSounds.Place type change (now SoundAttributes; access .Location for AssetLocation)
 - Remove no-op OnBlockBroken override (base method now obsolete)
 - Fix PlayerJoin handler subscribed before serverChannel was assigned (latent NRE on early join)
 - Tighten FindConnectedPostWithinDistanceInDirection: explicit null/air handling, safer guards
 - Guard against null interactions array in GetPlacedBlockInteractionHelp (server-side safety)
 - Fix unreachable lang fallback "block-woodenpost-empty-*" (variant order placed empty in 3rd slot, not 1st)
 - Log config-load exceptions instead of silently swallowing (catch was bare)
 - Gate torch-holder attachment (inventory consume + block exchange) on EnumAppSide.Server to avoid client/server desync
 - Move block-lookup before inventory consume so a missing torchholder block no longer eats the held item
 - Remove 8 dead variant keys (*-gns, *-ngs, *-egw, *-gew) from selectionboxbytype/collisionboxbytype in woodenpost.json
 - Add "rotten" and "veryrotten" to recipe skipVariants (1.22's debarkedlog now has these wood states; outputs were unresolvable)
 - Apply same skipVariants to decorbeam.json recipes for consistency
 - Add "ruined" to woodenpost-torchholder material variants (1.22 vanilla torchholder now has brass/aged/ruined; players couldn't attach ruined holders to posts)
 - Bundle modicon.png in build output (was present at source but missing from zip; per Anego's official .csproj template)
 - Add modinfo.json $schema reference for editor IntelliSense (per Anego's official modinfo template)
 - Hide woodenposttorchholder variants from survival handbook (matches vanilla pattern for lantern, supportbeam, since they're not directly craftable)
 - Add lang fallbacks (block-{woodenpost,woodenposttorchholder,woodenbeam,decorbeam}-*) for unknown wood species (e.g. mod-added woods)
 - Retarget CakeBuild project to .NET 10 to match main project (unblocks ./build.sh on machines without .NET 8 runtime)
 - Fix .vscode/launch.json hardcoded net7.0 path for CakeBuild debug profile

### v1.21.x-1.5.0

 - Update to VS-v1.21.6
 - Integrate ShaeTsu's MoreTorchholders support

### v1.20.x-1.4.0

- Fix recipe issue which prevented crafting debarked beams back into debarked posts (thanks, wwwDayDream!)
- Fix compat with wildcrafttree\_1.2.0
- Fix en lang; add lang gen scripts from WoodStain
- Add ability to place torch holders on wooden posts with no connections
- Rotate horizontal debarked texture on beams and posts by 90 degrees

### v1.19.x-1.3.2

 - Change plank textures for wildcraft tree beams to debarked textures
 - Added a decor beam, which does not support blocks but can be placed anywhere
 - Fix lightAbsorption not being 0 on beams and decor beams

### v1.19.x-1.3.1

 - Fix incorrect logic with shift placement

### v1.19.x-1.3.0

 - Merge posts and beams placing into single block
 - Add auto-place feature to posts and beams; shift disables
 - Add interaction help with place
 - Fix WildcraftTree compat

### v1.18.15-1.2.0

 - Migrate to net7
 - Update to 1.18.15
 - Add mod config and sync packets
 - Add attributes to blocks to allow use as supports for unstable stone in 1.19 pre versions
 - Rework of post/beam connections; must be connected to post in-line with beam direction with no non-beam or air blocks

### v1.17.x-1.1.0

 - Add debarked versions. Craft barked versions with an axe to debark them.
 - Fixed version issue in modinfo.json to allow all 1.17.x versions.

### v1.17.x-1.0.0

 - Initial release
