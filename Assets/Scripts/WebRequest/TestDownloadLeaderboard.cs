using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class TestDownloadLeaderboard : MonoBehaviour
{
    public string LeaderboardURL;

    public string[] ScoreLines;
    public List<ScoreData> MyScoreData = new List<ScoreData>();

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DownloadWebPage(LeaderboardURL));
    }

    // Update is called once per frame
    void Update()
    {
        
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
                for (int i = 0; i < ScoreLines.Length; i++)
                {
                    if (!string.IsNullOrEmpty(ScoreLines[i]))
                    {
                        MyScoreData.Add(new ScoreData(ScoreLines[i]));
                    }
                }
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
