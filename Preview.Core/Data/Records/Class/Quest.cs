using System.ComponentModel;
using System.Xml;

using CUE4Parse.BNS.Conversion;

using Xylia.Preview.Common.Attribute;
using Xylia.Preview.Common.Seq;
using Xylia.Preview.Data.Helper;
using Xylia.Preview.Data.Record.QuestData;
using Xylia.Preview.Data.Record.QuestData.Enums;

namespace Xylia.Preview.Data.Record;

[AliasRecord]
public sealed class Quest : BaseRecord
{
	#region Fields
	public Lazy<List<Acquisition>> Acquisition;

	public Lazy<List<MissionStep>> MissionStep;

	public Lazy<List<Completion>> Completion;

	public Lazy<List<Transit>> Transit;

	public Lazy<List<Complete>> Complete;

	public Lazy<List<GiveupLoss>> GiveupLoss;


	public int id;

	public sbyte MaxRepeat;

	[Side(ReleaseSide.Client)]
	public string Name;

	[Side(ReleaseSide.Client)]
	public Text Name2;

	[Signal("district-1")]
	public District District1;

	[Signal("district-2")]
	public District District2;

	[Signal("map-group-1-1")]
	public MapGroup1 Map_Group_1_1;

	[Signal("map-group-1-2")]
	public MapGroup1 Map_Group_1_2;

	[Side(ReleaseSide.Client)]
	public string Group;

	[Side(ReleaseSide.Client)]
	public Text Group2;

	[Side(ReleaseSide.Client)]
	public Text Desc;

	[Side(ReleaseSide.Client)]
	public Text CompletedDesc;

	public Category Category;

	public bool CompletedList;

	[Side(ReleaseSide.Client)]
	public /*Grade*/ sbyte Grade;

	public bool Tutorial;

	[Side(ReleaseSide.Client)]
	public bool ShowTutorialTag;

	public sbyte LastMissionStep;

	public bool EffectExist;


	public QuestDayOfWeekSeq DayOfWeek;

	public enum QuestDayOfWeekSeq
	{
		None,

		Daily,

		Weekly,

		Monthly,

		Mon,

		Tue,

		Wed,

		The,

		Fri,

		Sat,

		Sun,

		[Signal("sat-sun")]
		SatSun,

		[Signal("fri-sat-sun")]
		FriSatSun,
	}



	public ResetType ResetType;
	public ResetType2 ResetByAcquireTime;
	public BDayOfWeek ResetDayOfWeek;
	public sbyte ResetDayOfMonth;

	public Faction ActivatedFaction;
	public Faction MainFaction;

	public ProductionType Production;

	public SaveType SaveType;

	[Side(ReleaseSide.Client)]
	public bool InvokeFxMsg;

	public Dungeon Dungeon;

	public DungeonTypeSeq DungeonType;

	public enum DungeonTypeSeq
	{
		unbind,

		bind,
	}


	public CraftType CraftType;

	public ContentType ContentType;

	public bool Retired;

	[Repeat(3) , DefaultValue(true)]
	public bool[] ProgressDifficultyType;

	[DefaultValue(true)]
	public bool ProgressDifficultyTypeAlways = true;

	[Repeat(4)]
	public BaseRecord[] Attraction;

	public BaseRecord AttractionInfo;

	[Obsolete]
	public bool ResetEnable;

	[Obsolete]
	public bool ResetMoney;

	[Repeat(value: 4) , Obsolete]
	public Item[] ResetItem;

	[Repeat(value: 4), Obsolete]
	public sbyte[] ResetItemCount;

	[Side(ReleaseSide.Client)]
	public TalkSocial AcquireTalksocial;

	[Side(ReleaseSide.Client)]
	public float AcquireTalksocialDelay;

	[Side(ReleaseSide.Client)]
	public TalkSocial CompleteTalksocial;

	[Side(ReleaseSide.Client)]
	public float CompleteTalksocialDelay;

	public bool CheckVitality;

	public short ValidDateStartYear;
	public sbyte ValidDateStartMonth;
	public sbyte ValidDateStartDay;

	public short ValidDateEndYear;
	public sbyte ValidDateEndMonth;
	public sbyte ValidDateEndDay;

	public sbyte ValidTimeStartHour;
	public sbyte ValidTimeEndHour;


	public bool ValidDayofweekSun;
	public bool ValidDayofweekMon;
	public bool ValidDayofweekTue;
	public bool ValidDayofweekWed;
	public bool ValidDayofweekThu;
	public bool ValidDayofweekFri;
	public bool ValidDayofweekSat;

