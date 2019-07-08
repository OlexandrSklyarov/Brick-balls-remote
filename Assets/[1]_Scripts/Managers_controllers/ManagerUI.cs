using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace BrakeBricks
{
	public class ManagerUI : MonoBehaviour 
	{	

	#region Var

		public static ManagerUI Instance {get; private set;}


		//значение слайдера прицеливания (в игровом интерфейсе)
		public static float SliderControllValue 
		{
			get {return controlSlider.value;} 
			set{controlSlider.value = value;}
		}

		//главное меню игры с выбором уровней
		GameObject maimMenu; 
		GameObject videoAdButton; //кнопка просмотра видеорекламы
		GameObject videoAdButtonStub; //заглушка кнопки (когда реклама не загружена)


		//панель выиграша
		GameObject victoryPanel; 
		GameObject victoryPanelRewardAd; 
		GameObject victoryPanelRewardAdStub; 


		//панель проигрыша
		GameObject losePanel; 


		//игровой интерфейс при запущенном уровне
		GameObject gameInterface; 
		GameObject bottomPanel; //нижняя панель 1
		GameObject bottomPanel_2; //нижняя панель 2	
		GameObject actionButton_1_Push, actionButton_1_Buy, actionButton_1_ShowAd; //#1
		GameObject actionButton_2_Push, actionButton_2_Buy, actionButton_2_ShowAd; //#2
		GameObject shopButtonGameInterface; //магазин на панели игрового интерфейса
		GameObject infoPanel; //панель информации в игровом интерфейсе
		GameObject scorePanel; //панель с очками игрока		
		static Slider controlSlider;
		GameObject multiplayerInfo; // панель информации об игре в мультиплеере
		Text textScore;
		Text levelText;
		Text gameStepText;		
		GameObject ratingSlider; //слайдер рейтинга в ировом интерфейсе		


		//панель подтверждения выбора
		GameObject confirmationPanel; 


		//подменю игры (настройки)
		GameObject settingsPanel; 
		

		//панель результата после раунда по мультиплееру
		GameObject multiplayerCompletedPanel; 
		GameObject playerInfoMC; //панель инфы игрока
		GameObject oppInfoMC;  //панель инфы сщперника		
		

		//панель сообщений
		static Animator animatorMsgPanel; //аниматор панели с сообщениями
		static GameObject showMsgPanel;


		//панель с соперниками перед игрой в мультиплеер
		GameObject opponentsPanel;		


		//панель с анимацией загрузки	
		GameObject progressBarPanel; 

		
		//меню с уровнями для выбора
		DynamicGridScrollView contentScrolling;


		//Панель магазина товаров
		GameObject shop;
		GameObject gams_ItemPanel;
		GameObject ball_ItemPanel;
		GameObject pack_ItemPanel;


		// текущее активное меню
		ActiveMenu currentActiveMenu; 

		
		//список запущенных корутин
		Coroutine runningCoroutine;


		//панель вознаграждения игрока
		GameObject RewardPanel;


		//панель дополнительной жизни
		GameObject extralifePanel;
		GameObject extralifePanel_gems; //панель с количеством камней
		GameObject extralifePanel_videoAd; //панель со значком просмотра видео


		//панель приглашений в игру
		GameObject friaendsPanel;
		GameObject fpRealButtons; //контейнер для кнопок на панели пиглашений
		GameObject fpStubButtons; //контейнер для кнопок заглушек 
		

	#endregion


	#region Events

		public event Action<ActiveMenu> SetActiveMenuEvent;

	#endregion


	#region Init		

		//настройка UI

		void Awake()
		{			
			if (Instance == null)
				Instance = this;

			InitVictoryPanel();
			InitLosePanel();
			InitGameInterface();
			InitConfirmationPanel();
			InitSettingsPanel();
			InitMsgPanel();
			InitOpponentsPanel();
			InitShop();
			InitProgressBarPanel();
			InitMultiplayerCompletedPanel();
			InitGetRewardPanel();
			InitExtralifePanel();
			InitFriendsPanel();		

		}


		public void Init()		
		{			
			InitMainMenu();	
			Subscription(); //подписка

			SetActivMenu(ActiveMenu.MAIN_MENU);
		}


		void InitVictoryPanel()
		{
			victoryPanel = GameObject.Find("VictoryPanel").gameObject;
			victoryPanelRewardAd = victoryPanel.transform.Find("Button_WATCH_VIDEO_AD").gameObject;
			victoryPanelRewardAdStub = victoryPanel.transform.Find("Button_WATCH_VIDEO_Stub").gameObject;
			HideVictoryPanel();
		}


		void InitLosePanel()
		{
			losePanel = GameObject.Find("LosePanel").gameObject;
			HideLosePanel();
		}


		void InitMainMenu()
		{
			maimMenu = GameObject.Find("MainMenu").gameObject;
			contentScrolling = maimMenu.transform.Find("LevelsScrollView").GetComponent<DynamicGridScrollView>();
			videoAdButton = maimMenu.transform.Find("TopPanel/Button_WATCH_VIDEO_AD").gameObject;
			videoAdButtonStub = maimMenu.transform.Find("TopPanel/Button_WATCH_VIDEO_Stub").gameObject;
		}


		void InitGameInterface()
		{
			gameInterface = GameObject.Find("GameInterface").gameObject;
			scorePanel = gameInterface.transform.Find("TOP_PANEL/Score").gameObject;
			textScore = gameInterface.transform.Find("TOP_PANEL/Score/TextScore").GetComponent<Text>();

			shopButtonGameInterface = gameInterface.transform.Find("TOP_PANEL/InfoPanel/Button_SHOP").gameObject;

			infoPanel = gameInterface.transform.Find("TOP_PANEL/InfoPanel").gameObject;
			levelText = infoPanel.transform.Find("Level/TextLevel").GetComponent<Text>();
			gameStepText = infoPanel.transform.Find("Step/TextStep").GetComponent<Text>();

			//настройка панели мультиплеера
			multiplayerInfo = gameInterface.transform.Find("TOP_PANEL/Multiplayer_info").gameObject;

			//находим нижние панели
			bottomPanel = gameInterface.transform.Find("BOTTOM_PANEL").gameObject;
			bottomPanel_2 = gameInterface.transform.Find("BOTTOM_PANEL_2").gameObject;

			//настройка нижней панели интерфейса кнопки #1			
			actionButton_1_Push = gameInterface.transform.Find("BOTTOM_PANEL/Action_button_1/PUSH_Button").gameObject;
			actionButton_1_Buy = gameInterface.transform.Find("BOTTOM_PANEL/Action_button_1/BUY_ACTION_Button").gameObject;
			actionButton_1_ShowAd = gameInterface.transform.Find("BOTTOM_PANEL/Action_button_1/SHOW_AD_Button").gameObject;

			//настройка нижней панели интерфейса кнопки #2			
			actionButton_2_Push = gameInterface.transform.Find("BOTTOM_PANEL/Action_button_2/PUSH_Button").gameObject;
			actionButton_2_Buy = gameInterface.transform.Find("BOTTOM_PANEL/Action_button_2/BUY_ACTION_Button").gameObject;
			actionButton_2_ShowAd = gameInterface.transform.Find("BOTTOM_PANEL/Action_button_2/SHOW_AD_Button").gameObject;

			controlSlider = gameInterface.transform.Find("BOTTOM_PANEL/SliderPanel/Slider").GetComponent<Slider>();

			ratingSlider = gameInterface.transform.Find("TOP_PANEL/RatingSlider").gameObject;

			HideGameInterface();

		}


		void InitConfirmationPanel()
		{
			confirmationPanel = GameObject.Find("ConfirmationPanel").gameObject;
			HideConfirmationPanel();
		}


		void InitSettingsPanel()
		{
			settingsPanel = GameObject.Find("SettingsPanel").gameObject;			
			HideSettingsPanel();
		}


		void InitMsgPanel()
		{
			showMsgPanel = GameObject.Find("ShowMsgPanel").gameObject;
			animatorMsgPanel = GameObject.Find("ShowMsgPanel").GetComponent<Animator>();
			showMsgPanel.SetActive(false);
		}


		void InitOpponentsPanel()
		{
			opponentsPanel = GameObject.Find("OpponentsPanel").gameObject;
			HidePanelOpponent();
		}


		void InitShop()
		{
			shop = GameObject.Find("Shop").gameObject;
			gams_ItemPanel = shop.transform.Find("MainPanel/GAMS").gameObject;
			ball_ItemPanel = shop.transform.Find("MainPanel/BALL").gameObject;
			pack_ItemPanel = shop.transform.Find("MainPanel/PACK").gameObject;

			Shop_Hide();
		}

		void InitProgressBarPanel()
		{
			progressBarPanel = GameObject.Find("ProgressBar").gameObject;
			progressBarPanel.SetActive(false);
		}


		void InitMultiplayerCompletedPanel()
		{
			multiplayerCompletedPanel = GameObject.Find("MultiplayerCompletedPanel").gameObject;
			playerInfoMC = multiplayerCompletedPanel.transform.Find("Panel/PLAYER_INFO").gameObject;
			oppInfoMC = multiplayerCompletedPanel.transform.Find("Panel/OPPONENT_INFO").gameObject;

			HideMultiplayerResultPanel(); //прячем панельку
		}


		void InitGetRewardPanel()
		{
			RewardPanel = GameObject.Find("RewardPanel").gameObject;
			RewardPanelHide();
		}


		void InitExtralifePanel()
		{
			extralifePanel = GameObject.Find("ExtralifePanel").gameObject;
			extralifePanel_gems = extralifePanel.transform.Find("Panel/Info_Panel/GEMS_panel").gameObject;
			extralifePanel_videoAd = extralifePanel.transform.Find("Panel/Info_Panel/VIDEO_AD_panel").gameObject;

			ExtralifePanel_Hide();
		}


		void InitFriendsPanel()
		{
			friaendsPanel = GameObject.Find("FriendsPanel").gameObject;
			fpRealButtons = friaendsPanel.transform.Find("RealButtons").gameObject;
			fpStubButtons = friaendsPanel.transform.Find("StubButtons").gameObject;

			FriendPanelHide();
		}		


		void Subscription()
		{
			var gm = GameManager.Instance;
			gm.ScoreUpdateEvent += UpdateTextScore; //обновление очков
			gm.StepUpdateEvent += UpdateGameStepText;//обновляет количество шагов сделаные в игровой сессии
			gm.MoneyCountUpdateEvent += Shop_UpdateMoneyCount; //обновляет кол. денег в маге
			gm.MoneyCountUpdateEvent += MainMenu_UpdateInfo; //обновляет в main menu счётчик денег			
			gm.MoneyCountUpdateEvent += ExtralifePanel_UpdateButton; //обновляем панель continue
			gm.MoneyCountUpdateEvent += ActionButtons_AllUpdate; //обновляет кнопки ActionsForce
			gm.MoneyCountUpdateEvent += VictoryPanel_UpdateGemsCount; //обновляем счётчик камней на панели победы
			gm.GameWinEvent += Victory; //событие победы
			gm.GameLoseEvent += Loss; //событие проигрыша
			gm.StartGameEvent += StartGame;
			gm.StopGameEvent += StopGame;
			gm.SetPlaySoundGameEvent += SoundButtonUpdateText; //вкл./выкл. звука в игре
			gm.LevelRatingChangeEvent += UpdateRatinSliderGameInterface; //изменение рейтинга во время игры
			gm.ActivateRewardEvent += RewardPanelActive; //активация награды пользователю
			gm.ContinueGameEvent += ExtralifePanel_Active; //показ панели дополнительной жизни
			
			//магазин
			var store = gm.StoreManager;
			store.BuyGameContinueEvent += ExtralifePanel_Hide; //при покупке продолжения игры - оключаем екстра панель
			
			//События от Action manager для action кнопок
			gm.ManagerAction.Action_1_ActivateEvent += ActionButton_1_Update;
			gm.ManagerAction.Action_2_ActivateEvent += ActionButton_2_Update;

			//события от мультиплеера
			var gpgs = GameObject.FindObjectOfType<GPGS>();
			gpgs.TimeUpdateEvent += TimeMultiplayerUpdate; //время в текущей игре по мультиплееру
			gpgs.UpdateOpponentScoreEvent += UpdateMultiplayerScore_Opponent; //счёт от слперника
			gpgs.UpdateMyScoreEvent += UpdateMultiplayerScore_Player; //счёт игрока
			gpgs.GameResultEvent += ShowMultiplayerResultdPanel; //завершение игры по сети
			gpgs.AllParticipantsConfirmedEvent += ShowPanelOpponent;
			gpgs.InitMultiplayerEvent += ProgressBarStatus; //реакция на событие загрузки сетевой комнаты
			gpgs.LoginUpdateEvent += FriendPanel_UpdateButtons; //рекция на вход/выход из сервисов

			//менеджер ввода
			var im = InputManager.Instance;
			im.PressButtonRetryMultiplayer += HideMultiplayerResultPanel; //пряче панель результатов
			im.PressButtonPause += PauseReaction; //пауза в игре
			im.ShowConfirmationPanel += ConfirmationShow;
			im.PressButtonShopOpen += Shop_Active; //показывает магазин
			im.PressButtonShopClose += Shop_Hide; //прячет магазин
			im.PressButtonShop_GamsPanel += Shop_GamsPanelActive; //окрывает панель GAMS
			im.PressButtonShop_BallPanel += Shop_BallPanelActive; //BALL
			im.PressButtonShop_PackPanel +=	Shop_PackPanelActive; //PACK
			im.ClickRewardPanelEvent += RewardPanelHide; //прячем панель награды
			im.FriendsPanel_PressButtonCloseEvent += FriendPanelHide; //закрытие панели приглашения
			im.PressButtonOpenFriendsPanelEvent += FriendPanelActive; //показать панель приглашения
			
			//реклама
			var ad = AdMobManager.Instance;
			ad.RewardContinueGameEvent += ExtralifePanel_Hide; //при покупке продолжения игры - оключаем екстра панель
			
		}

	#endregion


	#region Victory panel

		void Victory()
		{
			SetActivMenu(ActiveMenu.VICTORY_PANEL);
			VictoryPanelUpdateInfo();
		}


		//обновление рейтинга на панели выиграша
		void VictoryPanelUpdateInfo()
		{
			StarRatingUpdate();
			ScaleRatingUpdate();
			VictoryPanel_UpdateGemsCount();
			VictoryPanel_UpdateStarsCount();
			VictoryPanel_EnableAD();
		}


		//обновляет изображения рейтинга (звёзды)
		void StarRatingUpdate()
		{
			var starImage_0 = victoryPanel.transform.Find("Panel/StarsRatingPanel/StarImage_0").GetComponent<Image>();
			var starImage_1 = victoryPanel.transform.Find("Panel/StarsRatingPanel/StarImage_1").GetComponent<Image>();
			var starImage_2 = victoryPanel.transform.Find("Panel/StarsRatingPanel/StarImage_2").GetComponent<Image>();

			var data = Resources.Load<DataScrollElement>("Data/DataScrollElement");

			switch(GameManager.Instance.CurrentLevel.Rating)
			{
				case 1:
					starImage_0.sprite = data.whiteStarImage;
					starImage_1.sprite = data.blackStarImage;
					starImage_2.sprite = data.blackStarImage;
				break;
				case 2:
					starImage_0.sprite = data.whiteStarImage;
					starImage_1.sprite = data.whiteStarImage;
					starImage_2.sprite = data.blackStarImage;
				break;
				case 3:
					starImage_0.sprite = data.whiteStarImage;
					starImage_1.sprite = data.whiteStarImage;
					starImage_2.sprite = data.whiteStarImage;
				break;
				default:
					starImage_0.sprite = data.blackStarImage;
					starImage_1.sprite = data.blackStarImage;
					starImage_2.sprite = data.blackStarImage;
				break;
			}
		}


		//обновляет шкалу общего рейтинга игрока
		void ScaleRatingUpdate()
		{
			var ratingSlider = victoryPanel.transform.Find("Panel/ScoreLinePanel/Slider").GetComponent<Slider>();
			ratingSlider.maxValue = GameManager.Config.maxPointReward;

			//текущий рейтинг
			var curRating = GameManager.CurrentGame.RewardScale;

			StopRunningRoutine();
			//плавно меняем значение слайдера
			runningCoroutine = StartCoroutine ( GradualSliderChange(curRating, ratingSlider.value, ratingSlider, 0.05f) );
		
		}		


		//обновляет количество камней на панеле победы
		void VictoryPanel_UpdateGemsCount()
		{
			if (!victoryPanel.activeSelf)
				return;

			var gemsCount = victoryPanel.transform.Find("Panel/MoneyCount/Text").GetComponent<Text>();
			gemsCount.text = GameManager.CurrentGame.MoneyCount.ToString();			
		}


		//обновляет количество звёзд на панеле победы
		void VictoryPanel_UpdateStarsCount()
		{
			if (!victoryPanel.activeSelf)
				return;

			var starsCount = victoryPanel.transform.Find("Panel/RatingCount/Text").GetComponent<Text>();
			starsCount.text = GameManager.CurrentGame.RatingCount.ToString();
		}


		//контролирует, стоит ли включать возможность просмотреть рекламу за вознаграждение
		void VictoryPanel_EnableAD()
		{
			//если реклама загрузилась - активируем кнопку просмотра рекламы
			if (AdMobManager.Instance.IsVideoAdLoad)
			{
				victoryPanelRewardAd.SetActive(true);
				victoryPanelRewardAdStub.SetActive(false);
			}
			else
			{
				victoryPanelRewardAd.SetActive(false);
				victoryPanelRewardAdStub.SetActive(true);
			}
		}


		void ActiveVictoryPanel()
		{
			victoryPanel.SetActive(true);			
		}


		void HideVictoryPanel()
		{
			//останавливаем корутину изменения слайдера рейтинга
			StopRunningRoutine();

			victoryPanel.SetActive(false);			
		}
		

	#endregion


	#region Lose panel

		void Loss()
		{
			SetActivMenu(ActiveMenu.LOSS_PANEL);				
		}


		void ActiveLosePanel()
		{
			losePanel.SetActive(true);

				
		}


		void HideLosePanel()
		{
			//останавливаем корутину изменения слайдера рейтинга
			StopRunningRoutine();

			losePanel.SetActive(false);
		}


	#endregion


	#region Main menu

		void ActiveMainMenu()
		{
			maimMenu.SetActive(true);
			contentScrolling.UpdateItemsOnMenuLoad();
			MainMenu_UpdateInfo();
			MainMenu_UpdateVideoAdButton();
		}


		void HideMeinMenu()
		{
			maimMenu.SetActive(false);
		}


		//обновляет отображение денег и рейтинга игрока на панели
		void MainMenu_UpdateInfo()
		{
			if (!maimMenu.activeSelf) return;

			//деньги
			var moneyText = maimMenu.transform.Find("TopPanel/MoneyCount/Text").GetComponent<Text>();
			moneyText.text = GameManager.CurrentGame.MoneyCount.ToString();

			//рейтинг
			var ratingText = maimMenu.transform.Find("TopPanel/RatingCount/Text").GetComponent<Text>();
			ratingText.text = GameManager.CurrentGame.RatingCount.ToString();
		}


		//отображает, или скрывает кнопку просмотра рекламы
		void MainMenu_UpdateVideoAdButton()
		{
			//если реклама загружена, то включаем реальную кнопку, 
			//иначе отображаем заглушку
			if (AdMobManager.Instance.IsVideoAdLoad)
			{				
				videoAdButton.SetActive(true);
				videoAdButtonStub.SetActive(false);
			}
			else
			{
				videoAdButton.SetActive(false);
				videoAdButtonStub.SetActive(true);
			}
		}

	#endregion


	#region Game interface

		//обновляет счётчик ходов
		void UpdateGameStepText(int step)
		{
			gameStepText.text = step.ToString();
		}


		//обновляет счёт в игре
		void UpdateTextScore(int score)
		{
			textScore.text = score.ToString();
		}


		//обновляет текст с номером уровня
		void UpdateTextLevel()
		{
			levelText.text = GameManager.Instance.CurrentLevel.Index.ToString();
		}


		//обновляет шкалу рейтинга игрока на панели игрового интерфейса
		void UpdateRatinSliderGameInterface(int _rating, int _maxRating)
		{
			var slider = ratingSlider.GetComponent<Slider>();
			slider.maxValue = _maxRating;
			
			StopRunningRoutine();

			//плавно меняем значение слайдера
			runningCoroutine = StartCoroutine ( GradualSliderChange(_rating, slider.value, slider, 2f) );			
		}



		//обновление времени в мультиплеере
		void TimeMultiplayerUpdate(float _time, float _maxValue)
		{
			var slider = multiplayerInfo.transform.Find("TimeScale").GetComponent<Slider>();
			slider.maxValue = _maxValue;
			slider.value = _time;
		}


		//счет соперника в мультиплеере
		void UpdateMultiplayerScore_Opponent(int _score, int _maxScore)
		{
			multiplayerInfo.transform.Find("Opponent_info/Score_text").GetComponent<Text>().text = "" + _score + "/" + _maxScore;
		}


		//счёт игрока в мультиплеере
		void UpdateMultiplayerScore_Player(int _score, int _maxScore)
		{
			multiplayerInfo.transform.Find("Player_info/Score_text").GetComponent<Text>().text = "" + _score + "/" + _maxScore;
		}
		

		//включает и выключает нужные компоненты интерфейса
		//в зависимости от режима игры
		void ActiveGameInterface()
		{
			gameInterface.SetActive(true);

			//выключаем некоторые элементы интерфейса игры, взависимости от режима
			switch(GameManager.Instance.ModeGame)
			{
				case GameMode.CLASSIC:
					scorePanel.SetActive(true); //on
					infoPanel.SetActive(true); //on
					infoPanel.transform.Find("Level").gameObject.SetActive(false);	//отключаем счётчик уровня
					multiplayerInfo.SetActive(false); //off	
					ratingSlider.SetActive(false); //off	
					shopButtonGameInterface.SetActive(true); //on			
				break;

				case GameMode.LEVELS:
					ratingSlider.SetActive(true); //on
					scorePanel.SetActive(false); //off
					infoPanel.SetActive(true); //on
					infoPanel.transform.Find("Level").gameObject.SetActive(true); //вкл. счётчик уровня
					multiplayerInfo.SetActive(false); //off		
					shopButtonGameInterface.SetActive(true); //on		
				break;

				case GameMode.MULTIPLAYER:					
					scorePanel.SetActive(false); //off
					infoPanel.SetActive(false); //off
					ratingSlider.SetActive(false); //off
					multiplayerInfo.SetActive(true); //ON	
					shopButtonGameInterface.SetActive(false); //off
					MutliplayerOpponentNameUpdate();							
				break;
			}
		}

		
		//обновляет имена соперников на панели интерфейса в мультиплеере
		void MutliplayerOpponentNameUpdate()
		{
			var data = GPGS.Instance.MultiplayerInfo;

			//моё имя
			var myName = multiplayerInfo.transform.Find("Player_info/Name_text").GetComponent<Text>();
			myName.text = data.playerName;

			//имя соперника
			var oppName = multiplayerInfo.transform.Find("Opponent_info/Name_text").GetComponent<Text>();
			oppName.text = data.opponentName;
		}


		void HideGameInterface()
		{
			//останавливаем корутину изменения слайдера рейтинга
			StopRunningRoutine();

			gameInterface.SetActive(false);			
		}
			

		//обновляет обе кнопки actionsForce 
		void ActionButtons_AllUpdate()
		{
			if (!gameInterface.activeSelf)
				return;

			var act = GameManager.CurrentGame.actionItems;
			ActionButton_1_Update(act[GameActionType.ICE_FORCE]);
			ActionButton_2_Update(act[GameActionType.TIME_BOMB]);
		}


		//Активация/девктивация action button #1
		void ActionButton_1_Update(int itemCount)
		{
			if (itemCount > 0)
			{
				//активируем PUSH кнопку
				actionButton_1_Push.SetActive(true);

				//меняем текст на кнопке
				var t = actionButton_1_Push.transform.Find("CountText").GetComponent<Text>();
				t.text = "x" + itemCount;

				//остальные кнопки выключаем
				actionButton_1_Buy.SetActive(false);
				actionButton_1_ShowAd.SetActive(false);
			}
			//если хватает на покупку  - активируем кнопку покупки
			else if (GameManager.CurrentGame.MoneyCount >= GameManager.Config.costAction)
			{
				//активируем BUY кнопку
				actionButton_1_Buy.SetActive(true);

				//меняем текст стоимости покупки Action
				var t = actionButton_1_Buy.transform.Find("CountText").GetComponent<Text>();
				t.text = "x" + GameManager.Config.costAction;
				
				//остальные кнопки выключаем
				actionButton_1_Push.SetActive(false);
				actionButton_1_ShowAd.SetActive(false);
			}
			//если видеореклама загрузилась - активируем кнопку просмотра рекламы
			else if (AdMobManager.Instance.IsVideoAdLoad)
			{
				//если мультиплеер рекламу смотреть нельзя
				if (GameManager.Instance.ModeGame == GameMode.MULTIPLAYER)
				{
					actionButton_1_ShowAd.SetActive(false);
					return;
				}

				//активируем BUY кнопку
				actionButton_1_ShowAd.SetActive(true);
				
				//остальные кнопки выключаем
				actionButton_1_Push.SetActive(false);
				actionButton_1_Buy.SetActive(false);
			}	
			else
			{
				actionButton_1_Push.SetActive(false);
				actionButton_1_Buy.SetActive(false);
				actionButton_1_ShowAd.SetActive(false);
			}		
		}


		//Активация/девктивация action button #2
		void ActionButton_2_Update(int itemCount)
		{			
			if (itemCount > 0)
			{
				//активируем PUSH кнопку
				actionButton_2_Push.SetActive(true);

				//меняем текст на кнопке
				var t = actionButton_2_Push.transform.Find("CountText").GetComponent<Text>();
				t.text = "x" + itemCount;

				//остальные кнопки выключаем
				actionButton_2_Buy.SetActive(false);
				actionButton_2_ShowAd.SetActive(false);
			}
			//если хватает на покупку  - активируем кнопку покупки
			else if (GameManager.CurrentGame.MoneyCount >= GameManager.Config.costAction)
			{
				//активируем BUY кнопку
				actionButton_2_Buy.SetActive(true);

				//меняем текст стоимости покупки Action
				var t = actionButton_2_Buy.transform.Find("CountText").GetComponent<Text>();
				t.text = "x" + GameManager.Config.costAction;
				
				//остальные кнопки выключаем
				actionButton_2_Push.SetActive(false);
				actionButton_2_ShowAd.SetActive(false);
			}
			//если видеореклама загрузилась - активируем кнопку просмотра рекламы
			else if (AdMobManager.Instance.IsVideoAdLoad)
			{
				//если мультиплеер рекламу смотреть нельзя
				if (GameManager.Instance.ModeGame == GameMode.MULTIPLAYER)
				{
					actionButton_2_ShowAd.SetActive(false);
					return;
				}					

				//активируем BUY кнопку
				actionButton_2_ShowAd.SetActive(true);
				
				//остальные кнопки выключаем
				actionButton_2_Push.SetActive(false);
				actionButton_2_Buy.SetActive(false);
			}
			else
			{
				actionButton_2_Push.SetActive(false);
				actionButton_2_Buy.SetActive(false);
				actionButton_2_ShowAd.SetActive(false);
			}

		}
		

		//активируется 2 панель с Return ball		
		void BottomPanel_2_Active()
		{
			bottomPanel_2.SetActive(true);
			bottomPanel.SetActive(false);						
		}


		//активируется 1 панель со слайдером и с кнопками Action
		void BottomPanel_1_Active()
		{
			bottomPanel.SetActive(true);	
			bottomPanel_2.SetActive(false);	

			//обновляем интерфейс кнопок GameAction			
			ActionButton_1_Update( GetCountAct(GameActionType.ICE_FORCE) );	
			ActionButton_2_Update( GetCountAct(GameActionType.TIME_BOMB) );
		}


		//возвращает остаток GameAction по типу
		int GetCountAct(GameActionType type)
		{
			var items = GameManager.CurrentGame.actionItems;

			if (items.ContainsKey(type))
				return items[type];

			return 0;
		}


		//подписка на рассылку от BallController
		void GI_SubscriptionBallController()
		{
			var gm = GameManager.Instance;
			gm.BallController.BallsPushEvent += BottomPanel_2_Active; //мячи стартанули
			gm.BallController.BallsToStartPositionEvent += BottomPanel_1_Active; //мячи вернулись
			gm.BallController.IDestroyEvent += UnsubscribeBallController;
		}


		//отписываемся от рассылки BallController
		void UnsubscribeBallController()
		{
			var gm = GameManager.Instance;
			gm.BallController.BallsPushEvent -= BottomPanel_2_Active; //мячи стартанули
			gm.BallController.BallsToStartPositionEvent -= BottomPanel_1_Active; //мячи вернулись
			gm.BallController.IDestroyEvent -= UnsubscribeBallController;
		}		

	#endregion


	#region Confirmation panel

		void ConfirmationShow(bool isShow)
		{
			if (isShow)
				ActiveConfirmationPanel();
			else
				HideConfirmationPanel();
		}


		void ActiveConfirmationPanel()
		{
			confirmationPanel.SetActive(true);
		}


		void HideConfirmationPanel()
		{
			confirmationPanel.SetActive(false);
		}


	#endregion


	#region Settings panel

		void PauseReaction(bool isPause)
		{
			if (isPause)
				ActiveSettingsPanel();
			else
				HideSettingsPanel();

		}


		void ActiveSettingsPanel()
		{
			settingsPanel.SetActive(true);
			SoundButtonUpdateText(); //обновим текст кнопки
		}


		void HideSettingsPanel()
		{
			settingsPanel.SetActive(false);
		}


		void SoundButtonUpdateText()
		{
			var soundButtonText = settingsPanel.transform.Find("Panel/SoundButton/Text").GetComponent<Text>();
			
			if (GameManager.CurrentGame.IsSoundPlay)
				soundButtonText.text = "SOUND ON";
			else
				soundButtonText.text = "SOUND OFF";
		}

	#endregion


	#region Multiplayer game completed panel

		//скрывает панель результата игры в мультиплеер
		void HideMultiplayerResultPanel()
		{
			multiplayerCompletedPanel.SetActive(false);
		}


		//показываем панель результата игры в мультиплеер
		void ActiveMultiplayerResultPanel()
		{
			multiplayerCompletedPanel.SetActive(true);
		}


		//вызывается по событию окончания игры по мультиплееру	
		void ShowMultiplayerResultdPanel(MultiplayerResult gameResult)
		{
			//включаем панельку результата
			SetActivMenu(ActiveMenu.MULTIPLAYER_RESULT_PANEL);			

			var drawText = multiplayerCompletedPanel.transform.Find("Panel/TEXT_DRAW").gameObject;
			
			var data = GPGS.Instance.MultiplayerInfo; //даные об игроках

			//информация игрока			
			SetResultInfo(playerInfoMC, data.playerIcon, data.myScore, data.maxScore);
			var winText_1 = multiplayerCompletedPanel.transform.Find("Panel/TEXT_WIN_1").gameObject;

			//информация опонента			
			SetResultInfo(oppInfoMC, data.opponentIcon, data.opponentScore, data.maxScore);
			var winText_2 = multiplayerCompletedPanel.transform.Find("Panel/TEXT_WIN_2").gameObject;

			switch(gameResult)
			{
				//моя победа
				case MultiplayerResult.WIN:
					drawText.SetActive(false); // off
					winText_1.SetActive(true); //on
					winText_2.SetActive(false); //off
				break;
				//противник выиграл
				case MultiplayerResult.LOSE:
					drawText.SetActive(false); //off					
					winText_1.SetActive(false); //off
					winText_2.SetActive(true); //on
				break;
				//ничья
				case MultiplayerResult.DRAW:
					drawText.SetActive(true); //on
					winText_1.SetActive(false); //off
					winText_2.SetActive(false); //off
				break;				
			}
		}		


		//устанавливает информацию об игроке на панеле результата игры по мультиплееру
		void SetResultInfo(GameObject _go, Sprite _icon, int _currScore, int _maxScore)
		{
			_go.transform.Find("Icon").GetComponent<Image>().sprite = _icon;
			_go.transform.Find("Text_score").GetComponent<Text>().text = "" + _currScore + "/" + _maxScore;
		}
		

	#endregion


	#region Shop

		//Shop*************************
		void Shop_Active()
		{
			shop.SetActive(true);

			Shop_UpdateMoneyCount();

			Shop_GamsPanelActive(); //on
			Shop_BallPanelHide(); //off
			Shop_PackPanelHide(); //off
		}


		void Shop_Hide()
		{
			shop.SetActive(false);
		}


		// обновление количества денег (рубинов) в панели магазина
		void Shop_UpdateMoneyCount()
		{
			if (!shop.activeSelf) return;

			var moneyText = shop.transform.Find("MoneyCount/Text").GetComponent<Text>();			
			moneyText.text = GameManager.CurrentGame.MoneyCount.ToString();
		}


		//GAMS*************************
		void Shop_GamsPanelActive()
		{
			gams_ItemPanel.SetActive(true);

			Shop_BallPanelHide();
			Shop_PackPanelHide();
		}


		void Shop_GamsPanelHide()
		{
			gams_ItemPanel.SetActive(false);
		}


		//BALL*************************
		void Shop_BallPanelActive()
		{
			ball_ItemPanel.SetActive(true);

			Shop_GamsPanelHide();
			Shop_PackPanelHide();
		}


		void Shop_BallPanelHide()
		{
			ball_ItemPanel.SetActive(false);
		}


		//PACK*************************
		void Shop_PackPanelActive()
		{
			pack_ItemPanel.SetActive(true);

			Shop_GamsPanelHide();
			Shop_BallPanelHide();
		}


		void Shop_PackPanelHide()
		{
			pack_ItemPanel.SetActive(false);
		}

	#endregion


	#region Progress bar

		void ProgressBarStatus(bool _isActive)
		{
			if (_isActive)
			{
				ShowProgressBar();
				FriendPanelHide();				
			}				
			else
			{
				HideProgressBar();
			}
		}

		//показать панель загрузки
		void ShowProgressBar()
		{
			progressBarPanel.SetActive(true);
			var stateText = progressBarPanel.transform.Find("STATE_MP_text").GetComponent<Text>();
			stateText.text = GPGS.Instance.State.ToString();		
		}


		//спрятать панель загрузки
		void HideProgressBar()
		{
			progressBarPanel.SetActive(false);
		}

	#endregion


	#region Opponents panel

		//показать панель с участниками игры
		void ShowPanelOpponent()
		{
			SetActivMenu(ActiveMenu.OPPONENT_PANEL_MULTIPLAYER);			
		}


		void UpdatePanelOpponents()
		{
			var data = GPGS.Instance.MultiplayerInfo;

			//player
			var playerPanel = opponentsPanel.transform.Find("Player").gameObject;
			playerPanel.transform.Find("Icon").GetComponent<Image>().sprite = data.playerIcon;
			playerPanel.transform.Find("NameText").GetComponent<Text>().text = data.playerName;

			//oponent
			var opponentPanel = opponentsPanel.transform.Find("Opponent").gameObject;
			opponentPanel.transform.Find("Icon").GetComponent<Image>().sprite = data.opponentIcon;
			opponentPanel.transform.Find("NameText").GetComponent<Text>().text = data.opponentName;
		}


		void ActivePanelOpponents()
		{
			opponentsPanel.SetActive(true);
			UpdatePanelOpponents();
		}


		//прячем панель с участниками угры по мультиплееру
		void HidePanelOpponent()
		{
			opponentsPanel.SetActive(false);
		}

	#endregion


	#region Reward panel

		void RewardPanelActive()
		{
			RewardPanel.SetActive(true);
			RewardUpdateTextCount();
		}


		//обновляет текст с количеством на панели награды
		void RewardUpdateTextCount()
		{
			var countText = RewardPanel.transform.Find("CountRewardText").GetComponent<Text>();
			countText.text = GameManager.Instance.RewardCount.ToString();			
		}


		void RewardPanelHide(int countReward = 0)
		{
			RewardPanel.SetActive(false);
		}

	#endregion


	#region Extralife Panel

		void ExtralifePanel_Active()
		{
			extralifePanel.SetActive(true);
			ExtralifePanel_UpdateButton();
		}

		void ExtralifePanel_Hide()
		{
			extralifePanel.SetActive(false);
		
			//обновляем кнопки actionForce, если открыт игровой интерфейс
			ActionButtons_AllUpdate();			
		}


		void ExtralifePanel_UpdateButton()
		{
			if (!extralifePanel.activeSelf)
				return;

			var gameMoney = GameManager.CurrentGame.MoneyCount;
			var cost = GameManager.Config.costContinueGame;

			if (gameMoney >= cost)
			{
				extralifePanel_gems.SetActive(true);
				extralifePanel_videoAd.SetActive(false);
			}
			//если реклама загрузилась
			else if (AdMobManager.Instance.IsVideoAdLoad)
			{
				extralifePanel_videoAd.SetActive(true);
				extralifePanel_gems.SetActive(false);
			}
			else
			{
				extralifePanel_videoAd.SetActive(false);
			}
		}

	#endregion


	#region Friends Panel

		void FriendPanelActive()
		{
			friaendsPanel.SetActive(true);
			FriendPanel_UpdateButtons();
		}


		void FriendPanelHide()
		{
			friaendsPanel.SetActive(false);
		}


		void FriendPanel_UpdateButtons()
		{
			//если не активна панель - выходим
			if (!friaendsPanel.activeSelf)
				return;

			var textLoginBtn = friaendsPanel.transform.Find("LOGIN_button/Text").GetComponent<Text>();

			if (GPGS.Instance.IsLogin)
			{
				fpRealButtons.SetActive(true);
				fpStubButtons.SetActive(false);
				textLoginBtn.text = "SIGN OUT";
			}
			else
			{
				fpRealButtons.SetActive(false);
				fpStubButtons.SetActive(true);
				textLoginBtn.text = "SIGN IN";
			}
		}

	#endregion


	#region Update
				
		void StartGame()
		{
			SetActivMenu(ActiveMenu.GAME);
			UpdateTextLevel();	

			//подписываем интерфейс на рассылку от BallController
			GI_SubscriptionBallController();
			//активируем панель с слайдером и кнопками Actions
			BottomPanel_1_Active();				
		}


		//отключает игровой интерфейс и включает главное меню
		void StopGame()
		{
			//останавливаем корутину изменения слайдера рейтинга
			StopRunningRoutine();

			//включаем главное меню
			SetActivMenu(ActiveMenu.MAIN_MENU);	
			contentScrolling.UpdateItemsOnMenuLoad();				
		}


		//переключает панельки
		void SetActivMenu(ActiveMenu _activeMenu)
		{
			currentActiveMenu = _activeMenu;
			HideAllMenu();

			switch(currentActiveMenu)
			{
				case ActiveMenu.MAIN_MENU:					
					ActiveMainMenu(); // показываем меню					
				break;
				case ActiveMenu.SETTINGS:
					ActiveSettingsPanel(); //включаем панель настроек
				break;
				case ActiveMenu.GAME:
					ActiveGameInterface(); //включаем интерфейс игры
				break;
				case ActiveMenu.VICTORY_PANEL:
					ActiveVictoryPanel(); //включаем панель победы
				break;
				case ActiveMenu.LOSS_PANEL:
					ActiveLosePanel(); //включаем панель проигрыша
				break;
				case ActiveMenu.CONFIRMATION_PANEL:
					ActiveConfirmationPanel();	//включаем панель подтверждения выбора			
				break;
				case ActiveMenu.MULTIPLAYER_RESULT_PANEL:
					//включаем панель результата игры по мультиплееру	
					ActiveMultiplayerResultPanel();	
				break;
				case ActiveMenu.OPPONENT_PANEL_MULTIPLAYER:
					//включаем панель опонентов перед игрой в мультиплеер	
					ActivePanelOpponents();
				break;
			}

			SetActiveMenuEvent?.Invoke(currentActiveMenu);
		}


		//скрывает все UI панели
		void HideAllMenu()
		{	
			HideConfirmationPanel();
			HideGameInterface();
			HideMeinMenu();
			HideLosePanel();
			HideVictoryPanel();
			HideSettingsPanel();		
			HideMultiplayerResultPanel();	
			HidePanelOpponent();
			HideProgressBar();
			Shop_Hide();
			ExtralifePanel_Hide();
			FriendPanelHide();			
		}
		

	#endregion


	#region Static method

		//выводит сообщение в всплывающем окошке
		public static void ShowMsg(string msg)
		{			
			showMsgPanel.SetActive(true);
			showMsgPanel.transform.Find("Panel/Text").GetComponent<Text>().text = msg;
			animatorMsgPanel.SetTrigger(StaticPrm.ANIM_TRIGGER_SHOW_MSG);			
		}		

	#endregion	
	

	#region General methods

		//постепенно изменяет значение слайдера 
		IEnumerator GradualSliderChange(int _newRating, float _startValue, Slider _ratingSlider, float _addNum)
		{			
			for (float i = _startValue; i < _newRating; i += _addNum)
			{				
				//если текущее значение меньше нужного - добавляем
				if (_ratingSlider.value < _newRating)
				{
					_ratingSlider.value = i;

					//ограничиваем значение
					if (_ratingSlider.value > _newRating)
						_ratingSlider.value = _newRating;
				}
				
				yield return null;
			}
		}


		//останавливаем работающие корутины
		void StopRunningRoutine()
		{
			if (runningCoroutine != null)
			{
				StopCoroutine(runningCoroutine);
				runningCoroutine = null;				
			}				
		}

	#endregion

	}


	public enum ActiveMenu
	{
		MAIN_MENU, 
		SETTINGS, 
		GAME, 
		VICTORY_PANEL, 
		LOSS_PANEL, 
		CONFIRMATION_PANEL, 
		MULTIPLAYER_RESULT_PANEL,
		OPPONENT_PANEL_MULTIPLAYER
	}
}