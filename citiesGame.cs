using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace TelegramBot {

    /// <summary>
    /// Класс реализует игру в города игрока с чат-ботом
    /// </summary>
    class CitiesGame {
        #region Properties

        /// <summary>
        /// Идентификатор игры
        /// </summary>
        public long IdGame { get; }

        /// <summary>
        /// Последний названный ботом город
        /// </summary>
        public string LastCityBotSay { get; set; }
        
        /// <summary>
        /// Города о которых знает бот (рандом из городов Вики бота с шагом 20-80)
        /// </summary>
        public List<string> CitiesKnowBOT { get; set; }

        /// <summary>
        /// Города о которых знает Википедия бота (все города из переданного списка)
        /// </summary>
        public List<string> CitiesWikiBOT { get; set; }

        #endregion


        #region Consructors

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public CitiesGame() { }

        /// <summary>
        /// Конструктор 1
        /// </summary>
        /// <param name="idGame">Идентификатор игры</param>
        public CitiesGame(long idGame) {
            IdGame = idGame;
        }
        
        /// <summary>
        /// Конструктор 2.1
        /// </summary>
        /// <param name="idGame">Идентификатор игры</param>
        /// <param name="citiesWikiBot">Лист всех городов, которые "знает" "википедия" бота</param>
        /// <param name="citiesKnowBOT">Лист городов, которые знает бот (много меньше городов в "википедии" бота)</param>
        public CitiesGame(long idGame, List<string> citiesWikiBot, List<string> citiesKnowBOT) {
            IdGame = idGame;
            CitiesWikiBOT = citiesWikiBot;
            //CitiesWikiBOT.AddRange(citiesWikiBot);
            CitiesKnowBOT = citiesKnowBOT;
            //CitiesKnowBOT.AddRange(citiesKnowBOT);
        }

        /// <summary>
        /// Конструктор 2.2
        /// </summary>
        /// <param name="idGame">Идентификатор игры</param>
        /// <param name="citiesWikiBot"></param>
        public CitiesGame(long idGame, List<string> citiesWikiBot) : this(idGame, citiesWikiBot, new List<string>()) {
            this.makeCityKnowBot(citiesWikiBot);
        }

        #endregion



        #region Methods

        /// <summary>
        /// Возвращаем первое название города, начинающееся на переданную букву в "знаниях" бота и удаляем город из "знаний"
        /// </summary>
        /// <param name="firstLetter">Проверяемая буква в "знаниях городов" бота</param>
        /// <returns>Название города</returns>
        public string cityWhoBotKnow(char firstLetter) {
            firstLetter = char.ToUpper(firstLetter);    // переводим переданную букву в верхний регистр

            if (CitiesKnowBOT != null) {
                string city = CitiesKnowBOT.Find(item => item.StartsWith(firstLetter));

                if (!String.IsNullOrEmpty(city))
                    // Если есть название города начинающееся с буквы firstLetter
                    return city;
            }

            return String.Empty;
        }

        /// <summary>
        /// "Знает" ли википедия бота название города (названного пользователем)
        /// </summary>
        /// <returns>true - знает, false - не знает</returns>
        public bool isWikiKnowCity(string city) {
            city = city.ToUpper();

            return CitiesWikiBOT.Contains(city);
        }


        /// <summary>
        /// Удаление города из "википедии" бота
        /// </summary>
        /// <param name="city">Название удаляемого города</param>
        public void delCitiyInWikiBOT(string city) {
            city = city.ToUpper();

            if (CitiesWikiBOT.Contains(city))
                    CitiesWikiBOT.Remove(city);
        }

        /// <summary>
        /// Удаление города из "знаний" бота
        /// </summary>
        /// <param name="city">Название удаляемого города</param>
        public void delCityInKnowBOT(string city) {
            city = city.ToUpper();

            if (CitiesKnowBOT.Contains(city))
                    CitiesKnowBOT.Remove(city);
        }


        /// <summary>
        /// Создать базу знаний названий городов бота
        /// </summary>
        /// <param name="citiesWikiBot">Города, которые "знает" бот</param>
        private void makeCityKnowBot(List<string> citiesWikiBot) {
            Random rnd = new Random(DateTime.Now.Millisecond);

            for (int i = rnd.Next(10, 40); i < citiesWikiBot.Count; i += rnd.Next(20, 90)) {
                CitiesKnowBOT.Add(citiesWikiBot[i]);
            }
        }

        #endregion
    }
}
