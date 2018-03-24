using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace YourNetworkingTools
{

	// -------------------------------------------
	/* 
	 * ItemMultiObjectEntry
	 */
	public class ItemMultiObjectEntry : IEqualityComparer<ItemMultiObjectEntry>
	{
		private List<object> m_objects;

		public List<object> Objects
		{
			get { return m_objects; }
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public ItemMultiObjectEntry(params object[] _list)
		{
			m_objects = new List<object>();
			for (int i = 0; i < _list.Length; i++)
			{
				if (_list[i] != null)
				{
					m_objects.Add(_list[i]);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * AddObject
		 */
		public void AddObject(object _item)
		{
			if (m_objects == null)
			{
				m_objects = new List<object>();
			}
			m_objects.Add(_item);
		}

		// -------------------------------------------
		/* 
		 * EqualsEntry
		 */
		public bool EqualsEntry(params object[] _list)
		{
			bool output = true;
			for (int i = 0; i < m_objects.Count; i++)
			{
				if (i < _list.Length)
				{
					if (m_objects[i] != _list[i])
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
		 * Equals
		 */
		public bool Equals(ItemMultiObjectEntry _origin, ItemMultiObjectEntry _target)
		{
			bool output = true;
			for (int i = 0; i < _origin.Objects.Count; i++)
			{
				if (i < _target.Objects.Count)
				{
					if (_origin.Objects[i] != _target.Objects[i])
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
		 * GetHashCode
		 */
		public int GetHashCode(ItemMultiObjectEntry _obj)
		{
			int output = 0;
			for (int i = 0; i < _obj.Objects.Count; i++)
			{
				output += _obj.Objects[i].GetHashCode();
			}
			return output;
		}

		// -------------------------------------------
		/* 
		 * GetHashCode
		 */
		public override string ToString()
		{
			string output = "";
			for (int i = 0; i < m_objects.Count; i++)
			{				
				output += m_objects[i].GetType().ToString() + ";" + m_objects[i].ToString();
				if (i + 1 < m_objects.Count)
				{
					output += "#";
				}
			}
			return output;
		}

		// -------------------------------------------
		/* 
		 * Parse
		 */
		public static ItemMultiObjectEntry Parse(string _data)
		{
			ItemMultiObjectEntry output = new ItemMultiObjectEntry();
			string[] data = _data.Split('#');
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i] != null)
				{
					string[] item = data[i].Split(';');
					if (item.Length == 2)
					{
						Type myType = Type.GetType(item[0]);
						if (myType == Type.GetType("System.Int32"))
						{
							int myIntType;
							if (int.TryParse(item[1], out myIntType))
							{
								output.AddObject(myIntType);
							}							
						}
						else
						if (myType == Type.GetType("System.Double"))
						{							
							float myFloatType;							
							if (float.TryParse(item[1], out myFloatType))
							{
								output.AddObject(myFloatType);
							}							
						}
						else
						if (myType == Type.GetType("System.Boolean"))
						{
							bool myBoolType;
							if (bool.TryParse(item[1], out myBoolType))
							{
								output.AddObject(myBoolType);
							}
						}
						else
						if (myType == Type.GetType("System.String"))
						{
							string myStringType = item[1];
							output.AddObject(myStringType);
						}
					}
				}
			}
			return output;
		}
	}
}