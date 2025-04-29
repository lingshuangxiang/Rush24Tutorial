using System;
using System.Collections;
using System.Collections.Generic;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QuestionList : MonoBehaviour
{
    [SerializeField] public GameObject ButtonWrapper;
    [SerializeField] public Transform MyIndexPointer,OtherIndexPointer;
    [SerializeField] public Transform PartnerIndexPointer;
    public UnityEvent<int> OnSetIndex;
    private List<QuestionIndexButton> btnList;
    [NonSerialized]
    public int myIndex = -1, partnerIndex = -1;
    
    // Start is called before the first frame update
    void Start()
    {
        btnList = new List<QuestionIndexButton>();
        for (int i = 0; i < ButtonWrapper.transform.childCount; i++)
        {
            btnList.Add(ButtonWrapper.transform.GetChild(i).GetComponent<QuestionIndexButton>());
        }
        //init selection
        StartCoroutine(InitIndex());
    }
    
    IEnumerator InitIndex()
    {
        yield return 100;
        SetIndex(0);
        SetPartnerIndex(0);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    public void ShowOtherIndex(int index)
    {
        OtherIndexPointer.SetParent(ButtonWrapper.transform.GetChild(index));
        OtherIndexPointer.localPosition = new Vector3(OtherIndexPointer.localPosition.x, 0,
            OtherIndexPointer.localPosition.z);
    }

    /// <summary>
    /// 下一题上一题目的按钮
    /// </summary>
    /// <param name="mindex"></param>
    public void btn_SetNextButton(int mindex)
    {
        if(myIndex+mindex >= 5||myIndex+mindex<0)return;
        SetIndex(myIndex+mindex);
    }

    public void SetIndex(int i)
    {
        StartCoroutine(_SetIndex(i));
    }

    IEnumerator _SetIndex(int i)
    {
        Debug.Log("HasSetIndex:" + i);
        MuninnManager.SyncStatus(i);
        myIndex = i;
        ResetOthers(i);
        yield return null;
        
        btnList[i].Select();
        // ButtonWrapper.transform.GetChild(i).GetComponent<Button>().onClick.Invoke();
        
        MyIndexPointer.SetParent(ButtonWrapper.transform.GetChild(i));
        MyIndexPointer.localPosition = new Vector3(MyIndexPointer.localPosition.x, 0,
            MyIndexPointer.localPosition.z);

        string resolvedTeam = BattleEffectManager.GetResolvedTeam(i);

        if (!MuninnManager.MyTeamTag.ToString().Equals(resolvedTeam) && resolvedTeam != "")
        {
            BattleEffectManager.instance.Effect_SelectOtherTeamDone();
        }
        else if (MuninnManager.MyTeamTag.ToString().Equals(resolvedTeam) && resolvedTeam != "")
        {
            BattleEffectManager.instance.Effect_MyTeamDone();
        }
        else
        {
            BattleEffectManager.instance.Effect_NoTeamDoneYet();
        }
        
        OnSetIndex?.Invoke(i);
    }

    public void SetCorrect(int i,string teamsName="")
    {
        if (i < 0) return;
        btnList[i].SetCorrect(teamsName);
    }
    
    public void SetPartnerIndex(int i)
    {
        if (partnerIndex != i)
        {
            partnerIndex = i;
            
            PartnerIndexPointer.SetParent(ButtonWrapper.transform.GetChild(i));
            PartnerIndexPointer.localPosition = new Vector3(PartnerIndexPointer.localPosition.x, 0,
                PartnerIndexPointer.localPosition.z);
        }
    }

    private void ResetOthers(int cur)
    {
        for (int i=0; i < btnList.Count; i++)
        {
            if (cur != i)
            {
                btnList[i].Deselect();
            }
        }
    }
}
