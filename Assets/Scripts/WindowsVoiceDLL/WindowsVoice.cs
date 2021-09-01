using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
//using UniExtensions;

public class WindowsVoice : MonoBehaviour {
  [DllImport("WindowsVoice")]
  public static extern void initSpeech();
  [DllImport("WindowsVoice")]
  public static extern void destroySpeech();
  [DllImport("WindowsVoice")]
  public static extern void addToSpeechQueue(string s);
  [DllImport("WindowsVoice")]
  public static extern void clearSpeechQueue();
  [DllImport("WindowsVoice")]
  public static extern void statusMessage(StringBuilder str, int length);
  public static WindowsVoice theVoice = null;
	// Use this for initialization
	void OnEnable () {
    if (theVoice == null)
    {
      theVoice = this;
      Debug.Log("Initializing speech");
      initSpeech();
    }
	}
  public static void Speak(string msg, float delay = 0f) {
    int volume = (int)(Game_Settings.currAudioSettings.voicePromptVolume * 100.0f);
    print(volume.ToString());
    if ( delay == 0f )
      addToSpeechQueue("<volume level=\"" + volume.ToString() + "\"><voice required=\"Gender=Female;Age!=Child\"><rate speed=\"2\">" + msg + "</rate></voice></volume>");
    else{}
      //theVoice.ExecuteLater(delay, () => speak(msg));
  }
  void OnDestroy()
  {
    if (theVoice == this)
    {
      Debug.Log("Destroying speech");
      destroySpeech();
      theVoice = null;
    }
  }
  public static string GetStatusMessage()
  {
    StringBuilder sb = new StringBuilder(40);
    statusMessage(sb, 40);
    return sb.ToString();
  }
}