using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour 
{	
	public static AudioManager Instance {get; private set;}	

	public Sound[] sounds;	
	

	void Awake () 
	{
		if (Instance == null)
			Instance = this;
		
		foreach(Sound s in sounds)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;
			s.source.volume = s.volume;
			s.source.pitch = s.pitch;
			s.source.loop = s.loop;
			s.source.maxDistance = 5f;	
		}			
	}
		

	public void Play (string name) 
	{
		if (!BrakeBricks.GameManager.CurrentGame.IsSoundPlay)
			return;

		Sound s = Array.Find(sounds, sound => sound.name == name);

		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not faund!");
			return;
		}

		if (!s.source.isPlaying)
			s.source.Play();
	}


	public void Stop (string name) 
	{
		Sound s = Array.Find(sounds, sound => sound.name == name);

		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not faund!");
			return;
		}

		if (s.source.isPlaying)
			s.source.Stop();
	}

	

	public void SetMusicVolume(float value)
	{		
		foreach(Sound s in sounds)
		{
			//если это музыка
			if (s.isMusic)
				s.source.volume = value;
		}
	}


	public void SetSoundVolume(float value)
	{		
		foreach(Sound s in sounds)
		{
			//если это звук
			if (!s.isMusic)
				s.source.volume = value;
		}
	}


	public float GetSoundValue()
	{
		int count = 0;
		float calcValue = 0f;

		foreach(Sound s in sounds)
		{
			//если это звук
			if (!s.isMusic)
			{
				count++;
				calcValue += s.source.volume;
			}				
		}

		return calcValue / count;
	}


	public float GetMusicValue()
	{
		int count = 0;
		float calcValue = 0f;

		foreach(Sound s in sounds)
		{
			//если это музыка
			if (s.isMusic)
			{
				count++;
				calcValue += s.source.volume;
			}				
		}

		return calcValue / count;
	}

}
