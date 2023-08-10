using Xylia.Preview.Common.Attribute;
using Xylia.Preview.Common.Interface;

namespace Xylia.Preview.Data.Record;
public sealed class ClassicFieldZone : BaseRecord, IAttraction
{
	public short Id;

	public string Alias;

	[Repeat(2)]
	public Zone[] Zone;

	public AttractionGroup Group;

	[Repeat(5)]
	public Quest[] AttractionQuest;

	public bool UiFilterAttractionQuestOnly;

	public Text RespawnConfirmText;

	public Text EscapeCaveConfirmText;

	public short RecommendAttackPower;

	public Item StandardGearWeapon;

	public Text ClassicFieldZoneName2;
	public Text ClassicFieldZoneDesc;

	public string ThumbnailImage;

	public AttractionRewardSummary RewardSummary;

	public sbyte UiTextGrade;

	public Text Tactic;

	public ContentsJournalRecommendItem RecommendAlias;

	public sbyte RecommendLevelMin;
	public sbyte RecommendLevelMax;
	public sbyte RecommendMasteryLevelMin;
	public sbyte RecommendMasteryLevelMax;



	#region Interface
	public string GetName() => this.ClassicFieldZoneName2.GetText();

	public string GetDescribe() => this.ClassicFieldZoneDesc.GetText();
	#endregion
}