using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace TessOCR
{
	/// <summary>
	///                         Wacth Me !!!
	/// the OCR engine is Tesseract-ocr, to reconition text need the .trained file,
	/// it could be download from here https://github.com/tesseract-ocr/tessdata/tree/3.04.00 
	/// the file name is the language code, don't change it, if need to recognition more than 1 
	/// language, should have the .traineddata and when call the Init(path,lang) ,lang="eng+de+chi_sim"
	/// it means english and german and chinese
	/// </summary>
	public static class TesseractAPI
	{
        public static bool Inited = false;

		static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
		static AndroidJavaClass TessOCR = new AndroidJavaClass("com.example.tessplugin.TessOCR");
		/// <summary>
		/// Initializes the Tesseract class , all func is static
		/// </summary>

		/// <summary>
		/// Get the card's utf8 text from tess-ocr s, the result is a json array, decode it to TextLine[] array.
		/// </summary>
		/// <returns>Json string.</returns>
		/// <param name="path">the picture's full path it looks like /storage/emulated/0/tesseract/pic1.jpg</param>
		/// <param name="saveCardimg">If set to <c>true</c> save the card area to .jpg, then you can read it.
		///     if the picture's path is /Pictures/pic1.jpg, the card image will save to /Pictures/vpic1.jpg
		/// </param>
		public static string Getutf8String(string path, bool saveCardimg = false)
		{
			return TessOCR.CallStatic<string>("getutf8String", path, saveCardimg);
		}
		/// <summary>
		/// Init the stesseract and opencv, before call getUtf8String
		/// </summary>
		/// <returns>The init.</returns>
		/// <param name="path">.traineddata's parent folder, if the .traineddata file is in 
		///                     "/storage/emulated/0/tesseract/tessdata/eng.traineddata" 
		///                     then the path will be "/storage/emulated/0/tesseract/" 
		///             traineddata could get in github, see below url
		/// </param>
		/// <param name="lang">language such as "eng" see https://github.com/tesseract-ocr/tessdata/tree/3.04.00 </param>
		public static bool Init(string path, string lang)
		{
			TessOCR.CallStatic("init", currentActivity);
            return Inited=TessOCR.CallStatic<bool>("Init", path, lang);
		}
	}
	[Serializable]
	public class TextData
	{
		public List<TextLine> TextLine;
	}
	[Serializable]
	public class TextLine
	{
        /// <summary>
        /// text line
        /// </summary>
        /// <value>The text.</value>

        public string Text;
        /// <summary>
        /// the text line bound's height, means the character's height or its font size
        /// </summary>
        /// <value>The height.</value>

        public int Height;
        /// <summary>
        /// the text line bound's width, it depend on the number of character the text line have
        /// </summary>
        /// <value>The width.</value>

        public int Width;
	}

}
