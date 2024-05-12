using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace PostsAndBeams.ModBlockBehavior
{
	/// <summary>
	/// A version of vanilla's BlockBehaviorNWOrientable that allows customization of drops
	/// </summary>
	public class BlockBehaviorNWOrientableCustomDrops : BlockBehaviorNWOrientable
	{
		public BlockBehaviorNWOrientableCustomDrops(Block block) : base(block)
		{
		}
		
		public override void OnLoaded(ICoreAPI api)
		{
			base.OnLoaded(api);
			JsonItemStack jsonItemStack = this.drop;
			if (jsonItemStack == null)
			{
				return;
			}
			IWorldAccessor world = api.World;
			string str = "NWOrientableCustomDrops drop for ";
			AssetLocation code = this.block.Code;
			jsonItemStack.Resolve(world, str + ((code != null) ? code.ToString() : null), true);
		}

		public override void Initialize(JsonObject properties)
		{
			base.Initialize(properties);
			if (properties["dropBlockFace"].Exists)
			{
				this.dropBlockFace = properties["dropBlockFace"].AsString(null);
			}
			if (properties["drop"].Exists)
			{
				this.drop = properties["drop"].AsObject<JsonItemStack>(null, this.block.Code.Domain);
			}
		}

		public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref float dropQuantityMultiplier, ref EnumHandling handled)
		{
			handled = EnumHandling.PreventDefault;
			JsonItemStack jsonItemStack = this.drop;
			if (((jsonItemStack != null) ? jsonItemStack.ResolvedItemstack : null) != null)
			{
				ItemStack[] array = new ItemStack[1];
				int num = 0;
				JsonItemStack jsonItemStack2 = this.drop;
				array[num] = ((jsonItemStack2 != null) ? jsonItemStack2.ResolvedItemstack.Clone() : null);
				return array;
			}
			return new ItemStack[]
			{
				new ItemStack(world.BlockAccessor.GetBlock(this.block.CodeWithVariant(this.variantCode, this.dropBlockFace)), 1)
			};
		}

		public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos, ref EnumHandling handled)
		{
			handled = EnumHandling.PreventDefault;
			if (this.drop == null)
			{
				return new ItemStack(world.BlockAccessor.GetBlock(this.block.CodeWithVariant(this.variantCode, this.dropBlockFace)), 1);
			}
			JsonItemStack jsonItemStack = this.drop;
			if (jsonItemStack == null)
			{
				return null;
			}
			return jsonItemStack.ResolvedItemstack.Clone();
		}

		private string dropBlockFace = "ns";

		private string variantCode = "orientation";

		private JsonItemStack drop;
	}
}