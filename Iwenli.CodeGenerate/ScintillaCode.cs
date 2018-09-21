﻿using ScintillaNET;
using System.Drawing;

namespace Iwenli.CodeGenerate
{
	/// <summary>
	/// Scintilla相关代码
	/// </summary>
	public partial class MainForm
	{
		/// <summary>
		/// 初始化Scintilla配置
		/// </summary>
		/// <param name="scintilla"></param>
		public void InitScintilla(Scintilla scintilla)
		{
			InitColors(scintilla);
			InitSyntaxColoring(scintilla);
			InitNumberMargin(scintilla);
			InitBookmarkMargin(scintilla);
			InitCodeFolding(scintilla);
		}

		#region Scintilla
		private void InitColors(Scintilla scintilla)
		{
			scintilla.SetSelectionBackColor(true, IntToColor(0x114D9C));
		}

		private void InitSyntaxColoring(Scintilla scintilla)
		{

			// Configure the default style
			scintilla.StyleResetDefault();
			scintilla.Styles[Style.Default].Font = "Consolas";
			scintilla.Styles[Style.Default].Size = 10;
			scintilla.Styles[Style.Default].BackColor = IntToColor(0x212121);
			scintilla.Styles[Style.Default].ForeColor = IntToColor(0xFFFFFF);
			scintilla.StyleClearAll();

			// Configure the CPP (C#) lexer styles
			scintilla.Styles[Style.Cpp.Identifier].ForeColor = IntToColor(0xD0DAE2);
			scintilla.Styles[Style.Cpp.Comment].ForeColor = IntToColor(0xBD758B);
			scintilla.Styles[Style.Cpp.CommentLine].ForeColor = IntToColor(0x40BF57);
			scintilla.Styles[Style.Cpp.CommentDoc].ForeColor = IntToColor(0x2FAE35);
			scintilla.Styles[Style.Cpp.Number].ForeColor = IntToColor(0xFFFF00);
			scintilla.Styles[Style.Cpp.String].ForeColor = IntToColor(0xFFFF00);
			scintilla.Styles[Style.Cpp.Character].ForeColor = IntToColor(0xE95454);
			scintilla.Styles[Style.Cpp.Preprocessor].ForeColor = IntToColor(0x8AAFEE);
			scintilla.Styles[Style.Cpp.Operator].ForeColor = IntToColor(0xE0E0E0);
			scintilla.Styles[Style.Cpp.Regex].ForeColor = IntToColor(0xff00ff);
			scintilla.Styles[Style.Cpp.CommentLineDoc].ForeColor = IntToColor(0x77A7DB);
			scintilla.Styles[Style.Cpp.Word].ForeColor = IntToColor(0x48A8EE);
			scintilla.Styles[Style.Cpp.Word2].ForeColor = IntToColor(0xF98906);
			scintilla.Styles[Style.Cpp.CommentDocKeyword].ForeColor = IntToColor(0xB3D991);
			scintilla.Styles[Style.Cpp.CommentDocKeywordError].ForeColor = IntToColor(0xFF0000);
			scintilla.Styles[Style.Cpp.GlobalClass].ForeColor = IntToColor(0x48A8EE);

			scintilla.Lexer = Lexer.Cpp;

			scintilla.SetKeywords(0, "class extends implements import interface new case do while else if for in switch throw get set function var try catch finally while with default break continue delete return each const namespace package include use is as instanceof typeof author copy default deprecated eventType example exampleText exception haxe inheritDoc internal link mtasc mxmlc param private return see serial serialData serialField since throws usage version langversion playerversion productversion dynamic private public partial static intrinsic internal native override protected AS3 final super this arguments null Infinity NaN undefined true false abstract as base bool break by byte case catch char checked class const continue decimal default delegate do double descending explicit event extern else enum false finally fixed float for foreach from goto group if implicit in int interface internal into is lock long new null namespace object operator out override orderby params private protected public readonly ref return switch struct sbyte sealed short sizeof stackalloc static string select this throw true try typeof uint ulong unchecked unsafe ushort using var virtual volatile void while where yield");
			scintilla.SetKeywords(1, "void Null ArgumentError arguments Array Boolean Class Date DefinitionError Error EvalError Function int Math Namespace Number Object RangeError ReferenceError RegExp SecurityError String SyntaxError TypeError uint XML XMLList Boolean Byte Char DateTime Decimal Double Int16 Int32 Int64 IntPtr SByte Single UInt16 UInt32 UInt64 UIntPtr Void Path File System Windows Forms ScintillaNET");

		}

		#region Numbers, Bookmarks, Code Folding

		/// <summary>
		/// the background color of the text area
		/// </summary>
		private const int BACK_COLOR = 0x2A211C;

		/// <summary>
		/// default text color of the text area
		/// </summary>
		private const int FORE_COLOR = 0xB7B7B7;

