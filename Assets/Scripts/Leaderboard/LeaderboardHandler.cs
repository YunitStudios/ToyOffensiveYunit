using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using EditorAttributes;
using Newtonsoft.Json;

public class LeaderboardHandler : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Input field for the player's name")]
    [SerializeField] private TMP_InputField inputName;

    [Tooltip("Text  for the player's score")]
    [SerializeField] private TMP_Text inputScore;

    [Tooltip("The content container inside the scroll view where score entries will be added")]
    [SerializeField] private Transform scrollableContent;

    [SerializeField] private GameObject submitWindow;

    [Header("Prefab")]
    [SerializeField] private GameObject scoreEntryPrefab;
    
    private Score[] scores;

    private bool loaded;
    
    public void LoadScores()
    {
        if (loaded)
            return;
        
        RefreshScores();
    }

    public void RefreshScores()
    {
        // get request from the db at https://katie.games/custom-html/soldiers/get_scores.php
        StartCoroutine(LoadScoresCoroutine());
    }

    public void StartSubmitScore()
    {
        submitWindow.SetActive(true);
        inputScore.text = ""+GameManager.ScoreTracker.CurrentScore;
    }

    public void UploadScore()
    {
        submitWindow.SetActive(false);
        // post request to the db at https://katie.games/custom-html/soldiers/upload_score.php
        StartCoroutine(UploadScoreCoroutine(inputName.text, inputScore.text != "" ? int.Parse(inputScore.text) : 0));
    }

    private void DisplayScores()
    {
        // populate the scrollable content with score entries

        foreach (Transform child in scrollableContent)
        {
            Destroy(child.gameObject);
        }
        
        if(scores == null)
            return;
        
        foreach(Score score in scores)
        {
            GameObject entry = Instantiate(scoreEntryPrefab, scrollableContent);
            TMP_Text[] texts = entry.GetComponentsInChildren<TMP_Text>();
            texts[0].text = ""+score.Name;
            texts[1].text = ""+score.Points;
            texts[2].text = $"{score.Date:yyyy-MM-dd}";
        }
    }

    private IEnumerator UploadScoreCoroutine(string playerName, int playerScore)
    {
        playerName = SanitizePlayerName(playerName);

        WWWForm form = new WWWForm();
        form.AddField("name", playerName);
        form.AddField("score", playerScore);

        using (UnityWebRequest www = UnityWebRequest.Post("https://katie.games/custom-html/soldiers/upload_score.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Debug.Log("Score uploaded successfully! Response: " + www.downloadHandler.text);
                LoadScores();   // show the scores
            }
            else
            {
                Debug.LogError("Failed to upload score: " + www.error);
            }
        }
    }

    private IEnumerator LoadScoresCoroutine()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://katie.games/custom-html/soldiers/get_scores.php"))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = webRequest.downloadHandler.text;
                // Debug.Log("Scores loaded successfully! Response: " + jsonResponse);
                ParseScoresFromJson(jsonResponse);
                DisplayScores();    // show the scores
            }
            else
            {
                Debug.LogError("Failed to load scores: " + webRequest.error);
            }
        }
    }

    private string SanitizePlayerName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "Player";
        }

        // trim to max 16 characters
        if (name.Length > 16)
        {
            name = name.Substring(0, 16);
        }

        // remove any characters that are not letters, numbers, spaces, underscores, or hyphens
        name = System.Text.RegularExpressions.Regex.Replace(name, @"[^a-zA-Z0-9 _\-]", "");

        name = name.Trim();

        // if the name is now empty after sanitization use a default
        if (string.IsNullOrEmpty(name))
        {
            return "Player";
        }

        return name;
    }


    private void ParseScoresFromJson(string json)
    {
        // going to use newtonsoft json for this as we needed it anyway for the save system
        scores = JsonConvert.DeserializeObject<Score[]>(json);
    }
}
