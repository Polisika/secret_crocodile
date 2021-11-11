using secret_crocodile.PlayModels;
using System;
using System.Collections.Generic;

namespace secret_crocodile.Server
{
    class Session
    {
        public List<Card> AcceptedLaws { get; private set; }
        private Card CurrentCard { get; set; }

        public List<Player> players;
        public int Round { get; set; } = 1;
        public Player President { get; private set; }
        private Player _cancellor;
        public Player Cancellor
        {
            get => _cancellor;
            private set
            {
                if (President == _cancellor)
                    throw new Exception("Президент не может быть одновременно и канцлером");
                if (_cancellor.wereCancellor)
                    throw new Exception("Невозможно выбрать канцлером данного игрока, так как он был канцлером на прошлом ходу.");

                _cancellor = value;
            }
        }

        public Session(List<Player> players) {
            this.players = players;    
        }
        
        private void choosePresident() {
        /* Меняет поле President */
        }
        private void chooseCancellor() {
        /* Меняет поле Cancellor */
        }
        private void waitAllPlayers() {
        /* Проверяет наличие события старта.
         * Если игроков не хватает - выкидывает ошибку */
        }
        private void setRoles() { 
            /* Случайно устанавливает всем роли */
        }
        private void giveCardsPresident() {
            /* Меняет поле Cards у игрока */
        }
        private void giveCardsCancellor() {
            /* Меняет поле Cards у президента и перекидывает их канцлеру */
        }
        private void veto() {
            /* Проверяет, есть ли право Вето. Если есть, то запускает голосование и обнуляет карту, которую приняли. */
        }
        private void acceptLaw(bool generateCard) {
            /* Принимает карту. Меняет поле AcceptedLaws и CurrentCard. Если в поле CurrentCard null, то ничего не меняет. 
             * Если установлен флаг generateCard - принимается случайный закон */
        }
        private void showParty() {
            /* Проверяет, выполнены ли условия для показа партии. Если да, то запускает процедуру показа партии */
        }
        private void killPresident() {
            /* Проверяет, выполнены ли условия для убийства игрока президентом. Если да, то запускает процедуру убийства игрока */
        }
        private void checkWin() {
            /* Проверяет условия завершения игры.
             * Игра завершена если:
             * 1) канцлером выбрали крокодила, когда фашистских законов больше трёх 
             * 2) карт одной или другой партии равно числу для выигрыша.
             * 3) количество раундов не превысило критического.
             * 4) все игроки отключены.
             */
        }

        public void play()
        {
            // field isReady
            waitAllPlayers();
            setRoles();

            while (true)
            {
                // field isPresident
                choosePresident();
                acceptLaw(true);
                // fields isCancellor, wereCancellor
                chooseCancellor();
                checkWin();

                // fields with cards
                giveCardsPresident();
                choosePresident();
                giveCardsCancellor();
                chooseCancellor();

                showParty();
                killPresident();
                veto();

                acceptLaw(false);

                checkWin();
                Round++;
            }
        }
    }
}
