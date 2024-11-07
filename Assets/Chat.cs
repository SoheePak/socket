using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{
    Network network;
    public InputField id;
    public InputField chat;

    List<string> list;
    public Text[] text;
    public Image backUI;

    public GameObject[] player;

    void Start()
    {
        network = GetComponent<Network>();
        list = new List<string>();
    }

    public void BeginServer()
    {
        network.ServerStart(10000);
        player[0].SetActive(true);
        network.name = id.text;
    }

    public void BeginClient()
    {
        network.ClientStart("127.0.0.1", 10000);
        network.name = id.text;
    }

    void Update()
    {
        if (network != null && network.IsConnect())
        {
            byte[] bytes = new byte[1024];
            int length = network.Receive(ref bytes, bytes.Length);
            if (length > 0)
            {
                string str = System.Text.Encoding.UTF8.GetString(bytes);
                AddTalk(str);
                SetAnimation(false);
            }
            UpdateUI(); 
        }
    }

    void SetAnimation(bool bSend)
    {
        int iPlayer;

        if (bSend)
            iPlayer = network.IsServer() ? 0 : 1;
        else
            iPlayer = network.IsServer() ? 1 : 0;


        player[iPlayer].GetComponent<Animator>().SetTrigger("dance");
    }

    void AddTalk(string str)
    {
        while (list.Count >= 5)
        {
            list.RemoveAt(0);
        }

        list.Add(str);
        UpdateTalk();
    }

    public void SendTalk()
    {
        string str = network.name + ": " + chat.text;
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
        network.Send(bytes, bytes.Length);

        AddTalk(str);
        SetAnimation(true);
    }

    void UpdateTalk()
    {
        for (int i = 0; i < list.Count; i++)
        {
            text[i].text = list[i];
        }
    }

    void UpdateUI()
    {
        if (!backUI.IsActive())
        {
            backUI.gameObject.SetActive(true);
            player[0].SetActive(true);
            player[1].SetActive(true);
        }
    }
}
