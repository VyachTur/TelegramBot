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
        public AllGames() {
            games = new List<CitiesGame>();
        }

        /// <summary>
        /// Конструктор 1
        /// </summary>
        /// <param name="game"></param>
        public AllGames(CitiesGame game) {
            games = new List<CitiesGame>();
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
            // Если игр не создавалось, то возвращаем false
            if (games == null)
                return false;

            // Если игра с идентификатором находится то return true
            if (games.Find(item => item.IdGame == id) != null)
                    return true;

            return false;
        }

        /// <summary>
        /// Добавляет игру во все игры бота
        /// </summary>
        /// <param name="game">Игра</param>
        public void addGame(CitiesGame game) {
            if (games == null) return;

            games.Add(game);
        }

        /// <summary>
        /// Возвращает игру по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор игры</param>
        /// <returns>Игра</returns>
        public CitiesGame returnGame(long id) {
            if (games == null) return new CitiesGame();

            return games.Find(item => item.IdGame == id);
        }

        /// <summary>
        /// Удаляет игру из всех игр бота
        /// </summary>
        /// <param name="id">Идентификатор игры</param>
        public void removeGame(long id) {
            if (games == null) return;

            games.Remove(games.Find(item => item.IdGame == id));
        }

        #endregion


        #region Fields

        private List<CitiesGame> games; // все игры с ботом

        #endregion
    }
}
