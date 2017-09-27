using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.IO;
using System;

using System.Xml;
using System.Xml.Serialization;

using System.Collections.Generic;

[XmlRoot("MonsterCollection")]
public class MonsterContainer
{
	[XmlArray("Monsters"),XmlArrayItem("Monster")]
	public Monster[] Monsters;
	
	public void Save(string path)
	{
		var serializer = new XmlSerializer(typeof(MonsterContainer));
		using(var stream = new FileStream(path, FileMode.Create))
		{
			serializer.Serialize(stream, this);
		}
	}
	
	public static MonsterContainer Load(string path)
	{
		var serializer = new XmlSerializer(typeof(MonsterContainer));
		using(var stream = new FileStream(path, FileMode.Open))
		{
			return serializer.Deserialize(stream) as MonsterContainer;
		}
	}
	
	//Loads the xml directly from the given string. Useful in combination with www.text.
	public static MonsterContainer LoadFromText(string text) 
	{
		var serializer = new XmlSerializer(typeof(MonsterContainer));
		return serializer.Deserialize(new StringReader(text)) as MonsterContainer;
	}

}


public class Monster
{ 
	[XmlAttribute("name")]
	public string Name;
	
	public int Health;
}