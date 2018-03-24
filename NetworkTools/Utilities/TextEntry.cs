using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * TextEntry
	 * 
	 * Class used for the app text system and multiple languages
	 * 
	 * @author Esteban Gallardo
	 */
	public class TextEntry
	{
		private string m_id;
		private Hashtable m_texts;

		public string Id
		{
			get { return m_id; }
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public TextEntry(string _id, XmlNodeList _textEntryParameters)
		{
			m_id = _id;
			m_texts = new Hashtable();
			foreach (XmlNode itemParameter in _textEntryParameters)
			{
				if (m_texts[itemParameter.Name] != null)
				{
					Debug.LogError("TAG REPETIDO[" + m_id + "]");
				}
				else
				{
					m_texts.Add(itemParameter.Name, itemParameter.InnerText);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * GetText
		 */
		public string GetText(string _language)
		{
			if (m_texts[_language] != null)
			{
				string buffer = (string)m_texts[_language];
				string output = "";
				int indexNewLine = -1;
				while ((indexNewLine = buffer.IndexOf("+")) != -1)
				{
					output += buffer.Substring(0, indexNewLine) + '\n';
					buffer = buffer.Substring(indexNewLine + 1, buffer.Length - (indexNewLine + 1));
				}
				output += buffer;
				return output;
			}
			else
			{
				return null;
			}
		}
	}
}