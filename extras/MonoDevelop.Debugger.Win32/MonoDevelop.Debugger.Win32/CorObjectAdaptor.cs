// Util.cs
//
// Author:
//   Lluis Sanchez Gual <lluis@novell.com>
//
// Copyright (c) 2008 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using SR = System.Reflection;
using Mono.Debugging.Client;
using Mono.Debugging.Backend;
using Microsoft.Samples.Debugging.CorDebug;
using Mono.Debugging.Evaluation;
using CorElementType = Microsoft.Samples.Debugging.CorDebug.NativeApi.CorElementType;
using CorDebugMappingResult = Microsoft.Samples.Debugging.CorDebug.NativeApi.CorDebugMappingResult;
using System.Diagnostics.SymbolStore;
using Microsoft.Samples.Debugging.CorMetadata;
using MonoDevelop.Core.Collections;

namespace MonoDevelop.Debugger.Win32
{
	public class CorObjectAdaptor: ObjectValueAdaptor
	{
		public override bool IsPrimitive (EvaluationContext ctx, object val)
		{
			return GetRealObject (val) is CorGenericValue;
		}

		public override bool IsArray (EvaluationContext ctx, object val)
		{
			return GetRealObject (val) is CorArrayValue;
		}

		public override bool IsClassInstance (EvaluationContext ctx, object val)
		{
			return GetRealObject (val) is CorObjectValue;
		}

		public override bool IsNull (EvaluationContext ctx, object gval)
		{
			CorValRef val = (CorValRef) gval;
			return val == null || ((val.Val is CorReferenceValue) && ((CorReferenceValue) val.Val).IsNull);
		}

		public override bool IsClass (object type)
		{
			return ((CorType)type).Class != null;
		}

		public override string GetTypeName (EvaluationContext ctx, object gtype)
		{
			CorType type = (CorType) gtype;
			CorEvaluationContext cctx = (CorEvaluationContext) ctx;
			Type t;
			if (CorMetadataImport.CoreTypes.TryGetValue (type.Type, out t))
				return t.FullName;
			try {
				if (type.Type == CorElementType.ELEMENT_TYPE_ARRAY || type.Type == CorElementType.ELEMENT_TYPE_SZARRAY)
					return GetTypeName (ctx, type.FirstTypeParameter) + "[]";

				if (type.Type == CorElementType.ELEMENT_TYPE_BYREF)
					return GetTypeName (ctx, type.FirstTypeParameter) + "&";

				if (type.Type == CorElementType.ELEMENT_TYPE_PTR)
					return GetTypeName (ctx, type.FirstTypeParameter) + "*";

				return type.Class.GetTypeInfo (cctx.Session).FullName;
			}
			catch (Exception ex) {
				Console.WriteLine (ex);
				throw;
			}
		}

		public override object GetValueType (EvaluationContext ctx, object val)
		{
			return GetRealObject (val).ExactType;
		}

		public override object[] GetTypeArgs (EvaluationContext ctx, object type)
		{
			CorType[] types = ((CorType)type).TypeParameters;
			return CastArray<object> (types);
		}

		IEnumerable<Type> GetAllTypes (EvaluationContext gctx)
		{
			CorEvaluationContext ctx = (CorEvaluationContext) gctx;
			foreach (CorModule mod in ctx.Session.GetModules ()) {
				CorMetadataImport mi = ctx.Session.GetMetadataForModule (mod.Name);
				if (mi != null) {
					foreach (Type t in mi.DefinedTypes)
						yield return t;
				}
			}
		}

		public override object GetType (EvaluationContext gctx, string name, object[] gtypeArgs)
		{
			CorType[] typeArgs = CastArray<CorType> (gtypeArgs);

			CorEvaluationContext ctx = (CorEvaluationContext) gctx;
			foreach (CorModule mod in ctx.Session.GetModules ()) {
				CorMetadataImport mi = ctx.Session.GetMetadataForModule (mod.Name);
				if (mi != null) {
					foreach (Type t in mi.DefinedTypes)
						if (t.FullName == name) {
							CorClass cls = mod.GetClassFromToken (t.MetadataToken);
							return cls.GetParameterizedType (CorElementType.ELEMENT_TYPE_CLASS, typeArgs);
						}
				}
			}
			return null;
		}

