using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Multiplayer;
using UnityEngine.SocialPlatforms;
using UnityEngine.Analytics;

namespace BrakeBricks
{	
	
	public class GPGS : MonoBehaviour, RealTimeMultiplayerListener  
	{

	#region Var

		public static GPGS Instance;

		public MultiplayerInfo MultiplayerInfo {get; private set;}	//настройки игры по мультиплееру
		public bool IsLogin {get { return Social.localUser.authenticated; }}
		public MultiplayerState State {get; private set;}	
		
		DataMultiplayerGame dataMultiplayer;  //данные для игры по мультиплееру	
		
		bool isPlayInvitation; //игра по приглашению
				
		bool isPlayerConfirmGame; //подтверждение игрока
		bool isOpponentConfirmGame; // /подтверждение соперника			
		
		float currentTime;

		int myIndexLevel = 0; //индекс уровня в мультиплеере (номер шаблона)
		int oppIndexLevel = 0; //такой же от соперника
		int indexLevel = -1; // конечный индекс текущего уровня (-1 по умолчанию)

		string opponentName; //имя соперника

		Image myAvatar;
		Image opponentAvatar; //аватар сопеника
		
		const string START_GAME = "Start";
		const string TIME_OUT = "TimeOut";		
		const string SCORE = "Score";
		const string WIN = "WIN";			
		
	#endregion


	#region Event

		public event Action AllParticipantsConfirmedEvent;
		public event Action AcceptFromInboxEvent; //игрок принял вызов поиграть из входящих приглашений
		public event Action DisconectEvent;
		public event Action<float, float> TimeUpdateEvent;		
		public event Action<int, int> UpdateOpponentScoreEvent;
		public event Action<int, int> UpdateMyScoreEvent;		
		public event Action<MultiplayerResult> GameResultEvent;
		public event Action<bool> InitMultiplayerEvent;
		public event Action LoginUpdateEvent; //событие входа и выхода из сервисов
		public event Action PlayByInvitationEvent; //игра по приглашению
				
	#endregion


	#region Init

		void Awake()
		{
			if (Instance == null) 
				Instance = this;
				
			dataMultiplayer = Resources.Load<DataMultiplayerGame>("Data/DataMultiplayerGame");
			MultiplayerInfo = new MultiplayerInfo();
			
			SetState(MultiplayerState.PREPARATION); //задаем состояние			

			InitializeServices(); //регестрация сервисов
		}


		void Start()
		{
			Subscriptions(); //подписка
			//Login();
		}


		//инициализация сервисов google
		void InitializeServices()
		{
			PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().Build();

			PlayGamesPlatform.InitializeInstance(config);
			PlayGamesPlatform.DebugLogEnabled = true;
			PlayGamesPlatform.Activate();											
		}	


		void Login()
		{
			//если уже вошел - пропускаем
			if (IsLogin)	
				return;
				
			Social.localUser.Authenticate((bool success) => 
			{
				if (success) 
				{
					LoginUpdateEvent?.Invoke();
					ManagerUI.ShowMsg("Login successful");
					Debug.Log("Login ON");
				}
				else
				{
					ManagerUI.ShowMsg("Login failed :(");
					Debug.Log("Login failure :(");
				}
			});			
		}


		void Subscriptions()
		{			
			GameManager.Instance.MultiplayerSetupEvent += InitMultiplayer;//подготовка мультиплеера
			GameManager.Instance.StartGameEvent += StartRaund; //старт игры
			GameManager.Instance.ScoreUpdateEvent += AddScoreMultiplayer;	
			GameManager.Instance.QuitGameEvent += QuitGame;	
			GameManager.Instance.LeaderboardAddResultEvent += UpdateLeaderbord; //обновление таб. лидеров
			AchievementManager.Instance.AchievementUpdateEvent += GetAchivement; //обновление достижения

			InputManager.Instance.PressButtonMainMenu += MultiplayerGameExit; //выход в меню
			InputManager.Instance.PressButtonAchivment += ShowAchievement;
			InputManager.Instance.PressButtonLeaderbord += ShowLeaderbord;
			InputManager.Instance.PressButtonRetryMultiplayer += PreparationMultiplayerGame; //перезапуск игры в MP
			InputManager.Instance.FriendsPanel_PressButtonInvitationsEvent += AcceptFromInbox;//просмотр приглашений от игроков
			
			InputManager.Instance.FriendsPanel_PressButtonLoginEvent += () => 
			{
				if (!IsLogin) 
					Login();
				else
					SingOut();
			};
		}


