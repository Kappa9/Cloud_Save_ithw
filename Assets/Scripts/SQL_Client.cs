using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SQL_Client : MonoBehaviour
{
    private SqlAccess sqlAce;   //引用封装类
    MySqlConnection con;

    //项目基于.NET4.x，服务器中MySQL版本为8.0
    //需要8.0版本的MySql.Data.dll,Google.Protobuf.dll,I18N.dll,I18N.CJK.dll,I18N.West.dll以支持连接MySQL
    //API兼容性设置为.net4.x时，才能在打包后连接上数据库

    public GameObject PlayerWindow;
    public GameObject RegisterWindow;
    public GameObject LoginWindow;
    public InputField RegisterName;
    public InputField RegisterPW;
    public Text RegisterFailText;
    public InputField LoginName;
    public InputField LoginPW;
    public Text LoginFailText;
    public GameObject exitButton;

    string username;    //用户名

    void OnEnable() //初始化各界面状态
    {
        RegisterWindow.SetActive(false);
        LoginWindow.SetActive(true);
        PlayerWindow.SetActive(false);
    }

    public void Register()  //注册
    {
        username = RegisterName.text;
        string pw = RegisterPW.text;
        if (username.Length < 3 || username.Length > 10) RegisterFailText.text = "用户名应为3-10个英文或数字！";
        else if (pw.Length < 6 || pw.Length > 14) RegisterFailText.text = "密码应为6-14个字符！";
        else
        {
            bool fail = false;
            RegisterFailText.text = "";
            try
            {
                sqlAce = new SqlAccess();
                con = SqlAccess.con;
                string sql = ("select name from users ");
                Dictionary<int, List<string>> dic = sqlAce.QueryInfo(sql, con);             //字典在封装类中
                for (int i = 0; i < dic.Count; i++)             //用户名查重
                {
                    if (dic[i][0] == username)
                    {
                        RegisterFailText.text = "用户名已经存在，请更换新的用户名！";
                        fail = true;
                        break;
                    }
                }
                sqlAce.CloseMySQL();
                if (!fail)                                      //查重通过后添加用户
                {
                    sqlAce = new SqlAccess();
                    con = SqlAccess.con;
                    sql = string.Format("insert into users(name,password) values('{0}','{1}')", username, SHA1Encrypt(pw));
                    sqlAce.InsertInfo(sql, con);
                    RegisterFailText.text = "注册成功！请返回登录窗口。";
                    RegisterName.text = "";
                    RegisterPW.text = "";
                }
            }
            catch (Exception e)
            {
                RegisterFailText.text = "未知错误，可能是因为未连接到服务器";
                Debug.Log(e.ToString()); return;
            }
            finally { sqlAce.CloseMySQL(); }
        }
    }

    public void Login()         //登录，测试用时，用户名example4，密码123456
    {
        username = LoginName.text;
        string pw = LoginPW.text;
        if (username.Length < 3 || username.Length > 10) LoginFailText.text = "用户名应为3-10个英文或数字！";
        else if (pw.Length < 6 || pw.Length > 14) LoginFailText.text = "密码应为6-14个字符！";
        else
        {
            bool fail = true;
            LoginFailText.text = "";
            try
            {
                sqlAce = new SqlAccess();
                con = SqlAccess.con;
                string sql = ("select name,password from users ");
                Dictionary<int, List<string>> dic = sqlAce.QueryInfo(sql, con);
                for (int i = 0; i < dic.Count; i++)
                {
                    if (dic[i][0] == username && dic[i][1] == SHA1Encrypt(pw))
                    {
                        fail = false;
                        LoadGame();                 //用户名密码验证通过后加载游戏
                        break;
                    }
                }
                if (fail) LoginFailText.text = "用户名或密码错误！";
            }
            catch (Exception e)
            {
                LoginFailText.text = "未知错误，可能是因为未连接到服务器";
                Debug.Log(e.ToString()); return;
            }
            finally { sqlAce.CloseMySQL(); }
        }
    }
    
    public void SwitchWindow()          //切换登录注册界面
    {
        bool windowState = LoginWindow.activeInHierarchy;
        LoginWindow.SetActive(!windowState);
        RegisterWindow.SetActive(windowState);
        RegisterName.text = "";
        RegisterPW.text = "";
        LoginName.text = "";
        LoginPW.text = "";
    }

    public void LoadGame()
    {
        string[] stats = new string[6];
        try                                         //从数据库中获取人物六维
        {
            sqlAce = new SqlAccess();
            con = SqlAccess.con;
            string sql = string.Format("select STR,DEX,CON,INTE,WIS,CHA from users where name ='{0}'", username);
            Dictionary<int, List<string>> dic = sqlAce.QueryInfo(sql, con);         //封装类中的字典会自动将得到的数据转化为字符串格式
            for (int i = 0; i <= 5; i++)
            {
                stats[i] = dic[0][i];
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString()); return;
        }
        finally { sqlAce.CloseMySQL(); }
        LoginWindow.SetActive(false);
        PlayerWindow.SetActive(true);
        PlayerWindow.GetComponent<Player>().LoadInClient(username, stats);      //切换窗口，调用玩家类方法改变界面中文本
    }

    public void SaveGame(string[] stats)        //存档
    {
        try
        {
            sqlAce = new SqlAccess();
            con = SqlAccess.con;
            string sql = string.Format("update users set STR='{0}',DEX='{1}',CON='{2}',INTE='{3}',WIS='{4}',CHA='{5}' where name='{6}'", stats[0], stats[1], stats[2], stats[3], stats[4], stats[5], username);
            sqlAce.UpdateInfo(sql, con);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());return;
        }
        finally { sqlAce.CloseMySQL(); }
    }

    public void DisconnectGame()    //回到登录界面的同时初始化各项参数
    {
        PlayerWindow.SetActive(false);
        LoginWindow.SetActive(true);
        username = "";
        LoginName.text = "";
        LoginPW.text = "";
    }

    public void ExitGame()      //退出程序
    {
        Application.Quit();
    }

    //简单从网上摘一段SHA1加密代码
    string SHA1Encrypt(string normalTxt)
    {
        var bytes = Encoding.Default.GetBytes(normalTxt);
        var SHA = new SHA1CryptoServiceProvider();
        var encryptbytes = SHA.ComputeHash(bytes);
        return Convert.ToBase64String(encryptbytes);
    }
}
