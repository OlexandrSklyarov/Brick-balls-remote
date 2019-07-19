using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace BrakeBricks
{

	public class GameManager : MonoBehaviour 
	{

	#region Other class

		class RatingLevel
		{
			public int maxRatingScore; //максимум очков для максимального рейтинга
			public int curRatingScore; //текущие очки рейтинга за игру
			public int rating; // балы рейтинга
		}


		class ExtraBall
		{
			public int TempCount {get; set;} //количество за текущую игру
		}


		class MultiplayerGameSetting
		{
			public Dictionary<GameActionType, int> actionItems = new Dictionary<GameActionType, int>();
			public MpGameMode mpGameMode;
		}

	#endregion


	#region Var

		public static GameManager Instance;
		public static Game CurrentGame {get; private set;}		
        public static DataGameConfig Config {get; private set;}

		public GameMode ModeGame {get; private set;} //режим игры	
		public GameState State {get; private set;} //состояние игры (меню, игра)
		public GameLevel CurrentLevel {get { return CurrentGame.Levels[currentLevel]; }} //текущий уровень		
		public MpGameMode MP_GAME_MODE {get { return mp_gameSetting.mpGameMode; }} //режим игры в MP
		public bool IsGameCreate {get; private set;}  // создан ли уровень		
		public int StepCount {get; private set;}//количество шагов за уровень
		public int RewardCount {get; private set;} //количество награды игроку	
		public int ExBallForGame {get {return exBalls.TempCount;}} // колю мячей за текущую игру

		public ManagerUI Manager_UI  {get; private set;}
		public ActionManager ManagerAction {get; private set;}
		public BallController BallController {get; private set;}
		public StoreManager StoreManager {get; private set;} //магазин		

		GPGS gpgs; //сервисы google
		DataGameManager data; // данные для создания игры
		BricksController brickController;			
		
		GameObject world;	
		GameObject level;	
		
		int gameScoreClassicMode; //счет для классического режима
		int currentLevel; //текущий уровень	

		RatingLevel rating = new RatingLevel(); //рейтинг текущего уровня	
		ExtraBall exBalls = new ExtraBall(); //подсчёт доп. мячей
		MultiplayerGameSetting mp_gameSetting = new MultiplayerGameSetting();
		//свойство с количеством GameAction для режима MP
		public Dictionary<GameActionType, int> MP_GameActions {get {return mp_gameSetting.actionItems;} }
					

	#endregion


	#region Event

		public event Action<int> ScoreUpdateEvent; //обновление очков
		public event Action<int> StepUpdateEvent; //обновление шагов
		public event Action MoneyCountUpdateEvent; //обновление количества игровых денег
		public event Action GameWinEvent;
		public event Action GameLoseEvent;
		public event Action StartGameEvent;
		public event Action StopGameEvent;
		public event Action QuitGameEvent;		
		public event Action MultiplayerSetupEvent; //подготовка мультиплеера	
		public event Action SetPlaySoundGameEvent; //событие изменения звука в ире (вкл./ выкл.)	
		public event Action<int, int> LevelRatingChangeEvent; //изменение рейтинга в уровне	
		public event Action AddActionItemEvent; //событие добавления количества Actions (лёд, бомба)
		public event Action LeaderboardAddResultEvent; //добавление нового результата к таблице лидеров
		public event Action<AchievementType> StatisticUpdateEvent; //обновление достижений
		public event Action ActivateRewardEvent; //активация награды игроку
		public event Action ContinueGameEvent; //показ панели о продолжении игры
		public event Action<Sprite> SetNewBallSpriteEvent; // смена спрайта для мяча
		
	#endregion


	#region Init

		void Awake()
		{
			if (Instance == null)
				Instance = this;

			data = Resources.Load<DataGameManager>("Data/DataGameManager");
			Config = Resources.Load<DataGameConfig>("Data/DataGameConfig");						
			
			//получаем базу данных игры	
			GetGameDataBase(); 		

			//сотояние: меню
			SetState(GameState.MENU);			
			
			//режим игры
			SetGameMode(GameMode.LEVELS);	

			InitManagerActions();	
			InitStoreManager();
			InitGPGS(); //сервисы от goggle
			InitWorld();
			InitUI();
			InitManagerInput();	
			InitAdMobManager();		
			
			TimeDefault(); //timescale = 1
			currentLevel = 0;
			SetCurrentLevel(currentLevel);					
		}	


		void Start()
		{
			//отключаем затухание экрана
			DisableScreenSleep();

			//регестрируем очередной запус приложения
			RememberAppStart();			
		}


		//запрещает гаснуть экрану во время игры
		void DisableScreenSleep()
		{
			Screen.sleepTimeout = 0;
		}


		void GetGameDataBase()
		{		
			//создаем новую игру
			CurrentGame = new Game();

			//если не удалось загрузить
			//создаем данные по новой
			if (!SaveManager.Load())
			{
				SetupNewGame();	
			}

		#if UNITY_EDITOR
			if (!Config.isFirstStartGame)
			{
				SetupNewGame();						
			}		
		#endif	

			//DemoGameProtected();											
		}


		//завершает приложение, если превышено количество дней (demo.MaxLaunchDays) 
		//со дня первого запуска.
		void DemoGameProtected()
		{
			var demo = CurrentGame.Demo;
			var lastDate = DateTime.Parse(demo.GameLaunchDate);
			var countDay = (DateTime.Now - lastDate).Days;

			if (countDay > demo.MaxLaunchDays)
			{
				Application.Quit();
			}			
		}


		void SetupNewGame()
		{
			//создаем новую игру
			CurrentGame = new Game();			

			for(int i = 0; i < Config.openLevels; i++)
			{
				CurrentGame.Levels[i].Status = StatusLevel.OPEN;
			}				

			SaveGame();

			//помечаем, что игру уже один раз запускали
			Config.isFirstStartGame = true;	
		}
		

		void InitManagerActions()
		{
			//находим менеджер действий
			ManagerAction = GameObject.FindObjectOfType<ActionManager>();

			//использование GameAction
			ManagerAction.ExecuteActionEvent += ExecuteAction;
			//добавление GameAction
			ManagerAction.AddActionEvent += AddItemsAction;
		}


		void InitStoreManager()
		{
			StoreManager = GameObject.FindObjectOfType<StoreManager>();
			StoreManager.BuyCoinsEvent += AddMoney;
			StoreManager.BuyNewBallEvent += AddNewBall;
			StoreManager.BuyPackageEvent += AddPackege;
			StoreManager.ChangePurchasedBallEvent += ChangeBallItem;			
			StoreManager.BuyGameActionEvent += BuyAction;

			//списуем камни за покупку продолжения игры
			StoreManager.BuyGameContinueEvent += () => 
			{ 
				RemoveMoney(GameManager.Config.costContinueGame); 
			};
		}

		void InitGPGS()
		{
			//находим NetworkManager 
			gpgs = GPGS.Instance;
			gpgs.LoginUpdateEvent += LoadGameCloud; // проверяет сохранения в облаке
			gpgs.AllParticipantsConfirmedEvent += StartMultiplayer;
			gpgs.PlayByInvitationEvent += RegisteringInviteFriends; //регестрируем приглашение в игру
			gpgs.DisconectEvent += DisconectGame;
			gpgs.GameResultEvent += MultiplayerGameCompleted;			
			gpgs.AcceptFromInboxEvent += () => { SetGameMode(GameMode.MULTIPLAYER); }; //событие от принимающего игру 

			MpSetMode(MpGameMode.QUIK_GAME);
		}


		//загружает сохранения из облака
		void LoadGameCloud()
		{
			if (gpgs.IsAuthenticated)
			{
				//считываем данные сохранений
				gpgs.ReadSaveData(GPGS.DEFAULT_SAVE_NAME, (status, data) =>
				{
					if (status == GooglePlayGames.BasicApi.SavedGame.SavedGameRequestStatus.Success && data.Length > 0)
					{
						SaveManager.LoadGameFromBinaryArray(data);
						SaveManager.Save();
					}
				});
			}
		}
		

		//находим контейнер для уровня
		void InitWorld()
		{
			world = GameObject.Find("[WORLD]").gameObject;
		}


		void InitUI()
		{
			Manager_UI = GameObject.Find("ManagerUI").GetComponent<ManagerUI>();
			Manager_UI.Init(); //инициализируем UI компоненты			
		}	


		void InitManagerInput()
		{
			var im = InputManager.Instance;			

			im.PressButtonGameModeEvent += StartNewGame;//событие выбора режима
			//старт в MP с приглашением (от игрока который создал приглашение поиграть)
			im.FriendsPanel_PressButtonInviteFriendsEvent += MPstartNewGameWithInvitation; 
			im.PressButtonMainMenu += MainMenu; //событие нажатия кнопки меню
			im.PressButtonChangeLevel += ChangeLevel; //выбор уровня из меню
			im.PressButtonPause += Pause; //пауза в игре
			im.PressButtonRestartLevel += Restart; //событие перезапуска текущего уровня
			im.PressButtonNextLevel += NextLevel; //событие перхода на следующий уровень	
			im.PressButtonQuitGame += QuitGame; //собитые нажатия на кнопку выход из игры
			im.PressButtonSound += SetPlaySound; //переключает звук: вкл/выкл
			im.ClickRewardPanelEvent += AddMoney;
			im.ExLifePanel_PressButtonCloseEvent += Lose; //отказ продолжить игру и проигрыш
		}

		
		void InitAdMobManager()
		{
			var ad = AdMobManager.Instance;
			ad.RewardToWatchEvent += AddMoney; //награда за видеорекламу
			ad.RewardToActionEvent += AddItemsAction; //добавить GameActions за просмотр рекламы
			ad.RewardBallEvent += ChangeBallItem; //устанавливает спрайт мячика который игрок получил в награду за просмотр рекламы
		}
		

	#endregion


	#region Level settings

		//создает игровой мир
		void CreateLevel()
		{		
			ClearLevel();	
			level = Instantiate(data.levelPrefab) as GameObject;
			level.transform.parent = world.transform;
			InitBricksController();			
			InitBallController();

			InputManager.Instance.Init(BallController);		
			
			//событие создания уровня
			brickController.LevelCreateEvent += CanPlay; 
			//событие об отсутсвии блоков на уровне
			brickController.NoBricksEvent += Win; 
			//подписываемся на события от ballController (мячи на стартовой позиции)
			BallController.BallsToStartPositionEvent += UpdateGame;				
		}


		//чистим уровень
		void ClearLevel()
		{
			foreach (Transform child in world.transform)
			{
				if (child.name == "Level(Clone)" || child.name == "BallController(Clone)")
					Destroy(child.gameObject);
			}
		}


		//удаляет уровень
		void DestroyLevel()
		{
			if (level != null)
			{
				Destroy(level.gameObject);
				level = null;
				Debug.Log("Destroy current level");
			}

			if (BallController != null)
				Destroy(BallController.gameObject);
		}


		void InitBricksController()
		{
			brickController = level.transform.Find("BrickConteiner").GetComponent<BricksController>();
			brickController.Init(this);

			SetLevelBackgraund();
		}


		//задаем уровню фон в зависимости от режима игры
		void SetLevelBackgraund()
		{
			var renderer = level.transform.Find("Backgraund").GetComponent<SpriteRenderer>();
			
			switch(ModeGame)
			{
				case GameMode.LEVELS:
					renderer.sprite = data.levelsMode_backgraund;
				break;
				case GameMode.CLASSIC:
					renderer.sprite = data.classicMode_backgraund;
				break;
				case GameMode.MULTIPLAYER:
					//spr = data.classicMode_backgraund;
				break;					
			}
		}


		void InitBallController()
		{
			var goBallController = Instantiate(data.ballControllerPrefab) as GameObject;
			goBallController.transform.parent = world.transform;
			BallController = goBallController.GetComponent<BallController>();

			var sprites = Resources.Load<DataShopItem>("Data/DataShopItem").itemBalls;

			var sprite = GetBallSprite(CurrentGame.CurrentBallSpriteIndex);
			BallController.Init(sprite);
		}


		Sprite GetBallSprite(int spriteIndex)
		{
			var itemBalls = Resources.Load<DataShopItem>("Data/DataShopItem").itemBalls;	
			return itemBalls[spriteIndex].iconItem;
		}

		
		//выбор конкретного уровня из базы
		void ChangeLevel(int indexLevel)
		{			
			SetCurrentLevel(indexLevel-1);				
			StartNewGame(GameMode.LEVELS);			
		}


		//включает и выключает звук в игре
		void SetPlaySound()
		{
			//меняет переменную на обратное значение (вкл./выкл.)
			CurrentGame.IsSoundPlay = !CurrentGame.IsSoundPlay;
			//событие изменение настроек звука
			SetPlaySoundGameEvent?.Invoke(); 
		}

	#endregion


	#region Store

		//задаёт спрайт текущего игровоо мяча
		void ChangeBallItem(int spriteIndex)
		{
			CurrentGame.CurrentBallSpriteIndex = spriteIndex;
			var ballSprite = GetBallSprite(CurrentGame.CurrentBallSpriteIndex);	
			AddPurchasedBalls(spriteIndex);				
			SaveGame();
			SetNewBallSpriteEvent?.Invoke(ballSprite);
		}


		//добавление нового мяча
		void AddNewBall(int spriteIndex, int cost)
		{			
			AddPurchasedBalls(spriteIndex);			
			ChangeBallItem(spriteIndex);
			RemoveMoney(cost);// отнимаем сумму за покупку
		}


		//добавляет индекс мяча к списку купленых игроком
		void AddPurchasedBalls(int indexBall)
		{
			if (!CurrentGame.BoughtBalls.Contains(indexBall))
				CurrentGame.BoughtBalls.Add(indexBall);
		}


		//добавление денег
		void AddMoney(int coins)
		{
			CurrentGame.MoneyCount += coins;
			MoneyCountUpdateEvent?.Invoke(); //событие обновления игровых денег
			SaveGame(); 
			Debug.Log("[GM: add money]");
		}


		//списание денег
		void RemoveMoney(int coins)
		{
			CurrentGame.MoneyCount -= coins;
			MoneyCountUpdateEvent?.Invoke(); //событие обновления игровых денег
			SaveGame(); 
			Debug.Log("[GM: remove money]");
		}
		

		//добавление пакета 
		void AddPackege(int iceForceItems, int timeBombItems, int coins)
		{			
			//Actions
			AddItemsAction(GameActionType.ICE_FORCE, iceForceItems);
			AddItemsAction(GameActionType.TIME_BOMB, timeBombItems);		

			//Money
			AddMoney(coins);
				
			SaveGame();
		}


		//добавляет спец возможности игроку
		void AddItemsAction(GameActionType type, int count)
		{
			//добавляем отдельно для режима MP
			if (ModeGame == GameMode.MULTIPLAYER)
			{				
				if (mp_gameSetting.actionItems.ContainsKey(type))
				{
					mp_gameSetting.actionItems[type] += count;			
				}
				else
				{
					mp_gameSetting.actionItems.Add(type, count);
				}	
			}			
			else //для остальных режимов (Classic, Levels)
			{				
				if (CurrentGame.actionItems.ContainsKey(type))
				{
					CurrentGame.actionItems[type] += count;				
				}
				else
				{
					CurrentGame.actionItems.Add(type, count);
				}
			}

			AddActionItemEvent?.Invoke();				
		}


		//покупка GameActions за камни
		void BuyAction(GameActionType type, int count, int cost)
		{
			RemoveMoney(cost);	// отнимаем сумму за покупку
			AddItemsAction(type, count);
		}
		

	#endregion


	#region Multiplayer		

		//стартануть игру с окном приглашения
		void MPstartNewGameWithInvitation()
		{
			MpSetMode(MpGameMode.IVITATIONS_GAME);
			StartNewGame(GameMode.MULTIPLAYER);
		}


		//отсылает событие о настройке мультиплеера
		void MultiplayerSetup()
		{
			// Проверьте, не может ли устройство подключиться к Интернету
        	if ( Application.internetReachability == NetworkReachability.NotReachable )
			{
				ManagerUI.ShowMsg("No internet connection ...");
				return;
			}
				

			mp_gameSetting.actionItems = new Dictionary<GameActionType, int>();
			MultiplayerSetupEvent?.Invoke();
		}


		//переключает режим игры в MP (быстрая игра, игра с приглашением)
		void MpSetMode(MpGameMode mode)
		{
			mp_gameSetting.mpGameMode = mode;
		}

		
		//запуск игры с мультиплеером
		void StartMultiplayer()
		{	
			StartCoroutine( WaitStartMultiplayer() );											
		}

		
		IEnumerator WaitStartMultiplayer()
		{
			yield return new WaitForSeconds(3.5f);

			SetState(GameState.GAME);	
			CreateLevel();
			StartBricksPreparation(); //подготовка кирпичей	
		
			StartGameEvent?.Invoke(); //событие старта иры

			UpdateScoreOnStart();			
		}
		

		//обрабатывает результат после завершения мультиплеера
		void MultiplayerGameCompleted(MultiplayerResult gameResult)
		{
			CloseGame();
			SetState(GameState.MENU);
			MpSetMode(MpGameMode.QUIK_GAME);

			if (gameResult == MultiplayerResult.WIN)
			{
				CurrentGame.StatisticGame.NumWinsMP++;
				LeaderboardAddResultEvent?.Invoke(); //событие добавдения результата
				CheckingWinsMP(); //проверяем сколько раз выиграли
			}

			CheckingRunMultiplayerMode(); //регестирируем очередной запуск игры в MP
		}


		void DisconectGame()
		{
			MainMenu();			
		}

	#endregion


	#region  Game update

		void StartNewGame(GameMode _gameMode)
		{

			ExtraBallNumReset(); //сбрасываем значения счётчика мячей
			TimeDefault();
			SetGameMode(_gameMode);	
			
			switch(ModeGame)
			{
				case GameMode.MULTIPLAYER:			
					//подготавливаем игру по мультиплееру		
					MultiplayerSetup(); 
				break;

				case GameMode.CLASSIC:
				case GameMode.LEVELS:
					SetState(GameState.GAME);
					
					//открываем уровень
					if (ModeGame == GameMode.LEVELS) OpenCurrentLevel();

					CreateLevel();
					StartBricksPreparation(); //подготовка кирпичей	

					RaitingLevelReset(); //сбрасываем рейтинг текущей игры
			
					StartGameEvent?.Invoke(); //событие старта игры

					UpdateScoreOnStart();
					StepCount = 0; //обнуляем шаги в игре
					StepUpdate();	

					//проверяем количество запусков игры в КЛАССИЧЕСКОМ режиме
					if (ModeGame == GameMode.CLASSIC) CheckingRunClassicMode();						
				break;
			}	
		}		


		//обновляет счет при старте игры
		void UpdateScoreOnStart()
		{
			int tempScore = 0;

			switch(ModeGame)
			{
				case GameMode.CLASSIC:
					gameScoreClassicMode = 0;
					tempScore = gameScoreClassicMode;
				break;

				case GameMode.LEVELS:
					tempScore = CurrentGame.GameScore;					
					LevelRatingChangeEvent?.Invoke(0, 0); //событие изменения рейтинга
				break;

				case GameMode.MULTIPLAYER:
					Debug.LogWarning("UpdateScoreOnStart() - MULTIPLAYER - NO IMPLEMENT!!!");
				break;

				default:
					Debug.LogWarning("Другой режим игры, счет : " + tempScore);
				break;
			}

			//событие обновления счёта
			ScoreUpdateEvent?.Invoke(tempScore);
		}


		void UpdateGame()
		{			
			switch(State)
            {
                case GameState.GAME:                    
					NextStep();
				break;
                case GameState.MENU:
                    Debug.Log("GAME_MANGER state: " + State);
                break;
            }						
		}


		//обновляет шаг в игре
		void NextStep()
		{
			StepUpdate();			

			switch(ModeGame)
			{
				case GameMode.LEVELS:
					//есть ли блоки на уровне
					if (brickController.IsBricksExistOnLevel())
					{
						//есть ли место под последним рядом блоков
						if (!brickController.IsBricksHitBottom())
						{
							brickController.UpdateStep();												
						}	
						else
							ContinueGame(); //Lose(); //проиграл				
					}
					else
						Win();	//победа!!!	
				break;

				case GameMode.CLASSIC:
					if (!brickController.IsBricksHitBottom())
					{
						brickController.UpdateStep();											
					}	
					else
						ContinueGame(); //Lose(); //проиграл	
				break;

				case GameMode.MULTIPLAYER:					
					if (brickController.IsBricksExistOnLevel())
					{
						brickController.UpdateStep();
					}	
				break;
			}	
		}


		//обновляет шаги в игре		
		void StepUpdate()
		{
			StepCount++;
			StepUpdateEvent?.Invoke(StepCount);
		}


		//обновляет счет в игре (_timeBal нужен для очков рейтинга)
		public void AddScore(int _score)
		{			
			switch(ModeGame)
			{
				//Classic
				case GameMode.CLASSIC:
					gameScoreClassicMode += _score;

					//обновляем HI-score в классическом режиме
					if (CurrentGame.HiscoreClassicMode < gameScoreClassicMode)
						CurrentGame.HiscoreClassicMode = gameScoreClassicMode;

					ScoreUpdateEvent?.Invoke(gameScoreClassicMode);
				break;

				//Levels
				case GameMode.LEVELS:
					CurrentGame.GameScore += _score;
					ScoreUpdateEvent?.Invoke(CurrentGame.GameScore);
					RatingLevelAdd(_score);
				break;

				//Multiplayer
				case GameMode.MULTIPLAYER:			
					int fixedScore = 1;	//фиксировано добавляем одно очко	
					ScoreUpdateEvent?.Invoke(fixedScore); //событие обновлениея счёта
				break;
			}	
		}


		void Win()
		{		
			if (ModeGame == GameMode.MULTIPLAYER)
				return;

			CloseGame();

			SetState(GameState.MENU);
			SetStatusCurrentLevel(StatusLevel.COMPLETED);
			OpenNextLevel();

			CheckingMaxRating(); //проверяем текущий уровень на рейтинг прохождения
			SaveRatingCurrentLevel(); //сохраняем рейтинг пройденому уровню
			
			GameWinEvent?.Invoke(); //событие выиграша

			CheckingStepsTaken(); // проверяем кол. шагов
			CheckingCompletetedLevels(); //проверяем количество пройденых уровней
			
			AudioManager.Instance.Play(StaticPrm.SOUND_WIN_GAME);
		}		


		//врзможность продолжить игру
		void ContinueGame()
		{
			AudioManager.Instance.Play(StaticPrm.SOUND_EXTRA_PANEL_OPEN);
			ContinueGameEvent?.Invoke();
		}


		void Lose()
		{
			CloseGame();
			
			var state = CurrentGame.Levels[currentLevel].Status;

			if (state != StatusLevel.CLOSE)
				SetStatusCurrentLevel(StatusLevel.OPEN);

			SetState(GameState.MENU);		
			
			GameLoseEvent?.Invoke(); //событие проигрыша	

			AudioManager.Instance.Play(StaticPrm.SOUND_LOSS_GAME);
		}


		//завершает игру и сохраняет данные
		void CloseGame()
		{
			StopGameEvent?.Invoke(); //событие стоп игра

			SaveGame();
			DestroyLevel();
			IsGameCreate = false;
		}


		//разрешает игру для ирока
		void CanPlay()
		{						
			StartCoroutine( CanPlayCorutine() );
		}


		IEnumerator CanPlayCorutine()
		{			
			yield return new WaitForSeconds(Config.timeDeleyBallCreate);			
			StartBallCreate();
			IsGameCreate = true;	
		}


		//подготовка мячей при запуске игры
		void StartBallCreate()
		{
			AudioManager.Instance.Play(StaticPrm.SOUND_START_GAME);
			BallController.BallPreparation();			
		}


		//подготовка кирпичей при запуске игры
		void StartBricksPreparation()
		{
			brickController.BricksPreparetion(); 
		}


		void NextLevel()
		{		
			SetCurrentLevel(currentLevel+1); //передаём индекс следующего уровня	
			StartNewGame(ModeGame);
		}


		void Restart()
		{			
			StopGameEvent?.Invoke(); //событие стоп игра

			SetState(GameState.MENU);		
			DestroyLevel();					
			StartNewGame(ModeGame);			
		}		


		void MainMenu()
		{
			TimeDefault();
			SetState(GameState.MENU);
			CloseGame();					
			SetGameMode(GameMode.LEVELS); //режим по умолчанию									
		}


		//пауза в игре
		void Pause(bool isPause)
		{
			if (isPause)
			{
				SetState(GameState.MENU);	
				TimeStop();
			}
			else
			{
				SetState(GameState.GAME);	
				TimeDefault();
			}
		}


		//остановить время
		void TimeStop()
		{
			Time.timeScale = 0;
		}


		//время по умолчанию
		void TimeDefault()
		{
			Time.timeScale = 1;
		}


		//выходим из игры
		void QuitGame()
		{
			SaveGame();
			SaveCloudGame();
			if (QuitGameEvent != null) QuitGameEvent();
			Application.Quit();
		}


		void SaveGame()
		{							
			SaveManager.Save();					
		}


		//сохранение в облаке
		void SaveCloudGame()
		{
			if (gpgs.IsAuthenticated)
			{
				gpgs.WriteSaveData(SaveManager.GetCurrentGameToByteArray());
			}
		}

		
		void OpenCurrentLevel()
		{
			if (CurrentLevel.Status == StatusLevel.CLOSE)
				SetStatusCurrentLevel(StatusLevel.OPEN);
		}


		//задает уровень в игре
		void SetCurrentLevel(int index)
		{
			//если индекс больше максимально уровня в игре
			if (index > CurrentGame.Levels.Length-1)
			{
				index = 0;
			}				

			currentLevel = index;			
		}	


		//меняет статус уровна (открыт, пройден...)
		void SetStatusCurrentLevel(StatusLevel _status)
		{			
			CurrentGame.Levels[currentLevel].Status = _status;			
		}	


		//открывает следующий уровень
		void OpenNextLevel()
		{
			if (currentLevel < Config.countLevels - 1)
			{
				CurrentGame.Levels[currentLevel + 1].Status = StatusLevel.OPEN;
			}
		}


		//задает режим игры
		void SetGameMode(GameMode _mode)
		{
			ModeGame = _mode;			
		}


		//задает состояние игрового процесса
		void SetState(GameState _state)
		{
			State = _state;
		}


		//награждает игрока
		void RewardPlayer(RewardType type)
		{
			//отображаем количество награды в зависимости от
			switch (type)
            {
                case RewardType.EVERYDAY:
                    RewardCount = GameManager.Config.everydayReward;
                break;
               	case RewardType.MAX_SCALE_REWARD:
                    RewardCount = GameManager.Config.maxScaleReward;
                break;
            }

			ActivateRewardEvent?.Invoke(); //активация награды игроку
			AudioManager.Instance.Play(StaticPrm.SOUND_REWARD_PLAYER);
		}

		
		//использование GameActions
		void ExecuteAction(GameActionType type)
		{
			if (ModeGame == GameMode.MULTIPLAYER)
			{
				mp_gameSetting.actionItems[type]--;
			}
			else
			{
				CurrentGame.actionItems[type]--;
			}
			
			RegisteringUseForce(type);
		}

		
		//вызывается при сворачивании игры
		void OnApplicationPause()
		{
			SaveGame();
		}
		

	#endregion


	#region Rating level

		//сброс рейтинга
		void RaitingLevelReset()
		{
			//75% от общей суммы жизней кирпичей
			rating.maxRatingScore = (int)(brickController.SumLiveBricks * 0.75f);
			rating.curRatingScore = 0;
			rating.rating = 1;

			if (State != GameState.MENU)
			{
				//событие изменения рейтинга
				LevelRatingChangeEvent?.Invoke(rating.curRatingScore, rating.maxRatingScore);		
			}
		}


		//установка рейтинга
		void RatingLevelAdd(int score)
		{
			rating.curRatingScore += score / StepCount;

			if (rating.rating < 2 && rating.curRatingScore > rating.maxRatingScore * 0.7f)
			{
				rating.rating++;
			}
			else if (rating.rating < 3 && rating.curRatingScore > rating.maxRatingScore)
			{
				rating.rating++;
			}	

			if (State != GameState.MENU)
			{
				//событие изменения рейтинга
				LevelRatingChangeEvent?.Invoke(rating.curRatingScore, rating.maxRatingScore);		
			}
		}
			

		//сохраняет рейтинг для текущего уровня
		void SaveRatingCurrentLevel()
		{
			//разница между текущим рейтингом за игру и рейтинге уровня
			var sum  = rating.rating - CurrentGame.Levels[currentLevel].Rating;
			if (sum > 0)
			{
				
				CurrentGame.RatingCount += sum; // общий рейтинг
				CurrentGame.RewardScale += sum; // шкала вознаграждения

				AddMoney(sum); //добавляем камни игроку в количесте набраного рейтинга

				//если заполнили шкалу награды - награждаем игрока
				if (CurrentGame.RewardScale > Config.maxPointReward)
				{					
					RewardPlayer(RewardType.MAX_SCALE_REWARD);
					CurrentGame.RewardScale = 0;
				}
			}

			CurrentGame.Levels[currentLevel].Rating = rating.rating; //запись для уровня						 
		}

	
	#endregion
	

	#region Statistic

		//проверка совершённых шагов
		void CheckingStepsTaken()
		{
			//добавляем счётчик
			if (StepCount == 1)
				CurrentGame.StatisticGame.VictoryOneMove++;
			
			StatisticUpdateEvent?.Invoke(AchievementType.VICTORY_ONE_MOVE);			
		}


		//проверка достижения выиграных игр в MP
		void CheckingWinsMP()
		{
			StatisticUpdateEvent?.Invoke(AchievementType.WINS_MP);			
		}


		//проверка на количество пройденых уровней
		void CheckingCompletetedLevels()
		{
			//если уровень раньше не был пройден
			if (CurrentGame.Levels[currentLevel].Status != StatusLevel.COMPLETED)
				CurrentGame.StatisticGame.NumCompletedLevels++;

			StatisticUpdateEvent?.Invoke(AchievementType.COMPLITED_LEVELS);		
		}


		//Проверка пройден ли новый уровень с максимальным рейтингом
		void CheckingMaxRating()
		{
			//если уровень раньше не достигал максимального рейтинга его прошли на отлично
			if (CurrentGame.Levels[currentLevel].Rating < 3 && rating.rating == 3)
				CurrentGame.StatisticGame.MaxRatingCount++;

			StatisticUpdateEvent?.Invoke(AchievementType.MAX_RATING);				
		}


		//проверяем количество запусков в режиме мультиплеера
		void CheckingRunMultiplayerMode()
		{
			CurrentGame.StatisticGame.NumMultiplayerLaunches++;

			StatisticUpdateEvent?.Invoke(AchievementType.RUN_MP);
		}


		//проверяем количество запусков в классическом режиме
		void CheckingRunClassicMode()
		{
			CurrentGame.StatisticGame.NumClassicLaunches++;

			StatisticUpdateEvent?.Invoke(AchievementType.RUN_CLASSIC);			
		}


		//Проверяем игровую серию (в днях)
		void CheckingPlayingSeries()
		{
			StatisticUpdateEvent?.Invoke(AchievementType.PLAYING_DAY_SERIES);					
		}


		//записывает данные старта приложения
		void RememberAppStart()
		{
			CurrentGame.StatisticGame.CountLaunchApp++; //добавляем счётчик запусков приложения
			
			var lastDate = DateTime.Parse(CurrentGame.DateLastVisit);
			var diff = DateTime.Now - lastDate;
			
			//если разница в один день - увелициваем серию
			if (diff.Days == 1)
			{
				CurrentGame.StatisticGame.DailyGameSeries++;				
			}				
			else
			{
				//обнуляем счётчик серии дней
				CurrentGame.StatisticGame.DailyGameSeries = 0;
			}			

			//награда за новый запуск (если прошло больше суток)
			if (diff.Days  >= 1)
			{
				RewardPlayer(RewardType.EVERYDAY);
			}

			//проверка на достижение
			CheckingPlayingSeries();

			//записываем текущую дату
			CurrentGame.DateLastVisit = DateTime.Now.ToString();	
			SaveGame();
			
			
			//сообщение об количестве дней которое игрок отсутствовал
			ManagerUI.ShowMsg(string.Format("You were absent: [{0}] days [{1}] h [{2}] min", 
											diff.Days , diff.Hours, diff.Minutes));
					
		}
		

		//делает подсчёт использования сил
		void RegisteringUseForce(GameActionType typeForce)
		{
			switch(typeForce)
			{
				case GameActionType.ICE_FORCE: //Ice
					CurrentGame.StatisticGame.NumUsedIcePower++;
					StatisticUpdateEvent?.Invoke(AchievementType.USING_ICE_FORCE);
				break;
				case GameActionType.TIME_BOMB: //Bomb
					CurrentGame.StatisticGame.NumUsedBombPower++;
					StatisticUpdateEvent?.Invoke(AchievementType.USING_BOMB_FORCE);
				break;
			}				
		}		


		//делаем подсчёт приглашённых друзей поиграть в MP
		void RegisteringInviteFriends()
		{
			CurrentGame.StatisticGame.NumInvitedFriends++;
			StatisticUpdateEvent?.Invoke(AchievementType.INVITE_FRIENDS);
		}

	#endregion


	#region Statistic extra balls

		// сбрасывает всё подсчёты доп. мячей за игру
		void ExtraBallNumReset()
		{
			exBalls.TempCount = 0;
		}


		//прибавляет к счётчику собраных мячей
		public void ExtraBallAdd()
		{
			//записуем количество мячей за текущую игру
			exBalls.TempCount++;
			StatisticUpdateEvent?.Invoke(AchievementType.COLLECT_BALL_ONE_GAME);

			//записуем в общее количество собраных мячей
			CurrentGame.StatisticGame.NumExtraBalls++;
			StatisticUpdateEvent?.Invoke(AchievementType.COLLECT_BALL_ALL_TIME);			
		}	

	#endregion

	}

	public enum GameState
	{
		MENU, GAME
	}


	public enum GameMode
    {
        CLASSIC, LEVELS, MULTIPLAYER
    }


	public enum RewardType
	{
		EVERYDAY, MAX_SCALE_REWARD
	}
	
}