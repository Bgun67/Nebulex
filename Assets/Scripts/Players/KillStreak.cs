using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillStreak : MonoBehaviour
{
	public AudioSource source;
	public float volume;
	public AudioClip[] tracks;
	int currentTrack;

	int localNum;
	int kills;
	int deaths;
	int deltaKills;

	Game_Controller gameController;
	// Start is called before the first frame update
	void Start()
    {
		gameController = FindObjectOfType<Game_Controller>();
		localNum = gameController.GetLocalPlayer();
	}
	void Reset()
	{
		source = gameObject.AddComponent<AudioSource>();
	}

	// Update is called once per frame
	void Update()
	{
		if (Time.frameCount % 6 != 0)
		{
			return;
		}

		CheckStreak();
		if(!source.isPlaying){
			NextTrack();
		}
	}
	void CheckStreak()
	{
		deltaKills = gameController.playerStats[localNum].kills - kills;
		//player has been killed
		if (gameController.playerStats[localNum].deaths > deaths)
		{
			ResetStreak();
			StartCoroutine(FadeSourceVolume(0f));
		}
		if (deltaKills > 1)
		{
			StartCoroutine(FadeSourceVolume(Mathf.Clamp01(deltaKills/10f)));
		}
	}
	void ResetStreak()
	{
		deltaKills = 0;
		kills = gameController.playerStats[localNum].kills;
		deaths = gameController.playerStats[localNum].deaths;
		print("Resetting Streak");

	}
	void NextTrack()
	{
		for (int j = 0; j < 4; j++)
		{
			int i = Random.Range(0, tracks.Length - 1);
			if (i != currentTrack)
			{
				currentTrack = i;
				break;
			}
		}
		source.clip = tracks[currentTrack];
		source.Play();
	}
	IEnumerator FadeSourceVolume(float nextVolume)
	{
		while (Mathf.Abs(volume-nextVolume)>0.01f)
		{
			volume = Mathf.Lerp(volume, nextVolume, 0.1f);
			source.volume = volume;
			yield return new WaitForEndOfFrame();
		}
	}
}
