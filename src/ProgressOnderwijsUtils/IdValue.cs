using System;

namespace ProgressOnderwijsUtils {
	public class IdValue {
		string val;
		string id;
		public string Value { get {return val;}}
		public string Id { get {return id;}}
		public IdValue(string id, string val) { this.id = id; this.val = val;}
	}
}