using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

public class WindowsVoice : MonoBehaviour
{
#if !UNITY_SERVER
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
#endif

    public static WindowsVoice theVoice = null;
    // Use this for initialization
    void OnEnable()
    {
        if (theVoice == null)
        {
            theVoice = this;
            Debug.Log("Initializing speech");
#if !UNITY_SERVER
      initSpeech();
#endif
        }
    }
    public static void Speak(string msg, float delay = 0f)
    {
#if !UNITY_SERVER
      int volume = (int)(Game_Settings.currAudioSettings.Get("voice_prompt_volume", 0.5f) * 100.0f);
      print(volume.ToString());
      if ( delay == 0f )
        addToSpeechQueue("<volume level=\"" + volume.ToString() + "\"><voice required=\"Gender=Female;Age!=Child\"><rate speed=\"2\">" + msg + "</rate></voice></volume>");
      else{}
        //theVoice.ExecuteLater(delay, () => speak(msg));
#endif
    }
    void OnDestroy()
    {
        if (theVoice == this)
        {
#if !UNITY_SERVER
      destroySpeech();
#endif
            theVoice = null;
        }
    }
    public static string GetStatusMessage()
    {
#if !UNITY_SERVER
    StringBuilder sb = new StringBuilder(40);
    statusMessage(sb, 40);
    return sb.ToString();
#else
        return "";
#endif
    }
}