using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.IO;
using System;

using System.Xml;
using System.Xml.Serialization;

using System.Collections.Generic;

[XmlRoot("ViewpointCollection")]
public class ViewpointContainer
{
	[XmlArray("Viewpoints"),XmlArrayItem("ViewpointXML")]
	public ViewpointXML[] viewpoints;
	
	public void Save(string path)
	{
		var serializer = new XmlSerializer(typeof(ViewpointContainer));
		using(var stream = new FileStream(path, FileMode.Create))
		{
			serializer.Serialize(stream, this);
		}
	}
	
	public static ViewpointContainer Load(string path)
	{
		if (File.Exists (path)) {
			var serializer = new XmlSerializer(typeof(ViewpointContainer));
			using(var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				return serializer.Deserialize(stream) as ViewpointContainer;
			}
		} else {
			Debug.LogError ("ViewpointContainer: " + path + " not found!");
			return null;
		}
	}
	
	//Loads the xml directly from the given string. Useful in combination with www.text.
	public static ViewpointContainer LoadFromText(string text) 
	{
		var serializer = new XmlSerializer(typeof(ViewpointContainer));
		System.IO.StringReader stringReader = new System.IO.StringReader(text);
//		stringReader.Read(); // skip BOM
		return serializer.Deserialize(stringReader) as ViewpointContainer;
	}

}