		T[] CastArray<T> (object[] array)
		{
			T[] ret = new T[array.Length];
			Array.Copy (array, ret, array.Length);
			return ret;
		}

		public override object RuntimeInvoke (EvaluationContext gctx, object gtargetType, object gtarget, string methodName, object[] gargTypes, object[] gargValues)
		{
			CorType targetType = (CorType) gtargetType;
			CorValRef target = (CorValRef) gtarget;
			CorType[] argTypes = CastArray<CorType> (gargTypes);
			CorValRef[] argValues = CastArray<CorValRef> (gargValues);

			CorEvaluationContext ctx = (CorEvaluationContext) gctx;
			Type type = targetType.Class.GetTypeInfo (ctx.Session);
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic;
			if (target != null)
				flags |= BindingFlags.Instance;
			else
				flags |= BindingFlags.Static;

			MethodInfo method = null;
			foreach (MethodInfo mi in type.GetMethods (flags)) {
				if (mi.Name == methodName) {
					ParameterInfo[] pi = mi.GetParameters ();
					if (pi.Length == argValues.Length) {
						method = mi;
						break;
					}
				}
			}
			if (method != null) {
				CorValRef v = new CorValRef (delegate {
					CorFunction func = targetType.Class.Module.GetFunctionFromToken (method.MetadataToken);
					CorValue[] args = new CorValue[argValues.Length];
					for (int n = 0; n < args.Length; n++)
						args[n] = argValues[n].Val;
					return ctx.RuntimeInvoke (func, new CorType[0], target != null ? target.Val : null, args);
				});
				if (v.Val == null)
					return null;
				else
					return v;
			}
			else
				throw new EvaluatorException ("Invalid method name or incompatible arguments.");

		}

		public override string[] GetImportedNamespaces (EvaluationContext ctx)
		{
			Set<string> list = new Set<string> ();
			foreach (Type t in GetAllTypes (ctx)) {
				list.Add (t.Namespace);
			}
			string[] arr = new string[list.Count];
			list.CopyTo (arr, 0);
			return arr;
		}

		public override void GetNamespaceContents (EvaluationContext ctx, string namspace, out string[] childNamespaces, out string[] childTypes)
		{
			Set<string> nss = new Set<string> ();
			List<string> types = new List<string> ();
			foreach (Type t in GetAllTypes (ctx)) {
				if (t.Namespace == namspace)
					types.Add (t.FullName);
				else if (t.Namespace.StartsWith (namspace + ".")) {
					if (t.Namespace.IndexOf ('.', namspace.Length + 1) == -1)
						nss.Add (t.Namespace);
				}
			}
			childNamespaces = new string[nss.Count];
			nss.CopyTo (childNamespaces, 0);
			childTypes = types.ToArray ();
		}

		public override object TryCast (EvaluationContext ctx, object val, object type)
		{
			CorType ctype = (CorType) GetValueType (ctx, val);
			string tname = GetTypeName (ctx, type);
			while (ctype != null) {
				if (GetTypeName (ctx, ctype) == tname)
					return val;
				ctype = ctype.Base;
			}
			return null;
		}

		public bool IsEnum (EvaluationContext ctx, CorType targetType)
		{
			return targetType.Type == CorElementType.ELEMENT_TYPE_VALUETYPE && GetTypeName (ctx, targetType.Base) == "System.Enum";
		}

		public override object CreateValue (EvaluationContext gctx, object value)
		{
			CorEvaluationContext ctx = (CorEvaluationContext) gctx;
			if (value is string) {
				return new CorValRef (delegate {
					return ctx.Session.NewString (ctx, (string) value);
				});
			}

			foreach (KeyValuePair<CorElementType, Type> tt in CorMetadataImport.CoreTypes) {
				if (tt.Value == value.GetType ()) {
					CorValue val = ctx.Eval.CreateValue (tt.Key, null);
					CorGenericValue gv = val.CastToGenericValue ();
					gv.SetValue (value);
					return new CorValRef (val);
				}
			}
			throw new NotSupportedException ();
		}

