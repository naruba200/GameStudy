using TMPro;
using UnityEngine;

public class PlaytimeTracker : MonoBehaviour
{
    [SerializeField] private TMP_Text playtimeText;
    [SerializeField] private bool useUnscaledTime = true;

    private void Update()
    {
        if (playtimeText == null)
        {
            return;
        }

        float elapsedSeconds = useUnscaledTime ? Time.unscaledTime : Time.time;
        int totalMinutes = Mathf.FloorToInt(elapsedSeconds / 60f);
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        if (hours > 0)
        {
            playtimeText.text = "Playtime: " + hours + "h " + minutes.ToString("00") + "m";
        }
        else
        {
            playtimeText.text = "Playtime: " + minutes + "m";
        }
    }
}