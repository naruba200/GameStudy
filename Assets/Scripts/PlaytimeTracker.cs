using TMPro;
using UnityEngine;

public class PlaytimeTracker : MonoBehaviour
{
    [SerializeField] private TMP_Text playtimeText;

    private void Update()
    {
        if (playtimeText == null)
        {
            return;
        }

        float elapsedSeconds = SessionPlaytime.GetSeconds();
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