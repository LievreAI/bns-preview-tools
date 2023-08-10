using Xylia.Preview.Common.Attribute;
using Xylia.Preview.Common.Interface;

namespace Xylia.Preview.Data.Record;
[AliasRecord]
public class FieldZone : BaseRecord, IAttraction
{
	public short Id;

	public string Alias;

	[Repeat(30)]
	public Zone[] Zone;

	public AttractionGroup Group;

	[Repeat(5)]
	public Quest[] AttractionQuest;

	public bool UiFilterAttractionQuestOnly;

	public Text RespawnConfirmText;

	public Text Name2;

	public Text Desc;

	public sbyte UiTextGrade;

	public AttractionRewardSummary RewardSummary;

	public sealed class Normal : FieldZone
	{
	}

	public sealed class GuildBattleFieldEntrance : FieldZone
	{
		public GuildBattleFieldZone GuildBattleFieldZone;

		public sbyte MinFixedChannel;
	}


	#region Interface
	public string GetName() => this.Name2.GetText();

	public string GetDescribe() => this.Desc.GetText();
	#endregion
}