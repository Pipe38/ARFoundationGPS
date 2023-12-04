using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

public class TestDownloadLeaderboard : MonoBehaviour
{
    public string LeaderboardURL;

    public string[] ScoreLines;
    public List<ScoreData> MyScoreData = new List<ScoreData>();

    [Header ("Ui Variables: ")]
    public SetEntryData EntryPrefab;
    public RectTransform ContentRectTransform;
    public float OffsetBetweenEntries = 5;
    List<SetEntryData> scoreEntries = new List<SetEntryData>();
    bool instantiateEntries;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("UpdateScoreboard", 1, 5);
    }

    // Update is called once per frame
    void Update()
    {
        if (instantiateEntries)
        {
            for (int i = 0; i < scoreEntries.Count; i++)
            {
                Destroy(scoreEntries[i].gameObject);
            }
            scoreEntries.Clear();

            float entryHeight = EntryPrefab.GetComponent<RectTransform>().rect.height;

            ContentRectTransform.sizeDelta = new Vector2(ContentRectTransform.sizeDelta.x, (entryHeight + OffsetBetweenEntries) * MyScoreData.Count);

            for (int i = 0; i < MyScoreData.Count; i++)
            {
                SetEntryData tempEntry = Instantiate(EntryPrefab, ContentRectTransform.transform);
                tempEntry.SetData(MyScoreData[i]);
                tempEntry.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (-entryHeight -OffsetBetweenEntries) * i);
                scoreEntries.Add(tempEntry);
            }         

            instantiateEntries = false;
        }
    }

    void UpdateScoreboard()
    {
        StartCoroutine(DownloadWebPage(LeaderboardURL));
    }


    IEnumerator DownloadWebPage(string urlToDownload)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(urlToDownload);
        yield return webRequest.SendWebRequest();

        switch (webRequest.result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogError("Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError("HTTP Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.Success:
                Debug.Log("Received: " + webRequest.downloadHandler.text);

                ScoreLines = webRequest.downloadHandler.text.Split('\n');

                MyScoreData.Clear();
                for (int i = 0; i < ScoreLines.Length; i++)
                {
                    if (!string.IsNullOrEmpty(ScoreLines[i]))
                    {
                        MyScoreData.Add(new ScoreData(ScoreLines[i]));
                    }
                }
                instantiateEntries = true;
                break;
        }


        yield return null;
    }
}

[System.Serializable]
public class ScoreData
{
    public string UserName;
    public int UserScore;

    public ScoreData(string inUserName, int inUserScore)
    {
        UserName = inUserName;
        UserScore = inUserScore;
    }

    public ScoreData(string commaWebData)
    {
        string[] dataFields = commaWebData.Split(',');
        UserName = dataFields[0].Substring(1, dataFields[0].Length -2);
        UserScore = int.Parse(dataFields[1].Substring(1, dataFields[1].Length - 2));
    }
}