	public string ReplayEpicOriginal;
	public bool ReplayEpicStartPoint;

	public ZonePcSpawn ReplayEpicPcspawn;

	public Dungeon Dungeon2;

	public sbyte DuelMissionSteps;
	public sbyte DuelMissions;
	public sbyte DuelCases;
	public short DuelCaseSubtypes;

	public sbyte ExceedLevelNextLevel;

	public ContentsReset ContentsReset;



	[DefaultValue(BroadcastCategory.None)]
	public BroadcastCategory BroadcastCategory;

	[Repeat(3) , Side(ReleaseSide.Server)]
	public Achievement[] ExtraQuestCompleteAchievement;

	[Side(ReleaseSide.Server)]
	public string ReplayEpicZoneLeaveCinematic;

	public bool CinemaCheck;

	public bool ReplayCheck;
	#endregion



	#region	Properties
	public Color ForeColor
	{
		get
		{
			if (Retired) return Color.Red;

			var RecommendedLevel = Acquisition.Value?.FirstOrDefault()?.RecommendedLevel ?? 0;
			if (RecommendedLevel < 60 - 10) return Color.Gray;

			return Color.LightGreen;
		}
	}

	public Image FrontIcon
	{
		get
		{
			string respath()
			{
				bool IsRepeat = ResetType != ResetType.None;
				switch (Category)
				{
					case Category.Epic: return "Map_Epic_Start";
					case Category.Job: return "Map_Job_Start";
					case Category.Dungeon: return null;
					case Category.Attraction: return "Map_attraction_start";
					case Category.TendencySimple: return "Map_System_start";
					case Category.TendencyTendency: return "Map_System_start";
					case Category.Mentoring: return "mento_mentoring_start";
					case Category.Hunting: return IsRepeat ? "Map_Hunting_repeat_start" : "Map_Hunting_start";
					case Category.Normal:
					{
						//faction quest
						if (MainFaction?.alias != null)
							return IsRepeat ? "Map_Faction_repeat_start" : "Map_Faction_start";

						return ContentType switch
						{
							ContentType.Festival => IsRepeat ? "Map_Festival_repeat_start" : "Map_Festival_start",
							ContentType.Duel or ContentType.PartyBattle => IsRepeat ? "Map_Faction_repeat_start" : "Map_Faction_start",
							ContentType.SideEpisode => "Map_side_episode_start",
							ContentType.Special => "Map_Job_Start",

							_ => IsRepeat ? "Map_Repeat_start" : "Map_Normal_Start",
						};
					}

					default: throw new NotImplementedException();
				}
			}


			var res = respath();
			if (res is null) return null;

			return FileCache.Provider.LoadObject($"BNSR/Content/Art/UI/GameUI/Resource/GameUI_Map_Indicator/{res}")?.GetImage();
		}
	}
	#endregion


	#region Functions
	public override void LoadData(XmlElement data)
	{
		base.LoadData(data);

		Acquisition = new(() => LoadChildren<Acquisition>(data, "acquisition"));
		MissionStep = new(() => LoadChildren<MissionStep>(data, "mission-step"));
		Transit = new(() => LoadChildren<Transit>(data, "transit"));
		Completion = new(() => LoadChildren<Completion>(data, "completion"));
		GiveupLoss = new(() => LoadChildren<GiveupLoss>(data, "giveup-loss"));
		Complete = new(() => LoadChildren<Complete>(data, "complete"));
	}

	public static void GetEpic(Action<Quest> act, JobSeq TargetJob = JobSeq.소환사) => GetEpic("q_epic_221", act, TargetJob);

	public static void GetEpic(string alias, Action<Quest> act, JobSeq TargetJob = JobSeq.소환사)
	{
		var quest = FileCache.Data.Quest[alias];
		if (quest is null) return;

		// act
		act(quest);

		// get next
		var Completion = quest.Completion.Value?.FirstOrDefault();
		if (Completion is null) return;
		foreach (var NextQuest in Completion.NextQuest)
		{
			var jobs = NextQuest.Job;
			if(jobs is null || jobs[0] == JobSeq.JobNone || jobs.FirstOrDefault(job => job == TargetJob) != JobSeq.JobNone)
				GetEpic(NextQuest.Quest?.alias, act, TargetJob);
		}
	}
	#endregion
}