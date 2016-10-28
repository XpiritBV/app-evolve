namespace DataManager.ViewModels
{
    public class ConferenceFeedbackViewModel
    {
        public int Q { get; set; }
        public decimal Score { get; set; }
        public int OneStar { get; set; }
        public int TwoStar { get; set; }
        public int ThreeStar { get; set; }
        public int FourStar { get; set; }
        public int FiveStar { get; set; }

        public string QuestionText => Translate();

        string Translate()
        {
            switch(Q)
            {
                case 1:
                    return "Hoe tevreden bent u over het algemeen genomen met dit evenement?";
                case 2:
                    return "Hoe beoordeelt u de locatie (RAI Amsterdam)?";
                case 3:
                    return "Hoe beoordeelt u de online registratie en betaling voor TechDays 2016?";
                case 4:
                    return "Hoe beoordeelt u de website van TechDays 2016?";
                case 5:
                    return "Hoe beoordeelt u deze TechDays 2016 app?";
                case 6:
                    return "Wat is uw mening over Microsoft na deelname aan dit evenement?";
                case 7:
                    return "Kunt u het geleerde tijdens TechDays 2016 in de praktijk brengen?";
                default:
                    return "n.v.t.";
            }
        }
    }
}