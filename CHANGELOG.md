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
 - Hide woodenposttorchholder variants from survival handbook (matches vanilla pattern for lantern, supportbeam — they're not directly craftable)
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
