namespace secret_crocodile.PlayModels
{
    class Player
    {
        public RoleType role { get; }
        public Party party { get; }
        public bool isPresident { get; set; }
        public bool isChancellor { get; set; }
        public bool wereCancellor { get; set; }
    }
}