		/// <summary>
		/// change this to whatever margin you want the line numbers to show in
		/// </summary>
		private const int NUMBER_MARGIN = 1;

		/// <summary>
		/// change this to whatever margin you want the bookmarks/breakpoints to show in
		/// </summary>
		private const int BOOKMARK_MARGIN = 2;
		private const int BOOKMARK_MARKER = 2;

		/// <summary>
		/// change this to whatever margin you want the code folding tree (+/-) to show in
		/// </summary>
		private const int FOLDING_MARGIN = 3;

		/// <summary>
		/// set this true to show circular buttons for code folding (the [+] and [-] buttons on the margin)
		/// </summary>
		private const bool CODEFOLDING_CIRCULAR = true;

		private void InitNumberMargin(Scintilla scintilla)
		{

			scintilla.Styles[Style.LineNumber].BackColor = IntToColor(BACK_COLOR);
			scintilla.Styles[Style.LineNumber].ForeColor = IntToColor(FORE_COLOR);
			scintilla.Styles[Style.IndentGuide].ForeColor = IntToColor(FORE_COLOR);
			scintilla.Styles[Style.IndentGuide].BackColor = IntToColor(BACK_COLOR);

			var nums = scintilla.Margins[NUMBER_MARGIN];
			nums.Width = 30;
			nums.Type = MarginType.Number;
			nums.Sensitive = true;
			nums.Mask = 0;

			scintilla.MarginClick += TextArea_MarginClick;
		}

		private void InitBookmarkMargin(Scintilla scintilla)
		{

			//TextArea.SetFoldMarginColor(true, IntToColor(BACK_COLOR));

			var margin = scintilla.Margins[BOOKMARK_MARGIN];
			margin.Width = 20;
			margin.Sensitive = true;
			margin.Type = MarginType.Symbol;
			margin.Mask = (1 << BOOKMARK_MARKER);
			//margin.Cursor = MarginCursor.Arrow;

			var marker = scintilla.Markers[BOOKMARK_MARKER];
			marker.Symbol = MarkerSymbol.Circle;
			marker.SetBackColor(IntToColor(0xFF003B));
			marker.SetForeColor(IntToColor(0x000000));
			marker.SetAlpha(100);

		}

		private void InitCodeFolding(Scintilla scintilla)
		{

			scintilla.SetFoldMarginColor(true, IntToColor(BACK_COLOR));
			scintilla.SetFoldMarginHighlightColor(true, IntToColor(BACK_COLOR));

			// Enable code folding
			scintilla.SetProperty("fold", "1");
			scintilla.SetProperty("fold.compact", "1");

			// Configure a margin to display folding symbols
			scintilla.Margins[FOLDING_MARGIN].Type = MarginType.Symbol;
			scintilla.Margins[FOLDING_MARGIN].Mask = Marker.MaskFolders;
			scintilla.Margins[FOLDING_MARGIN].Sensitive = true;
			scintilla.Margins[FOLDING_MARGIN].Width = 20;

			// Set colors for all folding markers
			for (int i = 25; i <= 31; i++)
			{
				scintilla.Markers[i].SetForeColor(IntToColor(BACK_COLOR)); // styles for [+] and [-]
				scintilla.Markers[i].SetBackColor(IntToColor(FORE_COLOR)); // styles for [+] and [-]
			}

			// Configure folding markers with respective symbols
			scintilla.Markers[Marker.Folder].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlus : MarkerSymbol.BoxPlus;
			scintilla.Markers[Marker.FolderOpen].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinus : MarkerSymbol.BoxMinus;
			scintilla.Markers[Marker.FolderEnd].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlusConnected : MarkerSymbol.BoxPlusConnected;
			scintilla.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
			scintilla.Markers[Marker.FolderOpenMid].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinusConnected : MarkerSymbol.BoxMinusConnected;
			scintilla.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
			scintilla.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

			// Enable automatic folding
			scintilla.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);

		}

		private void TextArea_MarginClick(object sender, MarginClickEventArgs e)
		{
			if (e.Margin == BOOKMARK_MARGIN)
			{
				var scintilla = sender as Scintilla;
				// Do we have a marker for this line?
				const uint mask = (1 << BOOKMARK_MARKER);
				var line = scintilla.Lines[scintilla.LineFromPosition(e.Position)];
				if ((line.MarkerGet() & mask) > 0)
				{
					// Remove existing bookmark
					line.MarkerDelete(BOOKMARK_MARKER);
				}
				else
				{
					// Add bookmark
					line.MarkerAdd(BOOKMARK_MARKER);
				}
			}
		}

		#endregion

		#region Utils

		public static Color IntToColor(int rgb)
		{
			return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
		}
		#endregion
		#endregion
	}
}
