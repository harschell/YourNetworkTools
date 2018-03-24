using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace YourNetworkingTools
{

	public class ItemMultiTextEntry
	{
		public string TOKEN_SEPARATOR_EVENTS = ";";

		private List<string> m_items;

		public List<string> Items
		{
			get { return m_items; }
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public ItemMultiTextEntry(params string[] _list)
		{
			m_items = new List<string>();
			for (int i = 0; i < _list.Length; i++)
			{
				if (_list[i].Length > 0)
				{
					m_items.Add(_list[i]);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Clone
		 */
		public ItemMultiTextEntry Clone()
		{
			ItemMultiTextEntry output = new ItemMultiTextEntry(m_items.ToArray());
			return output;
		}

		// -------------------------------------------
		/* 
		 * EqualsEntry
		 */
		public bool EqualsEntry(params string[] _list)
		{
			bool output = true;
			for (int i = 0; i < m_items.Count; i++)
			{
				if (i < _list.Length)
				{
					if (m_items[i] != _list[i])
					{
						output = false;
					}
				}
				else
				{
					output = false;
				}
			}
			return output;
		}

		// -------------------------------------------
		/* 
		 * EqualsEntry
		 */
		public bool EqualsEntry(ItemMultiTextEntry _item)
		{
			bool output = true;
			for (int i = 0; i < m_items.Count; i++)
			{
				if (i < _item.Items.Count)
				{
					if (m_items[i] != _item.Items[i])
					{
						output = false;
					}
				}
				else
				{
					output = false;
				}
			}
			return output;
		}

		// -------------------------------------------
		/* 
		 * Package
		 */
		public string Package()
		{
			string output = "";
			for (int i = 0; i < m_items.Count; i++)
			{
				if (i < m_items.Count - 1)
				{
					output += m_items[i] + TOKEN_SEPARATOR_EVENTS;
				}
				else
				{
					output += m_items[i];
				}
			}
			return output;
		}

		// -------------------------------------------
		/* 
		 * Package
		 */
		public string Package(string _separator)
		{
			string output = "";
			for (int i = 0; i < m_items.Count; i++)
			{
				if (i < m_items.Count - 1)
				{
					output += m_items[i] + _separator;
				}
				else
				{
					output += m_items[i];
				}
			}
			return output;
		}
		// -------------------------------------------
		/* 
		 * PackageInstructions
		 */
		public string PackageInstructions()
		{
			string output = "";
			List<string> instructions = new List<string>();
			for (int i = 1; i < m_items.Count; i++)
			{
				instructions.Add(m_items[i]);
			}

			for (int i = 0; i < instructions.Count; i++)
			{
				if (i < instructions.Count - 1)
				{
					output += instructions[i] + TOKEN_SEPARATOR_EVENTS;
				}
				else
				{
					output += instructions[i];
				}
			}
			return output;
		}

		// -------------------------------------------
		/* 
		 * UpdateInstructions
		 */
		public void UpdateInstructions(string _instructions)
		{
			// SET ONLY THE ITEM
			while (m_items.Count > 1)
			{
				m_items.RemoveAt(m_items.Count - 1);
			}

			// ADD THE INSTRUCTIONS
			string[] replaceInstructions = _instructions.Split(new string[] { TOKEN_SEPARATOR_EVENTS }, StringSplitOptions.None);
			for (int i = 0; i < replaceInstructions.Length; i++)
			{
				if (replaceInstructions[i].Length > 0)
				{
					m_items.Add(replaceInstructions[i]);
				}
			}
#if DEBUG_MODE_DISPLAY_LOG
			for (int i = 0; i < m_items.Count; i++)
			{
				Debug.LogError("Inventory Item[" + i + "]=" + m_items[i]);
			}
#endif
		}
	}
}