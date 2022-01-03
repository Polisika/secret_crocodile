using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace server
{
    public class AcceptedLaws
    {
        public List<Card> Accepted { get; private set; } = new List<Card>();
        public int _liberal = 0;
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
        public int whowin = 0;  // 1 - крокодил, 2 - либералы
        public AcceptedLaws _accepted = new AcceptedLaws();
        private Card CurrentCard { get; set; } = null;

        public List<Player?> players;
        public int Round { get; set; } = 1;
        private Random _rand;
        private int _prev_pres;
        private Player president = null;
        public Player President
        {
            get => president;
            private set
            {
                if (president != null)
                    president.isPresident = false;
                if (!(President is null) && !(_cancellor is null) && President == _cancellor)
                    throw new Exception("Ошибка назначения президента. Президент не может быть одновременно и канцлером");

                president = value;
                president.isPresident = true;
            }
        }
        private Player _cancellor = null;
        public Player Cancellor
        {
            get => _cancellor;
            private set
            {
                if (_cancellor != null)
                {
                    _cancellor.isChancellor = false;
                    if (_cancellor.role == RoleType.Crokodile && _accepted._croco >= 3)
                    {
                        Console.WriteLine("Так как канцлером выбран крокодил - крокодилы выиграли.");
                        throw new ApplicationException("Крокодилы победили");
                    }
                }

                _cancellor = value;
                _cancellor.isChancellor = true;
            }
        }

        public void StartSession(int count, string[] names)
        {
            if (count != 4)
                throw new Exception("Для начала нужно ровно 5 игроков");
            var rand = new Random();
            _rand = rand;
            List<Player> players = new List<Player>();
            int crocodile = rand.Next(count);
            int helper_croco = rand.Next(count);
            while (helper_croco == crocodile)
                helper_croco = rand.Next(count);
            for (int i = 0; i <= count; i++)
            {
                if (crocodile == i)
                    players.Add(new Player(RoleType.Crokodile, Party.Fascist, i, names[i]));
                else if (helper_croco == i)
                    players.Add(new Player(RoleType.Fascist, Party.Fascist, i, names[i]));
                else
                    players.Add(new Player(RoleType.Liberal, Party.Liberal, i, names[i]));
            }
            this.players = players;
        }

        private async Task<int> WaitVoteCancellor()
        {
            Console.WriteLine("ВЫБОР КАНЦЛЕРА");

            while (President.PlayerNumCancellor is null)
                await Task.Delay(1000);
            
            int num = (int)President.PlayerNumCancellor;
            President.PlayerNumCancellor = null;
            Console.WriteLine("Канцлер выбран: " + num.ToString());
            return num;
        }
        private void choosePresident()
        {
            /* Меняет поле President и players */
            if (Round == 1)
            {
                // На первом раунде президент выбирается без голосования.
                var playerNum = _rand.Next(players.Count);
                Console.WriteLine("Выбран президент: " + playerNum.ToString());
                _prev_pres = playerNum;
                President = players[playerNum];
                Console.WriteLine("Первый президент " + playerNum.ToString());
            }
            else
            {
                players[_prev_pres].isPresident = false;
                Console.WriteLine(_prev_pres.ToString() + " перестал быть президентом");
                // Президент передается по кругу.
                if (_prev_pres + 1 == players.Count)
                    _prev_pres = 0;
                else
                    _prev_pres++;
                Console.WriteLine(_prev_pres + " - президент");
                President = players[_prev_pres];
            }
        }
        private async Task chooseCancellor()
        {
            int n;
            try
            {
                n = await WaitVoteCancellor();
                Cancellor = players[n];
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private async Task giveCardsPresident()
        {
            /* Меняет поле Cards у игрока */
            var Cards = new List<Card>(3);
            for (int i = 0; i < 3; i++)
            {
                var isLiberal = generatePartyCard();
                Cards.Add(new Card(isLiberal));
            }
            President.cards = Cards;

            Console.WriteLine("Ожидаем выбор президента...");
            while (!President.cardDropped)
                await Task.Delay(500);
            // Возвращаем в исходное состояние
            President.cardDropped = false;
        }
        private async Task giveCardsCancellor()
        {
            /* Меняет поле Cards у президента и перекидывает их канцлеру */
            Cancellor.cards = President.cards;
            President.cards = new List<Card>();
            Console.WriteLine("Ожидаем выбор канцлера...");
            while (!Cancellor.cardDropped)
                await Task.Delay(500);

            Cancellor.cardDropped = false;
            CurrentCard = new Card(Cancellor.cards[0].isLiberal);
            Console.WriteLine("Текущая карта - " + CurrentCard.isLiberal.ToString());
            Cancellor.cards = new List<Card>();
        }
        public bool generatePartyCard()
        {
            var digit = _rand.NextDouble();
            bool isLiberal = true;
            // всего 17 карт, из них 6 - либеральные, 11 фашистские
            // Если digit / 17 > 6 / 17, то это фашистский закон, иначе - либеральный
            if (digit > (6.0 / 17.0))
                isLiberal = false;

            Console.WriteLine("Сгенерирован либеральный закон? " + isLiberal.ToString());
            return isLiberal;
        }
        private void acceptLaw(bool generateCard)
        {
            Console.WriteLine("Принимаем закон...");
            /* Принимает карту. Меняет поле AcceptedLaws и CurrentCard. Если в поле CurrentCard null, то ничего не меняет. 
             * Если установлен флаг generateCard - принимается случайный закон */
            if (generateCard)
            {
                var isLiberal = generatePartyCard();
                _accepted.Add(new Card(isLiberal));
            }
            else if (CurrentCard != null)
            {
                _accepted.Add(CurrentCard);
                Console.WriteLine("Закон принят");
            }
            else
            {
                for (int i = 0; i < 10; i++)
                    Console.WriteLine("ОШИБКА! ЗАКОН NULL ПО НЕПРЕДВИДЕННЫМ ОБСТОЯТЕЛЬСТВАМ");
            }

            CurrentCard = null;
        }

        public async Task play()
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("Начало " + Round.ToString() + " раунда");
                    // field isPresident
                    choosePresident();
                    // fields isCancellor, wereCancellor
                    await chooseCancellor();

                    // fields with cards
                    await giveCardsPresident();
                    await giveCardsCancellor();

                    acceptLaw(false);

                    Console.WriteLine("Конец " + Round.ToString() + " раунда");
                    Round++;
                }
            }
            catch (ApplicationException)
            {
                Console.WriteLine("Ну победили крокодильчики, чего бухтеть-то.");
                whowin = 1;
            }
            catch (AccessViolationException)
            {
                Console.WriteLine("Всё, ладно, победили либералы, давай заканчивай.");
                whowin = 2;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }

}
