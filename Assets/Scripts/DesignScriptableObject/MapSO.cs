using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapSODesign")]
public class MapSO : ScriptableObject
{
    public Level[] Levels;
    
    [Serializable]
    public class Level
    {
        public int xSize;
        public int ysize;
        public List<DarkCell> listDarkCell = new List<DarkCell>();
        public List<Sprite> characters = new List<Sprite>();
        public List<Sprite> darkCells = new List<Sprite>();

        [Serializable]
        public class DarkCell
        {
            public int xPos;
            public int yPos;
        }
    }
}
