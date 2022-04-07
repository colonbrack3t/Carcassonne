using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ChangePlayersButtonScript : MonoBehaviour
{
    public Text text;
    public GameScript gameScript;
    public Button btnL, btnR, confirm;
    // Start is called before the first frame update
 

    // Update is called once per frame
    public void RightButtonClick(){
        gameScript.numOfPlayers++;
        if (gameScript.numOfPlayers > 6)
            gameScript.numOfPlayers = 6;
		text.text = "Number of \nPlayers: " + gameScript.numOfPlayers;
	}

    public void LeftButtonClick(){
        gameScript.numOfPlayers--;
        if (gameScript.numOfPlayers < 2)
            gameScript.numOfPlayers = 2;
		text.text = "Number of \nPlayers: " + gameScript.numOfPlayers;
    }

    public void ConfirmButtonClick(){
        gameScript.Start_Game();

    }
}
