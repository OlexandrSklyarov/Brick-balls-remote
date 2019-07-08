using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

namespace BrakeBricks
{    
    public class AdMobManager : MonoBehaviour
    {

        enum RewardType
        {
            ACTION_1, 
            ACTION_2, 
            MONEY, 
            BALL, 
            CONTINUE_GAME
        }


    #region Var

        public static AdMobManager Instance {get; private set;}

        //загружена ли видеореклама
        public bool IsVideoAdLoad 
        {
            get 
            {
                if (rewardBasedVideo == null) return false;
                return rewardBasedVideo.IsLoaded();
            } 
        }

    
        GameManager gameMng;
        InputManager inputMng;
        ManagerUI uiMng;

        RewardType rewardType;

        //приватные свойства для хранения инфы о мяче который идет в награду за просмотр
        private Action UnlockBall {get; set;}
        private int BallSpriteIndex {get; set;}
        
        const string APP_ID= "ca-app-pub-7761980327324016~2037359147";
        
        const string MAIN_MENU_BANNER = "ca-app-pub-7761980327324016/8902150397";
        //const string NEXT_LEVEL_ITERSTITIAL = "ca-app-pub-7761980327324016/4709476710";   
        const string LOSE_GAME_ITERSTITIAL = "ca-app-pub-7761980327324016/1962047966";
        const string REWARD_VIEW = "ca-app-pub-7761980327324016/4214652699";
        
        /*
        //тестовые константы 
        const string MAIN_MENU_BANNER = "ca-app-pub-3940256099942544/6300978111";
        const string LOSE_GAME_ITERSTITIAL = "ca-app-pub-3940256099942544/1033173712";
        const string REWARD_VIEW = "ca-app-pub-3940256099942544/5224354917";
        */
        const string TEST_DEVICE_ID = "33BE2250B43518CCDA7DE426D04EE231";
        
        BannerView menuBanner; // баннер в меню
        InterstitialAd interstitialAd; //полноэкранная реклама
        RewardBasedVideoAd rewardBasedVideo; //видеореклама


    #endregion  


    #region  Events

        public event Action<int> RewardToWatchEvent; //награда за просмотр
        public event Action<GameActionType, int> RewardToActionEvent; //добавить Action за просмотр
        public event Action RewardContinueGameEvent; //наградить continue game
        public event Action<int> RewardBallEvent; //награда мячом

    #endregion


    #region Init
        
        void Awake()
        {
            if (Instance == null)
                Instance = this;            
        }


        void Start()
        {    
            MobileAds.Initialize(APP_ID);

            //получаем всё менеджеры
            gameMng = GameManager.Instance;
            inputMng = InputManager.Instance;
            uiMng = ManagerUI.Instance;

            //подписка на события
            Subscribe_Banner();
            Subscribe_Interstitial();    
            Subscribe_RewaedVideo();   

            //при выходе из игры уничножаем рекламу
            gameMng.QuitGameEvent += () => 
            {
                if (menuBanner != null) menuBanner.Destroy();
                if (interstitialAd != null) interstitialAd.Destroy();
            };
            
        }     

        
        //подписка на события для баннера
        void Subscribe_Banner()
        {   
            MenuBanner_Create(); //создаём
            
            //прячем при входе в маг
            inputMng.PressButtonShopOpen += () => 
            { 
                MenuBanner_Hide(); 
            };

            //при закрытии магазина - загружаем
            inputMng.PressButtonShopClose += () => 
            { 
                //если игра создана, то банер не показывать
                if (!gameMng.IsGameCreate)
                    MenuBanner_Load(); 
            };

            //при включении главного меню - загружаем
            uiMng.SetActiveMenuEvent += (ActiveMenu m) => 
            { 
                //это главное меню?
                if (m == ActiveMenu.MAIN_MENU)
                    MenuBanner_Load();            
            };     

            //подписываемся на событие загрузки банера
            menuBanner.OnAdLoaded += (object sender, EventArgs args) => 
            { 
                if (!gameMng.IsGameCreate)
                {
                    menuBanner.Show(); 
                }                    
                else
                {
                    MenuBanner_Hide();
                }
            };               
        }   


        //подписка на события для межстраничной рекламы
        void Subscribe_Interstitial()
        {           
            InterstitialAd_Create(); //создаём 

            //при проигрыше - загружаем межстраничную рекламу 
            gameMng.GameLoseEvent += () => 
            { 
                InterstitialAd_Load();  
            };           

            //показывает межстраничную рекламу при загрузке
            interstitialAd.OnAdLoaded += (object sender, EventArgs args) =>
            {
                if (!gameMng.IsGameCreate)
                {
                    interstitialAd.Show();  
                } 
            };
        }