		public override object CreateValue (EvaluationContext ctx, object type, params object[] gargs)
		{
			CorValRef[] args = CastArray<CorValRef> (gargs);
			return new CorValRef (delegate {
				return CreateCorValue (ctx, (CorType) type, args);
			});
		}

		public CorValue CreateCorValue (EvaluationContext ctx, CorType type, params CorValRef[] args)
		{
			CorEvaluationContext cctx = (CorEvaluationContext) ctx;
			CorValue[] vargs = new CorValue [args.Length];
			for (int n=0; n<args.Length; n++)
				vargs [n] = args [n].Val;

			Type t = type.Class.GetTypeInfo (cctx.Session);
			MethodInfo ctor = null;
			foreach (MethodInfo met in t.GetMethods ()) {
				if (met.IsSpecialName && met.Name == ".ctor") {
					ParameterInfo[] pinfos = met.GetParameters ();
					if (pinfos.Length == 1) {
						ctor = met;
						break;
					}
				}
			}
			if (ctor == null)
				return null;

			CorFunction func = type.Class.Module.GetFunctionFromToken (ctor.MetadataToken);
			return cctx.RuntimeInvoke (func, type.TypeParameters, null, vargs);
		}

		public override object CreateNullValue (EvaluationContext gctx, object type)
		{
			CorEvaluationContext ctx = (CorEvaluationContext) gctx;
			return new CorValRef (ctx.Eval.CreateValueForType ((CorType)type));
		}

		public override ICollectionAdaptor CreateArrayAdaptor (EvaluationContext ctx, object arr)
		{
			if (GetRealObject (arr) is CorArrayValue)
				return new ArrayAdaptor (ctx, (CorValRef) arr);
			else
				return null;
		}

		public static CorValue GetRealObject (object objr)
		{
			if (objr == null || ((CorValRef)objr).Val == null)
				return null;

			return GetRealObject (((CorValRef)objr).Val);
		}

		public static CorValue GetRealObject (CorValue obj)
		{
			if (obj == null)
				return null;

			try {
				if (obj is CorStringValue)
					return obj;

				if (obj is CorGenericValue)
					return obj;

				if (obj is CorGenericValue)
					return obj;

				if (obj is CorArrayValue)
					return obj;

				CorArrayValue arrayVal = obj.CastToArrayValue ();
				if (arrayVal != null)
					return arrayVal;

				CorReferenceValue refVal = obj.CastToReferenceValue ();
				if (refVal != null) {
					if (refVal.IsNull)
						return refVal;
					else
						return GetRealObject (refVal.Dereference ());
				}

				CorBoxValue boxVal = obj.CastToBoxValue ();
				if (boxVal != null)
					return GetRealObject (boxVal.GetObject ());

				if (obj.ExactType.Type == CorElementType.ELEMENT_TYPE_STRING)
					return obj.CastToStringValue ();

				if (CorMetadataImport.CoreTypes.ContainsKey (obj.Type)) {
					CorGenericValue genVal = obj.CastToGenericValue ();
					if (genVal != null)
						return genVal;
				}

				if (!(obj is CorObjectValue))
					return obj.CastToObjectValue ();
			}
			catch {
				// Ignore
				throw;
			}
			return obj;
		}

