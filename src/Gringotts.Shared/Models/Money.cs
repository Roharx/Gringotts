namespace Gringotts.Shared.Models
{
    public class Money
    {
        public int Galleons { get; set; }
        public int Sickles { get; set; }
        public int Knuts { get; set; }

        public int TotalKnuts => Galleons * 493 + Sickles * 29 + Knuts;
    }
}