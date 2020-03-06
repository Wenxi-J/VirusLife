using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour {

    public bool protectFlag = false;        //是否戴口罩
    public Material health;
    public Material ill;
    public Material incubation;
    public Material protect;

    public Text timeText;
    public Text monologueText;
    public Image overImage;

    private List<People> peopleList = new List<People>();
    private static Manager _instance;

    private int days = 1;
    private int hours = 1;          //凌晨1-夜间24
    private int seconds = 0;
    private bool isEmergency = false;
    public static Manager Instance
    {
        get
        {
            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;
    }
	// Use this for initialization
	void Start () {
        Invoke("RangePeople", 1.0f);
	}

    void FixedUpdate()
    {
        UpdateTime();
 
    }

    void CheckPeople()
    {
        bool overFlag = false;
        foreach(People people in peopleList)
        {
            if(people.GetHealthLevel() != HealthLevel.ILL)
            {
                overFlag = false;
                break;
            }
            else
            {
                overFlag = true;
            }
        }

        if(overFlag)
        {
            overImage.gameObject.SetActive(true);
        }
    }

    void UpdateTime()
    {
        seconds++;
        //Debug.Log("seconds:" + seconds);
        if (seconds >= 100)
        {
            seconds = 0;
            hours++;
            //Debug.Log("hours:" + hours);
            UpdateMonologueUI();
            CheckPeople();
        }

        if (hours >= 24)
        {
            hours = 1;
            days++;
            //Debug.Log("Day:" + days);
        }
        UpdateTimeUI(days, hours, seconds);
    }

    /*
     * 更新时间UI
     */
    void UpdateTimeUI(int days, int hours, int seconds)
    {
        timeText.text = "天:" + days + "\n" + "时:" + hours + "\n" + "秒:" + seconds;
    }

    void UpdateMonologueUI()
    {
        if (isEmergency == false)
        {
            if (hours >= 5 && hours <= 8)
            {
                monologueText.text = "快乐的小球，起床去吃早饭";
            }
            else if (hours > 8 && hours <= 12)
            {
                monologueText.text = "快乐的小球，吃完早饭去上课";
            }
            else if (hours > 12 && hours <= 13)
            {
                monologueText.text = "快乐的小球，上完课去吃午饭";
            }
            else if (hours > 13 && hours <= 18)
            {
                monologueText.text = "快乐的小球，吃完午饭去上课";
            }
            else if (hours > 18 && hours <= 21)
            {
                monologueText.text = "快乐的小球，上完课去吃晚饭";
            }
            else if (hours > 21 && hours <= 23)
            {
                monologueText.text = "快乐的小球，吃完晚饭回屋休息";
            }
            else if (hours > 23 || hours < 5)
            {
                monologueText.text = "快乐的小球，开始睡觉";
            }
        }
        else
        {
            monologueText.text = "紧急事件，全部回寝室隔离";
        }
 
    }

    /*
     * 随机取得一个球被感染
     */
    void RangePeople()
    {
        int total = peopleList.Count;
        int index = Random.Range(0, total-1);
        peopleList[index].SetHealthLevel(HealthLevel.INCUBATION);
    }

    public void AddPeople(People people)
    {
        peopleList.Add(people);
    }

    public void Emergency()
    {
        if(isEmergency == false)
        {
            isEmergency = true;
            foreach(People people in peopleList)
            {
                people.GoRoom();
            }
        }
    }

    public int GetDays()
    {
        return days;
    }

    public int GetHours()
    {
        return hours;
    }

    public int GetSeconds()
    {
        return seconds;
    }
}

