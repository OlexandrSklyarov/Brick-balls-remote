using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
    public class BricksController : MonoBehaviour 
    {
        class CellData
        {
            public Vector3 [,] cellPositions; //позиции ячеек для блоков
            public float sizeCell; //размер одной ячейки
            public readonly int COUNT_X = 9; //количество ячеек по горизонтали
            public int countY; //количество ячеек по вертикали
            public float startX;
            public float startY;
            public const float HOR_OFFSET = 0.2f;
        }       


        //для данных о игре по мультиплееру
        class MPGame
        {
            public int countCreatedBricks; //колличество созданых кирпичей
        }


        //для хранения данных об локальной игре
        class SimpleGame
        {
            public int maxCreateRandomLine; //максимальное количество дополнительных линий за игру  
            public int sumLiveBricks; //сумма жизней созданых кирпичей
            public readonly int MIN_ITEM_COUNT = 10; //минимум предметов на уровне
        }

    #region Var

        public GameObject VFX_conteiner {get; private set;} //контейнер для визуальных эффектов
        public bool IsFirstLineCreate {get; private set;} //создана ли первая линия
        public int SumLiveBricks {get {return gameSimple.sumLiveBricks;}} //сумма жизней созданых кирпичей

        GameManager gameManager;

        GameObject dangerLine; //линия сигнализирующая о скором проигрыше        

        CellData cellData = new CellData();   
        SimpleGame gameSimple = new SimpleGame();
        MPGame gameMP = new MPGame();

        List<Item> listItems = new List<Item>();        
        DataBrickController data;  
        DataGameActions dataGameActions;  
        

        System.Random r; // рандом (нужен для повторения генерации уровня)


    #endregion


    #region Event

        public event System.Action BricksHitBottomEvent;
        public event System.Action NoBricksEvent;
        public event System.Action LevelCreateEvent;

    #endregion


    #region Init

        public void Init(GameManager _gameManager)
        {
            gameManager = _gameManager;

            data = Resources.Load<DataBrickController>("Data/DataBrickController");  
            dataGameActions = Resources.Load<DataGameActions>("Data/DataGameActions");              

            VFX_conteiner = transform.parent.transform.Find("VFX_Conteiner").gameObject;

            dangerLine = transform.parent.transform.Find("DangerLine").gameObject;
            dangerLine.SetActive(false);

            Subscriptions();
            CreateCells();                              
        }


        void Subscriptions()
        {
            gameManager.ManagerAction.ExecuteActionEvent += GameActionReaction;
            //подписка о покупке continue
            gameManager.StoreManager.BuyGameContinueEvent += DestroyLastThreeLines; 
            AdMobManager.Instance.RewardContinueGameEvent += DestroyLastThreeLines;
        }


        void Unsubscribe()
        {
            gameManager.ManagerAction.ExecuteActionEvent -= GameActionReaction;
            gameManager.StoreManager.BuyGameContinueEvent -= DestroyLastThreeLines;
            AdMobManager.Instance.RewardContinueGameEvent -= DestroyLastThreeLines;
        }
        

    #endregion


    #region Cell

        //создает ячейки и заполняет их в масив
        void CreateCells()
        {
            var border = GameObject.Find("Borders").gameObject;
            var top = border.transform.Find("top").gameObject;
            var bottom = border.transform.Find("bottom").gameObject;
            var right = border.transform.Find("right").gameObject;
            var left = border.transform.Find("left").gameObject;

            //количество в ширину - 9
            cellData.sizeCell = GetSizeCell(right.transform.position, left.transform.position, cellData.COUNT_X);
            cellData.countY = GetCountCallsVertical(top.transform.position, bottom.transform.position, cellData.sizeCell);
            Debug.Log("vertical count :" + cellData.countY);
            
            var centr = GetCentrPoint(top.transform.position.x, bottom.transform.position.x, left.transform.position.y, right.transform.position.y);
                        
            cellData.startX = (centr.x - cellData.COUNT_X * cellData.sizeCell / 2f) + cellData.sizeCell / 2f;
            cellData.startY = (centr.y + cellData.countY * cellData.sizeCell / 2f) - cellData.sizeCell / 2f;        

            cellData.cellPositions = new Vector3[cellData.COUNT_X, cellData.countY]; //создаем массив ячеек

            for (int x = 0; x < cellData.COUNT_X; x++)
            {
                for (int y = 0; y < cellData.countY; y++)
                {
                    var pos = new Vector3(cellData.startX + x * cellData.sizeCell, cellData.startY - y * cellData.sizeCell, 0f);
                    cellData.cellPositions[x, y]  = pos;
                    //Debug.Log("Cell pos: " + pos);
                }
            }
        }


        //возвращает центр поля
        Vector3 GetCentrPoint(float x1, float x2, float y1, float y2)
        {
            var minX = Mathf.Min(x1, x2);
            var maxX = Mathf.Max(x1, x2);
            var x = minX + Mathf.Abs(maxX - minX) / 2f;

            var minY = Mathf.Min(y1, y2);
            var maxY = Mathf.Max(y1, y2);
            var y = minY + Mathf.Abs(maxY - minY) / 2f;

            return new Vector3(x, y, 0f);
        }


        //размер ячейки
        float GetSizeCell(Vector3 pointA, Vector3 pointB, int maxCount)
        {
            float dist = (new Vector2(pointA.x, pointA.y) - new Vector2(pointB.x, pointB.y)).magnitude;
            //Debug.Log("hor dist: " + dist);
            dist -= CellData.HOR_OFFSET; //отступ от краёв
            return dist / maxCount;
        }


        //количество ячеек в высоту (по горизонтали)
        int GetCountCallsVertical(Vector3 pointA, Vector3 pointB, float cellSize)
        {
            float dist = (new Vector2(pointA.x, pointA.y) - new Vector2(pointB.x, pointB.y)).magnitude;
            return (int)(dist / cellSize);
        }

    #endregion


    #region Game mode


        public void BricksPreparetion()
        {
            UpdateStep();
        }
        

        public void UpdateStep()
        {
            switch(gameManager.ModeGame)
            {
                case GameMode.CLASSIC: //классический режим
                    ClassicMode();
                break;
                case GameMode.LEVELS: //режим на прохождение
                    LevelsMode();
                break;
                case GameMode.MULTIPLAYER: //режим мультиплеера
                    MultiplayerMode();
                break;
            }
        }


        void ClassicMode()
        {
            if (IsFirstLineCreate)
            {                
                ShiftDown();  
                AddLineClassic();                                  
            }
            else
            {
                AddLineClassic(); 
                SendCreateLevelMessage(); 
            }                        
        }


        void LevelsMode()
        {
            if (IsFirstLineCreate)
            {                
                ShiftDown();    
                CreateExtraRandomLine();                  
            }                
            else
            {
                gameSimple.maxCreateRandomLine = UnityEngine.Random.Range(2, 5); //случайно назначаем колличество создаваемых линий
                //TestCreate();

                //создаём уровень
                CreateRandomLevel(8, true);
                SendCreateLevelMessage(); 
            }
        }


        void MultiplayerMode()
        {
            if (IsFirstLineCreate)
            {
                //при обновлении игры
                ShiftDownSome();
                CreateExtraItemMultiplayer();
            }
            else
            {
                //при первом запуске
                CreateMultiplayerLevel();
                SendCreateLevelMessage(); 
            }
        }

        
        //сообщение о победе
        void WinMessage()
        {
            NoBricksEvent?.Invoke();
        }
        

        void SendCreateLevelMessage()
        {
            IsFirstLineCreate = true;
            LevelCreateEvent?.Invoke();
        }

    #endregion


    #region Brick create

        void TestCreate()
        {
            for (int y = 0; y < cellData.countY; y++)
            {    
                for (int x = 0; x < cellData.COUNT_X; x++)
                {
                    var go = data.quadBrick;
                    CreateItem(cellData.cellPositions[x,y], go, 10); 
                }
            }
        }

        
        //создает рандомный уровень на высоту строк указаных в парметре
        void CreateRandomLevel(int countRaws, bool isStaticRandom = false)
        {
            if (isStaticRandom)
                r = new System.Random(gameManager.CurrentLevel.Index);
            else
                r = new System.Random(UnityEngine.Random.Range(0, 100));

            //ограничиваем колличество строк до 9
            countRaws = Mathf.Clamp(countRaws, 1, 9);

            //максимальное количество кирпичей на уровне
            var maxBricks = r.Next(20, 26);

            //определяем максимальное количество жизней у блоков (на конкретной сложности)
            var maxLives = GetMaxLives(gameManager.CurrentLevel.Index, maxBricks);

            //случайные количество для кирпичей, которые взрываются            
            int countBlastedItem = r.Next(1, 2);//   

            //случайные количество для бонусного предмета
            int countItemBall = r.Next(2, 5);

            //максимальное количество предметов на уровне                  
            int countQuad = (int)(maxBricks * 0.85f);
            int countHexagon = (int)(maxBricks * 0.15f);
            int countTriangle = (int)(maxBricks * 0.05f);

            int liveSum = maxLives;

            int lightColum = r.Next(0, cellData.COUNT_X + 1);
            int liveSumLiteColum = 50;
            int maxCreatBricks = maxBricks;


            for (int y = 1; y <= countRaws; y++)
            {
                for (int x = 0; x < cellData.COUNT_X; x++)
                {
                    GameObject newItemPrefab;
                    int index;

                    if (countItemBall > 0 && r.Next(0, 100) > 77 
                        && x > 0 && x < cellData.COUNT_X - 1
                        && y <= cellData.countY / 2) //Bonus items 
                    {
                        index = r.Next(0, data.helpBrickPrefabs.Length);
                        newItemPrefab = data.helpBrickPrefabs[index];
                        countItemBall--;
                    }
                    else if (countBlastedItem > 0 && r.Next(0, 100) > 85)  //Blast items
                    {
                        index = r.Next(0, data.blastedBrickPrefabs.Length);
                        newItemPrefab = data.blastedBrickPrefabs[index];
                        countBlastedItem--;
                    }
                    else //Bricks
                    {
                        if (maxCreatBricks <= 0)
                            continue;

                        if (countQuad > 0 && r.Next(0, 100) > 40)
                        {
                            newItemPrefab = data.quadBrick;
                            countQuad--;
                        }
                        else if (countHexagon > 0 && r.Next(0, 100) > 45)
                        {
                            newItemPrefab = data.hexagonBrick;
                            countHexagon--;
                        }
                        else if (countTriangle > 0 && r.Next(0, 100) > 50)
                        {
                            var randIndex = r.Next(0, data.triangleBricks.Length);
                            newItemPrefab = data.triangleBricks[randIndex];
                            countTriangle--;
                        }
                        else
                        {
                            newItemPrefab = data.quadBrick;
                        }

                        maxCreatBricks--;

                    }

                    var randomLives = data.minLives;

                    if (x == lightColum)
                    {
                        int live = r.Next(6, 9);
                        randomLives = (live > 5) ? live : 6;
                        liveSumLiteColum -= live;
                    }
                    else if (liveSum > data.minLives)
                    {
                        int sum = maxLives / maxBricks;
                        int randLive = r.Next((int)(sum * 0.9f), sum + 1);
                        randomLives = randLive;
                        liveSum -= randLive;
                    }


                    CreateItem(cellData.cellPositions[x, y], newItemPrefab, randomLives);

                    x += r.Next(0, 2);
                }
            }
        }                
    

        int GetMaxLives(int curLevelIndex, int maxItems)
        {
            int multiply = 3;     
            
            if (curLevelIndex > 10 && curLevelIndex <= 50)       
            {
                multiply++;
            }    
            else if (curLevelIndex > 50) 
            {
                //Debug.Log("multiply to:" + multiply);
                multiply += Mathf.RoundToInt(curLevelIndex / 50) + 1;
                //Debug.Log("multiply from:" + multiply);
                
            }

            int liveMax = multiply * 10;                        
            liveMax = Mathf.Clamp(liveMax, data.minLives, data.maxLives);

            var sumLive = liveMax * maxItems;
           // Debug.Log("Sum_live: " + sumLive);
            
            return sumLive;
        }


        //добавляет новую линию вверху, случайно
        void AddRandomLine()
        {
            CreateRandomLevel(1);
        }  


        //добавляет одну линию вверху
        void AddLineClassic()
        {                                      
            int maxLive = gameManager.StepCount * 2;
            int number = UnityEngine.Random.Range(0, 101);

            bool isBlastCreate = false;
            bool isExtraBallCreate = false;

            for (int x = 0; x < cellData.COUNT_X; x++)
            {
                int itemNum = 0;

                if (UnityEngine.Random.Range(0 , 11) > 8 && !isExtraBallCreate)
                {
                    itemNum = -1; //extraBall
                    isExtraBallCreate = true;
                }
                else if (UnityEngine.Random.Range(0 , 11) > 6 && !isBlastCreate)
                {
                    itemNum = -2; //blastedBrick
                    isBlastCreate = true;
                }
                else
                {
                    // simply bricks
                    if (number < 50)
                    {
                        itemNum = 0; 
                    }                                                               
                    else if (number < 80)
                    {
                        itemNum = 1;                   
                    }
                    else if (number >= 80)
                    {
                        itemNum = 2;
                    }         
                }                              

                int minLive = (int)(maxLive - maxLive * 0.7f);
                minLive = Mathf.Clamp(minLive, 5, maxLive + 10);
                var randomLives = UnityEngine.Random.Range(minLive, maxLive + 10);
                                
                //создаем блоки со второй линии (индекс 1)
                var y_Line = 1;

                GameObject go;
                if (itemNum > -1)
                {
                    go = (itemNum == 0) ? data.quadBrick : (itemNum == 1)? data.hexagonBrick : 
                        data.triangleBricks[UnityEngine.Random.Range(0, data.triangleBricks.Length)];
                }                    
                else if (itemNum == -1)
                {
                    go = data.helpBrickPrefabs[0];
                }
                else
                {
                    int index = UnityEngine.Random.Range(0, data.blastedBrickPrefabs.Length);
                    go = data.blastedBrickPrefabs[index];
                }

                CreateItem(cellData.cellPositions[x,y_Line], go, randomLives); 

                var n = UnityEngine.Random.Range(1, 3);
                x += UnityEngine.Random.Range(0, n);  //делаем пропус ячейки            
            }
        }


        //создание предмета
        void CreateItem(Vector3 pos, GameObject itemPrefab, int itemLives)
        {           
            var go = Instantiate(itemPrefab, pos, Quaternion.identity) as GameObject;
            go.transform.parent = transform;                  

            //инициализация для кирпичей
            var itemBrick = go.GetComponent<ItemBrick>(); 
            if (itemBrick)         
            {
                itemBrick.Init(this, itemLives);  
                gameMP.countCreatedBricks++; //для мультиплеера   
                gameSimple.sumLiveBricks += itemLives;           
            }

            //инициализация для новых мячиков
            var itemBall = go.GetComponent<ItemBall>();  
            if (itemBall)         
            {
                itemBall.Init(this);               
            }

            AddItem(go.GetComponent<Item>());
        }
        

        //создает дополнительную рандомную линию, если это потребуется
        void CreateExtraRandomLine()
        {
            if (gameSimple.maxCreateRandomLine <= 0) return;
            if (gameManager.StepCount < 2) return;

            if (listItems.Count < gameSimple.MIN_ITEM_COUNT)
            {
                AddRandomLine();
                gameSimple.maxCreateRandomLine--;
            }
        }


        //создание уровня в режиме мультиплеера
        void CreateMultiplayerLevel()
        {
            gameMP.countCreatedBricks = 0;

            var multiData = GPGS.Instance.MultiplayerInfo;
            int maxScore = multiData.maxScore;            
            string map = multiData.map.text; //получаем карту

            GameObject go = null;
            int x = 0;
            int y = 0;
            
            foreach(char c in map)
            {    
                //проверяем какой символ выпал
                switch (c)
                {
                    case '-': //пустая                    
                          
                    break;                    
                    case '*': // дополнительные шарики                    
                        go = data.helpBrickPrefabs[0];
                    break; 
                    case 'q': //куб   
                        go = data.quadBrick;
                    break;
                    case '1': // триугольник -1                    
                        go = data.triangleBricks[0];
                    break;
                    case '2': // триугольник -2                    
                        go = data.triangleBricks[1];
                    break;
                    case '3': // триугольник -3                    
                        go = data.triangleBricks[2];
                    break;
                    case '4': // триугольник -4                    
                        go = data.triangleBricks[3];
                    break;
                    case 'h': // шестиугольник                    
                        go = data.hexagonBrick;
                    break; 
                    case ';': // конец строки                    
                        x = -1;
                        if (y < cellData.countY-1) y++;                        
                    break; 
                    default: //любой другой символ отнимаем от (x) 1
                        x--;
                    break;
                                                                                      
                }                                   
                    
                if (go != null)
                {
                    var livesRand = UnityEngine.Random.Range((int)(maxScore * 0.8f), maxScore);
                    CreateItem(cellData.cellPositions[x, y], go, livesRand);
                    go = null;
                }     

                x++;                                              
            }  
        }
        

        //создаёт дополнительные предметы, если это нужно
        void CreateExtraItemMultiplayer()
        {
            var maxItems = GPGS.Instance.MultiplayerInfo.maxScore;

            //если уже всё кубики созданы - выходим
            if (gameMP.countCreatedBricks >= maxItems)
                return;

            GameObject go = null;

            for (int x = 0; x < cellData.COUNT_X; x++)
            {
                var pos  = cellData.cellPositions[x,1];

                if (gameMP.countCreatedBricks < maxItems && IsCellFree(pos))
                {
                    int p =  UnityEngine.Random.Range(0, 100);

                    if (p < 70)
                        go = data.quadBrick;
                    else if (p >= 70 && p < 90)
                        go = data.hexagonBrick;
                    else
                        go = data.triangleBricks[UnityEngine.Random.Range(0, data.triangleBricks.Length)];

                    CreateItem(pos, go, maxItems);                   
                } 

                if (UnityEngine.Random.Range(0, 2) > 0) x++; //пропускаем ход если (1)
            }            
        }


        //свободна ли ячейка
        bool IsCellFree(Vector3 testPosition)
        {
            foreach(var it in listItems)
            {
                if (it.transform.position == testPosition)
                    return false;
            }

            return true;
        }

    #endregion


    #region Item management    


        //уничтожение последних 3-х линий
        void DestroyLastThreeLines()
        {
            IsBricksHitBottom();

            foreach(var item in listItems)
            {
                if (item is ItemBall) continue;

                //три клетки от нижней границы
                var minPos = cellData.cellPositions[0, cellData.countY-3].y;

                //проверяем, находится ли кубик на уровне, или ниже минимальной позиции
                if (item.transform.position.y <= minPos)
                {
                    item.GetComponent<ItemBrick>().ExternalForceReaction(1f);
                }
            }

            StartCoroutine( CheckingRemainderBricks() );
        }


        //проверка на остаток кирпичей в уровне
        IEnumerator CheckingRemainderBricks()
        {
            yield return new WaitForSeconds(1f);

            //если блоков не осталось - выиграли
            if (!IsBricksExistOnLevel())
                WinMessage();
        }


        //удаляет линии по горизонтали 
        public void DestroyHorizontalLine(float positionY)
        {
            foreach(var item in listItems)
            {
                if (item is ItemBall) continue;

                if (Mathf.Abs(item.transform.position.y - positionY) < 0.1f)
                {
                    item.GetComponent<ItemBrick>().ExternalForceReaction(1f);
                }
            }
        }


        //удаляет линии по веритикали
        public void DestroyVerticalLine(float positionX)
        {
            
            foreach(var item in listItems)
            {
                if (item is ItemBall) continue;

                if (Mathf.Abs(item.transform.position.x - positionX) < 0.1f)
                {
                    item.GetComponent<ItemBrick>().ExternalForceReaction(1f);
                }
            }
        }


        //удаляет линии по горизонтали и веритикали
        public void DestroyCrosslLine(float positionX, float positionY)
        {            
            DestroyVerticalLine(positionX);
            DestroyHorizontalLine(positionY);
        }               


        //сдвигает все блоки вниз на одну ячейку
        void ShiftDown()
        {
            if (listItems.Count > 0)
            {
                for (int i = listItems.Count-1; i >= 0; i--)
                {
                    var item = listItems[i];

                    if (item != null)
                    {
                        item.ShiftDown(cellData.sizeCell); 
                    } 

                    RemoveExtraBallsOverField(item);                                                              
                }

                AudioManager.Instance.Play(StaticPrm.SOUND_SHIFT_BRICKS);
            }
        }


        //удаляетдоп. мячи, которые вышли за границу поля
        void RemoveExtraBallsOverField(Item item)
        {
            if (item is ItemBall ball)
            {
                if (ball.transform.position.y < cellData.cellPositions[0, cellData.countY-3].y)
                {
                    ball.Disappear();
                }
            }
        }


        //сдвиг определённых предметов (под которыме свободно, и они не достигли дна)
        void ShiftDownSome()
        {
            //обновляем состояние о фиксации перед сдвигом
            ItemFixedUpdate();

            foreach (var item in listItems)
            {
                if (item == null) continue;
                
                if (!item.isItemFixed)                
                {
                    item.ShiftDown(cellData.sizeCell); 
                }
            } 

            AudioManager.Instance.Play(StaticPrm.SOUND_SHIFT_BRICKS);
        }


        //обновляет у предметов флаг IsItemFixed
        void ItemFixedUpdate()
        {
            foreach (var item in listItems)
            {
                //если предмет уже внизу, то устанавливаем флан в true и пропускаем итерацию
                if (item.transform.position.y <= cellData.cellPositions[0, cellData.countY-2].y)
                {
                    item.isItemFixed = true;
                    continue;
                }                 

                //проверяем, если под предметом другие закреплённые предметы
                item.isItemFixed = IsFixedBottomItem(item);              
            }
        }


        //проверка, есть ли поднизом закреплённый предмет
        bool IsFixedBottomItem(Item item)
        {
            foreach(var it in listItems)
            {                
                if (it == item) 
                    continue;

                //позиция по у больше (выше), и по х не на одной линии (не совпадают)
                if (it.transform.position.y >= item.transform.position.y || 
                    it.transform.position.x != item.transform.position.x) 
                    continue;

                var curItemPosition = new Vector2(0f, it.transform.position.y);
                var itemPos = new Vector2(0f, item.transform.position.y);                

                //ниже есть предмет
                if (Vector2.Distance(curItemPosition, itemPos) < cellData.sizeCell * 1.5f) 
                {
                    //он на дне
                    if (curItemPosition.y <= cellData.cellPositions[0, cellData.countY-2].y)
                        return true;

                    if (it.isItemFixed)
                        return true; //предмет закреплёт 
                    else
                        return IsFixedBottomItem(it);
                }
            }

            return false; // предмет не закреплен, или ниже свободно
        }


        // проверка: не достиг ли какой-либо кирпич дна
        public bool IsBricksHitBottom()
        {
            if (listItems.Count < 1)
                return false;

            float minPosY = float.MaxValue;

            foreach (var item in listItems)
            {
                if (item == null) 
                    continue;   

                if (item is ItemBall)  
                    continue;     

                //определяем минимальную позицию
                if (item.transform.position.y < minPosY)
                {
                    minPosY = item.transform.position.y;
                }       

                //если позиция по (у) текущего предмета меньше или равна (у) предпоследней линии
                if (item.transform.position.y < cellData.cellPositions[0, cellData.countY-3].y)
                {
                    return true;
                }               
            }  

            //активация линии опасности, если потребуется
            SetActiveDangerLine(minPosY);

            return false;         
        }


        //включает и отключает линию придупреждения игрока
        void SetActiveDangerLine(float itemPosY)
        {
            var minPosY = cellData.cellPositions[0, cellData.countY-4].y;

            if (itemPosY < minPosY)
            {
                if (!dangerLine.activeSelf)
                {                    
                    dangerLine.SetActive(true);   
                    AudioManager.Instance.Play(StaticPrm.SOUND_DANGER_LINE);                    
                }                
            }
            else
            {
                if (dangerLine.activeSelf)
                    dangerLine.SetActive(false);                
            }
        }


        //проверка количества оставшихся блоков
        public bool IsBricksExistOnLevel()
        {
            if (listItems.Count > 0)
            {
                foreach (var item in listItems)
                {
                    if (item is ItemBall) 
                        continue;
                        
                    return true;
                }                
            }                

            return false;
        }


        void AddItem(Item item)
        {
            listItems.Add(item);            
        }


        public void RemoveItem(Item item)
        {
            if (listItems.Contains(item))
            {    
                listItems.Remove(item);
                //Debug.Log("Remove item, count: " + listItems.Count);
            }              
        }


        public void Clear()
        {
            foreach (var item in listItems)
            {
                if (item == null) continue;

                Destroy(item.gameObject);
            }

            listItems.Clear();
            IsFirstLineCreate = false;            
        }
              

    #endregion


    #region Actions 
    
        //обрабатывает события от нажатия пользователем кнопок ACTIONS 
        void GameActionReaction(GameActionType typeAction)
        {
            switch(typeAction)
            {
                case GameActionType.TIME_BOMB:
                    StartCoroutine( TimeBombBlast() );
                break;
                case GameActionType.ICE_FORCE:
                    StartCoroutine( IceForce() );
                break;
            }
        }


        IEnumerator TimeBombBlast()
        {
            //делаем маленькую задержку перед взрывом
            yield return new WaitForSeconds(0.3f);

            //наносим урон кубикам
            foreach(var item in listItems)
            {
                if (item == null)
                    continue;

                if (item is ItemBrick)
                    item.GetComponent<ItemBrick>().ExternalForceReaction(0.25f); //25%
            }

            //создаем анимированый взрыв
            AudioManager.Instance.Play(StaticPrm.SOUND_TIME_BOMB_EXPLOSION);
            float timerExp = dataGameActions.timeAnimExplosion;
            var bombExp = Instantiate(dataGameActions.VFX_ExplosionBomb, transform, false) as GameObject;
            bombExp.transform.parent = VFX_conteiner.transform;
            Destroy(bombExp.gameObject, timerExp);

            yield return new WaitForSeconds(timerExp);

            //если предметов нет, то отсылаем сообщение подписчикам
            if (!IsBricksExistOnLevel())
                WinMessage();
        }


        IEnumerator IceForce()
        {            
            IceForceEffect();
            float minYpos = GetMinYpos();            

            yield return new WaitForSeconds(dataGameActions.iceForceTimer);            

            foreach(var item in listItems)
            {
                if (item is ItemBrick)
                {
                    if (item.transform.position.y <= minYpos)
                    {
                        var pos = item.transform.position;
                        IceBlastEffect(pos);                        
                        item.GetComponent<ItemBrick>().ExternalForceReaction(1f); //100%
                    }                    
                }
            }

            yield return new WaitForSeconds(dataGameActions.iceExplosionTimer);

            //если предметов нет, то отсылаем сообщение подписчикам
            if (!IsBricksExistOnLevel())
                WinMessage();
        }


        void IceForceEffect() 
        {            
            AudioManager.Instance.Play(StaticPrm.SOUND_ICE_FORCE);

            var forceVFX = Instantiate(dataGameActions.VFX_IceForce, transform, false) as GameObject;            
            forceVFX.transform.parent = VFX_conteiner.transform;
            Destroy(forceVFX.gameObject, dataGameActions.iceForceTimer);
        }


        float GetMinYpos()
        {
            float minYpos = float.MaxValue;           
            
            foreach(var item in listItems)
            {
                if (item is ItemBrick)
                {                    
                    if (item.transform.position.y < minYpos)
                        minYpos = item.transform.position.y;                    
                }
            }

            return minYpos;
        }


        void IceBlastEffect(Vector3 pos)
        {            
            AudioManager.Instance.Play(StaticPrm.SOUND_ICE_EXPLOSION);
            var iceBlastVFX = Instantiate(dataGameActions.VFX_IceForceExplosion, pos, Quaternion.identity) as GameObject;            
            iceBlastVFX.transform.parent = VFX_conteiner.transform;
            Destroy(iceBlastVFX.gameObject, dataGameActions.iceExplosionTimer);
        }
    
    #endregion


        void OnDestroy() 
        {
            Unsubscribe();
            Clear();
        }

    }
    
}