using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/**
 * 潜伏期感染率:2%
 * 生病期感染率:5%
 * 总抗体率:50%
 * 抗体强度:20个单位，每次被感染消耗一个单位
 */
public enum Status
{
    IDLETIME = 0,   //休息时间
    BREAKFASTTIME,  //早餐时间
    MORNINGTIME,    //上午上课
    LUNCHTIME,      //午餐时间
    AFTERMOONTIME,  //下午上课
    DINNERTIME,     //晚餐时间
    RESTTIME,       //休息时间
    EMERGENCYTIME,  //紧急情况
    ISOLATETIME     //隔离情况
}

public enum HealthLevel
{
    HEALTH = 20,    //健康
    INCUBATION,     //潜伏
    ILL             //生病
}


public class People : MonoBehaviour {

    public Transform roomPos;
    public Transform classRoomPos;
    public Transform canteenPos;
    public Transform isolatePos;

    private Transform startPos;
    private HealthLevel healthLevel = HealthLevel.HEALTH;
    private NavMeshAgent agent;

    private Status state = Status.IDLETIME; //当前学生在做什么
    private float incubDays = 1;
    private bool goRoom = false;    //发生紧急事件立即回寝室隔离
    private bool toIsolate = false; //是否需要被隔离

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        startPos = this.gameObject.transform;
    }

	// Use this for initialization
	void Start () {

        Manager.Instance.AddPeople(this);
        

    }
	

    void FixedUpdate()
    {
        //检测是否发病
        if (healthLevel == HealthLevel.INCUBATION && Manager.Instance.GetDays() >= incubDays)
        {
            SetHealthLevel(HealthLevel.ILL);
            //Manager.Instance.Emergency();
            toIsolate = true;
            state = Status.ISOLATETIME;
        }
            

        if(goRoom == false && toIsolate == false)
        {
            if (Manager.Instance.GetHours() >= 5 && Manager.Instance.GetHours() <= 8 && state == Status.BREAKFASTTIME)
            {
                state = Status.MORNINGTIME;
                agent.destination = canteenPos.position;
            }
            else if (Manager.Instance.GetHours() > 8 && Manager.Instance.GetHours() <= 12 && state == Status.MORNINGTIME)
            {
                state = Status.LUNCHTIME;
                agent.destination = classRoomPos.position;
            }
            else if (Manager.Instance.GetHours() > 12 && Manager.Instance.GetHours() <= 14 && state == Status.LUNCHTIME)
            {
                state = Status.AFTERMOONTIME;
                agent.destination = canteenPos.position;
            }
            else if (Manager.Instance.GetHours() > 14 && Manager.Instance.GetHours() <= 18 && state == Status.AFTERMOONTIME)
            {
                state = Status.DINNERTIME;
                agent.destination = classRoomPos.position;
            }
            else if (Manager.Instance.GetHours() > 18 && Manager.Instance.GetHours() <= 20 && state == Status.DINNERTIME)
            {
                state = Status.RESTTIME;
                agent.destination = canteenPos.position;
            }
            else if (Manager.Instance.GetHours() > 20 && Manager.Instance.GetHours() <= 23 && state == Status.RESTTIME)
            {
                state = Status.IDLETIME;
                agent.destination = roomPos.position;
            }
            else if ((Manager.Instance.GetHours() > 23 || Manager.Instance.GetHours() <5) && state == Status.IDLETIME)
            {
                if (Manager.Instance.GetHours() >= 4 && Manager.Instance.GetHours() < 5)
                {
                    agent.enabled = true;
                    state = Status.BREAKFASTTIME;
                }
                else
                {
                    //if(agent.enabled == true)
                    //agent.enabled = false;
                }
            }
        }
        else if(goRoom == true && toIsolate == false)
        {
            if(state == Status.EMERGENCYTIME)
            {
                state = Status.IDLETIME;
                agent.destination = roomPos.position;
            }
        }
        else if(toIsolate == true)
        {
            if(state == Status.ISOLATETIME)
            {
                state = Status.IDLETIME;
                agent.destination = isolatePos.position;
            }
            
        }
        
    }



    /*
     * 碰撞检测函数
     */
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "People")
        {
            float probability = Random.Range(0, 10000);
            if(probability >=0.0f && probability < 200.0f && this.healthLevel == HealthLevel.INCUBATION)
            {
                collision.gameObject.SendMessage("Incubation");
            }
            else if(probability >= 0.0f && probability < 300.0f && this.healthLevel == HealthLevel.ILL)
            {
                collision.gameObject.SendMessage("Incubation");
            }
        }
    }

    /*
     * 设置健康等级
     */
    public void SetHealthLevel(HealthLevel level)
    {
        this.healthLevel = level;
        switch(this.healthLevel)
        {
            case HealthLevel.HEALTH:
                this.gameObject.GetComponent<MeshRenderer>().material = Manager.Instance.health;
                break;
            case HealthLevel.INCUBATION:
                this.gameObject.GetComponent<MeshRenderer>().material = Manager.Instance.incubation;
                this.incubDays = Random.Range(3, 7);
                break;
            case HealthLevel.ILL:
                this.gameObject.GetComponent<MeshRenderer>().material = Manager.Instance.ill;
                break;
        }
    }

    /*
     * 获取健康等级
     */
    public HealthLevel GetHealthLevel()
    {
        return this.healthLevel;
    }

    /*
     * 设置被感染
     */
    public void Incubation()
    {
        if(healthLevel == HealthLevel.HEALTH)
        {
            SetHealthLevel(HealthLevel.INCUBATION);
        }
    }

    public void GoRoom()
    {
        state = Status.EMERGENCYTIME;
        this.goRoom = true;
    }
}
