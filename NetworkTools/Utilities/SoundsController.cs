using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * SoundsController
	 * 
	 * Class that handles the sounds played in the app
	 * 
	 * @author Esteban Gallardo
	 */
	public class SoundsController : MonoBehaviour
	{
		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------
		public const string SOUND_MAIN_MENU = "SOUND_MAIN_MENU";
		public const string SOUND_SELECTION_FX = "SOUND_FX_SELECTION";

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------
		private static SoundsController _instance;

		public static SoundsController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType<SoundsController>();
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------
		public const string SOUND_COOCKIE = "SOUND_COOCKIE";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------
		public AudioClip[] Sounds;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private AudioSource m_audio1;
		private AudioSource m_audio2;
		private bool m_enabled;

		public bool Enabled
		{
			get { return m_enabled; }
			set
			{
				m_enabled = value;
				if (!m_enabled)
				{
					StopAllSounds();
				}
				PlayerPrefs.SetInt(SOUND_COOCKIE, (m_enabled ? 1 : 0));
			}
		}

		// ----------------------------------------------
		// CONSTRUCTOR
		// ----------------------------------------------	
		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public SoundsController()
		{
		}

		// ----------------------------------------------
		// INIT/DESTROY
		// ----------------------------------------------	

		// -------------------------------------------
		/* 
		 * Destroy audio's gameObject		
		 */
		public void Initialize()
		{
			AudioSource[] aSources = GetComponents<AudioSource>();
			m_audio1 = aSources[0];
			m_audio2 = aSources[1];

			m_enabled = (PlayerPrefs.GetInt(SOUND_COOCKIE, 1) == 1);
		}

		// -------------------------------------------
		/* 
		 * Release resources on unload
		 */
		void OnDestroy()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * Destroy audio's gameObject		
		 */
		public void Destroy()
		{
			Destroy(m_audio1);
			Destroy(m_audio2);
		}

		// -------------------------------------------
		/* 
		 * StopAllSounds
		 */
		public void StopAllSounds()
		{
			m_audio1.Stop();
			m_audio2.Stop();
		}

		// -------------------------------------------
		/* 
		 * Play loop
		 */
		public void PlaySoundLoop(AudioClip _audio)
		{
			if (_audio == null) return;
			if (!m_enabled) return;

			m_audio1.clip = _audio;
			m_audio1.loop = true;
			if (!m_audio1.isPlaying)
			{
				m_audio1.Play();
			}
		}

		// -------------------------------------------
		/* 
		 * StopAllSounds
		 */
		public void StopMainLoop()
		{
			m_audio1.clip = null;
			m_audio1.Stop();
		}

		// -------------------------------------------
		/* 
		 * PlayLoopSound
		 */
		public void PlayLoopSound(string _audioName)
		{
			for (int i = 0; i < Sounds.Length; i++)
			{
				if (Sounds[i].name == _audioName)
				{
					PlaySingleSound(Sounds[i]);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * PlaySingleSound
		 */
		public void PlaySingleSound(string _audioName)
		{
			for (int i = 0; i < Sounds.Length; i++)
			{
				if (Sounds[i].name == _audioName)
				{
					PlaySingleSound(Sounds[i]);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * PlaySingleSound
		 */
		public void PlaySingleSound(AudioClip _audio)
		{
			if (!m_enabled) return;
			if (_audio != null)
			{
				m_audio2.PlayOneShot(_audio);
			}
		}

		// -------------------------------------------
		/* 
		 * PlayMainMenu
		 */
		public void PlayMainMenu()
		{
			SoundsController.Instance.PlayLoopSound(SOUND_MAIN_MENU);
		}

		// -------------------------------------------
		/* 
		 * PlaySingleSound
		 */
		public void PlayFxSelection()
		{
			SoundsController.Instance.PlaySingleSound(SOUND_SELECTION_FX);
		}

	}
}