using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;


namespace TessOCR{
	public class Utils
	{
		public static Texture2D LoadTextureFromfile(string path)
		{
			FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			fileStream.Seek(0, SeekOrigin.Begin);
			//创建文件长度缓冲区
			byte[] bytes = new byte[fileStream.Length];
			//读取文件
			fileStream.Read(bytes, 0, (int)fileStream.Length);
			//释放文件读取流
			fileStream.Close();
			fileStream.Dispose();
			fileStream = null;

			//创建Texture
			int width = 800;
			int height = 640;
			Texture2D texture = new Texture2D(width, height);
			texture.LoadImage(bytes);
			return texture;
		}
		//find the email
		public static string checkEmail(string email)
		{
			if (string.IsNullOrEmpty(email) || email.Length < 4)
				return string.Empty;
			if (email.Contains("@"))
			{
				if (email.Contains(" "))
				{
					var lines = email.Split(' ');
					foreach (var l in lines)
					{
						if (l.Contains("@"))
							return l;
					}
				}
				else return email;
			}
			// some time the @ will detete to ® or © 
			else if ((email.Contains("®") || email.Contains("©")) && email.Length > 6 && email.Contains("."))
			{
				if (email.Contains("®"))
					email=email.Replace(" ", "@");
				else if (email.Contains("©"))
					email=email.Replace(" ", "@");

				if (email.Contains(" "))
				{
					var ee = email.Split(' ');
					foreach (var e in ee)
					{
						if (e.Contains("@"))
						{
							var mm = e.Split('.');
							if (mm.Length < 2)
								return string.Empty;
							else
							{
								return e;
							}
						}

					}
				}
				else
				{
					var mm = email.Split('.');
					if (mm.Length < 2)
						return string.Empty;
					else
					{
						return email;
					}
				}
			}
			return string.Empty;
		}

		//guess a name by email adress
		public static string checkName(string email, string name)
		{
			if (string.IsNullOrEmpty(email) || name.Length < 2)
				return string.Empty;
            
			string fname = string.Empty; //first name
			string lname = string.Empty; // last name
			char ffn = ' '; //firstname's first char
			char fln = ' '; //llastname's first char

			var m = email.Split('@')[0];

			if (m.ToUpper().Contains("."))
			{
				var mm = m.Split('.');
				if (mm.Length >= 2)
				{
					fname = mm[0];
					lname = mm[1];
				}
			}
			else
			{
				if (m.Length > 2)
				{
					fname = m;
				}
				else if (m.Length == 2)
				{
					ffn = m[0];
					fln = m[1];
				}
			}

			int findex = 0;//first name index of text line
			if (!string.IsNullOrEmpty(fname) && name.ToUpper().Contains(fname.ToUpper()))
			{
				if (name.Contains(" "))
				{
					var mm = name.Split(' ');
					for (int i = 0; i < mm.Length; i++)
					{
						if (mm[i].ToUpper() == fname.ToUpper())
						{
							fname = mm[i];
							findex = i;
							break;
						}
					}
					if (mm.Length > findex + 1 && findex != 0)
						lname = mm[findex + 1];
					if (findex == 0 && mm.Length >= 2)
						lname = mm[1];
					if (!string.IsNullOrEmpty(fname) && !string.IsNullOrEmpty(lname))
						return fname + " " + lname;
					else if (!string.IsNullOrEmpty(fname))
						return fname;
					else if (!string.IsNullOrEmpty(lname))
						return lname;
				}
				else
				{
					return name;
				}
			}

			if (ffn != ' ' && fln != ' ')
			{
				if (name.ToUpper().Contains(ffn.ToString().ToUpper()) && name.ToUpper().Contains(fln.ToString().ToUpper()))
				{
					if (name.Contains(" "))
					{
						var mm = name.Split(' ');
						for (int i = 0; i < mm.Length; i++)
						{
							if (mm.Length > (i + 1) && mm[i][0].ToString().ToUpper() == ffn.ToString().ToUpper() && mm[i + 1][0].ToString().ToUpper() == fln.ToString().ToUpper())
							{
								return mm[i] + " " + mm[i + 1];
							}
						}
					}
					//else return string.Empty;
				}
			}

			return string.Empty;
		}
		//rotate the picture
		public static Color32[] RotateTexture(Color32[] pixels, int w, int h)
		{
			//Debug.Log(w.ToString()+","+h.ToString());
			Color32[] array = new Color32[pixels.Length];
			//Debug.Log("new:"+array.Length.ToString()+" old:"+pixels.Length.ToString()+"length:"+(w*h/4).ToString());
			for (int j = 0; j < h; j++)
				for (int i = 0; i < w; i++)
				{
					//Debug.Log((j * w + i).ToString() + ":" + ((h - 1 - j) * h + i));
					array[i * h + j] = pixels[j * w + w - 1 - i];
				}
			//foreach (var p in array)
			//Debug.Log(array[100].r);
			return array;
		}
		// it is a word or not ?
        public static bool IsWord(string text,bool findemail=true)
		{
			if (text.Length < 2)
				return false;
            if (!findemail){
                foreach(var c in text){
                    if (c == '.')
                        continue;
                    if(c<'0'|| c>'z' ||(c > 'Z' && c < 'a')||(c>'9'&&c<'@')){
                        return false;
                    }
                     
                }
            }
			
			else if (text == "com" || text == "www")
				return false;
			foreach (var c in text)
			{
				if (c < 'A' || c > 'z'||(c>'Z'&&c<'a'))
				{
					return false;
				}
			}
			return true;
		}

