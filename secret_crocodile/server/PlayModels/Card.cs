using System;
using System.Collections.Generic;
using System.Text;

namespace server
{
    public class Card
    {
        public Card(bool isLiberal)
        {
            this.isLiberal = isLiberal;
        }
        public bool isLiberal { get; set; }
    }
}
