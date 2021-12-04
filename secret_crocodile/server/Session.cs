using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server
{
    class AcceptedLaws
    { 
        public List<Card> Accepted { get; private set; }
        private int _liberal = 0;
        public int _croco { get; private set; } = 0;
        public void Add(Card card)
        {
            Accepted.Add(card);
            if (card.isLiberal)
                _liberal++;
            else
                _croco++;

            if (_liberal == 5)
                throw new AccessViolationException("Либералы выиграли");
            else if (_croco == 6)
                throw new ApplicationException("Крокодилы выиграли");
        }
    }

    public class Session
    {
        private AcceptedLaws _accepted;
        private Card CurrentCard { get; set; }

        public List<Player?> players;
        public int Round { get; set; } = 1;
        private Random _rand;
        public Events Event { get; set; }
        private int _prev_pres;
        private int _true_prev_pres;
        private int _killed = 0;
        private bool _haveVeto = false;
        private bool _wasShowParty = false;
        private Player president;
        public Player President { get => president; 
            private set
            {
                if (President == _cancellor)
                    throw new Exception("Президент не может быть одновременно и канцлером");

                president.isPresident = true;
                president = value;
            }
        }
        private Player _cancellor;
        public Player Cancellor
        {
            get => _cancellor;
            private set
            {
                if (President == _cancellor)
                    throw new ArgumentException("Президент не может быть одновременно и канцлером");
                if (_cancellor.wereCancellor)
                    throw new ArgumentException("Невозможно выбрать канцлером данного игрока, так как он был канцлером на прошлом ходу.");


                if (_cancellor.role == RoleType.Crokodile && _accepted._croco >= 3)
                    throw new ApplicationException("Крокодилы победили");

                _cancellor.isChancellor = true;
                _cancellor = value;
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
                players[playerNum].isPresident = true;
                President = players[playerNum];
            }
            else
            {
                players[_prev_pres].isPresident = false;

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
                Cancellor.isChancellor = false;

                bool? resp = await WaitVotes(Events.CHOOSE_CANCELLOR);
                // Президент передается по кругу.
                if (_prev_pres + 1 == players.Count)
                    _prev_pres = 0;
                else
                    _prev_pres++;

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
        private async void giveCardsPresident()
        {
            /* Меняет поле Cards у игрока */
            var Cards = new List<Card>(3);
            for (int i = 0; i < 3; i++)
            {
                var isLiberal = generatePartyCard();
                Cards.Add(new Card(isLiberal));
            }
            President.cards = Cards;

            // Нужно подождать выбора игрока, добавить флаг готовности?
        }
        private async void giveCardsCancellor()
        {
            /* Меняет поле Cards у президента и перекидывает их канцлеру */
            Cancellor.cards = President.cards;
        }
        private void veto()
        {
            /* Проверяет, есть ли право Вето. Если есть, то запускает голосование и обнуляет карту, которую приняли. */
        }
        public bool generatePartyCard()
        {
            var digit = _rand.NextDouble();
            bool isLiberal = true;
            // всего 17 карт, из них 6 - либеральные, 11 фашистские
            // Если digit / 17 > 6 / 17, то это фашистский закон, иначе - либеральный
            if ((digit / 17) > (6 / 17))
                isLiberal = false;

            return isLiberal;
        }
        private void acceptLaw(bool generateCard)
        {
            /* Принимает карту. Меняет поле AcceptedLaws и CurrentCard. Если в поле CurrentCard null, то ничего не меняет. 
             * Если установлен флаг generateCard - принимается случайный закон */
            if (generateCard)
            {
                var isLiberal = generatePartyCard();
                CurrentCard = new Card(isLiberal);
            }

            _accepted.Add(CurrentCard);
        }
        private void showParty()
        {
            /* Проверяет, выполнены ли условия для показа партии. Если да, то запускает процедуру показа партии */

        }
        private async void killByPresident()
        {
            /* Проверяет, выполнены ли условия для убийства игрока президентом. Если да, то запускает процедуру убийства игрока */
            if (!(_accepted._croco == 4 && _killed == 0) || !(_accepted._croco == 5 && _killed == 1))
                return;

            // Ждем, когда президент выберет
            var player = players[(int)President.KillPlayer];
            if (player.role == RoleType.Crokodile)
                throw new AccessViolationException("Либералы выиграли");

            players[(int)President.KillPlayer] = null;
            _killed++;
            }
        }

        public async void play()
        {
            while (true)
            {
                // field isPresident
                choosePresident();
                // fields isCancellor, wereCancellor
                chooseCancellor();

                // fields with cards
                giveCardsPresident();
                choosePresident();
                giveCardsCancellor();
                chooseCancellor();

                veto();

                acceptLaw(false);

                showParty();
                killByPresident();
                Round++;
            }
        }
    }

}
