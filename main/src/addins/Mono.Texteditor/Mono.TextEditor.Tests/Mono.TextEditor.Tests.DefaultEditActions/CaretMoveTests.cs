// CaretMoveTests.cs
//
// Author:
//   Mike Krüger <mkrueger@novell.com>
//
// Copyright (c) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using NUnit.Framework;

namespace Mono.TextEditor.Tests
{
	[TestFixture()]
	public class CaretMoveTests
	{
//		
//		[Test()]
//		public void TestCaretMoveDown ()
//		{
//			Document document = new Document ();
//			document.Buffer.Text = "1\n2\n3";
//			
//			Caret caret = new Caret (document);
//			caret.Location = new DocumentLocation (1, 0);
//			new CaretMoveDown ().Run (document, caret);
//			Assert.AreEqual (2, caret.Line);
//			Assert.AreEqual (0, caret.Column);
//		}
//		
//		[Test()]
//		public void TestCaretMoveLeft ()
//		{
//			Document document = new Document ();
//			document.Buffer.Text = "test";
//			
//			Caret caret = new Caret (document);
//			caret.Location = new DocumentLocation (0, 1);
//			new CaretMoveLeft ().Run (document, caret);
//			Assert.AreEqual (0, caret.Line);
//			Assert.AreEqual (0, caret.Column);
//			new CaretMoveLeft ().Run (document, caret);
//			Assert.AreEqual (0, caret.Line);
//			Assert.AreEqual (0, caret.Column);
//		}
//		
//		[Test()]
//		public void TestCaretMoveRight ()
//		{
//			Document document = new Document ();
//			document.Buffer.Text = "test";
//			
//			Caret caret = new Caret (document);
//			caret.Location = new DocumentLocation (0, 3);
//			new CaretMoveRight ().Run (document, caret);
//			Assert.AreEqual (4, caret.Column);
//		}
		
		[TestFixtureSetUp] 
		public void SetUp()
		{
			Gtk.Application.Init ();
		}
		
		[TestFixtureTearDown] 
		public void Dispose()
		{
		}
		
	}
}
