using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrakeBricks
{
    [CreateAssetMenu(menuName = "Data/DataMultiplayerGame", fileName = "DataMultiplayerGame")]
    public class DataMultiplayerGame : ScriptableObject
    {
        [Header("Icons")]
        public Sprite[] icons; //спрайты для иконок игроков        

        [Space]
        [Header("Level map info")]
        public NetworkLevelInformation[] multiplayerLevel; //заготовленые уровни для мультиплеера

    }
}