		void Unsubscriptions()
		{			
			GameManager.Instance.MultiplayerSetupEvent -= InitMultiplayer;
			GameManager.Instance.StartGameEvent -= StartRaund;
			GameManager.Instance.ScoreUpdateEvent -= AddScoreMultiplayer;	
			GameManager.Instance.QuitGameEvent -= QuitGame;		
			GameManager.Instance.LeaderboardAddResultEvent -= UpdateLeaderbord;
			AchievementManager.Instance.AchievementUpdateEvent -= GetAchivement;
			
			InputManager.Instance.PressButtonMainMenu -= MultiplayerGameExit;
			InputManager.Instance.PressButtonAchivment -= ShowAchievement;
			InputManager.Instance.PressButtonLeaderbord -= ShowLeaderbord;
			InputManager.Instance.PressButtonRetryMultiplayer -= PreparationMultiplayerGame;
		}


		void QuitGame()
		{
			Unsubscriptions(); // отписка
			SingOut();			
		}


		//выход и сервисов
		void SingOut()
		{
			PlayGamesPlatform.Instance.SignOut(); 
			LoginUpdateEvent?.Invoke();
		}

	#endregion

	
	#region Init Multiplayer

		void InitMultiplayer()
		{	
			if (State != MultiplayerState.PREPARATION)
			{
				ManagerUI.ShowMsg("Room already created!");
				return;
			}			

			//событие, что начата инициализация мультиплеера
			InitMultiplayerEvent?.Invoke(true);
				
			//если включён тест, то запускаем в тестовом режиме
			if (GameManager.Config.isTestMP)
			{
				TestLoadRoom();	//test	
			}
			else
			{
				try
				{
					AuthenticateMP();	
				}
				catch(Exception e)
				{
					AnalyticsEvent.Custom("Authenticate Multiplayer Exeption", new Dictionary<string, object>
					{
						{e.ToString(), e}
					});
				}
			}					
		}


		//авторизация
		void AuthenticateMP()
		{			
			//если уже вошел - создаём игру
			if (IsLogin)	
			{			
				CreateGame();
			}
			else //авторизуемся в сервисах
			{
				Social.localUser.Authenticate((bool success) => 
				{
					if (success)
					{				
						LoginUpdateEvent?.Invoke();	
						ManagerUI.ShowMsg("Authentication successeded");
						
						CreateGame();									
					}
					else
					{
						ManagerUI.ShowMsg("Authentication failed");	
						//событие завершения инициализации (прячем прогресс бар)
						InitMultiplayerEvent?.Invoke(false);				
					}
				});
			}			
		}


		//сбрасывает настройки игры перед её началом
		void ResetMultiplayerSettings()
		{
			//определяем на каком уровне будем играть
			SetLevelIndex(); 

			//score
			MultiplayerInfo.myScore = 0;
			MultiplayerInfo.opponentScore = 0;

			//game template
			MultiplayerInfo.maxScore = dataMultiplayer.multiplayerLevel[indexLevel].maxScore;
			MultiplayerInfo.map = dataMultiplayer.multiplayerLevel[indexLevel].map;		
			MultiplayerInfo.timer = dataMultiplayer.multiplayerLevel[indexLevel].time; 
						
			if (GameManager.Config.isTestMP)
			{
				TestParticipantInfo();
			}			
			else
			{
				//avatar image
				SetImageParticipant();

				//name
				MultiplayerInfo.playerName = (!string.IsNullOrWhiteSpace(Social.localUser.userName)) ? Social.localUser.userName : "First";				
				MultiplayerInfo.opponentName = (!string.IsNullOrWhiteSpace(opponentName)) ? opponentName : "Second";
				
			}
		}	


