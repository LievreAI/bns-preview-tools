using System.Web;
using System.Windows;
using System.Xml;

using Xylia.Extension;
using Xylia.Preview.Common.Arg;
using Xylia.Preview.Data.Helper;
using Xylia.Preview.Data.Models.DatData.DataProvider;
using Xylia.Xml;

namespace Xylia.Preview.GameUI.Scene.Game_CharacterInfo;
public partial class Game_CharacterInfo_Scene : Window
{
	public Game_CharacterInfo_Scene()
	{
		InitializeComponent();

		FileCache.Data.LoadData(false);
		XmlDoc = (FileCache.Data.Provider as DefaultProvider).ConfigData.EnumerateFiles("release.config2.xml").FirstOrDefault()?.Xml.Nodes;

		InitUrl(new Creature() { WorldId = 1911, Name = "天靑色等煙雨乀" });
	}

	private async void WebView_WebBrowserInitialized(object sender, EventArgs e)
	{
		await WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync($$"""
onmouseover = (e) => {
    var obj = e.target; 
	if (obj.tagName=='IMG' && obj.classList=="")
		chrome.webview.hostObjects.WebObject.Message(obj.getAttribute('title')); 
};
""");
	}

	private async void WebView_PostMessage(object sender, string meaasge)
	{
		var uri = new Uri(meaasge);
		if (uri.Scheme == "nc")
		{
			var query = HttpUtility.ParseQueryString(uri.Query);
			if (uri.Host == "bns.charinfo" && uri.AbsolutePath == "/ItemTooltip")
			{
				var data = query["item"].Split('.').Select(int.Parse).ToArray();
				Trace.WriteLine(data.Aggregate("", (sum, now) => sum + now + ";"));

				//Task.Run(() => FileCache.Data.Item[data[0], data[1]].PreviewShow());
			}
		}
	}



	XmlDocument XmlDoc;

	public void InitUrl(Creature creature)
	{
		var group = XmlDoc.SelectSingleNode("config/group[@name='in-game-web']");

		var CharacterInfoUrl = group.SelectSingleNode("./option[@name='character-info-url']").GetValue();
		var CharacterInfoUrl2 = group.SelectSingleNode("./option[@name='character-info-url-2']").GetValue();

		var CharacterInfoHomeUrn = group.SelectSingleNode("./option[@name='character-info-home-urn']").GetValue();
		var CharacterInfoOtherHomeUrn = group.SelectSingleNode("./option[@name='character-info-other-home-urn']").GetValue();
		var CharacterInfoDiffHomeUrn = group.SelectSingleNode("./option[@name='character-info-diff-home-urn']").GetValue();

		WebView.Source = new UriBuilder(CharacterInfoUrl.Replace("%s", creature.WorldId.ToString()[..2]) + CharacterInfoHomeUrn) { Query = $"c={creature.Name}&s={creature.WorldId}" }.Uri;
	}
}