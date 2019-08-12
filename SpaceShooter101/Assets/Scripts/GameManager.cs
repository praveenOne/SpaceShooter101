﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace praveen.One
{
    public class GameManager : MonoBehaviour
    {
        #region singleton stuff
        private static GameManager m_Instance;

        public static GameManager Instance
        {
            get { return m_Instance; }
        }
        #endregion


        #region MetaData
        string m_CoinsKey       = "SHOOTER.COINS";
        string m_HighScoreKey   = "SHOOTER.HIGHSCORE";
        string m_AmorKey        = "SHOOTER.AMOR";
        string m_ShieldKey      = "SHOOTER.SHIELD";
        #endregion

        #region PrivateFields
        int m_PlayerHp;
        int m_Level;
        int m_Coins;
        float m_ShieldTime;
        float m_ShieldActTime;
        int m_Score;
        int m_EnemiesKilled;
        int m_HighScore;
        bool m_IsNewRecord;
        int m_ShieldDuration;
        ShooterAmor m_ShooterAmor;
        #endregion

        private void Awake()
        {
            if (m_Instance != null && m_Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                m_Instance = this;
            }
            DontDestroyOnLoad(this.gameObject);
            m_PlayerHp = 3;
            PlayerPrefs.DeleteKey(m_AmorKey);
            GetSavedData();
        }

        void GetSavedData()
        {
            m_HighScore      = PlayerPrefs.GetInt(m_HighScoreKey, 0);
            m_Coins          = PlayerPrefs.GetInt(m_CoinsKey, 0);
            m_ShieldDuration = PlayerPrefs.GetInt(m_ShieldKey, 3);

            string amorData = PlayerPrefs.GetString(m_AmorKey, null);

            if (string.IsNullOrEmpty(amorData))
            {
                ShooterAmor amor = new ShooterAmor(1, 1, 1);
                m_ShooterAmor = amor;
            }
            else
            {
                m_ShooterAmor = JsonUtility.FromJson<ShooterAmor>(amorData);
            }

        }

        public void AddScore(int score)
        {
            m_Score += score;
            HudController.Instance.SetScore(m_Score);
        }

        public void UpdateEnemiesKilled()
        {
            m_EnemiesKilled += 1;
            HudController.Instance.EnemiesKilled(m_EnemiesKilled);
        }

        public void OnPlayerHit()
        {
            m_PlayerHp -= 1;
            HudController.Instance.SetPlayerHelth(m_PlayerHp);

            if(m_PlayerHp < 1)
            {
                GameOver();
            }
        }

        /// <summary>
        /// Returns Level
        /// </summary>
        /// <returns></returns>
        public int GetLevel()
        {
            return m_Level;
        }

        /// <summary>
        /// Returns world position of Upper Left Screen boundry
        /// </summary>
        /// <returns></returns>
        public Vector3 GetUpperLeftScreenBoundry()
        {
            return Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0));
        }

        /// <summary>
        /// Returns world position of Upper Right Screen boundry
        /// </summary>
        /// <returns></returns>
        public Vector3 GetUpperRightScreenBoundry()
        {
            return Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
        }

        /// <summary>
        /// Returns Lower Screen Y
        /// </summary>
        /// <returns></returns>
        public float GetLowerScreenY()
        {
            return Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        }

        /// <summary>
        /// Returns Upper Screen Y
        /// </summary>
        /// <returns></returns>
        public float GetUpperScreenY()
        {
            return Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0)).y;
        }

        public float GetShieldTime()
        {
            return m_ShieldTime;
        }

        public float GetShieldActTime()
        {
            return m_ShieldActTime;
        }

        public void AddCoin()
        {
            m_Coins++;
            HudController.Instance.SetCoins(m_Coins);
        }

        public int GetHighScore()
        {
            return m_HighScore;
        }

        private void GameOver()
        {
           m_IsNewRecord = false;

           if(m_Score > m_HighScore)
           {
                m_IsNewRecord = true;
                m_HighScore = m_Score;
           }

            SaveData();

            SceneManager.LoadScene("GameOver", LoadSceneMode.Single);

        }

        public void LoadShopScene()
        {
            SceneManager.LoadScene("ShopMenu", LoadSceneMode.Single);
        }

        public void NewGame()
        {
            m_PlayerHp = 3;
            m_Score = 0;
            HudController.Instance.SetCoins(m_Coins);
            HudController.Instance.SetScore(m_Score);
            HudController.Instance.EnemiesKilled(m_EnemiesKilled);
        }

        public GameOverUI GetGameOverUI()
        {
            return new GameOverUI(m_Score, m_HighScore, m_Coins, m_IsNewRecord);
        }

        void SaveData()
        {
            string amorData = JsonUtility.ToJson(m_ShooterAmor);
            PlayerPrefs.SetString(m_AmorKey, amorData);
            PlayerPrefs.SetInt(m_HighScoreKey, m_HighScore);
            PlayerPrefs.SetInt(m_CoinsKey, m_Coins);
            PlayerPrefs.SetInt(m_ShieldKey, m_ShieldDuration);
            PlayerPrefs.Save();
        }

        public ShooterAmor GetAmorData()
        {
            return m_ShooterAmor;
        }

        public int GetShieldDuration()
        {
            return m_ShieldDuration;
        }

        public int GetCoinCount()
        {
            return m_Coins;
        }

        public void UpgradeGun(System.Action<bool> callback)
        {

            int nextGunLvl = Shop.GetNextGunLevel(m_ShooterAmor.GunLevel);

            if (nextGunLvl == -1)
                return;

            int upgradeCost = Shop.GetGunUpgradeCost(nextGunLvl) ;
            if (m_Coins >= upgradeCost)
            {
                m_Coins -= upgradeCost;
                m_ShooterAmor.GunLevel = nextGunLvl;
                SaveData();
                callback.Invoke(true);
            }
            
        }

        public void UpgradeMagazine(System.Action<bool> callback)
        {
            int nextMagLevel = Shop.GetNextMissileMagLvl(m_ShooterAmor.MissileMagazineLvl);

            if (nextMagLevel == -1)
                return;

            int upgradeCost = Shop.GetMissileMagUpgrdCost(nextMagLevel);
            if (m_Coins >= upgradeCost)
            {
                m_Coins -= upgradeCost;
                m_ShooterAmor.MissileMagazineLvl = nextMagLevel;
                SaveData();
                callback.Invoke(true);
            }
        }

        public void BuyMissile(System.Action<bool> callback)
        {
            int mcost = Shop.GetMissileCost();
            if (m_Coins >= mcost)
            {
                m_Coins -= mcost;
                m_ShooterAmor.MissileCount += 1;
                SaveData();
                callback.Invoke(true);
            }
        }


        public void BuyShield(System.Action<bool> callback)
        {
            int currentLvl = Shop.GetCurrentShieldLevel(m_ShieldDuration);
            Shield shield = Shop.GetNextShieldDataByLvl(currentLvl+1);

            if(shield.Cost > -1)
            {
                if (m_Coins >= shield.Cost)
                {
                    m_Coins -= shield.Cost;
                    m_ShieldDuration = shield.Duration;
                    SaveData();
                    callback.Invoke(true);
                }
            }
            
        }
    }
}


