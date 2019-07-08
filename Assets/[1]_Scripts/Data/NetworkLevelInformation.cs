using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
    [System.Serializable]
    public class NetworkLevelInformation
    {
        public TextAsset map; //карта уровня в тестовом формате
        public int maxScore; // максимальное количество очков на уровне
        public float time; //врямя на уровень
        
    }
}