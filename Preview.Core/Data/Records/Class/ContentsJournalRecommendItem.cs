using Xylia.Preview.Common.Attribute;

namespace Xylia.Preview.Data.Record;

[AliasRecord]
public sealed class ContentsJournalRecommendItem : BaseRecord
{
	public string Alias;

	public int WeaponRecommendScore;
	public int NecklaceRecommendScore;
	public int EarringRecommendScore;
	public int RingRecommendScore;
	public int BraceletRecommendScore;
	public int BeltRecommendScore;
	public int GlovesRecommendScore;
	public int Soul1RecommendScore;
	public int Soul2RecommendScore;
	public int PetRecommendScore;
	public int Rune1RecommendScore;
	public int Rune2RecommendScore;
	public int NovaRecommendScore;
	public int SkillStone1RecommendScore;
	public int SkillStone2RecommendScore;
}