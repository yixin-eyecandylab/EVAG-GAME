using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.IO;
using System;

using System.Xml;
using System.Xml.Serialization;

using System.Collections.Generic;

[XmlRoot("HighScoreCollection")]
public class HighScoreContainer
{
	[XmlArray("HighScores"),XmlArrayItem("HighScore")]
	public HighScore[] highscores;
	
	public void Save(string path)
	{
        if (File.Exists(path))
        {
            File.Copy(path, path + ".bak", true);
        }
		var serializer = new XmlSerializer(typeof(HighScoreContainer));
		using(var stream = new FileStream(path, FileMode.Create))
		{
			var streamWriter = new StreamWriter(stream, System.Text.Encoding.UTF8);
			serializer.Serialize(streamWriter, this);
		}
	}
	
	public string SaveToText()
	{
		var serializer = new XmlSerializer(typeof(HighScoreContainer));
		var writer = new System.IO.StringWriter();
		serializer.Serialize(writer, this);
		return writer.ToString();
	}
	

	public static HighScoreContainer Load(string path)
	{
		if (File.Exists (path)) {
			var serializer = new XmlSerializer(typeof(HighScoreContainer));
			using(var stream = new FileStream(path, FileMode.Open))
			{
				return serializer.Deserialize(stream) as HighScoreContainer;
			}
		} else {
			Debug.LogError ("HighScoreContainer: " + path + " not found!");
			return null;
		}
	}
	
	//Loads the xml directly from the given string. Useful in combination with www.text.
	public static HighScoreContainer LoadFromText(string text) 
	{
		var serializer = new XmlSerializer(typeof(HighScoreContainer));
		System.IO.StringReader stringReader = new System.IO.StringReader(text);
//		stringReader.Read(); // skip BOM
		return serializer.Deserialize(stringReader) as HighScoreContainer;
	}

}