		//устанавливает изображения аватаров для участников игры
		void SetImageParticipant()
		{
			//моё изображение
			if (myAvatar != null)
			{
				MultiplayerInfo.opponentIcon  = myAvatar.sprite;
			}
			else
			{
				MultiplayerInfo.playerIcon = dataMultiplayer.icons[0];
			}

			//изображение соперника
			if (opponentAvatar != null)
			{
				MultiplayerInfo.opponentIcon  = opponentAvatar.sprite;
			}
			else
			{
				MultiplayerInfo.opponentIcon = dataMultiplayer.icons[1];
			}		
		}


		//устанавливает номер уровня
		void SetLevelIndex()
		{
			if (GameManager.Config.isTestMP)
			{
				//test
				TestSetLevel();
			}
			else
			{			
				//real game	
				//indexLevel = (myIndexLevel > oppIndexLevel) ? myIndexLevel : oppIndexLevel;

				indexLevel++;
				if (indexLevel > dataMultiplayer.multiplayerLevel.Length-1)
					indexLevel = 0;
			}			
		}
		
		
		//подгружает изображение игрока из сете
		IEnumerator GetMyAvatarImage()
		{
			Texture2D texture;
			while(Social.localUser.image == null)
			{
				yield return null;
			}

			texture = Social.localUser.image;
			myAvatar.sprite = Sprite.Create(texture, 
											new Rect(0f, 0f, texture.width, texture.height), 
											new Vector2(0f, 0f));
		}
		

	#endregion


	#region Multiplayer Update

		//устанавливает текущее состояние игры по мультиплееру
		void SetState(MultiplayerState _state)
		{
			State = _state;
		}


		void Update()
		{
			//выполняем действия в зависимости от состояния игры
			switch (State)
			{
				case MultiplayerState.WAIT:
					//если всё участники подтвердили участие, 
					//делаем рассылку о подключении	(GM обрабатывает)		
					CheckConfirmationParticipants();
				break;

				case MultiplayerState.PLAY:
					if (Time.time >= currentTime + 1f) //проверка раз в секунду 
					{
						currentTime = Time.time;				

						if (MultiplayerInfo.timer > 0)
						{
							var curTime = --MultiplayerInfo.timer;
							var maxTime = dataMultiplayer.multiplayerLevel[indexLevel].time;							
							
							//рассылка события о изменении времени (curTime, maxTime)
							TimeUpdateEvent?.Invoke(curTime, maxTime);					
						}
						else
						{						
							MultiplayerGameResult();													
						}
					}
				break;

				case MultiplayerState.PREPARATION:
					//игра остановлена
				break;
				
				case MultiplayerState.READY:
					//ира на старте, ждёт начала
				break;
			}
		}


		//отсылает сообщение об очках игрока для других участников
		void AddScoreMultiplayer(int score)
		{
			if (!isPlayerConfirmGame)
				return;

			MultiplayerInfo.myScore += score ;

			//событие обновления счёта
			UpdateMyScoreEvent?.Invoke(MultiplayerInfo.myScore, MultiplayerInfo.maxScore);

			if (!GameManager.Config.isTestMP) 
				SendMsg(SCORE, MultiplayerInfo.myScore.ToString(), false);
			
			CheckMyScore();					
		}


		//проверка счёта игрока, достиг ли цели
		void CheckMyScore()
		{
			//если я розбил всё блоки, посылаем сообщение о выиграше соперникам
			if (MultiplayerInfo.myScore >= MultiplayerInfo.maxScore)
			{
				MultiplayerGameResult();

				//отсылаем сообщение, если мы в реальной игре
				if (!GameManager.Config.isTestMP) 
					SendMsg(TIME_OUT, "", true);				
			}				
		}
		

	#endregion


	#region Multiplayer other

		//старт игры
		void StartRaund()
		{		
			//если игра не в стадии ожидания - выходим
			if (State != MultiplayerState.READY)
				return;
														
			currentTime = Time.time;
			SetState(MultiplayerState.PLAY);	

			//если игра с приглашённым другом - то рассылаем событие
			if (isPlayInvitation)
			{
				PlayByInvitationEvent?.Invoke();
				isPlayInvitation = false;
			}				
		}


