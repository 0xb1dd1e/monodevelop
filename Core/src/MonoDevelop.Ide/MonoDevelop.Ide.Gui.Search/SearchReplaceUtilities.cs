//  SearchReplaceUtilities.cs
//
//  This file was derived from a file from #Develop. 
//
//  Copyright (C) 2001-2007 Mike Krüger <mkrueger@novell.com>
// 
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA

using System;
using System.Collections;
using MonoDevelop.Core.Gui;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Content;

namespace MonoDevelop.Ide.Gui.Search
{
	internal sealed class SearchReplaceUtilities
	{
		public static bool IsTextAreaSelected {
			get {
				return IdeApp.Workbench.ActiveDocument != null && (IdeApp.Workbench.ActiveDocument.GetContent<ITextBuffer>() != null);
			}
		}
		
		public static bool IsWordSeparator (char c)
		{
			return Char.IsWhiteSpace (c) || (Char.IsPunctuation (c) && c != '_');
		}
		
/*		public static bool IsWholeWordAt(SourceEditorBuffer document, int offset, int length)
		{
			return (offset - 1 < 0 || IsWordSeparator (document.GetCharAt(offset - 1))) &&
			       (offset + length + 1 >= document.Length || IsWordSeparator (document.GetCharAt(offset + length)));
		}
*/
		public static bool IsWholeWordAt (ITextIterator it, int length)
		{
			char c = it.GetCharRelative (-1);
			if (c != char.MinValue && !IsWordSeparator (c)) return false;
			
			c = it.GetCharRelative (length);
			return (c == char.MinValue || IsWordSeparator (c));
		}

		/*public static int CalcCurrentOffset(IDocument document) 
		{
//			TODO:
//			int endOffset = document.Caret.Offset % document.TextLength;
//			return endOffset;
			return 0;
		}*/
		
		public static ISearchStrategy CreateSearchStrategy(SearchStrategyType type)
		{
			switch (type) {
				case SearchStrategyType.None:
					return null;
				case SearchStrategyType.Normal:
					return new BruteForceSearchStrategy(); // new KMPSearchStrategy();
				case SearchStrategyType.RegEx:
					return new RegExSearchStrategy();
				case SearchStrategyType.Wildcard:
					return new WildcardSearchStrategy();
				default:
					throw new System.NotImplementedException("CreateSearchStrategy for type " + type);
			}
		}
		
		
		public static IDocumentIterator CreateDocumentIterator(DocumentIteratorType type)
		{
			switch (type) {
				case DocumentIteratorType.None:
					return null;
				case DocumentIteratorType.CurrentDocument:
					return new CurrentDocumentIterator();
				case DocumentIteratorType.Directory:
					return new DirectoryDocumentIterator(SearchReplaceInFilesManager.SearchOptions.SearchDirectory, 
					                                     SearchReplaceInFilesManager.SearchOptions.FileMask, 
					                                     SearchReplaceInFilesManager.SearchOptions.SearchSubdirectories);
				case DocumentIteratorType.AllOpenFiles:
					return new AllOpenDocumentIterator();
				case DocumentIteratorType.WholeCombine:
					return new WholeProjectDocumentIterator();
				default:
					throw new System.NotImplementedException("CreateDocumentIterator for type " + type);
			}
		}
	}
	
}
