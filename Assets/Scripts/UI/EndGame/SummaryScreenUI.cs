using EditorAttributes;
using TMPro;
using UnityEngine;

public class SummaryScreenUI : MonoBehaviour
{
    [Title("\n<b><color=#ff8080>References", 15, 5, false)] 
    [SerializeField] private ScoreTrackerSO scoreTracker;
    [SerializeField] private TMP_Text outcomeText;
    [SerializeField] private TMP_Text totalScoreText;
    [SerializeField] private TMP_Text scoreCountsText;
    [SerializeField] private TMP_Text mainObjectiveText;
    [SerializeField] private TMP_Text bonusObjectiveText;

    [Title("\n<b><color=#ffd180>Attributes", 15, 5, false)] 
    [SerializeField] private Color completeColor;
    [SerializeField] private Color failColor;

    public void Setup()
    {
        totalScoreText.text = "Score: " + scoreTracker.CurrentScore;
        string scoreCounts = "";
        foreach (var scoreType in scoreTracker.RuntimeScoreCounts)
        {
            int scoreValue = scoreTracker.GetScoreValue(scoreType.Key, scoreType.Value);
            string totalScoreString = scoreValue > 0 ? "" + scoreValue : "";
            string scoreTypeString = scoreType.Key.ToString();
            // Add spaces between words
            scoreTypeString = System.Text.RegularExpressions.Regex.Replace(scoreTypeString, "(\\B[A-Z])", " $1");
            scoreCounts += $"x{scoreType.Value} {scoreTypeString} ({totalScoreString})\n";
        }
        scoreCountsText.text = scoreCounts;
        
        MissionSO currentMission = MissionManager.CurrentMission;

        string mainColorString = currentMission.MainObjective.Completed
            ? ColorUtility.ToHtmlStringRGB(completeColor)
            : ColorUtility.ToHtmlStringRGB(failColor);
            mainObjectiveText.text = $"<color=#{mainColorString}>{currentMission.MainObjective.GetObjectiveText()}</color>";

        string bonusString = "";
        foreach (var bonus in currentMission.BonusObjectives)
        {
            string bonusColorString = bonus.Completed
                ? ColorUtility.ToHtmlStringRGB(completeColor)
                : ColorUtility.ToHtmlStringRGB(failColor);
            bonusString += $"<color=#{bonusColorString}>{bonus.GetObjectiveText()}</color>\n";
        }
        bonusObjectiveText.text = bonusString;
    }


}
