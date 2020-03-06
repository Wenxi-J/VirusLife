using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour {

    public Material health;
    public Material ill;
    public Material incubation;

    public Text timeText;

    private List<People> peopleList = new List<People>();
    private static Manager _instance;

    private int days = 1;
    private int hours = 0;
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
        Invoke("RangePeople", 2.0f);
	}

    int i = 0;
    float time = 0.0f;
    void FixedUpdate()
    {
        i++;
        time += Time.deltaTime;
        Debug.Log("这是第" + i + "帧，时间:" + time);
        UpdateTime();
    }

    void UpdateTime()
    {
        seconds++;
        Debug.Log("seconds:" + seconds);
        if (seconds >= 300)
        {
            seconds = 0;
            hours++;
            Debug.Log("hours:" + hours);
        }

        if (hours >= 24)
        {
            hours = 0;
            days++;
            Debug.Log("Day:" + days);
        }
        UpdateTimeUI(days, hours, seconds);
    }

    void UpdateTimeUI(int days, int hours, int seconds)
    {
        timeText.text = "Days:" + days + "\n" + "Hours:" + hours + "\n" + "Seconds:" + seconds;
    }

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

