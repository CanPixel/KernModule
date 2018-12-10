using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class AVLevelConverter {
	public static Dictionary<string, IConvertType> FILETYPES = new Dictionary<string, IConvertType>();
	
	/* THE IMPORTED TYPE CONVERSIONS */
	/*To add more file type conversions, simply include the implementation of the IConvertType interface and the respective file extension as a string */
	public static IConvertType[] types = new IConvertType[]{new IPNG(), new IBruut(), new IMP3()};

	static AVLevelConverter() {
		foreach(IConvertType conv in types) FILETYPES.Add(conv.extension.ToLower(), conv);
	}
}
