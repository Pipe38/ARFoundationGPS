using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetEntryData : MonoBehaviour
{ 
    public TMP_Text UserNameText, UserScoreText;

    public void SetData(ScoreData entryData)
    {
        UserNameText.text = entryData.UserName;
        UserScoreText.text = entryData.UserScore.ToString();
    }
}
