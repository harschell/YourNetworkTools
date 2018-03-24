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
	 * LanguageController
	 * 
	 * It manages all the texts file and multiple languages
	 * 
	 * @author Esteban Gallardo
	 */
	public class LanguageController : MonoBehaviour
	{
		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------
		private const string LANGUAGE_COOCKIE = "LANGUAGE_COOCKIE";

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------
		private static LanguageController _instance;

		public static LanguageController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType<LanguageController>();
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// MEMBERS
		// ----------------------------------------------
		public TextAsset TextsXML;
		public Hashtable m_texts = new Hashtable();

		private string m_codeLanguage = "es";

		public string CodeLanguage
		{
			get { return m_codeLanguage; }
		}

		// -------------------------------------------
		/* 
		 * Release resources
		 */
		void OnDestroy()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			DestroyObject(_instance.gameObject);
			_instance = null;
		}

		// -------------------------------------------
		/* 
		 * SetLanguage
		 */
		public void SetLanguage(string _codeLanguage)
		{
			m_codeLanguage = _codeLanguage;
			PlayerPrefs.SetString(LANGUAGE_COOCKIE, m_codeLanguage);
		}

		// -------------------------------------------
		/* 
		 * LoadTextsXML
		 */
		public void Initialize()
		{
			m_codeLanguage = PlayerPrefs.GetString(LANGUAGE_COOCKIE, "null");

			if (m_codeLanguage == "null")
			{
				if (Application.systemLanguage == SystemLanguage.Spanish)
				{
					m_codeLanguage = "es";
				}
				else
				{
					m_codeLanguage = "en";
				}
				m_codeLanguage = "en";
				SetLanguage(m_codeLanguage);
			}
			m_codeLanguage = "en";
			SetLanguage(m_codeLanguage);

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(TextsXML.text);

			XmlNodeList textsList = xmlDoc.GetElementsByTagName("text");

			foreach (XmlNode textEntry in textsList)
			{
				XmlNodeList textNodes = textEntry.ChildNodes;
				string idText = textEntry.Attributes["id"].Value;
				m_texts.Add(idText, new TextEntry(idText, textNodes));
			}
		}

		// -------------------------------------------
		/* 
		 * LoadTextsJSON
		 */
		public void LoadTextsJSON(string _data)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(_data);

			XmlNodeList textsList = xmlDoc.GetElementsByTagName("text");
			m_texts.Clear();
			foreach (XmlNode textEntry in textsList)
			{
				XmlNodeList textNodes = textEntry.ChildNodes;
				string idText = textEntry.Attributes["id"].Value;
				if (((TextEntry)m_texts[idText]) != null)
				{
#if DEBUG_MODE_DISPLAY_LOG
					Debug.LogError("LoadTextsJSON::EXISTING(" + idText + ")");
#endif
				}
				else
				{
					m_texts.Add(idText, new TextEntry(idText, textNodes));
				}
			}
		}

		// -------------------------------------------
		/* 
		 * GetText
		 */
		public string GetText(string _id)
		{
			if (m_texts[_id] != null)
			{
				return ((TextEntry)m_texts[_id]).GetText(m_codeLanguage);
			}
			else
			{
				return _id;
			}
		}

		// -------------------------------------------
		/* 
		 * GetTextFirstUpper
		 */
		public string GetTextFirstUpper(string _id)
		{
			string text = GetText(_id);
			if ((text != null) && (text.Length > 0))
			{
				return text.Substring(0, 1).ToUpper() + text.Substring(1, text.Length - 1);
			}
			else
			{
				return _id;
			}
		}

		// -------------------------------------------
		/* 
		 * GetTextFirstUpper
		 */
		public string GetTextFirstUpper(string _id, params object[] _list)
		{
			string text = GetText(_id, _list);
			if ((text != null) && (text.Length > 0))
			{
				return text.Substring(0, 1).ToUpper() + text.Substring(1, text.Length - 1);
			}
			else
			{
				return _id;
			}
		}

		// -------------------------------------------
		/* 
		 * GetText
		 */
		public string GetText(string _id, params object[] _list)
		{
			if (m_texts[_id] != null)
			{
				string buffer = ((TextEntry)m_texts[_id]).GetText(m_codeLanguage);
				string result = "";
				for (int i = 0; i < _list.Length; i++)
				{
					string valueThing = (_list[i]).ToString();
					int indexTag = buffer.IndexOf("%");
					if (indexTag != -1)
					{
						result += buffer.Substring(0, indexTag) + valueThing;
						buffer = buffer.Substring(indexTag + 1, buffer.Length - (indexTag + 1));
					}
				}
				result += buffer;
				return result;
			}
			else
			{
				return _id;
			}
		}
	}
}
