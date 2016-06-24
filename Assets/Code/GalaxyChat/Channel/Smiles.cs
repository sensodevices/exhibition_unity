using UnityEngine;

public sealed class Smiles : MonoBehaviour {

	public static string Replace(string source) {
		return source;
		var result = source;
		for (int k = 0; k < Array.Length; k += 2){
			var s1 = Array[k];
			var s2 = Array[k+1];
			result = result.Replace(s1,s2);
		}
		return result;
	}
	
	private static string[] Array = {
		"{:)","",
		"<:)","",
		":plz:","",
		":fp:","",
		":lol:","",
		":ffuu:","",
		":troll:","",
		":)","<color=orange>☺️️</color>",
		":]","",
		";)","",
		":D","",
		":p","",
		":р","",
		"О:}","",
		"O:}","",
		":(","",
		":о","",
		":o","",
		":|","",
		":-/","",
		":\\","",
		"о_О","",
		"o_O","",
		":[","",
		"Х(","",
		"X(","",
		":О","",
		":O","",
		"()","<color=red>❤️</color>",
		"(-)","",
		"\\_/","",
		"]:->","",
		"^_^","",
		":-.","",
		":-*","",
		":-!","",
		"8-)","",
		"%)","",
		"%D","",
		"(*)","",
		"(|)","",
		"(I)","",
		"@}","",
		"(d=)","",
		"(q=)","",
		"8(","",
		"(:@","",
		"(.)(.)","",
		"@=","",
		"..|..","",
		":-j","",
		":-J","",
		":-3","",
		":=|","",
		":T","",
		":t","",
		":Т","",
		":т","",
		"y.e.","",
		"c.u.","",
		"u.c.","",
		"у.е.","",
		"Y.E.","",
		"C.U.","",
		"U.C.","",
		"У.Е.",""
	};
}
