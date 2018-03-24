using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * Class with a collection of generic functionalities
	 * 
	 * @author Esteban Gallardo
	 */
	public class UtilitiesNetwork
	{
		private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		// -------------------------------------------
		/* 
		 * GetTimestamp
		 */
		public static long GetTimestamp()
		{
			return (long)((DateTime.UtcNow - Jan1St1970).TotalMilliseconds);
		}

		// -------------------------------------------
		/* 
		 * GetTimestamp
		 */
		public static long GetTimestampSeconds()
		{
			return (long)(((DateTime.UtcNow - Jan1St1970).TotalMilliseconds) / 1000);
		}

		// -------------------------------------------
		/* 
		 * GetDaysFromSeconds
		 */
		public static int GetDaysFromSeconds(long _seconds)
		{
			return (int)(((_seconds / 60) / 60) / 24);
		}

		// -------------------------------------------
		/* 
		 * CheckTextForbiddenCharacters
		 */
		public static bool CheckTextForbiddenCharacters(string _text, char[] _forbidden)
		{
			for (int i = 0; i < _forbidden.Length; i++)
			{
				if (_text.IndexOf(_forbidden[i]) != -1)
				{
					return true;
				}
			}
			return false;
		}

		// -------------------------------------------
		/* 
		 * Clone
		 */
		public static Quaternion Clone(Quaternion _quaternion)
		{
			Quaternion output = new Quaternion();
			output.x = _quaternion.x;
			output.y = _quaternion.y;
			output.z = _quaternion.z;
			output.w = _quaternion.w;
			return output;
		}

		// -------------------------------------------
		/* 
		 * Clone
		 */
		public static Vector3 Clone(Vector3 _vector)
		{
			Vector3 output = new Vector3();
			output.x = _vector.x;
			output.y = _vector.y;
			output.z = _vector.z;
			return output;
		}


		// -------------------------------------------
		/* 
		 * Clone
		 */
		public static Vector2 Clone(Vector2 _vector)
		{
			Vector2 output = new Vector2();
			output.x = _vector.x;
			output.y = _vector.y;
			return output;
		}

		// -------------------------------------------
		/* 
		 * GetFormattedTimeSeconds
		 */
		public static string GetFormattedTimeMinutes(long _timestamp)
		{
			int totalSeconds = (int)_timestamp;
			int totalMinutes = (int)Math.Floor((double)(totalSeconds / 60));
			int totalHours = (int)Math.Floor((double)(totalMinutes / 60));
			int restSeconds = (int)(totalSeconds - (totalMinutes * 60));
			int restMinutes = (int)(totalMinutes - (totalHours * 60));

			// SECONDS
			String seconds;
			if (restSeconds < 10)
			{
				seconds = "0" + restSeconds;
			}
			else
			{
				seconds = "" + restSeconds;
			}

			// MINUTES
			String minutes;
			if (restMinutes < 10)
			{
				minutes = "0" + restMinutes;
			}
			else
			{
				minutes = "" + restMinutes;
			}

			return (minutes + ":" + seconds);
		}

		// -------------------------------------------
		/* 
		 * AddChild
		 */
		public static GameObject AddChild(Transform _parent, GameObject _prefab)
		{
			GameObject newObj = GameObject.Instantiate(_prefab);
			newObj.transform.SetParent(_parent, false);
			return newObj;
		}

		// -------------------------------------------
		/* 
		 * Adds a sprite component to the object.
		 * It's used to create the visual selectors.
		 */
		public static Sprite AddSprite(GameObject _parent, Sprite _prefab, Rect _rect, Rect _rectTarget, Vector2 _pivot)
		{
			RectTransform newTransform = _parent.AddComponent<RectTransform>();
			_parent.AddComponent<CanvasRenderer>();
			Image srcImage = _parent.AddComponent<Image>() as Image;
			Sprite sprite = Sprite.Create(_prefab.texture, _rect, _pivot);
			if ((_rectTarget.width != 0) && (_rectTarget.height != 0))
			{
				newTransform.sizeDelta = new Vector2(_rectTarget.width, _rectTarget.height);
			}
			srcImage.sprite = sprite;
			return sprite;
		}

		// -------------------------------------------
		/* 
		 * LoadPNG
		 */
		public static Texture2D LoadPNG(string _filePath)
		{
			Texture2D tex = null;
			byte[] fileData;

			if (File.Exists(_filePath))
			{
				fileData = File.ReadAllBytes(_filePath);
				tex = new Texture2D(2, 2);
				tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
			}
			return tex;
		}


		// -------------------------------------------
		/* 
		 * Replace the sprite with the content in the file
		 */
		public static void SetPictureByPath(Image _image, string _imagefilePath)
		{
			Texture2D newTexture = UtilitiesNetwork.LoadPNG(_imagefilePath);
			_image.sprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), Vector2.zero);
		}

		// -------------------------------------------
		/* 
		 * We apply a material on all the hirarquy of objects
		 */
		public static void ApplyMaterialOnImages(GameObject _go, Material _material)
		{
			foreach (Transform child in _go.transform)
			{
				ApplyMaterialOnImages(child.gameObject, _material);
			}
			if (_go.GetComponent<Image>() != null)
			{
				_go.GetComponent<Image>().material = _material;
			}

			if (_go.GetComponent<Text>() != null)
			{
				_go.GetComponent<Text>().material = _material;
			}
		}

		// -------------------------------------------
		/* 
		 * We apply a material on all the hirarquy of objects
		 */
		public static void ApplyMaterialOnObjects(GameObject _go, Material _material)
		{
			foreach (Transform child in _go.transform)
			{
				ApplyMaterialOnImages(child.gameObject, _material);
			}
			if (_go.GetComponent<Renderer>() != null)
			{
				_go.GetComponent<Renderer>().material = _material;
			}
		}

		// -------------------------------------------
		/* 
		 * Find out if the object is a number
		 */
		public static bool IsNumber(object _value)
		{
			return (_value is int) || (_value is float) || (_value is double);
		}

		// -------------------------------------------
		/* 
		 * Get the number as an integer
		 */
		public static int GetInteger(object _value)
		{
			if (_value is int)
			{
				return (int)_value;
			}
			if (_value is float)
			{
				return (int)((float)_value);
			}
			if (_value is double)
			{
				return (int)((double)_value);
			}
			return -1;
		}

		// -------------------------------------------
		/* 
		 * Get the number as an float
		 */
		public static float GetFloat(object _value)
		{
			if (_value is int)
			{
				return (int)_value;
			}
			if (_value is float)
			{
				return (float)_value;
			}
			if (_value is double)
			{
				return (float)((double)_value);
			}
			return -1;
		}

		// -------------------------------------------
		/* 
		 * Get the number as an float
		 */
		public static double GetDouble(object _value)
		{
			if (_value is int)
			{
				return (int)_value;
			}
			if (_value is float)
			{
				return (float)_value;
			}
			if (_value is double)
			{
				return (double)_value;
			}
			return -1;
		}

		// -------------------------------------------
		/* 
		 * Check if the string can be converted to a number
		 */
		public static bool IsStringInteger(string _value)
		{
			int valueInteger = -1;
			if (int.TryParse(_value, out valueInteger))
			{
				return true;
			}

			return false;
		}


		// -------------------------------------------
		/* 
		 * Check if the string can be converted to a number
		 */
		public static bool IsStringFloat(string _value)
		{
			float valueFloat = -1;
			if (float.TryParse(_value, out valueFloat))
			{
				return true;
			}

			return false;
		}

		// -------------------------------------------
		/* 
		 * Check if the string can be converted to a number
		 */
		public static bool IsStringDouble(string _value)
		{
			float valueDouble = -1;
			if (float.TryParse(_value, out valueDouble))
			{
				return true;
			}

			return false;
		}

		// -------------------------------------------
		/* 
		 * Check if the string can be converted to a vector3
		 */
		public static bool IsStringVector3(string _value)
		{
			float valueFloat = -1;
			string[] vector = _value.Split(',');
			if (vector.Length == 3)
			{
				for (int i = 0; i < vector.Length; i++)
				{
					if (!float.TryParse(vector[i], out valueFloat))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
	}
}