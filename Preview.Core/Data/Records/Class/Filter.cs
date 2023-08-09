using Xylia.Preview.Common.Attribute;
using Xylia.Preview.Common.Seq;
using Xylia.Preview.Common.Struct;

namespace Xylia.Preview.Data.Record;

[AliasRecord]
public abstract class Filter : BaseRecord
{
	public Script_obj Subject;
	public Script_obj Target;
	public Script_obj Subject2;
	public Script_obj Target2;

	#region Sub
	public sealed class Race : Filter
	{
		[Repeat(4)]
		public RaceSeq[] Value;

		public bool Either;
	}

	public sealed class Sex : Filter
	{
		[Repeat(4)]
		public SexSeq[] Value;

		public bool Either;
	}

	public sealed class Job : Filter
	{
		[Repeat(4)]
		public JobSeq[] Value;
		
		public bool Either;
	}

	public sealed class jobStyle : Filter
	{
		public sbyte Count;

		[Repeat(4)]
		public JobSeq[] job;

		[Repeat(4)]
		public JobStyleSeq[] JobStyle;

		public bool Either;
	}

	public sealed class Stance : Filter
	{
		[Repeat(4)]
		public StanceSeq[] Value;

		public bool Either;
	}

	public sealed class Prop : Filter
	{
		public string Field;  //creature_field

		public Op Op;

		public long Value;
	}

	public sealed class PropPercent : Filter
	{
		public string Field;

		public Op Op;

		public sbyte Value;
	}

	public sealed class PropFlag : Filter
	{
		public string Field;

		public Op Op;

		public bool Flag;
	}

	public sealed class EffectFlag : Filter
	{
		//public FlagTypeSeq FlagType;

		public bool Flag;
	}

	public sealed class Faction : Filter
	{
		public Record.Faction Value;
	}

	public sealed class ActiveFaction : Filter
	{
		public Record.Faction Value;
	}

	public sealed class FactionReputation : Filter
	{
		public Op Op;

		public short Value;
	}

	public sealed class FactionLevel : Filter
	{
		public Op Op;

		public short Value;
	}

	public sealed class EffectAttribute : Filter
	{
		[Repeat(4)]
		public EffectAttributeSeq[] Value;

		public bool Either;
	}

	public sealed class weaponType : Filter
	{
		[Repeat(4)]
		public Item.WeaponTypeSeq[] WeaponType;

		public bool Either;
	}

	public sealed class Inventory : Filter
	{
		public Item Item;

		public sbyte Amount;
	}

	public sealed class FieldItem : Filter
	{
		public Record.FieldItem fieldItem;
	}

	public sealed class NpcId : Filter
	{
		public Npc Value;
	}

	public sealed class NpcConvoy : Filter
	{
		public bool Convoy;
	}

	public sealed class EnvId : Filter
	{
		public ZoneEnv2Spawn Env2spawn;
	}

	public sealed class envState : Filter
	{
		public EnvState EnvState;
	}

	public sealed class EnvPrestate : Filter
	{
		public EnvState EnvState;
	}

	public sealed class EnvHpPercent : Filter
	{
		public Op Op;

		public sbyte Value;
	}

	public sealed class Skill : Filter
	{
		public Record.Skill Value;
	}

	public sealed class SkillId : Filter
	{
		public Record.Skill Value;
	}

	public sealed class Skill3 : Filter
	{
		public Record.Skill3 Value;
	}

	public sealed class Skill3Id : Filter
	{
		public Record.Skill3 Value;
	}

	public sealed class EffectId : Filter
	{
		public Record.Effect Value;
	}

	public sealed class EffectStackCount : Filter
	{
		//public EffectTypeSeq EffectType;

		//public EffectSlot EffectSlot;

		//public TermOp TermOp;


		[Repeat(2)]
		public Op[] Op;

		[Repeat(2)]
		public long[] Value;
	}

	public sealed class QuestComplete : Filter
	{
		public Quest Quest;

		public sbyte MissionStep;

		public short Count;

		public Op CountOp = Op.ge;
	}

	public sealed class QuestNotComplete : Filter
	{
		public Quest Quest;
	}

	public sealed class ContentQuotaCharge : Filter
	{
		public ContentQuota ContentQuota;

		public Op Op;

		public long ChargeValue;
	}

	public sealed class Cinematic : Filter
	{
		public Record.Cinematic Value;

	}



	public sealed class NpcSpawn : Filter
	{
		[Side(ReleaseSide.Server)]
		public string Spawn;
	}

	public sealed class NpcParty : Filter
	{
		[Side(ReleaseSide.Server)]
		public bool Leader;

		[Side(ReleaseSide.Server)]
		public Script_obj Party;
	}
	#endregion
}