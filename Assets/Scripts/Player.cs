using UnityEngine;
using UnityEngine.UI;


public class Player : MonoBehaviour
{
    public GameObject SQL;
    public Text playerName;
    public Text[] playerStats;
    public GameObject[] exitButtons;

    string[] texts;
    string[] stats;

    void Awake()    //存各个物体里面的文本，方便调用
    {
        stats = new string[6];
        texts = new string[7];
        for (int i = 0; i <= 5; i++) texts[i] = playerStats[i].text;
        texts[6] = playerName.text;
    }

    void OnEnable()     //每次进入游戏后初始化
    {
        exitButtons[0].SetActive(true);
        exitButtons[1].SetActive(false);
    }

    public void Roll()      //唯一游戏要素Roll点(x
    {
        int[] dice = new int[3];
        for (int i = 0; i <= 5; i++)
        {
            int point = 0;
            for (int j = 0; j <= 2; j++)
            {
                dice[j] = Random.Range(0, 7);
                if (dice[j] == 7) dice[j]--;
                point += dice[j];
            }
            stats[i] = point.ToString();
            playerStats[i].text = texts[i] + stats[i];
        }
    }

    public void LoadInClient(string name,string[] cloudStats)       //将传入的六维应用于界面
    {
        for (int i = 0; i <= 5; i++)
        {
            stats[i] = cloudStats[i];
            playerStats[i].text = texts[i] + stats[i];
        }
        playerName.text = texts[6] + name;
    }

    public void SavePreparation(bool ifexit)            //存档准备，bool值在各按钮处已经设定
    {
        if (ifexit)         //保存并退出
        {
            SQL.GetComponent<SQL_Client>().SaveGame(stats);
            SQL.GetComponent<SQL_Client>().DisconnectGame();
        }
        else SQL.GetComponent<SQL_Client>().SaveGame(stats);        //直接退出
    }

    public void SwitchExitOption()          //切换退出菜单
    {
        bool windowState = exitButtons[0].activeInHierarchy;
        exitButtons[0].SetActive(!windowState);
        exitButtons[1].SetActive(windowState);
    }
}