		protected override ObjectValue CreateObjectValueImpl (EvaluationContext ctx, IObjectValueSource source, ObjectPath path, object obj, ObjectValueFlags flags)
		{
			CorValue robj = GetRealObject (obj);

			if (robj == null)
				return ObjectValue.CreateObject (null, path, "", null, flags | ObjectValueFlags.ReadOnly, null);

			if ((robj is CorReferenceValue) && ((CorReferenceValue) robj).IsNull)
				return ObjectValue.CreateObject (null, path, GetTypeName (ctx, robj.ExactType), "(null)", flags, null);

			if ((robj is CorObjectValue) && robj.ExactType.Base != null && GetTypeName (ctx, robj.ExactType.Base) == "System.Enum")
				return ObjectValue.CreatePrimitive (source, path, GetTypeName (ctx, robj.ExactType), ctx.Evaluator.TargetObjectToExpression (ctx, obj), flags);

			if ((robj is CorArrayValue) || (robj is CorObjectValue))
				return ObjectValue.CreateObject (source, path, GetTypeName (ctx, robj.ExactType), ctx.Evaluator.TargetObjectToExpression (ctx, obj), flags, null);

			CorGenericValue genVal = robj as CorGenericValue;
			if (genVal != null) {
				return ObjectValue.CreatePrimitive (source, path, GetTypeName (ctx, robj.ExactType), ctx.Evaluator.TargetObjectToExpression (ctx, obj), flags);
			}

			CorStringValue sVal = robj as CorStringValue;
			if (sVal != null)
				return ObjectValue.CreatePrimitive (source, path, "System.String", ctx.Evaluator.TargetObjectToExpression (ctx, obj), flags);

			return ObjectValue.CreateError (path.LastName, "Unknown value type: " + GetValueTypeName (ctx, obj), flags);
		}

		public override object GetEnclosingType (EvaluationContext gctx)
		{
			CorEvaluationContext ctx = (CorEvaluationContext) gctx;
			if (ctx.Frame.FrameType != CorFrameType.ILFrame || ctx.Frame.Function == null)
				return null;

			CorClass cls = ctx.Frame.Function.Class;
			List<CorType> tpars = new List<CorType> ();
			foreach (CorType t in ctx.Frame.TypeParameters)
				tpars.Add (t);
			return cls.GetParameterizedType (CorElementType.ELEMENT_TYPE_CLASS, tpars.ToArray ());
		}

		public override IEnumerable<ValueReference> GetMembers (EvaluationContext ctx, object tt, object gval, BindingFlags bindingFlags)
		{
			CorType t = (CorType) tt;
			CorValRef val = (CorValRef) gval;

			if (val != null)
				t = GetRealObject (val).ExactType;

			if (t.Class == null)
				yield break;

			CorEvaluationContext cctx = (CorEvaluationContext) ctx;

			while (t != null) {
				Type type = t.Class.GetTypeInfo (cctx.Session);

				foreach (FieldInfo field in type.GetFields (bindingFlags))
					yield return new FieldReference (ctx, val, t, field);

				foreach (PropertyInfo prop in type.GetProperties (bindingFlags)) {
					MethodInfo mi = null;
					try {
						mi = prop.CanRead ? prop.GetGetMethod () : null;
					} catch {
						// Ignore
					}
					if (mi != null && mi.GetParameters ().Length == 0)
						yield return new PropertyReference (ctx, prop, val, t);
				}
				t = t.Base;
			}
		}
		
		public static string UnscapeString (string text)
		{
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < text.Length; i++) {
				char c = text[i];
				if (c != '\\') {
					sb.Append (c);
					continue;
				}
				i++;
				if (i >= text.Length)
					return null;
				
				switch (text[i]) {
					case '\\': c = '\\'; break;
					case 'a': c = '\a'; break;
					case 'b': c = '\b'; break;
					case 'f': c = '\f'; break;
					case 'v': c = '\v'; break;
					case 'n': c = '\n'; break;
					case 'r': c = '\r'; break;
					case 't': c = '\t'; break;
					default: return null;
				}
				sb.Append (c);
			}
			return sb.ToString ();
		}

