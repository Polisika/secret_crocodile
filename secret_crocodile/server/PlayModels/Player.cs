
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
        }
        public int? PlayerNumCancellor { get; set; } = null;
        public int? KillPlayer { get; set; } = null;
        public int? ShowParty { get; set; } = null;
        public RoleType role { get; set; }
        public Party party { get; set; }
        public bool isPresident { get; set; } = false;
        public bool isChancellor { get; set; } = false;
        public bool wereCancellor { get; set; } = false;
        public bool? vote { get; set; } = null;
        public List<Card> cards = new List<Card>();
        public bool? partyReady { get; set; } = null;
        public void DropCard(int index)
        {
            cards.RemoveAt(index);
        }
    }
}