		//проверяет все ли участники подтвердили участие в игре
		void CheckConfirmationParticipants()
		{			
			if (isPlayerConfirmGame && isOpponentConfirmGame)
			{				
				SetState(MultiplayerState.READY);					

				//настраиваем параметры перед игрой
				ResetMultiplayerSettings();	

				//событие, что инициализация мультиплеера завершилась => (false)
			    InitMultiplayerEvent?.Invoke(false);

				//ссобытие, что всё подтвердили участие
				AllParticipantsConfirmedEvent?.Invoke();															
			}
		}


		//подготовка к запуску уровня
		void PreparationMultiplayerGame()
		{	
			//если мы уже подтвердили участие - выходим
			if (State == MultiplayerState.READY)
				return;

			SetState(MultiplayerState.WAIT);	

			//игрок подтвердил участие
			PlayerConfirmStartGame(); 
			
			if (GameManager.Config.isTestMP) //test
			{			
				//имитируем подтверждение от соперника
				TestConfirmOpponent();					
			}					
			else //real time play
			{						 
				//посылаем аватар пользователя сопернику
				//SendMessageMyAvatar();				

				//выбираем случайно уровень
				var numLevel = UnityEngine.Random.Range(0, dataMultiplayer.multiplayerLevel.Length);

				//составляем сообщение
				var msg = "" + Social.localUser.userName + ":" + numLevel;
				
				SendMsg(START_GAME, msg, true);
			}			

			//событие начала инициализации (запускаем прогрессбар()
			InitMultiplayerEvent?.Invoke(true);						
		}


		/*
		//отсылает сообщение аватара пользователя
		void SendMessageMyAvatar()
		{
			if (myAvatar == null)
				return; 

			bool reliability = true; //надёжное сообщение
			
			//получаем аватар пользователя в байтах
			byte[]  myAvatarInBytes = myAvatar.sprite.texture.GetRawTextureData();			
			
			PlayGamesPlatform.Instance.RealTime.SendMessageToAll(reliability, myAvatarInBytes);			
		}
		*/		


		//подтверждение начала игры игроком
		void PlayerConfirmStartGame()
		{
			isPlayerConfirmGame = true;
		}


		//подтверждение начала игры соперником
		void OpponentConfirmStartGame ()
		{
			isOpponentConfirmGame = true;
		}


		//подводит итог игры
		void MultiplayerGameResult()
		{
			if (State == MultiplayerState.WAIT)
				return;

			SetState(MultiplayerState.WAIT);

			//сбрасываем всё флаги подтверждений
			ResetConfirm();					

			if (!GameManager.Config.isTestMP) 
				SendMsg(TIME_OUT, "", true);	

			//оповещает о результате игры
			SendGameResult();
		}


		//оповещает о результате игры
		void SendGameResult()
		{
			MultiplayerResult gameResult;

			//определяем результят игры
			if (MultiplayerInfo.myScore > MultiplayerInfo.opponentScore)
			{
				gameResult = MultiplayerResult.WIN; //победа
				AudioManager.Instance.Play(StaticPrm.SOUND_WIN_GAME);
			}				
			else if (MultiplayerInfo.myScore < MultiplayerInfo.opponentScore)
			{
				gameResult = MultiplayerResult.LOSE; //проигрыш
				AudioManager.Instance.Play(StaticPrm.SOUND_LOSS_GAME);
			}				
			else
			{
				gameResult = MultiplayerResult.DRAW; //ничья
				AudioManager.Instance.Play(StaticPrm.SOUND_DRAW_GAME);
			}				

			//отсылаем сообщение с результатом игры
			GameResultEvent?.Invoke(gameResult);
		}


		//сбрасываем всё подтверждения от участников
		void ResetConfirm()
		{
			isPlayerConfirmGame = false;
			isOpponentConfirmGame = false;				
		}


		//сброс индекс запускаемого уровня поумолчанию
		void LevelsResetValue()
		{
			indexLevel = -1;
		}


		//выход из игры по мультиплееру
		public void MultiplayerGameExit()
		{
			//если ира не запущена - выходим
			if (State == MultiplayerState.PREPARATION)
				return;

			//событие, что инициализация мультиплеера завершилась
			//на случай, если подключение не удалось
			InitMultiplayerEvent?.Invoke(false);

			SetState(MultiplayerState.PREPARATION);	

			//если запущенно в режиме теста в редакторе, то вызываем метод выхода напрямую
			if (GameManager.Config.isTestMP)
				OnLeftRoom();
			else
				PlayGamesPlatform.Instance.RealTime.LeaveRoom();
		}

