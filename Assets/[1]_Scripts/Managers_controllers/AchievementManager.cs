using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
    public class AchievementManager : MonoBehaviour
    {

    #region Var

        public static AchievementManager Instance;

    #endregion


    #region Event

        public event Action<string> AchievementUpdateEvent; //обновление достижений
       
    #endregion

     
    #region Init

        void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        void Start()
        {        
            
            Subsription();
        }


        void Subsription()
        {
            //обновление статистики
            var gm = GameManager.Instance;
            gm.StatisticUpdateEvent += SetAchievement;

            //событие расхода денег
            var store = gm.StoreManager;
            store.SpendMoneyEvent += SpendMoney; 

            //событие нажатия на кнопку поделиться
            //FBManager.Instance.ShareFacebookEvent += ShareFacebook;
            ShareImage.Instance.ShareGameEvent += ShareFacebook;

        }

    #endregion


    #region Handling achievements

        //проверяет получение достижения
        void SetAchievement(AchievementType achivType)
        {
            switch(achivType)
            {
                case AchievementType.VICTORY_ONE_MOVE:
                VictoryOneMove();
                break;
                
                case AchievementType.WINS_MP:
                WinsMP();
                break;

                case AchievementType.COMPLITED_LEVELS:
                CompletetedLevels();
                break;

                case AchievementType.MAX_RATING:
                MaxRating();
                break;

                case AchievementType.RUN_MP:
                RunMultiplayerGame();
                break;
                
                case AchievementType.RUN_CLASSIC:
                RunClassicGame();
                break;

                case AchievementType.PLAYING_DAY_SERIES:
                PlayingDaySeries();
                break;

                case AchievementType.USING_ICE_FORCE:
                UsingIceForce();
                break;

                case AchievementType.USING_BOMB_FORCE:
                UsingBombForce();
                break;

                case AchievementType.COLLECT_BALL_ONE_GAME:
                CollectBallOneGame();
                break;

                case AchievementType.COLLECT_BALL_ALL_TIME:
                CollectBallAllTime();
                break;

                case AchievementType.INVITE_FRIENDS:
                InviteFriends();
                break;                
            }
        }

    #endregion


    #region Achievement update

        //обрабатывает совершение покупки за реальные деньги ($)
		void SpendMoney(double costProduct)
		{			
			switch(costProduct)
			{
				case 5:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_buy_5_item_from_shop);
				break;
                case 10:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_buy_10_item_from_shop);
				break;
                case 100:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_buy_100_item_from_shop);
				break;
			}
		}


        //сколько раз прошёл уровнень за один ход
        void VictoryOneMove()
        {
            var value = GameManager.CurrentGame.StatisticGame.VictoryOneMove;

            //проверка достижений прохождения уровня с одного хода
			switch(value)
			{
				case 1:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_complete_game_with_1_move_1_time);
				break;
                case 10:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_complete_game_with_1_move_10_times);
				break;
                case 100:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_complete_game_with_1_move_100_times);
				break;
			}
        }


        //сколь пробед в MP
        void WinsMP()
        {
            var value = GameManager.CurrentGame.StatisticGame.NumWinsMP;
            
            switch(value)
			{
				case 10:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_win_mp_10_games);
				break;
                case 100:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_win_mp_100_games);
				break;
                case 1000:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_win_mp_1000_games);
				break;
			}
        }


        //сколько пройденых уровней
        void CompletetedLevels()
        {
            var value = GameManager.CurrentGame.StatisticGame.NumCompletedLevels;

            switch(value)
			{
				case 100:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_complete_100_levels);
				break;
                case 250:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_complete_250_levels);
				break;
                case 500:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_complete_500_levels);
				break;
                case 1000:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_complete_1000_lvls);
				break;
                case 3000:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_complete_3000_lvls);
				break;
			}
        }


        //получение максимального рейтинга (3 звезды)
        void MaxRating()
        {
            var value = GameManager.CurrentGame.StatisticGame.MaxRatingCount;

            switch(value)
			{
				case 10:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_get_3_____in_10_lvls);
				break;
                case 50:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_get_3_____in_50_lvls);
				break;
                case 100:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_get_3_____in_100_lvls);
				break;
                case 1000:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_get_3_____in_1000_lvls);
				break;
                case 2000:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_get_3_____in_2000_lvls);
				break;
                case 3000:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_get_3_____in_3000_lvls);
				break;
			}
        }


        //запус игры в MP
        void RunMultiplayerGame()
        {
            var value = GameManager.CurrentGame.StatisticGame.NumMultiplayerLaunches;

            switch(value)
			{
				case 10: 
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_play_multiplayer_10_times);
				break;
                case 100: 
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_play_multiplayer_100_times);
				break;
                case 1000: 
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_play_multiplayer_1000_times);
				break;
			}
        }


        //запуск слассического режима игры
        void RunClassicGame()
        {
            var value = GameManager.CurrentGame.StatisticGame.NumClassicLaunches;

            switch(value)
			{
				case 10:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_run_classic_game_10_times);
				break;
                case 100:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_run_classic_game_100_times);
				break;
                case 1000:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_run_classic_game_1000_times);
				break;
			}
        }


        //проверяет серию из количества дней, когда игрок запускал иру подряд (день за днем)
        void PlayingDaySeries()
        {
            var value = GameManager.CurrentGame.StatisticGame.DailyGameSeries;

            switch(value)
			{
				case 3:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_play_game_continue_3_days);
				break;
                case 7:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_play_game_continue_7_days);
				break;
                case 30:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_play_game_continue_30_days);
				break;
                case 60:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_play_game_continue_60_days);
				break;
			}
        }


        //Использование ICE_FORCE
        void UsingIceForce()
        {
            var value = GameManager.CurrentGame.StatisticGame.NumUsedIcePower;

            //проверка достижений IceForce
			switch(value)
			{
				case 10:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_use_ice_power_10_times);
				break;
                case 100:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_use_ice_power_10_times);
				break;
                case 500:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_use_ice_power_500_times);
				break;
                case 1000:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_use_ice_power_1000_times);
				break;
			}
        }


        //Использование BOMB_FORCE
        void UsingBombForce()
        {
            var value = GameManager.CurrentGame.StatisticGame.NumUsedBombPower;

            //проверка достижений TimeBomb
			switch(value)
			{
				case 10:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_use_bomb_power_10_times);
				break;
                case 100:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_use_bomb_power_100_times);
				break;
                case 5000:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_use_bomb_power_500_times);
				break;
                case 1000:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_use_bomb_power_1000_times);
				break;
			}
        }


        //проверка количетва собраных мячей за одну игру
        void CollectBallOneGame()
        {
            var value = GameManager.CurrentGame.StatisticGame.NumExtraBalls;

            //проверка на достижение
			switch(value)
			{
				case 10:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_collect_10_balls_in_gameplay);
				break;
                case 100:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_collect_100_balls_in_gameplay);
				break;
                case 1000:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_collect_1000_balls_in_gameplay);
				break;
                case 10000:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_collect_10k_balls_in_gameplay);
				break;
			}
        } 


        //проверка количетва собраных мячей за всё время
        void CollectBallAllTime()
        {
            var value = GameManager.Instance.ExBallForGame;

             //проверка на достижение
			switch(value)
			{
				case 10:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_collect_10_balls);
				break;
                case 30:
					AchievementUpdateEvent?.Invoke(GPGSIds.achievement_collect_30_balls);
				break;
			}
        } 


        //проверка количества приглашений в MP друзей
        void InviteFriends()
        {
            var value = GameManager.CurrentGame.StatisticGame.NumInvitedFriends;

            switch(value)
            {
                case 3:
                    AchievementUpdateEvent?.Invoke(GPGSIds.achievement_invite_3_friends);
                break;
                case 10:
                    AchievementUpdateEvent?.Invoke(GPGSIds.achievement_invite_10_friends);
                break;
                case 100:
                    AchievementUpdateEvent?.Invoke(GPGSIds.achievement_invite_100_friends);
                break;
                case 1000:
                    AchievementUpdateEvent?.Invoke(GPGSIds.achievement_invite_1000_friends);
                break;
            }
        }


        //поделиться результатом на facebook
        void ShareFacebook()
        {
            AchievementUpdateEvent?.Invoke(GPGSIds.achievement_share_best_score_on_facebook);
        }

    #endregion
    
    }
    
    public enum AchievementType
    {
        VICTORY_ONE_MOVE,
        WINS_MP,
        COMPLITED_LEVELS,
        MAX_RATING,
        RUN_MP,
        RUN_CLASSIC,
        PLAYING_DAY_SERIES,
        USING_ICE_FORCE,
        USING_BOMB_FORCE,
        COLLECT_BALL_ONE_GAME,
        COLLECT_BALL_ALL_TIME,
        INVITE_FRIENDS,
        SHARE_FACEBOOK
    }

    
}