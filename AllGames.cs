using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace TelegramBot {

    /// <summary>
    /// Класс реализует набор игр в города
    /// </summary>
    class AllGames {

        #region Constructors

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public AllGames() { }

        /// <summary>
        /// Конструктор 1
        /// </summary>
        /// <param name="game"></param>
        public AllGames(CitiesGame game) {
            games.Add(game);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Проверяет есть ли идентификатор игры среди игр
        /// </summary>
        /// <param name="id">Проверяемый идентификотор игры</param>
        /// <returns>true - идентификатор в играх есть (т.е. такая игра создана), false - идентификатора нет (игра не создана)</returns>
        public bool isFindId(long id) {
            // Если игра с идентификатором находится то return true
            if (games.Find(item => item.IdGame == id) != null)
                    return true;

            return false;
        }


        public void removeGame(long id) {
            games.Remove(games.Find(item => item.IdGame == id));
        }

        #endregion


        #region Fields

        private List<CitiesGame> games; // все игры с ботом

        #endregion
    }
}
