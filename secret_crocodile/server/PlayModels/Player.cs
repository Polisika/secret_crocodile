
using System.Collections.Generic;
using System.Threading.Tasks;

namespace server
{
    public class Player
    {
        public Player(RoleType role, Party party, int num, string name)
        {
            this.role = role;
            this.party = party;
            this.num = num;
            this.name = name;
        }
        public string name { get; set; }
        public int num { get; set; }
        public int? PlayerNumCancellor { get; set; } = null;
        public RoleType role { get; set; }
        public Party party { get; set; }
        public bool isPresident { get; set; } = false;
        public bool isChancellor { get; set; } = false;
        public bool wereCancellor { get; set; } = false;
        public List<Card> cards = new List<Card>();
        public bool cardDropped = false;
        public void DropCard(int index)
        {
            cards.RemoveAt(index);
            cardDropped = true;
        }
    }
}
