using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Linq;


#region License
//Microsoft Public License (Ms-PL)

//This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

//1. Definitions

//The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.

//A "contribution" is the original software, or any additions or changes to the software.

//A "contributor" is any person that distributes its contribution under this license.

//"Licensed patents" are a contributor's patent claims that read directly on its contribution.

//2. Grant of Rights

//(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.

//(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

//3. Conditions and Limitations

//(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.

//(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.

//(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.

//(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.

//(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.
#endregion

namespace ProgressOnderwijsUtils
{
	/// <summary>
	/// Class that allows comparison of two objects of the same type to each other.
	/// Supports classes, lists, arrays, dictionaries, child comparison and more. 
	/// HdG: Schijnt dat het (nog) niet werkt voor datatables, iemand meldt een oneindige lus
	/// http://comparenetobjects.codeplex.com/
	/// </summary>
	public class CompareObjects //TODO:emn:FIX!
	{
		#region Class Variables
		private List<String> Differences = new List<String>();
		private List<object> _parents = new List<object>();


		private int MaxDifferences = 1;
		#endregion




		#region Public Methods
		/// <summary>
		/// Compare two objects of the same type to each other.
		/// </summary>
		/// <remarks>
		/// Check the Differences or DifferencesString Properties for the differences.
		/// Default MaxDifferences is 1
		/// </remarks>
		public bool Compare(object object1, object object2)
		{
			if (object1 == null && object2 == null) return true; //gelijk
			if (object1 == null && object2 != null) return false; //niet gelijk
			if (object1 != null && object2 == null) return false; //niet gelijk

			string defaultBreadCrumb = string.Empty;

			//bool isEqual =
			//    object.Equals(object1, object2)
			//        || (
			//            object1 is IEnumerable &&
			//            object2 is IEnumerable &&
			//            Enumerable.SequenceEqual(
			//                ((IEnumerable)object1).Cast<object>(),
			//                ((IEnumerable)object2).Cast<object>()
			//            )
			//        );


			Differences.Clear();
			Compare(object1, object2, defaultBreadCrumb);

			return Differences.Count == 0;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Compare two objects
		/// </summary>
		/// <param name="breadCrumb">Where we are in the object hiearchy</param>
		private void Compare(object object1, object object2, string breadCrumb)
		{
			//If both null return true
			if (object1 == null && object2 == null)
				return;

			//Check if one of them is null
			if (object1 == null)
			{
				Differences.Add(string.Format("object1{0} == null && object2{0} != null ((null),{1})", breadCrumb, cStr(object2)));
				return;
			}

			if (object2 == null)
			{
				Differences.Add(string.Format("object1{0} != null && object2{0} == null ({1},(null))", breadCrumb, cStr(object1)));
				return;
			}

			Type t1 = object1.GetType();
			Type t2 = object2.GetType();

			//Objects must be the same type
			if (t1 != t2)
			{
				Differences.Add(string.Format("Different Types:  object1{0}.GetType() != object2{0}.GetType()", breadCrumb));
				return;
			}

			if (IsIList(t1)) //This will do arrays, multi-dimensional arrays and generic lists
			{
				CompareIList(object1, object2, breadCrumb);
			}
			else if (IsIDictionary(t1))
			{
				CompareIDictionary(object1, object2, breadCrumb);
			}
			else if (IsEnum(t1))
			{
				CompareEnum(object1, object2, breadCrumb);
			}
			else if (IsSimpleType(t1))
			{
				CompareSimpleType(object1, object2, breadCrumb);
			}
			else if (IsClass(t1))
			{
				CompareClass(object1, object2, breadCrumb);
			}
			else if (IsTimespan(t1))
			{
				CompareTimespan(object1, object2, breadCrumb);
			}
			else if (IsStruct(t1))
			{
				CompareStruct(object1, object2, breadCrumb);
			}
			else
			{
				throw new NotImplementedException("Cannot compare object of type " + t1.Name);
			}
		}

		private bool IsTimespan(Type t)
		{
			return t == typeof(TimeSpan);
		}

		private bool IsEnum(Type t)
		{
			return t.IsEnum;
		}

		private bool IsStruct(Type t)
		{
			return t.IsValueType;
		}

		private bool IsSimpleType(Type t)
		{
			return t.IsPrimitive
				|| t == typeof(DateTime)
				|| t == typeof(decimal)
				|| t == typeof(string)
				|| t == typeof(Guid);

		}

		private bool ValidStructSubType(Type t)
		{
			return IsSimpleType(t)
				|| IsEnum(t)
				|| IsArray(t)
				|| IsClass(t)
				|| IsIDictionary(t)
				|| IsTimespan(t)
				|| IsIList(t);
		}

		private bool IsArray(Type t)
		{
			return t.IsArray;
		}

		private bool IsClass(Type t)
		{
			return t.IsClass;
		}

		private bool IsIDictionary(Type t)
		{
			return t.GetInterface("System.Collections.IDictionary", true) != null;
		}

		private bool IsIList(Type t)
		{
			return t.GetInterface("System.Collections.IList", true) != null;
		}

		private bool IsChildType(Type t)
		{
			return IsClass(t)
				|| IsArray(t)
				|| IsIDictionary(t)
				|| IsIList(t)
				|| IsStruct(t);
		}

		/// <summary>
		/// Compare a timespan struct
		/// </summary>
		private void CompareTimespan(object object1, object object2, string breadCrumb)
		{
			if (((TimeSpan)object1).Ticks != ((TimeSpan)object2).Ticks)
			{
				Differences.Add(string.Format("object1{0}.Ticks != object2{0}.Ticks", breadCrumb));
			}
		}

		/// <summary>
		/// Compare an enumeration
		/// </summary>
		private void CompareEnum(object object1, object object2, string breadCrumb)
		{
			if (object1.ToString() != object2.ToString())
			{
				string currentBreadCrumb = AddBreadCrumb(breadCrumb, object1.GetType().Name, string.Empty, -1);
				Differences.Add(string.Format("object1{0} != object2{0} ({1},{2})", currentBreadCrumb, object1, object2));
			}
		}

		/// <summary>
		/// Compare a simple type
		/// </summary>
		private void CompareSimpleType(object object1, object object2, string breadCrumb)
		{
			if (object2 == null) //This should never happen, null check happens one level up
				throw new ArgumentNullException("object2");

			IComparable valOne = object1 as IComparable;

			if (valOne == null) //This should never happen, null check happens one level up
				throw new ArgumentNullException("object1");

			if (valOne.CompareTo(object2) != 0)
			{
				Differences.Add(string.Format("object1{0} != object2{0} ({1},{2})", breadCrumb, object1, object2));
			}
		}



		/// <summary>
		/// Compare a struct
		/// </summary>
		private void CompareStruct(object object1, object object2, string breadCrumb)
		{
			string currentCrumb;
			Type t1 = object1.GetType();

			//Compare the fields
			FieldInfo[] currentFields = t1.GetFields();

			foreach (FieldInfo item in currentFields)
			{
				//Only compare simple types within structs (Recursion Problems)
				if (!ValidStructSubType(item.FieldType))
				{
					continue;
				}

				currentCrumb = AddBreadCrumb(breadCrumb, item.Name, string.Empty, -1);

				Compare(item.GetValue(object1), item.GetValue(object2), currentCrumb);

				if (Differences.Count >= MaxDifferences)
					return;
			}

		}

		/// <summary>
		/// Compare the properties, fields of a class
		/// </summary>
		/// <param name="object1"></param>
		/// <param name="object2"></param>
		/// <param name="breadCrumb"></param>
		private void CompareClass(object object1, object object2, string breadCrumb)
		{
			try
			{
				_parents.Add(object1);
				_parents.Add(object2);
				Type t1 = object1.GetType();

				PerformCompareProperties(t1, object1, object2, breadCrumb);

				PerformCompareFields(t1, object1, object2, breadCrumb);
			}
			finally
			{
				_parents.Remove(object1);
				_parents.Remove(object2);
			}
		}

		/// <summary>
		/// Compare the fields of a class
		/// </summary>
		private void PerformCompareFields(Type t1,
			object object1,
			object object2,
			string breadCrumb)
		{
			object objectValue1;
			object objectValue2;
			string currentCrumb;

			FieldInfo[] currentFields;

			currentFields = t1.GetFields(); //Default is public instance

			foreach (FieldInfo item in currentFields)
			{


				objectValue1 = item.GetValue(object1);
				objectValue2 = item.GetValue(object2);

				bool object1IsParent = objectValue1 != null && (objectValue1 == object1 || _parents.Contains(objectValue1));
				bool object2IsParent = objectValue2 != null && (objectValue2 == object2 || _parents.Contains(objectValue2));

				//Skip fields that point to the parent
				if (IsClass(item.FieldType)
					&& (object1IsParent || object2IsParent))
				{
					continue;
				}

				currentCrumb = AddBreadCrumb(breadCrumb, item.Name, string.Empty, -1);

				Compare(objectValue1, objectValue2, currentCrumb);

				if (Differences.Count >= MaxDifferences)
					return;
			}
		}


		/// <summary>
		/// Compare the properties of a class
		/// </summary>
		private void PerformCompareProperties(Type t1,
			object object1,
			object object2,
			string breadCrumb)
		{
			object objectValue1;
			object objectValue2;
			string currentCrumb;

			PropertyInfo[] currentProperties;

			currentProperties = t1.GetProperties();

			foreach (PropertyInfo info in currentProperties)
			{
				//If we can't read it, skip it
				if (info.CanRead == false)
					continue;

				objectValue1 = info.GetValue(object1, null);
				objectValue2 = info.GetValue(object2, null);

				bool object1IsParent = objectValue1 != null && (objectValue1 == object1 || _parents.Contains(objectValue1));
				bool object2IsParent = objectValue2 != null && (objectValue2 == object2 || _parents.Contains(objectValue2));

				//Skip properties where both point to the corresponding parent
				if (IsClass(info.PropertyType)
					&& (object1IsParent && object2IsParent))
				{
					continue;
				}

				currentCrumb = AddBreadCrumb(breadCrumb, info.Name, string.Empty, -1);

				Compare(objectValue1, objectValue2, currentCrumb);

				if (Differences.Count >= MaxDifferences)
					return;
			}
		}

		/// <summary>
		/// Compare a dictionary
		/// </summary>
		/// <param name="object1"></param>
		/// <param name="object2"></param>
		/// <param name="breadCrumb"></param>
		private void CompareIDictionary(object object1, object object2, string breadCrumb)
		{
			IDictionary iDict1 = object1 as IDictionary;
			IDictionary iDict2 = object2 as IDictionary;

			if (iDict1 == null) //This should never happen, null check happens one level up
				throw new ArgumentNullException("object1");

			if (iDict2 == null) //This should never happen, null check happens one level up
				throw new ArgumentNullException("object2");

			//Objects must be the same length
			if (iDict1.Count != iDict2.Count)
			{
				Differences.Add(string.Format("object1{0}.Count != object2{0}.Count ({1},{2})", breadCrumb, iDict1.Count, iDict2.Count));

				if (Differences.Count >= MaxDifferences)
					return;
			}

			IDictionaryEnumerator enumerator1 = iDict1.GetEnumerator();
			IDictionaryEnumerator enumerator2 = iDict2.GetEnumerator();

			while (enumerator1.MoveNext() && enumerator2.MoveNext())
			{
				string currentBreadCrumb = AddBreadCrumb(breadCrumb, "Key", string.Empty, -1);

				Compare(enumerator1.Key, enumerator2.Key, currentBreadCrumb);

				if (Differences.Count >= MaxDifferences)
					return;

				currentBreadCrumb = AddBreadCrumb(breadCrumb, "Value", string.Empty, -1);

				Compare(enumerator1.Value, enumerator2.Value, currentBreadCrumb);

				if (Differences.Count >= MaxDifferences)
					return;
			}

		}

		/// <summary>
		/// Convert an object to a nicely formatted string
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		private string cStr(object obj)
		{
			try
			{
				if (obj == null)
					return "(null)";

				if (obj == DBNull.Value)
					return "System.DBNull.Value";

				return obj.ToString();
			}
			catch
			{
				return string.Empty;
			}
		}


		/// <summary>
		/// Compare an array or something that implements IList
		/// </summary>
		/// <param name="object1"></param>
		/// <param name="object2"></param>
		/// <param name="breadCrumb"></param>
		private void CompareIList(object object1, object object2, string breadCrumb)
		{
			IList ilist1 = object1 as IList;
			IList ilist2 = object2 as IList;

			if (ilist1 == null) //This should never happen, null check happens one level up
				throw new ArgumentNullException("object1");

			if (ilist2 == null) //This should never happen, null check happens one level up
				throw new ArgumentNullException("object2");

			//Objects must be the same length
			if (ilist1.Count != ilist2.Count)
			{
				Differences.Add(string.Format("object1{0}.Count != object2{0}.Count ({1},{2})", breadCrumb, ilist1.Count, ilist2.Count));

				if (Differences.Count >= MaxDifferences)
					return;
			}

			IEnumerator enumerator1 = ilist1.GetEnumerator();
			IEnumerator enumerator2 = ilist2.GetEnumerator();
			int count = 0;

			while (enumerator1.MoveNext() && enumerator2.MoveNext())
			{
				string currentBreadCrumb = AddBreadCrumb(breadCrumb, string.Empty, string.Empty, count);

				Compare(enumerator1.Current, enumerator2.Current, currentBreadCrumb);

				if (Differences.Count >= MaxDifferences)
					return;

				count++;
			}
		}



		/// <summary>
		/// Add a breadcrumb to an existing breadcrumb
		/// </summary>
		/// <param name="existing"></param>
		/// <param name="name"></param>
		/// <param name="extra"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		private string AddBreadCrumb(string existing, string name, string extra, int index)
		{
			bool useIndex = index >= 0;
			bool useName = name.Length > 0;
			StringBuilder sb = new StringBuilder();

			sb.Append(existing);

			if (useName)
			{
				sb.AppendFormat(".");
				sb.Append(name);
			}

			sb.Append(extra);

			if (useIndex)
				sb.AppendFormat("[{0}]", index);

			return sb.ToString();
		}



		#endregion

	}
}
