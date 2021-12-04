
using System.Collections.Generic;
using System.Threading.Tasks;

namespace server
{
    public class Player
    {
        public Player(RoleType role, Party party)
        {
            this.role = role;
            this.party = party;
            this.isPresident = false;
            this.isChancellor = false;
            this.wereCancellor = false;
        }
        public int? PlayerNumCancellor { get; set; } = null;
        public int? KillPlayer { get; set; } = null;
        public RoleType role { get; set; }
        public Party party { get; set; }
        public bool isPresident { get; set; }
        public bool isChancellor { get; set; }
        public bool wereCancellor { get; set; }
        public bool? vote { get; set; } = null;
        public List<Card> cards;

        public void DropCard(int index)
        {
            cards.RemoveAt(index);
        }
    }
}
