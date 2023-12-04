using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class TestWebRequest : MonoBehaviour
{
    public bool TestPostScore;

    public string MyDreamloLink;
    public string UserName;
    public int UserScore;
    public TextAsset BadWordsDatabaseIT;

    string Url;

    char[] illegalChars = new char[] {',','"'};
    public string[] badWordsArray;

    // Start is called before the first frame update
    void Start()
    {
        badWordsArray = BadWordsDatabaseIT.text.Split('\n');
    }

    // Update is called once per frame
    void Update()
    {
        if (TestPostScore)
        {
            if (ValidateString(UserName) && ValidateBadWords(UserName))
            {
                AddScoreToLeaderboard(UserName, UserScore);
            }
            else
            {
                Debug.LogError("Illegal Username!");
            }
            TestPostScore = false;
        }
    }

    bool ValidateString(string stringToValidate)
    {
        bool stringIsOk = true;

        for (int i = 0; i < illegalChars.Length; i++)
        {
            if (stringToValidate.Contains(illegalChars[i]))
            {
                stringIsOk = false;
            }
        }

        return stringIsOk;
    }

    bool ValidateBadWords(string stringToValidate)
    {
        bool stringIsOk = true;

        string lowerCaseStringToValidate = stringToValidate.ToLower();

        for (int i = 0; i < badWordsArray.Length; i++)
        {
            if (!string.IsNullOrEmpty(badWordsArray[i]))
            {
                if (lowerCaseStringToValidate.Contains(badWordsArray[i]))
                {
                    stringIsOk = false;
                }
            }
        }

        return stringIsOk;
    }

    public void AddScoreToLeaderboard(string usernameToPost, int scoreToPost)
    {
        Url = MyDreamloLink + "/add/" + usernameToPost + "/" + scoreToPost.ToString();
        StartCoroutine(PostScoreOnDreamlo(Url));
    }

    IEnumerator DownloadWebPage(string urlToDownload)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(urlToDownload);
        yield return webRequest.SendWebRequest();

        switch (webRequest.result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogError( "Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError( "HTTP Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.Success:
                Debug.Log("Received: " + webRequest.downloadHandler.text);
                break;
        }


        yield return null;
    }

    IEnumerator PostScoreOnDreamlo(string urlToDownload)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(urlToDownload);
        yield return webRequest.SendWebRequest();


        int numberOfTentatives = 5;
        bool pageOK = false;

        while (!pageOK && numberOfTentatives > 0)
        {
            if (webRequest.result == UnityWebRequest.Result.Success && webRequest.downloadHandler.text == "OK")
            {
                pageOK = true;
                print("Score Set!");
            }
            else
            {
                numberOfTentatives--;
                yield return webRequest.SendWebRequest();
            }            
        }
        if(numberOfTentatives <= 0)
        {
            Debug.LogError("Failed to post score!!");
            yield break;
        }


        yield return null;
    }
}
