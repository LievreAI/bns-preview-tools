﻿using System.Windows.Threading;

using HZH_Controls.Forms;

using Xylia.Preview.Data.Helper;
using Xylia.Preview.Data.Models.DatData;
using Xylia.Preview.Data.Models.DatData.DataProvider;
using Xylia.Preview.UI.Extension;

namespace Xylia.Preview.Helper;
public static class Register
{
	public static Dispatcher Dispatcher;

	public static void Main()
	{
		Dispatcher = Dispatcher.CurrentDispatcher;

		PreviewRegister.PreviewEvent += new((obj, w) => obj.PreviewShow(w));
		PreviewRegister.ShowTipEvent += new((s, e) => FrmTips.ShowTipsSuccess((string)s));

		DefaultProvider.select = new(() => new DatSelect());
	}
}