using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public List<GameObject> agents;
    public int matchNum; // tracks which match is being performed.
    public int maxMatches = 10; // highest number of matches to complete

    public List<GameObject> TextColumns;

    public Text[] playerNames;
    public List<float> scores; // scoreboard - updated with scores from each player
    public List<Text> scoresBoard; // all text entries in scoreboard

    public int updateInterval = 5;
    int count = 0;

    // Calls "Done()" for each agent
    public void ResetScenario(){
        if(matchNum < maxMatches){
            for(int i = 0; i < agents.Count; i++){
                agents[i].GetComponent<VisualAgent_FPS>().EndEpisode();
            }
            matchNum++;
        } else {
            for(int i = 0; i < agents.Count; i++){
                Destroy(agents[i]);
            }
        }
    }

    void Awake(){
        for(int i = 0; i < TextColumns.Count; i++){
            var entries = TextColumns[i].GetComponentsInChildren<Text>(true);
            foreach(Text entry in entries){
                if(entry.name != "Header"){
                    scoresBoard.Add(entry);
                }
            }
        }
    }
    void Start(){
        for(int i = 0; i < agents.Count; i++){
            playerNames[i].text = agents[i].gameObject.name;
        }
        matchNum = 1;
    }

    public void updateScore(string name, string updateType){
            
        for(int i = 0; i < agents.Count; i++){
            if(playerNames[i].text == name){
                if(updateType == "kill"){
                    // increment kill count -- (matchNum-1)*agents.Count + i = # kills index to update
                    var idx = (matchNum-1)*agents.Count + i;
                    scores[idx] += 1;
                    scoresBoard[idx].text = Mathf.RoundToInt(scores[idx]).ToString();

                    // increment total kills
                    var totalKillsIdx = 10*agents.Count + i;
                    scores[totalKillsIdx] += 1;
                    scoresBoard[totalKillsIdx].text = Mathf.RoundToInt(scores[totalKillsIdx]).ToString();

                    // update K/D ratio
                    var totalDeathIdx = 13*agents.Count + i;
                    if(scores[totalDeathIdx] != 0){ // have to be careful here, could div/0
                        var oaKDIdx = 14*agents.Count + i; 
                        scores[oaKDIdx] = scores[totalKillsIdx] / scores[totalDeathIdx];
                        scoresBoard[oaKDIdx].text = (Mathf.Round(scores[oaKDIdx]*100)/100).ToString();
                    }

                } else if(updateType == "health"){
                    // increment health count
                    var totalHealthIdx = 11*agents.Count + i;
                    scores[totalHealthIdx] += 1;
                    scoresBoard[totalHealthIdx].text = Mathf.Round(scores[totalHealthIdx]).ToString();;

                } else if(updateType == "ammo"){
                    // increment ammo pickup count
                    var totalAmmoIdx = 12*agents.Count + i;
                    scores[totalAmmoIdx] += 1;
                    scoresBoard[totalAmmoIdx].text = Mathf.RoundToInt(scores[totalAmmoIdx]).ToString();

                } else if(updateType == "death"){
                    // increment death count
                    var totalDeathIdx = 13*agents.Count + i;
                    scores[totalDeathIdx] += 1;
                    scoresBoard[totalDeathIdx].text = Mathf.RoundToInt(scores[totalDeathIdx]).ToString();

                    // update K/D ratio
                    var totalKillsIdx = 10*agents.Count + i;
                    var oaKDIdx = 14*agents.Count + i; 
                    scores[oaKDIdx] = scores[totalKillsIdx] / scores[totalDeathIdx];
                    scoresBoard[oaKDIdx].text =(Mathf.Round(scores[oaKDIdx]*100)/100).ToString();
                } else {
                    Debug.Log("What are you trying to update? Something is wrong here...");
                }
                
            }
        }
    }
}