	#endregion


	#region MP Room

		//создаёт игру в нужном режиме
		void CreateGame()
		{
			if (GameManager.Instance.MP_GAME_MODE == MpGameMode.QUIK_GAME)	
			{
				isPlayInvitation = false;
				CreateQuikGame();
			}				
			else if (GameManager.Instance.MP_GAME_MODE == MpGameMode.IVITATIONS_GAME)
			{
				isPlayInvitation = true;
				CreateWithInvitationScreen();
			}
				
		}


		//создание быстрой игры
		void CreateQuikGame()
		{
			const int minOpponents = 1;
			const int maxOpponents = 1;
			const int gameVariant = 0;
			PlayGamesPlatform.Instance.RealTime.CreateQuickGame(minOpponents, maxOpponents, gameVariant, this);		
		}


		//создать игру с возможностью пригласить друзей поиграть
		void CreateWithInvitationScreen()
		{
			const int minOpponents = 1;
			const int maxOpponents = 1;
   			const int gameVariant = 0;
   			PlayGamesPlatform.Instance.RealTime.CreateWithInvitationScreen(minOpponents, maxOpponents, gameVariant, this);
		}


		//открыть панель приглашений поиграть в MP
		void AcceptFromInbox()
		{
			AcceptFromInboxEvent?.Invoke();
			PlayGamesPlatform.Instance.RealTime.AcceptFromInbox(this);
		}


		public static void AcceptInvitation(string invitationId)
        {
        	var sInstance = new GPGS();
            PlayGamesPlatform.Instance.RealTime.AcceptInvitation(invitationId, sInstance);
        }


		//вызывается при выходе из комнаты
        public void OnLeftRoom()
        {			
			ManagerUI.ShowMsg("Exit game room");	

			//убираем панель загрузки (если она была)
			InitMultiplayerEvent?.Invoke(false);		

			//отсылаем событие об отключении от комнаты
            DisconectEvent?.Invoke();
        }


		//вызывается при отклонении опонентом приглашения поиграть
        public void OnParticipantLeft(Participant participant)
        {
            ManagerUI.ShowMsg("Your invitation has been declined");
			MultiplayerGameExit();
        }


		//вызывается при подключении нового участника
        public void OnPeersConnected(string[] participantIds)
        {
            ManagerUI.ShowMsg("New player connected");
        }


		//вызывается при отключении одного из участников
        public void OnPeersDisconnected(string[] participantIds)
        {
			ManagerUI.ShowMsg("Opponent left the room");
            MultiplayerGameExit();
        }


		//загрузка комнаты
		public void OnRoomSetupProgress(float percent)
        {			
			PlayGamesPlatform.Instance.RealTime.ShowWaitingRoomUI();
        }


		//подключение к комнате
        public void OnRoomConnected(bool success)
        {
			if (success)
			{					
				LevelsResetValue(); //сбрасываем индес запускаемого уровня на начальный
				PreparationMultiplayerGame();
			}
			else
			{
				ManagerUI.ShowMsg("Connection to the room failed");
				MultiplayerGameExit();
				InitMultiplayerEvent?.Invoke(false); //отключаем прогрессбар
			}
        }


		//составление и отправка сообщения
		void SendMsg(string header, string msg, bool reliability = false)
		{
			string data = header + ":" + msg;
			byte[] bytes = System.Text.ASCIIEncoding.Default.GetBytes(data);
			PlayGamesPlatform.Instance.RealTime.SendMessageToAll(reliability, bytes);
		}	


