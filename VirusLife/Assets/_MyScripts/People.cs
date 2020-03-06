using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/**
 * 潜伏期感染率:不戴口罩被感染率:4%  戴口罩被感染率:1%
 * 生病期感染率:不戴口罩被感染率:8%  戴口罩被感染率:2%
 * 抗体强度:2-10个单位，每次被感染消耗一个单位
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

    //教室，隔离室，食堂有两个地点方位，随机选择去哪个
    public Transform roomPos;
    public Transform classRoomPos1;
    public Transform canteenPos1;
    public Transform isolatePos1;
    public Transform classRoomPos2;
    public Transform canteenPos2;
    public Transform isolatePos2;

    private Transform classRoomPos;
    private Transform canteenPos;
    private Transform isolatePos;

    private HealthLevel healthLevel = HealthLevel.HEALTH;   //健康等级
    private NavMeshAgent agent;

    private Status state = Status.IDLETIME; //当前学生在做什么
    private int maxImmunity = 10;           //最大抵抗力单位
    private int minImmunity = 2;            //最小抵抗力单位
    private int infectionRate = 400;        //潜伏期感染率
    private int illInfectionRate = 800;     //生病期感染率
    private int maxDays = 10;
    private int minDays = 3;
    private int immunity = 0;               //抵抗力
    private float incubDays = 1;            //多少天后发病
    private bool goRoom = false;    //发生紧急事件立即回寝室隔离
    private bool toIsolate = false; //是否需要被隔离

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

	// Use this for initialization
	void Start () {

        int r = Random.Range(0, 2);
        if (0 == r)
        {
            classRoomPos = classRoomPos1;
            canteenPos = canteenPos1;
            isolatePos = isolatePos1;
        }
        else
        {
            classRoomPos = classRoomPos2;
            canteenPos = canteenPos2;
            isolatePos = isolatePos2;
        }

        if(Manager.Instance.protectFlag)
        {
            this.gameObject.GetComponent<MeshRenderer>().material = Manager.Instance.protect;
            infectionRate = 100;
            illInfectionRate = 200;
        }
        Manager.Instance.AddPeople(this);
        immunity = Random.Range(minImmunity, maxImmunity);

    }
	

    void FixedUpdate()
    {
        //检测是否发病
        if (healthLevel == HealthLevel.INCUBATION && Manager.Instance.GetDays() >= incubDays)
        {
            SetHealthLevel(HealthLevel.ILL);
            //Manager.Instance.Emergency();     //是否发布紧急事件，发布后全部回房间隔离
            toIsolate = true;
            goRoom = false;
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
            else if (Manager.Instance.GetHours() > 12 && Manager.Instance.GetHours() <= 15 && state == Status.LUNCHTIME)
            {
                state = Status.AFTERMOONTIME;
                agent.destination = canteenPos.position;
            }
            else if (Manager.Instance.GetHours() > 15 && Manager.Instance.GetHours() <= 18 && state == Status.AFTERMOONTIME)
            {
                state = Status.DINNERTIME;
                agent.destination = classRoomPos.position;
            }
            else if (Manager.Instance.GetHours() > 18 && Manager.Instance.GetHours() <= 21 && state == Status.DINNERTIME)
            {
                state = Status.RESTTIME;
                agent.destination = canteenPos.position;
            }
            else if (Manager.Instance.GetHours() > 21 && Manager.Instance.GetHours() <= 23 && state == Status.RESTTIME)
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
        else if(goRoom == false && toIsolate == true)
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
            int probability = Random.Range(0, 10000);
            if(probability >=0 && probability < infectionRate && this.healthLevel == HealthLevel.INCUBATION)
            {
                collision.gameObject.SendMessage("Incubation");
            }
            else if(probability >= 0 && probability < illInfectionRate && this.healthLevel == HealthLevel.ILL)
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
                this.incubDays = Random.Range(minDays, maxDays) + Manager.Instance.GetDays();
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
        if(healthLevel == HealthLevel.HEALTH && immunity <=0)
        {
            SetHealthLevel(HealthLevel.INCUBATION);
        }
        else
        {
            immunity--;
        }
    }

    public void GoRoom()
    {
        state = Status.EMERGENCYTIME;
        this.goRoom = true;
    }
}