		public static List<TextLine> GetTextLine(string str){
			List<TextLine> lines = new List<TextLine> ();
			JSONObject json = new JSONObject (str);
			string tl=json.getString("TextLine");
			JSONArray list = new JSONArray (tl);
			for (int i=0; i<list.length (); i++) {
				int w=list.getJSONObject(i).getInt("Width");
				int h=list.getJSONObject (i).getInt("Height");
				string text=list.getJSONObject (i).getString("Text");
				lines.Add(new TextLine{ Width=w,Height=h,Text=text});
			}
			return lines;
		}




	}
	public class FileHelper
	{

		/// <summary>  
		/// 删除文件  
		/// </summary>  
		/// <param name="path">Path.</param>  
		/// <param name="name">Name.</param>  

		public static void DeleteFile (string path, string name)
		{
			File.Delete (path + "/" + name);
		}

		/// <summary>  
		/// 删除指定目录及其所有子目录  
		/// </summary>  
		/// <param name="directoryPath">指定目录的绝对路径</param>  
		public static void DeleteDirectory (string directoryPath)
		{
			if (IsExistDirectory (directoryPath)) {
				Directory.Delete (directoryPath, true);
			}
		}


		/// <summary>  
		/// Creates the directory.  
		/// </summary>  
		/// <param name="directoryPath">Directory path.</param>  
		public static void CreateDirectory (string directoryPath)
		{
			//如果目录不存在则创建该目录  
			if (!IsExistDirectory (directoryPath)) {
				//Debug.Log("path doesnot exit");  
				Directory.CreateDirectory (directoryPath);
			}
		}
		public static bool IsExistDirectory (string directoryPath)
		{
			return Directory.Exists (directoryPath);
		}

		public static string [] GetFileNames (string directoryPath, string searchPattern, bool isSearchChild)
		{
			//如果目录不存在，则抛出异常  
			if (!IsExistDirectory (directoryPath)) {
				throw new FileNotFoundException ();
			}

			try {
				if (isSearchChild) {
					return Directory.GetFiles (directoryPath, searchPattern, SearchOption.AllDirectories);
				} else {
					return Directory.GetFiles (directoryPath, searchPattern, SearchOption.TopDirectoryOnly);
				}
			} catch (IOException ex) {
				throw ex;
			}
		}


		/// <summary>  
		/// 获取指定目录中所有文件列表  
		/// </summary>  
		/// <param name="directoryPath">指定目录的绝对路径</param>          
		public static string [] GetFileNames (string directoryPath)
		{
			//如果目录不存在，则抛出异常  
			if (!IsExistDirectory (directoryPath)) {
				throw new FileNotFoundException ();
			}

			//获取文件列表  
			return Directory.GetFiles (directoryPath);
		}//获取文件列  
		 /// <summary>  
		 /// 创建文件  
		 /// </summary>  


		public static void CreateModuleFile (string writepath, string name, byte [] info, int length)
		{
			Debug.Log (writepath + "//" + name);
			Stream sw = null;
			FileInfo t = new FileInfo (writepath + "/" + name);
			//stringToEdit +="t.Exists=="+ t.Exists  + "\n";  
			if (!t.Exists) {
				sw = t.Create ();
			} else {
				return;
			}
			sw.Write (info, 0, length);
			sw.Close ();
			sw.Dispose ();
		}
		public void CopyFile(string file, string path){
			var f  = File.OpenRead (file);
			var save = File.Create (path + "/" + file);
			//var array=f.re
			//var s=save.Read (array,0,)
		}


	}
}