		public override object TargetObjectToObject (EvaluationContext ctx, object objr)
		{
			CorValue obj = GetRealObject (objr);

			if ((obj is CorReferenceValue) && ((CorReferenceValue) obj).IsNull)
				return new LiteralExp ("(null)");

			CorStringValue stringVal = obj as CorStringValue;
			if (stringVal != null)
				return stringVal.String;

			CorArrayValue arr = obj as CorArrayValue;
			if (arr != null) {
				StringBuilder tn = new StringBuilder (GetTypeName (ctx, arr.ExactType.FirstTypeParameter));
				tn.Append ("[");
				int[] dims = arr.GetDimensions ();
				for (int n = 0; n < dims.Length; n++) {
					if (n > 0)
						tn.Append (',');
					tn.Append (dims [n]);
				}
				tn.Append ("]");
				return new LiteralExp (tn.ToString ());
			}

			CorEvaluationContext cctx = (CorEvaluationContext) ctx;
			CorObjectValue co = obj as CorObjectValue;
			if (co != null) {
				if (co.ExactType.Base != null && GetTypeName (ctx, co.ExactType.Base) == "System.Enum") {
					MetadataType rt = co.ExactType.Class.GetTypeInfo (cctx.Session) as MetadataType;
					bool isFlags = rt != null && rt.ReallyIsFlagsEnum;
					string enumName = GetTypeName (ctx, co.ExactType);
					ValueReference val = GetMember (ctx, objr, "value__");
					ulong nval = (ulong) Convert.ChangeType (val.ObjectValue, typeof(ulong));
					ulong remainingFlags = nval;
					string flags = null;
					foreach (ValueReference evals in GetMembers (ctx, co.ExactType, null, BindingFlags.Public | BindingFlags.Static)) {
						ulong nev = (ulong) Convert.ChangeType (evals.ObjectValue, typeof (ulong));
						if (nval == nev)
							return new LiteralExp (enumName + "." + evals.Name);
						if (isFlags && nev != 0 && (nval & nev) == nev) {
							if (flags == null)
								flags = enumName + "." + evals.Name;
							else
								flags += " | " + enumName + "." + evals.Name;
							remainingFlags &= ~nev;
						}
					}
					if (isFlags) {
						if (remainingFlags == nval)
							return nval;
						if (remainingFlags != 0)
							flags += " | " + remainingFlags;
						return new LiteralExp (flags);
					} else
						return nval;
				}
				TypeDisplayData tdata = GetTypeDisplayData (ctx, co.ExactType);
				if (co == null)
					return null;
				if (co.Class.GetTypeInfo (cctx.Session).Name == "System.Decimal")
					return new LiteralExp (CallToString (ctx, objr));
				if (tdata.ValueDisplayString != null) {
					try {
						string ev = EvaluateDisplayString (ctx, objr, tdata.ValueDisplayString);
						return new LiteralExp (ev);
					} catch (Exception ex) {
						cctx.WriteDebuggerError (ex);
					}
				}

				// Try using a collection adaptor
				ICollectionAdaptor col = CreateArrayAdaptor (ctx, objr);
				if (col != null)
					return new LiteralExp (ArrayElementGroup.GetArrayDescription (col.GetDimensions ()));

				// Return the type name
				if (tdata.TypeDisplayString != null)
					return new LiteralExp ("{" + tdata.TypeDisplayString + "}");
				return new LiteralExp ("{" + GetTypeName (ctx, co.ExactType) + "}");
			}

			CorGenericValue genVal = obj as CorGenericValue;
			if (genVal != null) {
				return genVal.GetValue ();
			}

			return new LiteralExp ("?");
		}

		public override ValueReference GetThisReference (EvaluationContext gctx)
		{
			CorEvaluationContext ctx = (CorEvaluationContext) gctx;
			if (ctx.Frame.FrameType != CorFrameType.ILFrame || ctx.Frame.Function == null)
				return null;

			MethodInfo mi = ctx.Frame.Function.GetMethodInfo (ctx.Session);
			if (mi == null || mi.IsStatic)
				return null;

			CorValRef vref = new CorValRef (delegate {
				return ctx.Frame.GetArgument (0);
			});

			return new VariableReference (ctx, vref, "this", ObjectValueFlags.Variable | ObjectValueFlags.ReadOnly);
		}

