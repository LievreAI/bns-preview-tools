using Xylia.Preview.Common.Attribute;
using Xylia.Preview.Common.Interface;

namespace Xylia.Preview.Data.Record;
[AliasRecord]
public sealed class GuildBattleFieldZone : BaseRecord, IAttraction
{
	public Text GuildBattleFieldZoneName2;

	public Text GuildBattleFieldZoneDesc;

	public string ThumbnailImage;

	public AttractionRewardSummary RewardSummary;


	#region Interface
	public string GetName() => this.GuildBattleFieldZoneName2.GetText();

	public string GetDescribe() => this.GuildBattleFieldZoneDesc.GetText();
	#endregion
}