        //подписка на события для видеорекламы
        void Subscribe_RewaedVideo()
        {
            //получаем ссылку на видеорекламу
            rewardBasedVideo = RewardBasedVideoAd.Instance;

            //загружаем видеорекламу
            RewardBasedVideo_Load(REWARD_VIEW);

            //когда видеореклама закончилась - наградить игрока
            rewardBasedVideo.OnAdRewarded += RewardToWatch;
            
            //реакция на нажатия кнопки просмотра видеорекламы
            inputMng.PressButtonWatchVideoAdEvent += () => 
            { 
                if (IsVideoAdLoad)
                {
                    RewardBasedVideo_Show(RewardType.MONEY);
                }      
                else
                {
                    RewardBasedVideo_Load(REWARD_VIEW);
                }          
            };

            //реакция на нажатия кнопки Action_1 за просмотра видеорекламы 
            inputMng.PressButton_Action1_Event += (ActionButton actButton) => 
            { 
                if (actButton != ActionButton.SHOW_AD)
                    return;

                if (IsVideoAdLoad)
                {
                    RewardBasedVideo_Show(RewardType.ACTION_1);
                }  
                else
                {
                    RewardBasedVideo_Load(REWARD_VIEW);
                }    

            };

            //реакция на нажатия кнопки Action_2 за просмотра видеорекламы 
            inputMng.PressButton_Action2_Event += (ActionButton actButton) => 
            { 
                 if (actButton != ActionButton.SHOW_AD)
                    return;
                    
                if (IsVideoAdLoad)
                {
                    RewardBasedVideo_Show(RewardType.ACTION_2);
                }                
                else
                {
                    RewardBasedVideo_Load(REWARD_VIEW);
                } 
            };

            //наградить continue game за просмотр рекламы
            inputMng.ExLifePanel_PressButtonVideoAdContinueEvent += () =>
            {
                if (IsVideoAdLoad)
                {
                    RewardBasedVideo_Show(RewardType.CONTINUE_GAME);
                } 
                else
                {
                    RewardBasedVideo_Load(REWARD_VIEW);
                } 
            };

            //подгружаем видео,при открытии магазина
            inputMng.PressButtonShopOpen += () => 
            { 
                if (!IsVideoAdLoad) RewardBasedVideo_Load(REWARD_VIEW); 
            };

            //подгружаем видео,при открытии главного меню
            inputMng.PressButtonMainMenu += () => 
            { 
                if (!IsVideoAdLoad) RewardBasedVideo_Load(REWARD_VIEW); 
            };

            //подгружаем видео,при старте игры
            gameMng.StartGameEvent += () => 
            { 
                if (!IsVideoAdLoad) RewardBasedVideo_Load(REWARD_VIEW); 
            };
        }


    #endregion


    #region Menu banner 

        //отображает банер
        void MenuBanner_Load()
        {
            MenuBanner_Create();                                    

            AdRequest request = new AdRequest.Builder().Build();
            //AdRequest request = GetTestRequest();
            menuBanner.LoadAd(request);            
        }


        //если не null -  прячем баннер
        void MenuBanner_Hide()
        {            
            menuBanner?.Hide();            
        }


        //создаёт баннер в меню, если до этого его небыло
        void MenuBanner_Create()
        {
            if (menuBanner == null)
            {
                menuBanner = new BannerView(MAIN_MENU_BANNER, AdSize.Banner, AdPosition.Top);
            } 
        }           


    #endregion


    #region Interstitial Adverstising     

        //полноэкранная реклама
        void InterstitialAd_Load()
        {
            InterstitialAd_Create();
            
            AdRequest request = new AdRequest.Builder().Build();
            //AdRequest request = GetTestRequest();
            interstitialAd.LoadAd(request);            
        }   


        //создает межстраничную рекламу
        void InterstitialAd_Create()
        {
            if (interstitialAd == null)
            {
                interstitialAd = new InterstitialAd(LOSE_GAME_ITERSTITIAL);
            }
        }

    #endregion


    #region Reward adverstising

        //показывает рекламу за вознаграждение
        void RewardBasedVideo_Load(string id)
        {  
            AdRequest request = new AdRequest.Builder().Build();
            //AdRequest request = GetTestRequest();
            rewardBasedVideo.LoadAd(request, id);            
        } 


        //показ видеорекламы
        void RewardBasedVideo_Show(RewardType rewType)
        {
            //фиксируем тип нарады
            rewardType = rewType; 
            rewardBasedVideo.Show();
        }


        //награда игроку за просмотр 
        void RewardToWatch(object sender, EventArgs args)
        {
            switch(rewardType)
            {
                case RewardType.MONEY:
                    RewardToWatchEvent?.Invoke(GameManager.Config.videoReward); 
                break;

                case RewardType.ACTION_1:
                    RewardToActionEvent?.Invoke(GameActionType.ICE_FORCE, 1);
                break;

                case RewardType.ACTION_2:
                    RewardToActionEvent?.Invoke(GameActionType.TIME_BOMB, 1);
                break;

                case RewardType.BALL:
                    UnlockBall?.Invoke();
                    RewardBallEvent?.Invoke(BallSpriteIndex);
                break;                

                case RewardType.CONTINUE_GAME:
                    RewardContinueGameEvent?.Invoke();
                break;
            }              

            //загружаем новую рекламу
            RewardBasedVideo_Load(REWARD_VIEW);                    
        }   


        //запускает показ рекламы, после которого награждает мячом
        public void RewardBallForViewingAd(int spriteIndex, bool isBuyed, Action setBuyStatus)
        {
            UnlockBall = setBuyStatus;
            BallSpriteIndex = spriteIndex;
            
            //если купили уже - сразу отсылаем событие, иначе смотрим видео
            if (isBuyed) 
            {
                UnlockBall?.Invoke();
                RewardBallEvent?.Invoke(BallSpriteIndex);
            }
            else 
            {
                if (IsVideoAdLoad)
                {
                    RewardBasedVideo_Show(RewardType.BALL);
                } 
            }
        }     
                 

    #endregion


    #region Test

        AdRequest GetTestRequest()
        {
            return new AdRequest.Builder().
                AddTestDevice(AdRequest.TestDeviceSimulator).
                AddTestDevice(TEST_DEVICE_ID).Build();
        }

    #endregion
        
    }
}