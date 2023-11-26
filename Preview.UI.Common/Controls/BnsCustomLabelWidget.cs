﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

using Xylia.Preview.UI.Documents;

namespace Xylia.Preview.UI.Controls;

[ContentProperty("Text")]
public class BnsCustomLabelWidget : BnsCustomBaseWidget
{
	#region Ctor
	static BnsCustomLabelWidget()
	{
		// some tag is special in real html
		// those are non-closed, so must to re-set in bHtml
		HtmlAgilityPack.HtmlNode.ElementsFlags.Remove("link");
	}

	public BnsCustomLabelWidget()
	{
		// Create TextContainer associated with it
		TextDocument container = new TextDocument(this, true);
		//container.CollectTextChanges = true;
		_container = container;
		_container.Changed += new EventHandler(OnContainerChanged);

		Params.Changed += new EventHandler(OnContainerChanged);
		Document = new();
	}
	#endregion


	#region Public Properties
	private TextDocument _container;

	/// <summary>
	/// content container
	/// </summary>
	protected Paragraph Document { get; set; }

	public Dictionary<int, Timer> Timers { get; } = new();
	#endregion

	#region Dependency Properties
	public static CopyMode CopyMode;

	/// <summary>
	/// DependencyProperty for <see cref="Text" /> property.
	/// </summary>
	public static readonly DependencyProperty TextProperty =
		DependencyProperty.Register("Text", typeof(string), typeof(BnsCustomLabelWidget),
			new FrameworkPropertyMetadata(string.Empty,
			   FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
			   OnTextChanged));

	/// <summary>
	/// The Text property defines the content (text) to be displayed.
	/// </summary>
	[Localizability(LocalizationCategory.Text)]
	public string Text
	{
		get { return (string)GetValue(TextProperty); }
		set { SetValue(TextProperty, value); }
	}

	public static readonly DependencyProperty ParamsProperty =
		DependencyProperty.Register("Params", typeof(ContentParams), typeof(BnsCustomLabelWidget),
			   new FrameworkPropertyMetadata(new ContentParams(),
			   FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

	public ContentParams Params
	{
		get { return (ContentParams)GetValue(ParamsProperty); }
		set { SetValue(ParamsProperty, value); }
	}

	/// <summary>
	/// DependencyProperty for <see cref="Orientation" /> property.
	/// </summary>
	public static readonly DependencyProperty OrientationProperty =
		DependencyProperty.Register("Orientation", typeof(Orientation), typeof(BnsCustomLabelWidget),
			new FrameworkPropertyMetadata(Orientation.Vertical,
				FrameworkPropertyMetadataOptions.AffectsMeasure));

	/// <summary>
	/// Specifies dimension of children stacking.
	/// </summary>
	public Orientation Orientation
	{
		get { return (Orientation)GetValue(OrientationProperty); }
		set { SetValue(OrientationProperty, value); }
	}
	#endregion


	#region Protected Methods
	protected override void SetText(string text) => this.Text = text;


	protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
	{
		base.OnMouseDoubleClick(e);

		var text = CopyMode switch
		{
			CopyMode.Trimmed => CutText(Params.Handle(Text)),
			CopyMode.Regular => Params.Handle(Text),
			CopyMode.Original => Text,

			_ => throw new NotSupportedException(),
		};

		if (string.IsNullOrWhiteSpace(text)) return;
		Clipboard.SetText(text);
	}

	/// <summary>
	/// Measure element size
	/// </summary>
	/// <param name="constraint"></param>
	/// <returns></returns>
	protected override Size MeasureOverride(Size constraint)
	{
		if (Document is null) return new Size();

		Element.InheritDependency(this, Document);
		Document.Measure(constraint);
		return Document.DesiredSize;
	}

	/// <summary>
	/// Arrange element rect
	/// </summary>
	/// <param name="arrangeBounds"></param>
	/// <returns></returns>
	protected override Size ArrangeOverride(Size arrangeBounds)
	{
		Document?.Arrange(new Rect(arrangeBounds));
		return arrangeBounds;
	}

	/// <summary>
	/// Render control's content.
	/// </summary>
	/// <param name="ctx">Drawing context.</param>
	protected override void OnRender(DrawingContext ctx)
	{
		ArgumentNullException.ThrowIfNull(ctx);

		// Draw background in rectangle.
		Brush background = Background;
		if (background != null) ctx.DrawRectangle(background, null, new Rect(0, 0, RenderSize.Width, RenderSize.Height));

		// Render child elements
		Document.Render(ctx);
	}
	#endregion

	#region Private Methods
	private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var widget = (BnsCustomLabelWidget)d;
		var text = (string)e.NewValue;

		widget.Document = new Paragraph(text);
	}

	private void OnContainerChanged(object? sender, EventArgs e)
	{
		InvalidateMeasure();
		InvalidateVisual();
	}

	private static string CutText(string Text)
	{
		if (Text is null) return null;

		var CopyTxt = HttpUtility.HtmlDecode(XmlConvert.DecodeName(Text));
		CopyTxt = new Regex(@"<\s*br\s*/\s*>").Replace(CopyTxt, "\n");
		return new Regex(@"<.*?>").Replace(CopyTxt, "");
	}
	#endregion


	public static ContentParams GetParams(UIElement element)
	{
		ArgumentNullException.ThrowIfNull(element);
		return (ContentParams)element.GetValue(BnsCustomLabelWidget.ParamsProperty);
	}

	public static void SetParams(UIElement element, ContentParams value)
	{
		ArgumentNullException.ThrowIfNull(element);
		element.SetValue(BnsCustomLabelWidget.ParamsProperty, value);
	}
}

public enum CopyMode
{
	/// <summary>
	/// Text after replacing parameters and remove other XML parameters 
	/// </summary>
	Trimmed,

	/// <summary>
	/// Text after replacing parameters
	/// </summary>
	Regular,

	/// <summary>
	/// original text
	/// </summary>
	Original,
}