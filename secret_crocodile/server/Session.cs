using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server
{
    public class Session
    {
        public List<Card> AcceptedLaws { get; private set; }
        private Card CurrentCard { get; set; }

        public List<Player> players;
        public int Round { get; set; } = 1;
        private Random _rand;
        public Events Event { get; set; }
        private int _prev_pres;
        private int _true_prev_pres;
        private Player president;
        public Player President { get => president; 
            private set
            {
                if (President == _cancellor)
                    throw new Exception("Президент не может быть одновременно и канцлером");

                president.isPresident = false;
                president = value;
                president.isPresident = true;
            }
        }
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
                
                _cancellor.isChancellor = false;
                _cancellor = value;
                _cancellor.isChancellor = true;
            }
        }

        public Session(int count)
        {
            var rand = new Random();
            this._rand = rand;
            List<Player> players = new List<Player>();
            int crocodile = rand.Next(count);
            int helper_croco = rand.Next(count);
            while (helper_croco == crocodile)
                helper_croco = rand.Next(count);
            for (int i = 0; i < count; i++)
            {
                if (crocodile == i)
                    players.Add(new Player(RoleType.Crokodile, Party.Fascist));
                else if (helper_croco == i)
                    players.Add(new Player(RoleType.Fascist, Party.Fascist));
                else
                    players.Add(new Player(RoleType.Liberal, Party.Liberal));
            }
            this.players = players;
        }

        private async Task<bool?> WaitVotes(Events inner_event)
        {
            Event = inner_event;
            while (true)
            {
                foreach (var p in players)
                    if (p.vote is null)
                    {
                        await Task.Delay(1);
                        continue;
                    }
                break;
            }
            Event = Events.NONE;

            var votes = 0;
            foreach (var p in players)
            {
                if (p.vote == false)
                    votes--;
                else
                    votes++;
                p.vote = null;
            }

            bool? result;
            if (votes == 0)
                result = null;
            else
                result = votes > 0;


            return result;
        }
        private async Task<int> WaitVoteCancellor()
        {
            Event = Events.CHOOSE_CANCELLOR;
            // чтобы не было бесконечного ожидания
            int i = 0;
            while (President.PlayerNumCancellor is null)
            {
                i++;
                await Task.Delay(1000);
                if (i == 15)
                {
                    Event = Events.NONE;
                    return _rand.Next(players.Count);
                }
            }
            Event = Events.NONE;
            return (int)President.PlayerNumCancellor;
        }
        private async void choosePresident()
        {
            /* Меняет поле President и players */
            if (Round == 1)
            {
                // На первом раунде президент выбирается без голосования.
                var playerNum = _rand.Next(players.Count);
                _prev_pres = playerNum;
                _true_prev_pres = playerNum;
                President = players[playerNum];
            }
            else
            {
                // Президент передается по кругу.
                if (_prev_pres + 1 == players.Count)
                    _prev_pres = 0;
                else
                    _prev_pres++;
            }
        }
        private async void chooseCancellor()
        {
            Cancellor = players[await WaitVoteCancellor()];
            /* Меняет поле Cancellor */
            if (Round != 1)
            {
                bool? resp = await WaitVotes(Events.CHOOSE_CANCELLOR);

                if ((bool)resp)
                    Cancellor = players[(int)President.PlayerNumCancellor];
                // Если три раза не выбрали президента - принимается рандомный закон из кучи.
                else if (Math.Abs(_true_prev_pres - _prev_pres) >= 3)
                {
                    acceptLaw(true);
                    _true_prev_pres = _prev_pres;
                }
            }
        }
        private void waitAllPlayers()
        {
            /* Проверяет наличие события старта.
             * Если игроков не хватает - выкидывает ошибку */
        }
        private void setRoles()
        {
            /* Случайно устанавливает всем роли */
        }
        private void giveCardsPresident()
        {
            /* Меняет поле Cards у игрока */
        }
        private void giveCardsCancellor()
        {
            /* Меняет поле Cards у президента и перекидывает их канцлеру */
        }
        private void veto()
        {
            /* Проверяет, есть ли право Вето. Если есть, то запускает голосование и обнуляет карту, которую приняли. */
        }
        private void acceptLaw(bool generateCard)
        {
            /* Принимает карту. Меняет поле AcceptedLaws и CurrentCard. Если в поле CurrentCard null, то ничего не меняет. 
             * Если установлен флаг generateCard - принимается случайный закон */
        }
        private void showParty()
        {
            /* Проверяет, выполнены ли условия для показа партии. Если да, то запускает процедуру показа партии */
        }
        private void killPresident()
        {
            /* Проверяет, выполнены ли условия для убийства игрока президентом. Если да, то запускает процедуру убийства игрока */
        }
        private void checkWin()
        {
            /* Проверяет условия завершения игры.
             * Игра завершена если:
             * 1) канцлером выбрали крокодила, когда фашистских законов больше трёх 
             * 2) карт одной или другой партии равно числу для выигрыша.
             * 3) количество раундов не превысило критического.
             * 4) все игроки отключены.
             */
        }

        public async void play()
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
