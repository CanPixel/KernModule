using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IConvertType {
	string extension {
		get;
		set;
	}
	void Convert(string path);
}

public class IPNG : IConvertType {
	public string extension {
		get {return "PNG";}
		set {}
	}

	public void Convert(string path) {
		
	}
}

public class IBruut : IConvertType {
	public string extension {
		get {return "Bruut";}
		set {}
	}

	public void Convert(string path) {
		
	}
}

public class IMP3 : IConvertType {
	public string extension {
		get {return "mp3";}
		set {}
	}

	public void Convert(string path) {
		
	}
}