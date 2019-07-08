using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrakeBricks
{
    public class MultiplayerInfo
    {

    #region Var

        public int myScore;
        public int opponentScore;  

        public int maxScore; //максимальное количество кирпичей которое можно разбить
        public TextAsset map; //карта для текущего уровня
        public float timer; //время до окончания игры
        
        
        public Sprite playerIcon; //иконка игрока
        public Sprite opponentIcon; //иконка противника
        
        public string playerName; //имя игрока
        public string opponentName; //имя соперника

        public string opponent_ID; //идентификатор соперника

    #endregion

    }
}