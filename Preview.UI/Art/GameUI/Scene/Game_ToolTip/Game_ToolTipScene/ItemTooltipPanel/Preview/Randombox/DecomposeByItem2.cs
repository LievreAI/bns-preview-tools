using Xylia.Preview.Data.Helper;
using Xylia.Preview.Data.Record;

namespace Xylia.Preview.GameUI.Scene.Game_ToolTipScene.ItemTooltipPanel.Preview.Randombox;
public sealed class DecomposeByItem2
{
    private readonly string _item;
	public int StackCount = 0;
	public Item Item => FileCache.Data.Item[_item];

	public DecomposeByItem2(string Item, int StackCount)
    {
        this._item = Item;
        this.StackCount = StackCount;
    }
}