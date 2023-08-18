using System.IO;

using Xylia.Configure;
using Xylia.Extension;
using Xylia.Preview.Data.Models.DatData.DataProvider;
using Xylia.Preview.Data.Models.DatData.DatDetect;
using Xylia.Windows.CustomException;

namespace Xylia.Preview.Data.Models.DatData;
public partial class DatSelect : Form, IDatSelect
{
	#region Fields
	public DatSelect() => InitializeComponent();

	private IEnumerable<FileInfo> list_xml;
	private IEnumerable<FileInfo> list_local;

	public string XML_Select;
	public string Local_Select;
	#endregion

	#region Functions (UI)
	private void Select_Load(object sender, EventArgs e)
	{
		Chk_HidenBpFiles_CheckedChanged(null, null);
	}

	private void Btn_Confirm_Click(object sender, EventArgs e)
	{
		XML_Select = comboBox1.Text.Replace("...", @"contents\Local");
		Local_Select = comboBox2.Text.Replace("...", @"contents\Local");

		this.DialogResult = DialogResult.OK;
		this.TimeInfo.Enabled = this.NoResponse.Enabled = false;
	}

	private void Btn_Cancel_Click(object sender, EventArgs e)
	{
		this.DialogResult = DialogResult.Cancel;
	}

	private void DataSelect_MouseEnter(object sender, EventArgs e)
	{
		StopCountDown();
		LastActTime = DateTime.Now;
	}

	private void TimeInfo_VisibleChanged(object sender, EventArgs e)
	{
		this.Chk_HidenBpFiles.Visible = !this.TimeInfo.Visible;
	}

	private void Chk_HidenBpFiles_CheckedChanged(object sender, EventArgs e)
	{
		Load_Cmb(comboBox1, list_xml);
		Load_Cmb(comboBox2, list_local);
	}

	private void Chk_64bit_CheckedChanged(object sender, EventArgs e)
	{
		Ini.Instance.WriteValue("DatSelect", "64bit", this.Chk_64bit.Checked);

		Chk_HidenBpFiles_CheckedChanged(null, null);
	}

	private void Load_Cmb(ComboBox Cmb, IEnumerable<FileInfo> FileCollection)
	{
		Cmb.Items.Clear();

		foreach (var w in FileCollection.GetFiles(this.Chk_64bit.Checked).Select(f => f.FullName))
		{
			var s = w.Replace(@"contents\Local", "...");

			//hide back files
			if (!Chk_HidenBpFiles.Checked) Cmb.Items.Add(s);
			else if (!s.Contains("backup")) Cmb.Items.Add(s);
		}

		if (Cmb.Items.Count > 0) Cmb.Text = Cmb.Items[0].ToString();

		Cmb.Enabled = Cmb.Items.Count != 1;
	}
	#endregion

	#region Functions (CountDown)
	/// <summary>
	/// 最后活动时间
	/// </summary>
	DateTime LastActTime = DateTime.Now;

	/// <summary>
	/// 倒计时启动时间
	/// </summary>
	DateTime dt = DateTime.Now;

	/// <summary>
	/// 倒计时总秒数
	/// </summary>
	readonly int CountDownSec = 10;

	/// <summary>
	/// 无响应上限时长
	/// </summary>
	readonly int NoResponseSec = 15;


	private void StartCountDown()
	{
		TimeInfo.Text = null;

		dt = DateTime.Now;
		this.CountDown.Enabled = true;
		this.TimeInfo.Visible = true;
	}

	private void StopCountDown()
	{
		this.CountDown.Enabled = false;
		this.TimeInfo.Visible = false;
	}

	private void DataSelect_Shown(object sender, EventArgs e)
	{
		StartCountDown();
		LastActTime = DateTime.Now;
		this.NoResponse.Enabled = true;
	}

	private void NoResponse_Tick(object sender, EventArgs e)
	{
		int CurNoResponseSec = (int)DateTime.Now.Subtract(LastActTime).TotalSeconds;
		if (CurNoResponseSec >= NoResponseSec)
		{
			StartCountDown();
			LastActTime = DateTime.Now;
		}
	}

	private void Timer_Tick(object sender, EventArgs e)
	{
		int RemainSec = CountDownSec - (int)DateTime.Now.Subtract(dt).TotalSeconds;
		TimeInfo.Text = $"将在 {RemainSec} 秒后自动选择";

		//自动选择
		if (RemainSec <= 0) Btn_Confirm_Click(null, null);
	}
	#endregion



	public DefaultProvider Show(IEnumerable<FileInfo> Xml, IEnumerable<FileInfo> Local)
	{
		this.list_xml = Xml;
		this.list_local = Local;
		this.Chk_64bit.Checked = Ini.Instance.ReadValue("DatSelect", "64bit").ToBool();

		if (!Xml.Has32bit() && !Local.Has32bit())
		{
			Chk_64bit.Enabled = false;
			Chk_64bit.Checked = true;
		}
		else if (!Xml.Has64bit() && !Local.Has64bit())
		{
			Chk_64bit.Enabled = false;
			Chk_64bit.Checked = false;
		}



		if (this.ShowDialog() != DialogResult.OK) throw new UserExitException();
		else return new DefaultProvider()
		{
			is64Bit = Chk_64bit.Checked,
			XmlData = XML_Select,
			LocalData = Local_Select,
		};
	}
}