		public override IEnumerable<ValueReference> GetParameters (EvaluationContext gctx)
		{
			CorEvaluationContext ctx = (CorEvaluationContext) gctx;
			if (ctx.Frame.FrameType == CorFrameType.ILFrame && ctx.Frame.Function != null) {
				MethodInfo met = ctx.Frame.Function.GetMethodInfo (ctx.Session);
				if (met != null) {
					foreach (ParameterInfo pi in met.GetParameters ()) {
						int pos = pi.Position;
						CorValRef vref = null;
						try {
							vref = new CorValRef (delegate {
								return ctx.Frame.GetArgument (pos);
							});
						}
						catch (Exception ex) {
						}
						if (vref != null)
							yield return new VariableReference (ctx, vref, pi.Name, ObjectValueFlags.Parameter);
					}
					yield break;
				}
			}

			int count = ctx.Frame.GetArgumentCount ();
			for (int n = 0; n < count; n++) {
				int locn = n;
				CorValRef vref = new CorValRef (delegate {
					return ctx.Frame.GetArgument (locn);
				});
				yield return new VariableReference (ctx, vref, "arg_" + (n + 1), ObjectValueFlags.Parameter);
			}
		}

		public override IEnumerable<ValueReference> GetLocalVariables (EvaluationContext ctx)
		{
			CorEvaluationContext wctx = (CorEvaluationContext) ctx;
			uint offset;
			CorDebugMappingResult mr;
			wctx.Frame.GetIP (out offset, out mr);
			return GetLocals (wctx, null, (int) offset, false);
		}

		IEnumerable<ValueReference> GetLocals (CorEvaluationContext ctx, ISymbolScope scope, int offset, bool showHidden)
		{
			if (scope == null) {
				ISymbolMethod met = ctx.Frame.Function.GetSymbolMethod (ctx.Session);
				if (met != null)
					scope = met.RootScope;
				else {
					int count = ctx.Frame.GetLocalVariablesCount ();
					for (int n = 0; n < count; n++) {
						int locn = n;
						CorValRef vref = new CorValRef (delegate {
							return ctx.Frame.GetLocalVariable (locn);
						});
						yield return new VariableReference (ctx, vref, "local_" + (n + 1), ObjectValueFlags.Variable);
					}
					yield break;
				}
			}

			foreach (ISymbolVariable var in scope.GetLocals ()) {
				if (var.Name == "$site")
					continue;
				if (!var.Name.StartsWith ("$") || showHidden) {
					int addr = var.AddressField1;
					CorValRef vref = new CorValRef (delegate {
						return ctx.Frame.GetLocalVariable (addr);
					});
					yield return new VariableReference (ctx, vref, var.Name, ObjectValueFlags.Variable);
				}
			}

			foreach (ISymbolScope cs in scope.GetChildren ()) {
				if (cs.StartOffset <= offset && cs.EndOffset >= offset) {
					foreach (VariableReference var in GetLocals (ctx, cs, offset, showHidden))
						yield return var;
				}
			}
		}

		protected override TypeDisplayData OnGetTypeDisplayData (EvaluationContext ctx, object gtype)
		{
			CorType type = (CorType) gtype;
			TypeDisplayData td = null;

			CorEvaluationContext wctx = (CorEvaluationContext) ctx;
			Type t = type.Class.GetTypeInfo (wctx.Session);
			if (t == null)
				return null;

			foreach (object att in t.GetCustomAttributes (false)) {
				DebuggerTypeProxyAttribute patt = att as DebuggerTypeProxyAttribute;
				if (patt != null) {
					if (td == null) td = new TypeDisplayData ();
					td.ProxyType = patt.ProxyTypeName;
					td.IsProxyType = true;
					continue;
				}
				DebuggerDisplayAttribute datt = att as DebuggerDisplayAttribute;
				if (datt != null) {
					if (td == null) td = new TypeDisplayData ();
					td.NameDisplayString = datt.Name;
					td.TypeDisplayString = datt.Type;
					td.ValueDisplayString = datt.Value;
					continue;
				}
			}

			ArrayList mems = new ArrayList ();
			mems.AddRange (t.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance));
			mems.AddRange (t.GetProperties (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance));

			foreach (MemberInfo m in mems) {
				object[] atts = m.GetCustomAttributes (typeof (DebuggerBrowsableAttribute), false);
				if (atts.Length > 0) {
					if (td == null) td = new TypeDisplayData ();
					if (td.MemberData == null) td.MemberData = new Dictionary<string, DebuggerBrowsableState> ();
					td.MemberData[m.Name] = ((DebuggerBrowsableAttribute) atts[0]).State;
				}
			}
			return td;
		}
	}
}