		//обрабатывает сообщения от участников игры
        public void OnRealTimeMessageReceived(bool isReliable, string senderId, byte[] data)
        {
			//если сообщение от когото из противников
			if (!PlayGamesPlatform.Instance.RealTime.GetSelf().ParticipantId.Equals(senderId))
			{
				/* 
				try
				{
					Image image = null;
					image.sprite.texture.LoadRawTextureData(data);

					if (image != null)
					{
						opponentAvatar = image;
						return;
					}
				}
				catch(Exception e)
				{
					ManagerUI.ShowMsg("Ex: " + e.Message);
					return;
				}	
				*/

				string rawData = System.Text.ASCIIEncoding.Default.GetString(data);
				string[] sliced = rawData.Split(new string[] {":"}, System.StringSplitOptions.RemoveEmptyEntries);

				//Очки противника
				if (sliced[0].Equals(SCORE))
				{
					int oppScore = Int32.Parse(sliced[1]);
					MultiplayerInfo.opponentScore = oppScore;

					if (UpdateOpponentScoreEvent != null) 
						UpdateOpponentScoreEvent(oppScore, MultiplayerInfo.maxScore);
				}

				//сообщение об окончании времени игры
				if (sliced[0].Equals(TIME_OUT))
				{
					//Подводим итог игры
					MultiplayerGameResult();
				}	

				//сообщение, что соперник хочет начать игру			
				if (sliced[0].Equals(START_GAME))
				{
					opponentName = sliced[1]; // имя соперника
					oppIndexLevel = int.Parse(sliced[2]); //выбраный им уровень					
					OpponentConfirmStartGame ();
				}	
			}
        }
        

	#endregion


	#region Test

		//тестовая загрузка кумнаты для игры по сети
		void TestLoadRoom()
		{			
			ManagerUI.ShowMsg("Test multiplayer in editor");
			StartCoroutine( TestConnectRoom(3f) );
		}


		//имитация подключения к комнате
		IEnumerator TestConnectRoom(float time)
		{	
			yield return new WaitForSeconds(time);
			
			PreparationMultiplayerGame();		
			
			//StartCoroutine( TestDisconectRoom() );			
		}


		//test
		IEnumerator TestDisconectRoom()
		{
			yield return new WaitForSeconds(3f);
			OnLeftRoom();
		}


		//имитация подтверждения от соперника
		void TestConfirmOpponent()
		{
			isOpponentConfirmGame = true;			
		}


		//тестовое заполнение информации об участниках
		void TestParticipantInfo()
		{
			//score
			MultiplayerInfo.playerIcon = dataMultiplayer.icons[0];
			MultiplayerInfo.opponentIcon = dataMultiplayer.icons[1];
			//name
			MultiplayerInfo.playerName = "First";
			MultiplayerInfo.opponentName = "Second";
		}

		//тестовый выбор уровня
		void TestSetLevel()
		{
			indexLevel = UnityEngine.Random.Range(0, dataMultiplayer.multiplayerLevel.Length);
		}

	#endregion
	

	#region Achievement

		void ShowAchievement()
		{
			//ManagerUI.ShowMsg("Show Acivement");
			AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);

			try
			{
				Login();
				Social.ShowAchievementsUI();
			}
			catch(Exception e)
			{
				AnalyticsEvent.Custom("Show Achievement Exeption", new Dictionary<string, object>
				{
					{e.ToString(), e}
				});
			}			
		}		


		void GetAchivement(string id)
		{
			Social.ReportProgress(id, 100d, (bool success) => 
			{
				if (success) Debug.Log("Get achivment: " + id);	
				else Debug.Log("not added acivement :(");				
			});
		}


	#endregion


	#region Lederbord

		void ShowLeaderbord()
		{
			AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
			//ManagerUI.ShowMsg("Show Leaderbord");	
				
			try
			{
				Login();
				Social.ShowLeaderboardUI();
			}
			catch(Exception e)
			{
				AnalyticsEvent.Custom("Show Leaderbord Exeption", new Dictionary<string, object>
				{
					{e.ToString(), e}
				});
			}						
		}


		void UpdateLeaderbord()
		{
			
			var score = GameManager.CurrentGame.StatisticGame.NumWinsMP;
			Social.ReportScore(score, GPGSIds.leaderboard_multiplayer_champion, (bool success) => 
			{
				if (success) Debug.Log("Score added in leaderboard!!!");
				else Debug.Log("not added leaderboard :((");	
			});
			
		}

	#endregion
		
	}

	public enum MultiplayerResult
	{
		WIN, LOSE, DRAW
	}


	public enum MultiplayerState
	{
		WAIT, PLAY, PREPARATION, READY
	}	

	public enum MpGameMode
	{
		QUIK_GAME, IVITATIONS_GAME
	}
	
}