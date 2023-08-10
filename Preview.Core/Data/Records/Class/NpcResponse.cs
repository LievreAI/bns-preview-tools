using Xylia.Preview.Common.Attribute;

namespace Xylia.Preview.Data.Record;

[AliasRecord]
public sealed class NpcResponse : BaseRecord
{
	public string Alias;

	public FactionCheckTypeSeq FactionCheckType;

	[Repeat(2)]
	public Faction[] Faction;

	public Quest RequiredCompleteQuest;
	public FactionLevelCheckTypeSeq FactionLevelCheckType;
	public NpcTalkMessage TalkMessage;
	public IndicatorSocial IndicatorSocial;
	public Social ApproachSocial;
	public IndicatorIdle Idle;
	public bool IdleVisible;
	public Social IdleStart;
	public Social IdleEnd;


	public enum FactionCheckTypeSeq : byte
	{
		Is,
		IsNot,
		IsNone,
	}

	public enum FactionLevelCheckTypeSeq : byte
	{
		None,
		CheckForSuccess,
		CheckForFail,
	}
}